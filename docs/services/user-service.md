# UserService

Dono do cadastro de usuários. Porta host **5002** (container 5000). Banco `userdb`.

## Responsabilidade

Registrar usuários e verificar credenciais para o AuthService. É a fonte de verdade da
identidade — nenhum outro serviço persiste dados de usuário.

## Endpoints

Controller `api/users`, decorado com `[Authorize]` na classe, mas **ambas as actions são
`[AllowAnonymous]`** — na prática a API é pública.

### `POST /api/users` — anônimo

```json
{ "name": "João Silva", "email": "joao@email.com", "password": "123456" }
```

`201 Created` (header `Location` apontando para a própria action de registro):
```json
{ "id": "9f3...", "name": "João Silva", "email": "joao@email.com" }
```

Erros:
- `400` `ValidationProblemDetails` — FluentValidation: `Name` obrigatório e ≥ 3;
  `Email` obrigatório e válido; `Password` obrigatório e ≥ 6.
- `400` `ProblemDetails` "Business error" — `"Email is already registered!"` ou mensagem
  de `DomainException` (invariantes em [domain/model.md](../domain/model.md#user--raiz-userservice)).

### `POST /api/users/validate-credentials` — anônimo, uso interno

Oculto do Swagger (`[ApiExplorerSettings(IgnoreApi = true)]`). Consumido pelo AuthService.

```json
{ "email": "joao@email.com", "password": "123456" }
```

Responde **sempre `200 OK`**, nunca erro, para não vazar existência de conta:
```json
{ "isValid": true, "userId": "9f3..." }
```
`isValid: false` (com `userId: null`) para usuário inexistente, bloqueado ou senha
incorreta.

## Casos de uso

| Caso | Fluxo |
|---|---|
| `RegisterUserUseCase` | busca por e-mail → se existe, falha → cria `Email` VO + hash BCrypt + `User` (captura `DomainException`) → `AddSync` |
| `ValidateCredentialsUseCase` | busca por e-mail normalizado → verifica `IsBlocked` → `IPasswordHasher.Verify` → devolve `Success` em todos os caminhos, variando `IsValid` |

## Persistência

`UserDbContext` com `Users`; `Email` mapeado por `OwnsOne` na coluna `Email`.
`UserRepository` normaliza o e-mail (`Trim().ToLowerInvariant()`) antes de consultar.
`Migrate()` roda no startup. Schema em [domain/model.md](../domain/model.md#userdb).

## Configuração

`ConnectionStrings:DefaultConnection` e `Jwt:*`. O `appsettings.json` versionado
**não tem seção `Jwt`** — sem `JWT_KEY` no ambiente, o startup falha com
`"Jwt:Key configuration not found."`.

## Lacunas conhecidas

- **Não existe `GET /api/users/id/{id}`**, endpoint que o ReservationService chama para
  enriquecer a resposta com o nome do usuário → hoje a criação de reserva falha com
  `"User not found."` ([backlog B1](../sdd/backlog.md#b1)).
- **Unicidade de e-mail sem índice único no banco**: dois cadastros simultâneos com o
  mesmo e-mail passam pela checagem e ambos são persistidos.
- Sem operações de atualização, exclusão, bloqueio/desbloqueio ou listagem — a entidade
  suporta (`Rename`, `ChangeEmail`, `Block`), a API não expõe.
- `Program.cs` não define `FallbackPolicy` (diferente de Room/Reservation/Gateway):
  qualquer controller novo sem `[Authorize]` explícito nasce público.
- `UserService.UnitTests` contém apenas um teste vazio de template.
