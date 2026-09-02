# 003 — Disponibilidade de salas por intervalo

| Campo | Valor |
|---|---|
| Status | Rascunho |
| Serviços afetados | ReservationService (consome RoomService) |
| Depende de | [002](002-consulta-por-id-e-auth-servico.md) |
| Autor / data | Darlan · 2026-09-02 |

## 1. Problema

Disponibilidade **não existe** no backend. Uma sala não carrega estado: livre ou ocupada é
resultado de cruzar as salas com as reservas de um intervalo, e ninguém faz esse cruzamento
hoje.

Sem isso não há planta com estado — que é a tela principal do produto — e a única forma de
saber se uma sala serve é ler a lista de reservas e comparar horários na cabeça.

## 2. Resultado esperado

Uma única chamada responde, para o andar inteiro e um intervalo escolhido, em que estado
cada sala está e até quando — o suficiente para pintar a planta e para decidir onde
reservar.

## 3. Escopo

**Incluído**

- `GET /api/reservations/availability?start=&end=` no ReservationService
- Cálculo dos três estados e dos campos de contexto temporal
- Uma única consulta ao RoomService por requisição (não uma por sala)

**Fora de escopo**

- Sugerir horários livres ("quando esta sala está livre hoje?")
- Filtrar por equipamento ou capacidade
- Cache; o volume (≤ 10 salas) não justifica

## 4. Contrato

### `GET /api/reservations/availability` — autenticado

| Parâmetro | Obrigatório | Formato |
|---|---|---|
| `start` | sim | instante UTC ISO-8601 |
| `end` | sim | instante UTC ISO-8601, maior que `start` |

`200 OK`:
```json
[
  {
    "roomId": "…", "roomName": "Sala Azul", "roomNumber": 101,
    "status": "EmUso",
    "busyUntil": "2026-09-02T15:30:00Z",
    "nextReservationAt": null
  },
  {
    "roomId": "…", "roomName": "Sala Verde", "roomNumber": 102,
    "status": "Reservada",
    "busyUntil": null,
    "nextReservationAt": "2026-09-02T17:00:00Z"
  },
  {
    "roomId": "…", "roomName": "Sala Âmbar", "roomNumber": 103,
    "status": "Disponivel",
    "busyUntil": null,
    "nextReservationAt": null
  }
]
```

Sempre devolve **todas** as salas, inclusive as sem nenhuma reserva. Ausência de sala na
resposta significa que a sala não existe, não que está livre.

Erros (`400` `ProblemDetails`, `title: "Business error"`):

| Condição | `detail` |
|---|---|
| `start` ou `end` ausente | `"Start and end are required."` |
| `start >= end` | `"Start must be before end."` |
| RoomService indisponível | `"Rooms are unavailable."` |

**Lista vazia devolve `200` com `[]`**, não `400`. Divergência deliberada do ADR-011: aqui
o vazio significa "não há salas cadastradas", que é resposta legítima e não erro de
negócio. O `requestList` do frontend continua funcionando.

## 5. Regras de negócio

### Os três estados

Avaliados por sala, contra o intervalo `[start, end)` consultado:

| Estado | Condição |
|---|---|
| `EmUso` | existe reserva que **sobrepõe** `[start, end)` |
| `Reservada` | não sobrepõe, mas existe reserva **começando depois de `end`, no mesmo dia** de `end` |
| `Disponivel` | nenhuma das anteriores |

`Reservada` responde à pergunta que a planta precisa: a sala está livre agora, mas tem
compromisso mais tarde — pode não servir para uma reunião longa.

O recorte "mesmo dia" foi escolhido em vez de uma janela em minutos por ser previsível e
não exigir número mágico configurável. Se na prática o dia inteiro provar-se largo demais,
vira parâmetro — não antes.

### Campos de contexto

- `busyUntil`: fim da reserva que causa `EmUso`. Reservas encadeadas contam como um só
  bloco — se uma termina às 15:00 e outra começa às 15:00, `busyUntil` é o fim da última.
- `nextReservationAt`: início da próxima reserva após `end`, quando `Reservada`.
- Ambos `null` quando não se aplicam.

Sobreposição usa a mesma regra da criação de reserva: `start < reservaFim && end > reservaInicio`.
Intervalos que apenas se tocam nas bordas **não** sobrepõem.

### Fuso

Datas em UTC, coerentes com `timestamptz`. A conversão para o fuso local é do frontend.

### Origem dos dados

As salas vêm de **uma** chamada a `GET /api/rooms` do RoomService, com o token propagado
(spec 002). As reservas vêm do banco local, em **uma** consulta filtrando pelo período
relevante — não uma consulta por sala.

## 6. Impacto em dados

Nenhuma migration. A consulta filtra por período; o índice em `RoomId` sugerido no
[backlog](../sdd/backlog.md) continua pendente e fora desta spec.

## 7. Impacto entre serviços

ReservationService passa a depender do RoomService também para leitura de disponibilidade.
Com o RoomService fora do ar, a chamada falha inteira com `"Rooms are unavailable."` — não
devolve resposta parcial, porque uma planta com salas faltando é pior que uma planta que
não carrega.

Nenhuma mudança de roteamento: `/api/reservations/*` já é coberto pelo Gateway.

## 8. Critérios de aceite

- [ ] Dado uma sala com reserva das 14:00 às 15:30, quando consulto `start=14:30&end=15:00`, então o estado é `EmUso` e `busyUntil` é 15:30.
- [ ] Dado a mesma sala, quando consulto `start=16:00&end=17:00`, então o estado é `Disponivel`.
- [ ] Dado uma sala com reserva às 17:00 e consulta das 09:00 às 10:00 do mesmo dia, então o estado é `Reservada` e `nextReservationAt` é 17:00.
- [ ] Dado uma sala com reserva apenas no dia seguinte, quando consulto hoje, então o estado é `Disponivel`.
- [ ] Dado reservas encadeadas 14:00–15:00 e 15:00–16:00, quando consulto às 14:30, então `busyUntil` é 16:00.
- [ ] Dado uma reserva que apenas encosta no fim do intervalo consultado, então não conta como `EmUso`.
- [ ] Dado nenhuma sala cadastrada, então recebo `200` com `[]`.
- [ ] Dado `end` anterior a `start`, então recebo `400` com `"Start must be before end."`.
- [ ] Dado nenhum token, então recebo `401`.
- [ ] Dado 10 salas, a requisição faz **uma** chamada ao RoomService e **uma** consulta ao banco.
- [ ] Testes de unidade cobrindo os três estados, o encadeamento e as bordas de intervalo, sem banco real.

## 9. Decisões em aberto

Nenhuma.
