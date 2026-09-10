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
{ "name": "Sala Azul", "number": 101, "equipmentIds": ["<guid>"], "planSlot": 3 }
```

`planSlot` é opcional (1 a 10) e liga a sala ao polígono da planta fixa.

`201 Created`:
```json
{ "room": { "id": "...", "name": "Sala Azul", "number": 101, "planSlot": 3,
            "equipments": [{ "id": "...", "type": "Tv", "placement": "Parede",
                             "brand": "Samsung", "serialNumber": "SN-002",
                             "purchaseDate": "2023-05-02T00:00:00Z", "roomId": "..." }] } }
```

`400` "Erro de negócio": `"Esta sala já está cadastrada."` (nome **ou** número em uso),
`"Equipment with ID {id} not found!"`, `"Há equipamento com identificador inválido."`,
`"Equipment {id} is already allocated to another room."`,
`"Esta posição da planta já está ocupada."`, ou mensagem de `DomainException`.

### `GET /api/rooms?name=&number=`

Busca única com filtros opcionais por query string, resolvida assim:

| `name` | `number` | Comportamento |
|---|---|---|
| ✔ | ✔ | sala com nome **e** número correspondentes (0 ou 1 resultado) |
| — | ✔ | sala pelo número (0 ou 1) |
| ✔ | — | sala pelo nome (0 ou 1) |
| — | — | todas as salas |

`200 OK` com `RoomResponse[]`. **Lista vazia devolve `400`** com `"Nenhuma sala encontrada."`
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

`400`: `"Sala não encontrada."`, `"Informe ao menos um campo para atualizar."`,
`"Já existe uma sala com esse nome."`, `"Já existe uma sala com esse número."`,
ou `DomainException`. Não altera equipamentos — ver os dois endpoints seguintes,
adicionados na [spec 007](../specs/007-atribuir-equipamentos-as-salas.md).

### `POST /api/rooms/{id:guid}/equipments`

Aloca um ou mais equipamentos livres à sala existente.

```json
{ "equipmentIds": ["<guid>", "<guid>"] }
```

`200 OK` com `{ "room": { ... } }` (mesmo envelope do `PATCH`, sala inteira e atualizada).

`400`: `"Sala não encontrada."`, `"Informe ao menos um equipamento."`,
`"Há equipamento com identificador inválido."`, `"Equipamento não encontrado."`,
`"Este equipamento já está alocado a outra sala."`, `"Este equipamento já está na sala."`.
Validação é atômica: se um id da lista falhar, nenhum é alocado.

### `DELETE /api/rooms/{id:guid}/equipments/{equipmentId:guid}`

Desaloca um equipamento da sala; ele volta a aparecer em
`GET /api/equipments?unassigned=true`. Sem corpo. `200 OK` com `{ "room": { ... } }`.

`400`: `"Sala não encontrada."`, `"Este equipamento não está na sala."`.

### `POST /api/equipments`

Controller separado, rota `api/equipments`, exposta pelo Gateway desde a spec 004.

```json
{ "type": "Projetor", "brand": "Epson", "serialNumber": "SN-123", "purchaseDate": "2024-01-10" }
```

`type` aceita apenas valores do vocabulário `EquipmentType`, sem diferenciar caixa
(`"projetor"` funciona). A resposta traz `placement`, derivado do tipo.

`201 Created` com `{ "equipment": { ... } }`.
`400`: `"Tipo de equipamento desconhecido. Valores aceitos: …"`, `"Este equipamento já está cadastrado."`
(serial repetido) ou `DomainException`
(invariantes em [domain/model.md](../domain/model.md#equipment--raiz-roomservice)).

### `GET /api/equipments?type=&unassigned=`

| Parâmetro | Efeito |
|---|---|
| `type` | filtra pelo tipo do vocabulário |
| `unassigned=true` | só equipamentos não alocados a nenhuma sala |

`200 OK` com `EquipmentResponse[]`, cada item com `placement` e `roomId` (nulo quando
livre). **Vazio devolve `200` com `[]`**, não o `400` do ADR-011: ausência de equipamento é
resposta legítima. Uma consulta só, com left join — sem N+1.

## Casos de uso

| Caso | Notas |
|---|---|
| `RegisterRoomUseCase` | valida unicidade da sala → existência dos equipamentos → alocação → cria `Room` e adiciona equipamentos → persiste |
| `GetRoomsUseCase` | resolve a estratégia de busca conforme a tabela acima; falha se vazio |
| `UpdateRoomDetailsUseCase` | carrega a sala, valida colisão de nome/número, aplica `Rename`/`ChangeNumber` |
| `RegisterEquipmentUseCase` | valida serial e cria `Equipment` |
| `AssignEquipmentsToRoomUseCase` | valida sala → lista não vazia/sem `Guid.Empty` → existência de cada equipamento → não alocado em outra sala nem já nesta → `Room.AddEquipment` para cada um → persiste |
| `RemoveEquipmentFromRoomUseCase` | valida sala → `Room.RemoveEquipment` → persiste |

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
- ~~Defeito em `RegisterRoomUseCase.ValidateAsync`~~ — corrigido na spec 004.
- `ValidateEquipmentIdsAsync` checa `Guid.Empty` **depois** de buscar cada id no
  repositório; um `Guid.Empty` na lista falha antes com `"Equipment with ID ... not found!"`.
- ~~Sem endpoints para adicionar/remover equipamento de uma sala existente~~ — resolvido
  pela [spec 007](../specs/007-atribuir-equipamentos-as-salas.md).
- Sem endpoints para atualizar ou remover equipamentos (listagem entregue pela spec 004)
  nem para remover sala.
- Unicidade de nome, número e número de série sem índice único no banco.
- `RoomService.UnitTests` cobre âncoras, vocabulário de tipo, normalização de data, limites
  de `planSlot` e os dois casos de uso de alocação/desalocação (35 testes).
- Diferente dos demais, este `Program.cs` registra `AddSecurityRequirement` no Swagger —
  por isso o botão *Authorize* aplica o token automaticamente aqui e no
  ReservationService, mas não em Auth/User.
