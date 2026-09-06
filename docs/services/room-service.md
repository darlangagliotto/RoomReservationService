# RoomService

Dono das salas e do patrimônio alocado a elas. Porta host **5003** (container 5000).
Banco `roomdb`.

## Responsabilidade

Cadastrar salas e equipamentos, manter a alocação equipamento→sala (no máximo uma sala
por equipamento) e responder consultas de sala. É a fonte de verdade de `Room` e
`Equipment`; o ReservationService só guarda o `RoomId`.

## Endpoints

Todos exigem JWT (`[Authorize]` no controller + `FallbackPolicy` no `Program.cs`).

### `POST /api/rooms`

```json
{ "name": "Sala Azul", "number": 101, "equipmentIds": ["<guid>", "<guid>"] }
```

`201 Created`:
```json
{ "room": { "id": "...", "name": "Sala Azul", "number": 101,
            "equipments": [{ "id": "...", "type": "Projetor", "brand": "Epson",
                             "serialNumber": "SN-123", "purchaseDate": "2024-01-10T00:00:00Z" }] } }
```

`400` "Business error": `"Room is already registered."` (nome **ou** número em uso),
`"Equipment with ID {id} not found!"`, `"There is equipment with an invalid ID."`,
`"Equipment {id} is already allocated to another room."` (ver defeito abaixo), ou
mensagem de `DomainException`.

### `GET /api/rooms?name=&number=`

Busca única com filtros opcionais por query string, resolvida assim:

| `name` | `number` | Comportamento |
|---|---|---|
| ✔ | ✔ | sala com nome **e** número correspondentes (0 ou 1 resultado) |
| — | ✔ | sala pelo número (0 ou 1) |
| ✔ | — | sala pelo nome (0 ou 1) |
| — | — | todas as salas |

`200 OK` com `RoomResponse[]`. **Lista vazia devolve `400`** com `"No rooms found."`
(consequência do ADR-011).

### `GET /api/rooms/{id:guid}`

`200 OK` com o `RoomResponse` completo, incluindo `equipments`.
`404 Not Found` quando não existe — ver [ADR-013](../architecture/decisions.md).

Consumido pelo ReservationService. Busca por nome ou número continua sendo
`GET /api/rooms?name=&number=`: as rotas `/name/{n}` e `/number/{n}` que o client antigo
esperava foram **descartadas** na [spec 002](../specs/002-consulta-por-id-e-auth-servico.md).

### `PATCH /api/rooms/{id:guid}`

```json
{ "name": "Sala Verde", "number": 102 }
```
Ambos opcionais; o `id` da rota sobrescreve o do corpo. `200 OK` com
`{ "room": { ... } }`.

`400`: `"Room not found!"`, `"Provide at least one field to update!"`,
`"A room with this name already exists."`, `"A room with this number already exists."`,
ou `DomainException`. Não altera equipamentos.

### `POST /api/equipments`

Controller separado, rota `api/equipments`. **Não é exposta pelo Gateway** — só na porta
5003.

```json
{ "type": "Projetor", "brand": "Epson", "serialNumber": "SN-123", "purchaseDate": "2024-01-10" }
```

`201 Created` com `{ "equipment": { ... } }`.
`400`: `"Equipment is already registered!"` (serial repetido) ou `DomainException`
(invariantes em [domain/model.md](../domain/model.md#equipment--raiz-roomservice)).

## Casos de uso

| Caso | Notas |
|---|---|
| `RegisterRoomUseCase` | valida unicidade da sala → existência dos equipamentos → alocação → cria `Room` e adiciona equipamentos → persiste |
| `GetRoomsUseCase` | resolve a estratégia de busca conforme a tabela acima; falha se vazio |
| `UpdateRoomDetailsUseCase` | carrega a sala, valida colisão de nome/número, aplica `Rename`/`ChangeNumber` |
| `RegisterEquipmentUseCase` | valida serial e cria `Equipment` |

`IEquipmentResponseMapper` (`Common/Services`) traduz `RoomEquipment` → `EquipmentResponse`
buscando cada equipamento **um a um** no repositório; equipamento não encontrado é
silenciosamente omitido.

## Persistência

`RoomDbContext`: `Rooms`, `Equipments` e `RoomEquipment` (configurado via
`modelBuilder.Entity<RoomEquipment>` sem `DbSet` exposto; acessado por
`_context.Set<RoomEquipment>()`). Schema, FKs e o índice único em
[domain/model.md](../domain/model.md#roomdb). `Migrate()` no startup.

Carregamento de `Equipments`: todos os caminhos de leitura usam `Include`, exceto
`GetByNameOrNumberAsync`, que só serve à checagem de unicidade no cadastro e não alimenta
resposta. O `Include` em `GetByIdAsync`, `GetByNameAsync` e `GetByNameAndNumberAsync` foi
adicionado na spec 002 — sem ele a resposta saía com a lista de equipamentos vazia,
mentindo sobre o conteúdo da sala.

## Configuração

`ConnectionStrings:DefaultConnection`, `Jwt:*`. O Compose ainda define
`Services__UserServiceUrl` e `Services__AuthServiceUrl` para este serviço, mas
**nenhuma das duas é lida pelo código** — o RoomService não chama outros serviços.

## Lacunas conhecidas

- ~~Faltam rotas de consulta por identificador~~ — resolvido pela
  [spec 002](../specs/002-consulta-por-id-e-auth-servico.md).
- **Defeito em `RegisterRoomUseCase.ValidateAsync`**: quando a validação de alocação
  falha, o método retorna `equipmentIdsValidation` (que é sucesso) em vez de
  `equipmnetAllocationValidation` — o erro é engolido e a inserção prossegue, sendo
  barrada só pelo índice único do banco, o que produz `500` em vez de `400`
  ([backlog B3](../sdd/backlog.md#b3)).
- `ValidateEquipmentIdsAsync` checa `Guid.Empty` **depois** de buscar cada id no
  repositório; um `Guid.Empty` na lista falha antes com `"Equipment with ID ... not found!"`.
- Sem endpoints para listar/atualizar/remover equipamentos, remover sala, ou
  adicionar/remover equipamento de uma sala existente (o domínio suporta
  `RemoveEquipment`, a API não expõe).
- Unicidade de nome, número e número de série sem índice único no banco.
- `RoomService.UnitTests` está vazio.
- Diferente dos demais, este `Program.cs` registra `AddSecurityRequirement` no Swagger —
  por isso o botão *Authorize* aplica o token automaticamente aqui e no
  ReservationService, mas não em Auth/User.
