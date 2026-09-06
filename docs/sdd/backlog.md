# Backlog técnico

Lacunas e defeitos identificados na engenharia reversa, com evidência no código.
Cada item é candidato a uma spec em `docs/specs/`. Ordem = ordem sugerida de execução.

## Bloqueadores — o sistema não funciona ponta a ponta sem eles

### ~~B1 — Endpoints de consulta por id ausentes~~ ✅ {#b1}

> Resolvido pela [spec 002](../specs/002-consulta-por-id-e-auth-servico.md).

O ReservationService chama quatro endpoints que **não existem**:

| Chamada | Origem | Destino esperado |
|---|---|---|
| `GET /api/users/id/{id}` | `UserServiceClient` | UserService |
| `GET /api/rooms/id/{id}` | `RoomServiceClient` | RoomService |
| `GET /api/rooms/number/{n}` | `RoomServiceClient` | RoomService |
| `GET /api/rooms/name/{n}` | `RoomServiceClient` | RoomService |

Efeito: `POST /api/reservations` sempre falha com `"Usuário não encontrado."`;
`GET /api/reservations` devolve `userName`/`roomName` vazios e `roomNumber: 0`.

Além de criar os endpoints, é preciso resolver **autenticação serviço-a-serviço**: os
clients não propagam o header `Authorization`, e os destinos exigem JWT. Decidir entre
(a) repassar o token do chamador, (b) token de serviço, ou (c) marcar as rotas internas
como anônimas e protegê-las por rede — a escolha vira ADR.

Decisão de contrato: `GET /api/rooms/{id}` (padrão REST) versus `/api/rooms/id/{id}`
(o que o client espera). Alterar o client é mais barato e mais correto.

### B2 — Login e cadastro inacessíveis pelo Gateway {#b2}

A `FallbackPolicy` do Gateway exige autenticação em todas as rotas do proxy, inclusive
`/api/auth/login` e `POST /api/users` — que são justamente as rotas de quem ainda não
tem token. Correção: declarar `"AuthorizationPolicy": "anonymous"` nas rotas
correspondentes (ou separar rotas públicas por path). Hoje o fluxo só funciona batendo
direto nas portas 5001/5002.

### ~~B3 — Validação de alocação de equipamento é ignorada~~ ✅ {#b3}

> Corrigido junto da spec 004: o método devolvia a variável errada.

`RegisterRoomUseCase.ValidateAsync` retorna `equipmentIdsValidation` (sucesso) quando
`ValidateEquipmentAllocationAsync` falha. O equipamento já alocado passa pela validação
e só é barrado pelo índice único do banco → `500` em vez de `400` com mensagem clara.
Correção de uma linha; merece teste de regressão.

## Correções de correção/robustez

### B4 — Concorrência na criação de reserva

Verificação de overlap sem transação nem constraint. Duas requisições simultâneas para a
mesma sala e horário criam reservas sobrepostas. Opções: `EXCLUDE USING gist` com
`tstzrange` no Postgres (garantia real no banco) ou transação serializável com re-check.

### B5 — Unicidade sem índice único

`Users.Email`, `Rooms.Name`, `Rooms.Number` e `Equipments.SerialNumber` têm unicidade
verificada apenas em código. Adicionar índices únicos por migration e tratar a violação
convertendo-a em erro de negócio.

### B6 — `roomdb` fora do script de init

`Infra/db/init/00-create-service-databases.sql` cria `userdb` e `reservationdb`, não
`roomdb`. Funciona porque `Migrate()` cria o banco, mas a inconsistência confunde.
Decidir: script completo ou remover o script (o EF já cobre).

### ~~B7 — Divergências de nulabilidade~~ ✅

`IRoomServiceClient` e `IUserServiceClient` declaram retornos não-anuláveis com
implementações que retornam `null`; `IReservationRepository.GetReservationById` idem.
Corrigir as assinaturas para `T?`.

### B8 — `ClockSkew` inconsistente

Só o AuthService usa `ClockSkew = TimeSpan.Zero`; os demais aceitam 5 minutos de
tolerância. Padronizar (o valor é a decisão; a inconsistência é o problema).

## Higiene

### B9 — Código morto no AuthService

`User`, `Email`, `IUserRepository`, `AuthenticationService`, `IPasswordHasher` e as
dependências EF Core/Npgsql do `AuthService.Api.csproj` não participam de nenhum fluxo.
Remover, e reescrever os testes unitários contra o caso de uso real (`LoginUserUseCase`).

### ~~B10 — URLs fixas no ReservationService~~ ✅

`http://roomservice:5000` e `http://userservice:5000` estão no
`Application/DependencyInjection`. Mover para configuração (`Services:RoomServiceUrl`,
`Services:UserServiceUrl`) e declarar no Compose, como já faz o AuthService.

### B11 — Vazamento do EF Core na camada Application

`IReservationRepository.Query()` devolve `IQueryable` e o caso de uso chama
`ToListAsync()`. Substituir por um método de repositório que receba os critérios de
busca e devolva `List<Reservation>`, removendo o `PackageReference` de EF Core do
`ReservationService.Application`. Remover também as referências `Infrastructure →
Application` de Room e Reservation.

### B12 — Arquivos, campos e configs inertes

- `GetRoomResponse.cs` e `GetUserResponse.cs` com conteúdos trocados.
- `userName` em `GetReservationsRequest` aceito e nunca usado.
- `Services__UserServiceUrl` / `Services__AuthServiceUrl` definidos no Compose para o
  RoomService, que não os lê.
- `.http` de Auth/User/Room apontando para `/weatherforecast` do template.
- `UseHttpsRedirection()` em containers só-HTTP.

### B13 — Cobertura de testes

`UserService.UnitTests`, `RoomService.UnitTests` e `ReservationService.UnitTests` estão
vazios. Prioridade: invariantes de `Reservation` e `Room`, regra de overlap, resolução
de filtros em `GetReservationsUseCase` e `GetRoomsUseCase`.

### B14 — CI ausente

`.github/workflows/` existe e está vazio. Mínimo útil: restore, build da solution e
`dotnet test` de todos os serviços a cada push.

## Evoluções planejadas

### B15 — Mensageria (RabbitMQ)

Substituir o enriquecimento síncrono por eventos (`ReservationCreated`,
`ReservationCancelled`) mais projeção local dos dados de leitura de usuário e sala,
eliminando o N+1 remoto e o acoplamento temporal (ADR-006). Requer revisar o ADR-010:
exclusão física não sobrevive a um modelo baseado em eventos.

### B16 — Frontend React + TypeScript com planta baixa

Visualização gráfica das salas e seu status. **Depende de conceitos de domínio que não
existem**: posição/coordenada da sala, andar ou setor, capacidade, e um conceito de
"status agora" (livre/ocupada) que hoje só pode ser derivado consultando reservas.
Modelar isso é pré-requisito da spec de frontend — não é trabalho de UI.

### B17 — Observabilidade

Correlação de requisição ponta a ponta, log estruturado e tracing distribuído. Sem isso,
depurar a criação de reserva (três serviços) é tentativa e erro.
