# Visão de arquitetura

## Propósito

Reservar salas de reunião: cadastrar usuários, salas e equipamentos, autenticar, e
criar/consultar/cancelar reservas com validação de conflito de horário.

## Estilo

Microsserviços por subdomínio, cada um com **Clean Architecture em 4 projetos**
(`Domain`, `Application`, `Infrastructure`, `Api`), banco próprio, deploy próprio
(imagem Docker própria) e nenhum código compartilhado entre serviços.

## Componentes

| Componente | Porta host | Porta container | Banco | Estado |
|---|---|---|---|---|
| Gateway (YARP) | 5000 | 5000 | — | implementado |
| AuthService | 5001 | 5000 | — (stateless) | implementado |
| UserService | 5002 | 5000 | `userdb` | implementado |
| RoomService | 5003 | 5000 | `roomdb` | implementado |
| ReservationService | 5004 | 5000 | `reservationdb` | implementado |
| PostgreSQL 15 | 5432 | 5432 | instância única | implementado |
| Frontend (React + nginx) | 5005 | 5000 | — | login, navegação e salas (specs 001, 005) |
| Mensageria | — | — | — | **não existe** (ADR-006) |

Portas de execução local fora do Docker (`launchSettings.json`): Gateway `5046/7091`,
AuthService `5223/7036`, RoomService `5274/7118`, ReservationService `5275/7119`,
UserService `5291`.

## Topologia

```
                       Cliente
                          │  Authorization: Bearer <jwt>
                          ▼
              ┌───────────────────────┐
              │  Gateway :5000 (YARP) │  valida JWT e roteia por prefixo de path
              └───────────┬───────────┘
       /api/auth  /api/users  /api/rooms  /api/reservations
            │          │          │            │
            ▼          ▼          ▼            ▼
      AuthService  UserService  RoomService  ReservationService
            │          ▲          ▲            │  │
            └──────────┘          └────────────┘  │   HTTP síncrono
              validate-           GET room by id   │   (sem mensageria)
              credentials                          │
                          ┌──────────────────────┐ │
                          │  PostgreSQL :5432    │◄┘
                          │ userdb roomdb        │
                          │ reservationdb        │
                          └──────────────────────┘
```

O Gateway roteia `/api/auth`, `/api/users`, `/api/rooms`, `/api/equipments` e
`/api/reservations`. Ver [gateway.md](../services/gateway.md).

## Matriz de dependências em runtime

| De → Para | Protocolo | Chamada | Onde |
|---|---|---|---|
| Gateway → todos | HTTP | proxy transparente, repassa `Authorization` | `Gateway.Api/appsettings.json` |
| AuthService → UserService | HTTP | `POST /api/users/validate-credentials` | `AuthService.Infrastructure/Services/UserValidationService.cs` |
| ReservationService → UserService | HTTP | `GET /api/users/{id}` | `ReservationService.Application/Services/UserServiceClient.cs` |
| ReservationService → RoomService | HTTP | `GET /api/rooms/{id}`, `GET /api/rooms?name=&number=` | `ReservationService.Application/Services/RoomServiceClient.cs` |
| User/Room/Reservation → Postgres | TCP | EF Core + Npgsql | `*.Infrastructure/DependencyInjection` |

O ReservationService **propaga o `Authorization` do chamador** nessas chamadas
([ADR-012](decisions.md)); sem isso os destinos responderiam `401`.

Ordem de subida definida em `Infra/docker-compose.yml`:
`db` → `userservice` → (`authservice`, `roomservice`, `reservationservice`) → `gateway`
→ `frontend`.

O frontend é servido por nginx, que também faz o proxy de `/api` para o Gateway — por
isso não há CORS envolvido: para o navegador, aplicação e API estão na mesma origem
(`localhost:5005`). `/api/auth` e `/api/users` são desviados direto para os serviços
enquanto [B2](../sdd/backlog.md#b2) não for corrigido.

## Fluxo 1 — Login

1. Cliente → `POST /api/auth/login` `{ email, password }`.
2. `LoginUserUseCase` chama `IUserValidationService` (HTTP) → `POST /api/users/validate-credentials`.
3. UserService busca por e-mail normalizado, verifica bloqueio e compara hash BCrypt,
   responde `{ isValid, userId }` sempre com `200`.
4. Falha → `Result.Failure("E-mail ou senha inválidos.")` → `400 ProblemDetails`.
   Sucesso → `ITokenGenerator` emite JWT HS256 → `{ token, expiresAt }`.

Nenhum dado de usuário é persistido no AuthService.

## Fluxo 2 — Criar reserva

1. Cliente → `POST /api/reservations` com Bearer token.
2. `CreateReservationUseCase`: resolve usuário (HTTP UserService) → resolve sala
   (HTTP RoomService) → verifica overlap consultando as reservas da sala no banco
   local → constrói `Reservation` (invariantes no construtor) → persiste.
3. Resposta `201` com dados enriquecidos (`userName`, `roomName`, `roomNumber`)
   vindos das chamadas HTTP.

Cada etapa que falha vira `400 ProblemDetails` com o texto da regra violada.

## Fluxo 3 — Cancelar reserva

`DELETE /api/reservations/{id}` → recusa se `StartTime <= UtcNow` → **delete físico**
da linha (não há status de cancelamento — ADR-010).

## Maturidade por eixo

| Eixo | Situação |
|---|---|
| Camadas / separação de responsabilidades | consistente nos 4 serviços |
| Autenticação | JWT em todos os serviços, defesa em profundidade |
| Persistência | EF Core + migrations aplicadas no startup |
| Contratos entre serviços | íntegros desde a spec 002 |
| Testes | 89 no total: Auth 6, Room 24, Reservation 12, frontend 53. `UserService.UnitTests` segue vazio |
| CI | `.github/workflows/` existe e está **vazio** |
| Observabilidade | apenas `ILogger` padrão; sem tracing, métricas ou correlação |
| Resiliência | sem retry/circuit breaker; timeout só no cliente do AuthService (10 s) |
