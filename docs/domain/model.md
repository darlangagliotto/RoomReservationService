# Modelo de domínio

Fonte única das regras de negócio e do esquema físico. Os arquivos de serviço em
`docs/services/` descrevem contratos HTTP e apontam para cá.

## Linguagem ubíqua

| Termo | Definição | Onde vive |
|---|---|---|
| **User** | Pessoa que se autentica e reserva salas | UserService (`userdb.Users`) |
| **Email** | Value object: endereço normalizado e validado | UserService, AuthService |
| **Room** | Sala reservável, identificada por nome e número | RoomService (`roomdb.Rooms`) |
| **Equipment** | Item físico (projetor, TV…) com número de série | RoomService (`roomdb.Equipments`) |
| **RoomEquipment** | Alocação de um equipamento a uma sala | RoomService (`roomdb.RoomEquipment`) |
| **Reservation** | Ocupação de uma sala por um usuário num intervalo | ReservationService (`reservationdb.Reservations`) |
| **Overlap** | Interseção de intervalos na mesma sala — proibida | `CreateReservationUseCase` |
| **Blocked user** | Usuário que não pode autenticar (`IsBlocked`) | UserService |

Não existem os conceitos de: papel/permissão, andar/prédio, capacidade da sala, status
de reserva, participantes convidados. O front planejado (planta baixa com status das
salas) depende de conceitos que ainda **não existem no domínio**.

## Agregados e invariantes

### User — raiz (UserService)

| Campo | Tipo | Invariante |
|---|---|---|
| `Id` | `Guid` | gerado no construtor |
| `Name` | `string` | obrigatório; ≥ 3 caracteres após `Trim`; armazenado sem espaços nas bordas |
| `Email` | `Email` (VO) | obrigatório |
| `PasswordHash` | `string` | obrigatório (hash BCrypt, nunca a senha) |
| `IsBlocked` | `bool` | inicia `false`; alterado por `Block()` / `Unblock()` |

Mutação por `Rename`, `ChangeEmail`, `ChangePasswordHash`. Violação → `DomainException`.

**Regras de aplicação** (fora da entidade): e-mail não pode estar cadastrado
(`RegisterUserUseCase`); senha em texto claro exige ≥ 6 caracteres
(`RegisterUserRequestValidator`) — a entidade não conhece essa regra, pois só vê o hash.

> `AuthService.Domain.Entities.User` é uma cópia sem uso em runtime: nenhum caso de uso
> do AuthService a instancia. Existe apenas como alvo dos testes unitários.

### Email — value object (UserService, AuthService)

Obrigatório; normalizado com `Trim().ToLowerInvariant()`; validado por
`^[^@\s]+@[^@\s]+\.[^@\s]+$`. Comparação por valor (`record`).

### Room — raiz (RoomService)

| Campo | Tipo | Invariante |
|---|---|---|
| `Id` | `Guid` | gerado no construtor |
| `Name` | `string` | obrigatório; ≥ 3 caracteres após `Trim` |
| `Number` | `int` | > 0 |
| `Equipments` | `IReadOnlyCollection<RoomEquipment>` | lista interna; sem duplicata de `EquipmentId` |

`AddEquipment(equipmentId)`: rejeita `Guid.Empty` e equipamento já associado à sala.
`RemoveEquipment`: rejeita equipamento não associado.

**Regras de aplicação** (`RegisterRoomUseCase` / `UpdateRoomDetailsUseCase`):
nome **ou** número já existente impede o cadastro; todo `equipmentId` informado deve
existir; um equipamento não pode estar alocado a outra sala; no update, ao menos um
campo deve ser informado e nome/número não podem colidir com outra sala.

### Equipment — raiz (RoomService)

| Campo | Invariante |
|---|---|
| `Type` | obrigatório; ≥ 3 caracteres após `Trim` |
| `Brand` | obrigatório; ≥ 3 caracteres após `Trim` |
| `SerialNumber` | obrigatório; ≥ 3 caracteres (validado sobre o valor com `Trim`, mas **armazenado sem `Trim`**) |
| `PurchaseDate` | ≠ `default`; não pode ser futura (comparação por data, em UTC); ano ≥ 1990 |

**Regra de aplicação**: número de série já cadastrado impede o registro
(`RegisterEquipmentUseCase`) — sem índice único no banco.

### RoomEquipment — entidade filha de Room

Chave composta (`RoomId`, `EquipmentId`), ambos não vazios. Construtor `internal`:
só `Room.AddEquipment` cria a associação. **Índice único em `EquipmentId`** garante no
banco que um equipamento pertence a no máximo uma sala.

### Reservation — raiz (ReservationService)

| Campo | Invariante |
|---|---|
| `UserId` | ≠ `Guid.Empty` (existência verificada por HTTP, não pelo banco) |
| `RoomId` | ≠ `Guid.Empty` (idem) |
| `StartTime` | > `DateTime.UtcNow` no momento da criação |
| `EndTime` | > `StartTime` |

`SchedulePeriod` avalia nesta ordem: início no passado → início ≥ fim →
`MinValue` em início → `MinValue` em fim. Como `MinValue` já cai na primeira regra,
as duas últimas mensagens são inalcançáveis na prática.

**Regras de aplicação**:
- Sem overlap na mesma sala: proíbe quando `novoInício < fimExistente && novoFim > inícioExistente`
  (intervalos que só se tocam nas bordas são permitidos) — `CreateReservationUseCase`.
- Cancelamento só antes do início: `StartTime <= UtcNow` recusa — `CancelReservationUseCase`.

Não há: duração máxima, antecedência mínima, limite de reservas por usuário, horário
comercial, nem recorrência. A verificação de overlap **não é transacional nem protegida
por constraint** — duas requisições simultâneas podem criar reservas sobrepostas.

## Esquema físico

### `userdb`

**Users** — migration `20260321233303_InitialCreate`

| Coluna | Tipo | Restrições |
|---|---|---|
| `Id` | `uuid` | PK |
| `Name` | `text` | NOT NULL |
| `Email` | `text` | NOT NULL — coluna do VO via `OwnsOne` |
| `PasswordHash` | `text` | NOT NULL |
| `IsBlocked` | `boolean` | NOT NULL |

**Sem índice único em `Email`**: a unicidade depende só da checagem prévia no caso de uso.

### `roomdb`

**Rooms** — `Id uuid` PK, `Name text` NOT NULL, `Number integer` NOT NULL.
Sem índice único em `Name` nem em `Number` (unicidade só no caso de uso).

**Equipments** — `Id uuid` PK, `Type`/`Brand`/`SerialNumber` `text` NOT NULL,
`PurchaseDate timestamptz` NOT NULL. Sem índice único em `SerialNumber`.

**RoomEquipment** — PK composta (`RoomId`, `EquipmentId`);
FK → `Rooms` com `ON DELETE CASCADE`; FK → `Equipments` com `ON DELETE RESTRICT`;
**índice único em `EquipmentId`** (migration `20260405230418_AddUniqueIndexOnEquipmentId`).

### `reservationdb`

**Reservations** — migration `20260616181712_InitialCreate`

| Coluna | Tipo | Restrições |
|---|---|---|
| `Id` | `uuid` | PK |
| `UserId` | `uuid` | NOT NULL, sem FK |
| `RoomId` | `uuid` | NOT NULL, sem FK |
| `StartTime` | `timestamptz` | NOT NULL |
| `EndTime` | `timestamptz` | NOT NULL |

Sem índice em `RoomId` — a busca de overlap faz varredura completa filtrando por sala.
