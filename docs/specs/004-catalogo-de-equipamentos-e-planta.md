# 004 — Catálogo de equipamentos e vínculo com a planta

| Campo | Valor |
|---|---|
| Status | Implementada |
| Serviços afetados | RoomService, Gateway |
| Depende de | — (pode correr em paralelo com 002 e 003) |
| Autor / data | Darlan · 2026-09-02 |

## 1. Problema

Três lacunas impedem a planta de mostrar dados reais:

1. **`Equipment.Type` é texto livre.** "Monitor", "monitor" e "Tela" são três coisas
   distintas para o sistema e nenhuma mapeia para um marcador na planta.
2. **Não existe listagem de equipamentos** — só `POST /api/equipments`. A tela de
   Equipamentos não teria como carregar dados. E `/api/equipments` **não é roteado pelo
   Gateway**.
3. **Nada liga uma sala ao seu lugar na planta.** Com a planta fixa
   ([arquitetura de informação](../product/information-architecture.md#tratamento-visual)),
   cada sala precisa apontar para o polígono que a representa.

## 2. Resultado esperado

Equipamento tem tipo de um conjunto conhecido, cada tipo sabe onde fica na sala, a lista de
equipamentos é consultável, e cada sala aponta para seu lugar na planta — o suficiente para
a planta desenhar marcadores a partir do cadastro.

## 3. Escopo

**Incluído**

- `EquipmentType` como vocabulário controlado, com âncora padrão por tipo
- `RoomEquipment.Placement` opcional, para sobrescrever a âncora
- `GET /api/equipments` com filtros opcionais
- `Room.PlanSlot` ligando a sala ao polígono da planta
- Rota `/api/equipments/*` no Gateway
- Migrations e mapeamento dos dados existentes

**Fora de escopo**

- Editor de posicionamento por arrastar-e-soltar — só quando alguém provar que a posição
  exata importa ([arquitetura de informação](../product/information-architecture.md))
- Dimensões, formato ou capacidade da sala — desnecessários com planta fixa
- Alterar ou remover equipamento; associar equipamento a sala já criada
- Múltiplos andares

## 4. Contrato

### `EquipmentType` e âncoras

| Tipo | Âncora padrão |
|---|---|
| `Tv`, `Monitor`, `QuadroBranco` | `Parede` |
| `Projetor`, `ArCondicionado` | `Teto` |
| `Telefone`, `Notebook`, `Dock` | `Mesa` |
| `Flipchart`, `Cadeira` | `Piso` |
| `Outro` | `Piso` |

A âncora é conhecimento de domínio e vive no backend — um projetor **está** no teto. O
frontend recebe a âncora pronta e não replica a tabela.

### `POST /api/equipments` — **mudança incompatível**

`type` deixa de aceitar texto livre e passa a exigir um valor do vocabulário.

```json
{ "type": "Projetor", "brand": "Epson", "serialNumber": "SN-123", "purchaseDate": "2024-01-10" }
```

Valor fora do vocabulário → `400` com `"Tipo de equipamento desconhecido."` e a lista dos aceitos no
`detail`.

A resposta ganha `placement`:
```json
{ "equipment": { "id": "…", "type": "Projetor", "placement": "Teto", "brand": "Epson", "serialNumber": "SN-123", "purchaseDate": "2024-01-10T00:00:00Z" } }
```

### `GET /api/equipments` — autenticado

| Parâmetro | Efeito |
|---|---|
| `type` | filtra pelo tipo |
| `unassigned=true` | só equipamentos não alocados a nenhuma sala |

`200 OK` com `EquipmentResponse[]`, cada item incluindo `placement` e `roomId` (nulo quando
não alocado). **Vazio devolve `200` com `[]`** — mesma justificativa da spec 003: aqui o
vazio é resposta legítima.

O `roomId` evita que a tela de Equipamentos precise cruzar listas no cliente para saber o
que está livre para alocar.

### `Room.PlanSlot`

Inteiro de 1 a 10, único entre as salas, opcional. Identifica qual polígono da planta fixa
representa a sala. Sala sem `planSlot` simplesmente não aparece na planta — aparece nas
listas normalmente.

Aceito em `POST /api/rooms` e `PATCH /api/rooms/{id}`, devolvido em `RoomResponse`.

Slot já usado por outra sala → `400` com `"Esta posição da planta já está ocupada."`

## 5. Regras de negócio

- `EquipmentType` é enum de domínio; a âncora padrão é derivada dele, não armazenada por
  equipamento.
- `RoomEquipment.Placement` é **nulo por padrão** e só existe para sobrescrever. Nulo
  significa "use a âncora do tipo". Nenhum cadastro precisa preenchê-lo.
- `PlanSlot` entre 1 e 10 (limite da planta atual); fora disso, `DomainException`.
- Unicidade de `PlanSlot` é validada no caso de uso **e** por índice único no banco — ao
  contrário das unicidades existentes, que só têm a checagem em código
  ([B5](../sdd/backlog.md)). Não repetir aqui a dívida que já temos.

## 6. Impacto em dados

Três migrations, nesta ordem:

1. **`Equipments.Type` para o vocabulário.** Armazenado como texto com o nome do enum.
   Dados existentes: mapeamento por correspondência case-insensitive do valor atual; o que
   não casar vira `Outro`. Nenhuma linha é perdida.
2. **`RoomEquipment.Placement`**, texto anulável.
3. **`Rooms.PlanSlot`**, inteiro anulável, com índice único filtrado (ignora nulos).

Reversível: os `Down` restauram texto livre e removem as colunas novas.

> O ambiente de desenvolvimento sobe com banco vazio ou quase; o mapeamento para `Outro`
> tem custo real próximo de zero hoje. Registrado porque em produção não teria.

## 7. Impacto entre serviços

Rota nova no Gateway:

```
/api/equipments/{**catch-all}  →  equipments-cluster  →  http://roomservice:5000
```

Isso torna `POST /api/equipments` acessível pela porta 5000 pela primeira vez — hoje só
funciona batendo direto na 5003.

`POST /api/equipments` é **quebra de contrato**: qualquer cliente que envie tipo livre
passa a receber `400`. Não há cliente em produção; o único consumidor é o Swagger.

Nenhum outro serviço lê `Equipment` — RoomService é o único dono.

## 8. Critérios de aceite

> Verificados em 2026-09-06 com a pilha em Docker, via Gateway na 5000.

- [x] Dado `type: "Projetor"`, quando cadastro um equipamento, então recebo `201` com `placement: "Teto"`.
- [x] Dado `type: "Holograma"`, então recebo `400` com `"Tipo de equipamento desconhecido."` e a lista de tipos aceitos.
- [x] Dado `type: "projetor"` em minúsculas, então é aceito — a comparação não diferencia caixa.
- [x] Dado equipamentos cadastrados, quando consulto `GET /api/equipments`, então recebo todos, cada um com `placement` e `roomId`.
- [x] Dado nenhum equipamento, então recebo `200` com `[]`.
- [x] Dado `unassigned=true`, então recebo apenas os que não estão em nenhuma sala.
- [x] Dado uma sala com `planSlot: 3`, quando cadastro outra com `planSlot: 3`, então recebo `400` com `"Esta posição da planta já está ocupada."`.
- [x] Dado `planSlot: 11`, então recebo `400`.
- [x] Dado uma sala sem `planSlot`, então ela é criada normalmente e aparece nas listagens.
- [ ] Dado um equipamento com `Type` livre gravado antes da migration, quando aplico as migrations, então ele passa a valer `Outro` e nenhuma linha é perdida. — **não verificado**: o banco de desenvolvimento não tinha linhas legadas. O SQL de normalização está na migration `EquipmentCatalogAndPlanSlot` e roda antes do `AlterColumn`.
- [x] `POST /api/equipments` responde através do Gateway na porta 5000.
- [x] Testes de unidade cobrindo a âncora por tipo, o tipo desconhecido e os limites de `planSlot`.

## 9. Decisões em aberto

Nenhuma.
