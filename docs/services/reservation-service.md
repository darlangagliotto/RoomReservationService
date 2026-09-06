# ReservationService

Dono do agendamento. Porta host **5004** (container 5000). Banco `reservationdb`.

## Responsabilidade

Criar, consultar e cancelar reservas, garantindo que a sala não seja reservada em
horários sobrepostos. Depende de UserService e RoomService (HTTP síncrono) para validar
existência e para enriquecer respostas com nome do usuário e dados da sala.

## Endpoints

Todos exigem JWT (`[Authorize]` + `FallbackPolicy`).

### `POST /api/reservations`

```json
{ "userId": "<guid>", "roomId": "<guid>",
  "startDate": "2026-09-10T14:00:00Z", "endDate": "2026-09-10T15:00:00Z" }
```

`201 Created`:
```json
{ "reservation": { "id": "...", "userId": "...", "userName": "João Silva",
                   "roomId": "...", "roomName": "Sala Azul", "roomNumber": 101,
                   "startDate": "2026-09-10T14:00:00Z", "endDate": "2026-09-10T15:00:00Z" } }
```

`400` "Business error", nesta ordem de verificação:
`"User not found."` → `"Room not found."` →
`"Room is already reserved for the requested period."` → mensagem de `DomainException`
(`"Start time must be in the future."`, `"Start time must be before end time."`, …).

Datas devem ser UTC — ver [cross-cutting.md](../architecture/cross-cutting.md#persistência).

### `GET /api/reservations`

Query string: `userId`, `userName`, `roomId`, `roomNumber`, `roomName`, `startDate`,
`endDate` — todos opcionais.

Resolução dos filtros (`GetReservationsUseCase`):
1. `userId` → resolve o usuário via UserService; se encontrado, filtra por ele.
2. `roomId` / `roomNumber` / `roomName` → resolve a sala via RoomService; se mais de um
   identificador for informado e apontarem para salas diferentes, a resposta é vazia.
   Se algum identificador de sala foi informado e nada foi resolvido, resposta vazia.
3. `startDate` → `StartTime >= startDate`; `endDate` → `EndTime <= endDate`.
4. Para cada reserva encontrada, busca nome do usuário e dados da sala por HTTP.

`200 OK` com `ReservationResponse[]`. **Resultado vazio devolve `400`** com
`"No reservations found."`.

`userName` é aceito no request e **nunca usado** — filtrar por nome de usuário não tem efeito.

### `DELETE /api/reservations/{id:guid}`

`200 OK` com `{ "id": "<guid>" }`.
`400`: `"Reservation not found."` ou
`"Cannot cancel a reservation that has already started."`.
A linha é **removida fisicamente** (ADR-010).

## Clients HTTP

`Application/Services/`, registrados por `AddHttpClient` em `AddApplication()`:

| Client | Chamada | Destino |
|---|---|---|
| `IUserServiceClient.GetUserByIdAsync` | `GET /api/users/id/{id}` | `http://userservice:5000` |
| `IRoomServiceClient.GetRoomByIdAsync` | `GET /api/rooms/id/{id}` | `http://roomservice:5000` |
| `IRoomServiceClient.GetRoomByNumberAsync` | `GET /api/rooms/number/{n}` | idem |
| `IRoomServiceClient.GetRoomByNameAsync` | `GET /api/rooms/name/{n}` | idem |

Qualquer status não-2xx vira `null` (sem distinção entre 404, 401 e indisponibilidade;
sem timeout, retry ou circuit breaker). **O `Authorization` do chamador não é
propagado** — como os endpoints de destino exigem JWT, as chamadas seriam rejeitadas com
`401` mesmo se as rotas existissem ([backlog B1](../sdd/backlog.md#b1)).

As URLs são **fixas no código**, não vêm de configuração: fora da rede do Compose o
serviço não consegue falar com os outros.

## Persistência

`ReservationDbContext` com `Reservations`; schema em
[domain/model.md](../domain/model.md#reservationdb). `Migrate()` no startup.

`IReservationRepository` expõe `Query()` devolvendo `IQueryable<Reservation>`, usado
pelo `GetReservationsUseCase` para compor filtros — é o que faz a camada `Application`
depender de `Microsoft.EntityFrameworkCore` (ver
[conventions.md](../architecture/conventions.md#regra-de-dependência)).

## Configuração

`ConnectionStrings:DefaultConnection` e `Jwt:*`. Sem seção `Services` (ver acima).

## Lacunas conhecidas

- **Integração quebrada**: nenhum dos quatro endpoints consumidos existe
  ([backlog B1](../sdd/backlog.md#b1)). Hoje `POST` sempre falha com `"User not found."`
  e o `GET` devolve `userName`/`roomName` vazios e `roomNumber: 0`.
- **N+1 remoto na listagem**: duas chamadas HTTP por reserva retornada.
- **Overlap sem proteção de concorrência**: a verificação lê e grava sem transação nem
  constraint de exclusão — requisições simultâneas podem sobrepor reservas.
- `GetByRoomId` carrega **todas** as reservas da sala (inclusive passadas) para checar
  overlap em memória; sem índice em `RoomId`.
- `IRoomServiceClient` declara retornos não-anuláveis (`Task<GetRoomResponse>`) enquanto
  a implementação devolve `null` — divergência de nulabilidade que gera warning e
  esconde o caminho de erro. Mesma situação em
  `IReservationRepository.GetReservationById`.
- Arquivos `GetRoomResponse.cs` e `GetUserResponse.cs` estão com os conteúdos trocados
  (cada um declara o record do outro nome).
- `userName` no filtro é inerte; não há filtro por status (não existe status).
- Sem endpoint de reserva por id, sem atualização/reagendamento.
- `ReservationService.UnitTests` está vazio.

## `GET /api/reservations/availability` (spec 003)

Estado de cada sala do andar num intervalo. É a base da planta.

| Parâmetro | Obrigatório |
|---|---|
| `start` | sim — instante UTC ISO-8601 |
| `end` | sim — maior que `start` |

`200 OK` com **todas** as salas, inclusive as sem reserva:

```json
[{ "roomId": "…", "roomName": "Sala Azul", "roomNumber": 101,
   "status": "EmUso", "busyUntil": "2026-09-06T19:00:00Z", "nextReservationAt": null }]
```

Três estados, avaliados contra `[start, end)`:

| Estado | Condição |
|---|---|
| `EmUso` | há reserva **sobrepondo** o intervalo |
| `Reservada` | não sobrepõe, mas há reserva começando depois de `end`, **no mesmo dia** |
| `Disponivel` | nenhuma das anteriores |

`busyUntil` considera **reservas encadeadas** como um bloco só: 14–15 seguida de 15–16
devolve 16, não 15 — senão a UI prometeria uma sala que continua ocupada.

Bordas que apenas se tocam não sobrepõem, mesma regra da criação de reserva.

**Vazio devolve `200` com `[]`**, não o `400` do ADR-011: "não há salas cadastradas" é
resposta legítima. Com o RoomService fora do ar, a chamada falha inteira com
`"Rooms are unavailable."` — planta com salas faltando é pior que planta que não carrega.

Custo: **uma** chamada ao RoomService e **uma** consulta ao banco, independente do número
de salas.
