# UserService

Dono do cadastro de usuários. Porta host **5002** (container 5000). Banco `userdb`.

## Responsabilidade

Registrar usuários e verificar credenciais para o AuthService. É a fonte de verdade da
identidade — nenhum outro serviço persiste dados de usuário.

## Endpoints

Controller `api/users`, decorado com `[Authorize]` na classe. As duas rotas de escrita são
`[AllowAnonymous]`; a consulta por id, não.

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
- `400` `ProblemDetails` "Erro de negócio" — `"Este e-mail já está cadastrado."` ou mensagem
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

### `GET /api/users/{id}` — **autenticado**

Consumido pelo ReservationService para exibir o nome de quem reservou.

`200 OK`:
```json
{ "id": "9f3...", "name": "João Silva", "email": "joao@email.com" }
```

`404 Not Found` quando não existe · `401` sem token.

Nunca devolve `passwordHash` nem `isBlocked`. Ausência responde `404`, e não o `400` de
negócio — ver [ADR-013](../architecture/decisions.md).

## Casos de uso

| Caso | Fluxo |
|---|---|
| `GetUserByIdUseCase` | busca por id → `Failure` se ausente, mapeada para `404` pelo controller |
| `RegisterUserUseCase` | busca por e-mail → se existe, falha → cria `Email` VO + hash BCrypt + `User` (captura `DomainException`) → `AddSync` |
| `ValidateCredentialsUseCase` | busca por e-mail normalizado → verifica `IsBlocked` → `IPasswordHasher.Verify` → devolve `Success` em todos os caminhos, variando `IsValid` |

## Persistência

`UserDbContext` com `Users`; `Email` mapeado por `OwnsOne` na coluna `Email`.
`UserRepository` normaliza o e-mail (`Trim().ToLowerInvariant()`) antes de consultar.
`Migrate()` roda no startup. Schema em [domain/model.md](../domain/model.md#userdb).

## Seed de desenvolvimento

`Api/Seeding/DevelopmentSeeder.cs` cria um usuário fixo no startup, depois do
`Migrate()`, para permitir login logo após `docker compose up` sem cadastrar conta à mão.

| Chave | Valor no Compose |
|---|---|
| `Seed:DefaultUser:Enabled` | `true` |
| `Seed:DefaultUser:Name` | `Administrador` |
| `Seed:DefaultUser:Email` | `admin@admin.com` |
| `Seed:DefaultUser:Password` | `admin` |

Duas travas impedem que isso vire conta permanente com senha pública: o seeder **sai
imediatamente fora do ambiente `Development`**, e pode ser desligado por
`Seed:DefaultUser:Enabled=false`. É idempotente — e-mail já existente encerra a execução,
então reinício sobre volume populado não recria nem redefine a senha.

O usuário é criado pela entidade de domínio com hash BCrypt real, não por SQL: as
invariantes valem para ele e a senha funciona no `validate-credentials` normalmente.

## Configuração

`ConnectionStrings:DefaultConnection` e `Jwt:*`. O `appsettings.json` versionado
**não tem seção `Jwt`** — sem `JWT_KEY` no ambiente, o startup falha com
`"Jwt:Key configuration not found."`.

## Lacunas conhecidas

- **Unicidade de e-mail sem índice único no banco**: dois cadastros simultâneos com o
  mesmo e-mail passam pela checagem e ambos são persistidos.
- Sem operações de atualização, exclusão, bloqueio/desbloqueio ou listagem — a entidade
  suporta (`Rename`, `ChangeEmail`, `Block`), a API não expõe.
- `Program.cs` não define `FallbackPolicy` (diferente de Room/Reservation/Gateway):
  qualquer controller novo sem `[Authorize]` explícito nasce público.
- `UserService.UnitTests` contém apenas um teste vazio de template.
