# AuthService

Emissor de tokens do sistema. Porta host **5001** (container 5000).

## Responsabilidade

Trocar credenciais válidas por um JWT. **Não armazena usuários** — delega a verificação
de e-mail/senha ao UserService por HTTP e é totalmente stateless.

## Estrutura

`AuthService.Api` · `.Application` · `.Infrastructure` · `.Domain` (solution clássica
`AuthService.sln`, não `.slnx`). Testes em `tests/AuthService.UnitTests`.

## Endpoints

### `POST /api/auth/login` — anônimo

Request:
```json
{ "email": "joao@email.com", "password": "123456" }
```

`200 OK`:
```json
{ "token": "<jwt>", "expiresAt": "2026-09-02T18:30:00Z" }
```

`400 Bad Request` (`ProblemDetails`, `title: "Business error"`):
`"Invalid email or password!"` — usado para credencial errada, usuário bloqueado,
usuário inexistente **e falha de comunicação com o UserService** (o erro remoto é
logado e convertido em credencial inválida).

## Caso de uso

`LoginUserUseCase` → `IUserValidationService.ValidateCredentialsAsync(email, password)`
→ se `isValid` e há `userId`, `ITokenGenerator.Generate(userId, email)` → devolve
`token` e `expiresAt`.

## Portas e adaptadores

| Porta (Domain) | Adaptador (Infrastructure) | Nota |
|---|---|---|
| `IUserValidationService` | `UserValidationService` | `HttpClient` tipado, `BaseAddress = Services:UserServiceUrl`, timeout 10 s; `POST /api/users/validate-credentials` |
| `ITokenGenerator` | `JwtTokenGenerator` (singleton) | HS256; claims em [cross-cutting.md](../architecture/cross-cutting.md#autenticação-e-autorização) |
| `IPasswordHasher` | `BCryptPasswordHasher` (scoped) | **registrado mas nunca resolvido** |

`JwtOptions` (seção `Jwt`) é validada duas vezes no startup — em
`AddInfrastructure` e de novo em `Program.cs`: exige `Issuer`, `Key` com ≥ 32 caracteres
e `AccessTokenExpirationMinutes > 0`.

## Configuração

`Services:UserServiceUrl` (Compose: `http://userservice:5000`), seção `Jwt` completa
incluindo `AccessTokenExpirationMinutes=60`. Sem `ConnectionStrings`.

## Lacunas conhecidas

- **Sem `/health`**: é o único serviço que não chama `AddHealthChecks()`/`MapHealthChecks`,
  e o Dockerfile não instala `curl`. O Compose acompanha (usa `service_started`), mas o
  serviço fica sem sonda de liveness.
- **Código morto no `Domain`**: `User`, `Email`, `IUserRepository`, `AuthenticationService`
  e `IPasswordHasher` não participam de nenhum fluxo — resquício de quando o AuthService
  tinha banco próprio (removido no commit `c2a2eaa`). Os testes unitários existentes
  cobrem justamente essa entidade morta, não o caso de uso real.
- **Dependências não usadas** em `AuthService.Api.csproj`: `Microsoft.EntityFrameworkCore`,
  `Npgsql.EntityFrameworkCore.PostgreSQL`, `EntityFrameworkCore.Design`.
- Sem refresh token, sem revogação, sem bloqueio por tentativas repetidas
  (`IsBlocked` só pode ser alterado por código, não há endpoint).
- `AuthService.Api.http` ainda aponta para `/weatherforecast` do template.
