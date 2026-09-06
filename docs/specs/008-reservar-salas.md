# 008 — Reservar salas

| Campo | Valor |
|---|---|
| Status | Rascunho |
| Serviços afetados | Frontend (ReservationService consumido, sem alteração) |
| Depende de | [002](002-consulta-por-id-e-auth-servico.md) (sem ela o `POST` sempre falha), [005](005-navegacao-e-salas.md) (navegação e salas cadastradas) |
| Autor / data | Darlan · 2026-09-06 |

## 1. Problema

**Não dá para reservar uma sala pela interface.** É o ato que dá nome ao produto e o motivo
de ele existir, e hoje só acontece pelo Swagger do ReservationService — montando o JSON,
descobrindo o `userId` e o `roomId` à mão e convertendo horário para UTC na cabeça.

O backend está pronto desde a spec 002, que consertou a integração quebrada: antes disso
`POST /api/reservations` respondia `"Usuário não encontrado."` em **toda** requisição, porque
os endpoints de consulta por id não existiam e o token não era propagado. Com isso resolvido,
criar, listar e cancelar funcionam de ponta a ponta — falta a tela.

## 2. Resultado esperado

Uma pessoa autenticada escolhe uma sala e um intervalo, reserva, e passa a ver a reserva na
seção **Reservas**. Consegue cancelar o que ainda não começou. Quando a sala já está tomada
naquele horário, recebe a recusa com clareza e pode tentar outro intervalo sem recomeçar o
preenchimento.

## 3. Escopo

**Incluído**

- Item **Reservas** na barra de navegação, apontando para `/reservas`
- `/reservas` — minhas reservas: tabela acima de 768px, cartões abaixo
- Painel de nova reserva, com seleção de sala e intervalo
- Cancelamento, com confirmação e com a ação **ausente** quando a reserva já começou
- Conversão de horário local ↔ UTC em um único lugar
- Os quatro estados de tela: carregando, vazio, erro, sucesso

**Fora de escopo**

- A planta e a entrada contextual de reserva a partir dela — spec 009. Ver §5: o painel
  desta spec é construído para ser reaproveitado ali.
- Detalhe da sala com agenda do dia — spec 010
- Reagendar (alterar horário de uma reserva existente) — **a API não expõe**; cancela-se e
  cria-se outra
- Reserva recorrente, convidados, notificação, integração com calendário
- Ver reservas de outras pessoas — ver §5, "De quem são as reservas"
- Corrigir a concorrência de sobreposição ([B4](../sdd/backlog.md)) — ver §7

## 4. Contrato

Nenhum endpoint novo. Consome, pelo Gateway:

| Chamada | Uso |
|---|---|
| `POST /api/reservations` | criar |
| `GET /api/reservations?userId=` | minhas reservas |
| `DELETE /api/reservations/{id}` | cancelar |
| `GET /api/rooms` | salas para o seletor |

### `POST /api/reservations` — autenticado

```json
{ "userId": "<guid>", "roomId": "<guid>",
  "startDate": "2026-09-10T14:00:00Z", "endDate": "2026-09-10T15:00:00Z" }
```

`201 Created` com `{ "reservation": { …, "userName": "…", "roomName": "…", "roomNumber": 101 } }`.

Erros (`400` `ProblemDetails`), **nesta ordem de verificação**:

| Condição | `detail` |
|---|---|
| Usuário não resolvido | `"Usuário não encontrado."` |
| Sala não resolvida | `"Sala não encontrada."` |
| Sobreposição | `"A sala já está reservada nesse período."` |
| Invariante | `"O horário de início precisa estar no futuro."`, `"O horário de início precisa ser anterior ao de término."`, … |

### `GET /api/reservations?userId=` — autenticado

`200 OK` com `ReservationResponse[]`. **Resultado vazio devolve `400`** com
`"Nenhuma reserva encontrada."` ([ADR-011](../architecture/decisions.md)) — logo, esta é uma
chamada de `requestList`, não de `request`, e "não tenho nenhuma reserva" precisa virar
estado vazio, jamais tela de erro.

> Acoplamento a vigiar: `requestList` reconhece a lista vazia pelo **texto** da mensagem.
> Mudar a redação no backend quebra o estado vazio em silêncio. O caminho definitivo é o
> `200` com `[]`, como já fazem `/api/equipments` e `/api/reservations/availability`; até
> lá, qualquer alteração dessa mensagem é mudança de contrato.

### `DELETE /api/reservations/{id:guid}` — autenticado

`200 OK` com `{ "id": "<guid>" }`. A linha é **removida fisicamente** (ADR-010): não há
status "cancelada" e a reserva desaparece da lista.

| Condição | `detail` |
|---|---|
| Inexistente | `"Reserva não encontrada."` |
| Já iniciada | `"Não é possível cancelar uma reserva já iniciada."` |

Roteamento no Gateway: já coberto.

## 5. Regras de negócio

### De quem são as reservas

O `userId` sai do claim `sub` do JWT — o mesmo token já decodificado para exibição desde a
[spec 001](001-login-e-sessao.md). Não há endpoint de "minhas reservas"; a lista é
`GET /api/reservations?userId=<sub>`.

**Isto é conveniência de tela, não segurança.** O backend não amarra a reserva a quem está
autenticado: qualquer pessoa autenticada pode criar reserva com o `userId` de outra e listar
as reservas de qualquer um trocando o parâmetro. A interface não oferece esse caminho, e
isso **não** o fecha. Fica registrado aqui e pertence à mesma decisão adiada de papéis e
autorização em
[information-architecture.md](../product/information-architecture.md#navegação) — não se
resolve nesta spec, e a UI não deve dar a entender que resolve.

### Tempo

| Regra | Onde |
|---|---|
| Início no futuro | invariante da entidade; o cliente também impede, para não gastar ida ao servidor |
| Início anterior ao término | idem |
| Bordas que se tocam **não** sobrepõem | servidor; 14–15 e 15–16 convivem |
| Sobreposição na mesma sala | servidor, autoridade única |

A pessoa escolhe horário **local**; o contrato é **UTC**
([cross-cutting.md](../architecture/cross-cutting.md#persistência)). A conversão acontece em
**um único módulo**, na borda da API, e nunca espalhada por componente — a alternativa é
descobrir o fuso trocado três telas depois. Toda data que entra na tela é convertida para
local na mesma fronteira.

Duração padrão sugerida ao abrir o painel: **1 hora**, a partir da próxima meia hora cheia.
É palpite deliberado, para o caso comum custar dois cliques em vez de quatro — e é editável.

### Cancelamento

A ação de cancelar **não aparece** para reserva já iniciada. Mostrar um botão que só serve
para produzir `"Não é possível cancelar uma reserva já iniciada."` é oferecer o que não
existe. A verificação do cliente é por conveniência; a recusa continua sendo do servidor, e
se ela vier assim mesmo (relógios diferentes, aba aberta há muito tempo), a mensagem é
exibida e a lista recarregada.

Cancelar pede confirmação: a operação é **irreversível** — a linha é apagada, não marcada.

### Onde a reserva nasce

Nesta spec o painel de nova reserva é aberto a partir de `/reservas`, com seleção de sala
entre as cadastradas. Quando a planta existir (spec 009), a mesma peça é aberta com sala e
horário **pré-preenchidos** pelo que a pessoa estava olhando, conforme
[information-architecture.md](../product/information-architecture.md#nova-reserva--painel-sobre-a-planta-ou-o-detalhe).

Por isso o painel é construído desde já recebendo sala e intervalo iniciais como entrada
opcional, e não lendo nada do contexto da tela onde está. É a única concessão que esta spec
faz ao futuro, e é barata: um parâmetro. Não construir a entrada pela planta agora.

## 6. Impacto em dados

Nenhum. Frontend puro — nenhuma entidade, coluna ou migration.

## 7. Impacto entre serviços

O frontend passa a chamar o ReservationService, que já chama UserService e RoomService.
Nenhum contrato muda; nenhuma ordem de implantação é imposta.

Indisponibilidade a jusante — o comportamento herdado é ruim e a UI não deve disfarçá-lo:

| Cenário | Resposta | O que a tela mostra |
|---|---|---|
| UserService fora | `400` `"Usuário não encontrado."` | a mensagem do servidor, como está |
| RoomService fora | `400` `"Sala não encontrada."` | idem |
| ReservationService fora | erro de rede | estado de erro com opção de repetir |

O cliente HTTP do ReservationService converte **qualquer** não-2xx em `null`, sem distinguir
`404` de indisponibilidade — por isso um serviço fora do ar aparece como "não encontrado".
Não inventar mensagem de indisponibilidade que a tela não tem como comprovar; a mesma
disciplina da spec 001 sobre o login.

**Concorrência**: a verificação de sobreposição lê e grava sem transação nem constraint
([B4](../sdd/backlog.md)), então duas reservas simultâneas podem se sobrepor. Esta spec
**não** corrige isso e não tem como mascarar — é bug de correção no coração do produto e
merece spec própria. Registrado para que a entrega desta tela não seja lida como garantia
que ela não dá.

## 8. Critérios de aceite

**Criar**

- [ ] Dado que estou autenticado, então **Reservas** aparece na barra do topo e a seção atual está indicada.
- [ ] Dado que abro o painel de nova reserva, então vejo as salas cadastradas no seletor e um intervalo pré-preenchido de 1 hora no futuro.
- [ ] Dado sala e intervalo válidos, quando confirmo, então a reserva aparece na minha lista sem eu recarregar a página.
- [ ] Dado um horário de início no passado, então vejo erro no campo e nenhuma requisição é enviada.
- [ ] Dado término anterior ao início, então vejo erro no campo e nenhuma requisição é enviada.
- [ ] Dado que a sala já está reservada no período, então vejo `"A sala já está reservada nesse período."` junto ao painel, **com o que preenchi preservado**.
- [ ] Dado que escolho 14:00–15:00 local, então o corpo enviado tem os instantes correspondentes em UTC, e a lista exibe 14:00–15:00 de volta.

**Listar**

- [ ] Dado que não tenho reserva nenhuma, então vejo estado vazio com ação de reservar — **não** mensagem de erro.
- [ ] Dado que tenho reservas, então vejo sala, número, início e fim de cada uma, ordenadas por início.
- [ ] Dado que a API demora, então vejo skeleton com a forma do conteúdo, não spinner solto.
- [ ] Dado que a API falha com `500`, então vejo estado de erro com opção de repetir.

**Cancelar**

- [ ] Dado uma reserva futura, quando cancelo e confirmo, então ela some da lista.
- [ ] Dado uma reserva já iniciada, então a ação de cancelar **não** é oferecida.
- [ ] Dado que o servidor recusa o cancelamento, então vejo a mensagem dele e a lista é recarregada.
- [ ] Dado que aciono cancelar, então há confirmação antes de a requisição sair.

**Interface e qualidade**

- [ ] Em 375px não há scroll horizontal, e a tabela vira cartões.
- [ ] Todo o fluxo é percorrível apenas com teclado, com foco visível; o painel fecha com `Esc`.
- [ ] Nenhum token aparece no console.
- [ ] `npx tsc --noEmit` limpo, sem `any`, e `npm test` passando.

## 9. Decisões em aberto

Nenhuma. As duas resolvidas em §5: origem do `userId` (claim `sub`, com a limitação de
autorização declarada) e onde a reserva nasce (`/reservas` agora, painel preparado para
receber contexto da planta depois).
