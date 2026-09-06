# Aspectos transversais

## Autenticação e autorização

**Esquema**: JWT Bearer, assinatura simétrica **HS256**, mesma chave em todos os
serviços. Emitido exclusivamente pelo AuthService
(`AuthService.Infrastructure/Security/JwtTokenGenerator.cs`).

**Claims emitidas** por `Generate(Guid userId, string email)` — o caminho usado pelo
login:

| Claim | Valor |
|---|---|
| `sub` | `userId` |
| `email` | e-mail informado no login |
| `jti` | `Guid` novo |
| `iss` / `aud` | `Jwt:Issuer` / `Jwt:Audience` |
| `nbf` / `exp` | agora / agora + `AccessTokenExpirationMinutes` (60) |

Existe uma sobrecarga `Generate(User user)` que adiciona `unique_name`, mas ela **não é
chamada** por nenhum caso de uso. Não há claim de papel/permissão — a autorização é
binária (autenticado ou não).

**Validação** (`TokenValidationParameters` em cada `Program.cs`): issuer, audience,
lifetime e assinatura, todos ativos. Divergência real: apenas o AuthService define
`ClockSkew = TimeSpan.Zero`; Gateway, UserService, RoomService e ReservationService
usam o padrão de 5 minutos de tolerância.

**Política de autorização** — três estilos coexistem:

| Serviço | Mecanismo | Efeito |
|---|---|---|
| Gateway | `FallbackPolicy = RequireAuthenticatedUser` | todas as rotas proxy exigem token |
| RoomService, ReservationService | `FallbackPolicy = RequireAuthenticatedUser` | tudo exige token; `/health` marcado `AllowAnonymous` |
| UserService | `AddAuthorization()` + `[Authorize]` no controller | exige token, mas ambas as actions têm `[AllowAnonymous]` → API pública |
| AuthService | `AddAuthorization()` sem `[Authorize]` | `/api/auth/login` público |

**Defesa em profundidade**: o Gateway valida o token e repassa o header `Authorization`
intacto; o serviço de destino valida de novo (ADR-004).

**Segredo**: `Jwt:Key` vem da variável `JWT_KEY` (arquivo `Infra/.env`, não versionado
por `.gitignore`). Os `appsettings.json` versionados trazem o placeholder
`CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_WITH_AT_LEAST_32_CHARS`. `JwtOptions.Validate()`
(só no AuthService) exige Issuer, Key e mínimo de 32 caracteres.

> O arquivo `Infra/.env` presente na árvore de trabalho contém uma chave real; ela é
> ignorada pelo git, mas deve ser rotacionada se algum dia for commitada.

## Tratamento de erros

Duas faixas, sem sobreposição:

| Faixa | Origem | Resposta |
|---|---|---|
| Erro de negócio | `Result.Failure(...)` devolvido pelo caso de uso | `400` `ProblemDetails` com `title: "Erro de negócio"` e `detail` = mensagem da regra |
| Erro inesperado | exceção não tratada | `500` `ProblemDetails` via `ExceptionMiddleware` |

`ExceptionMiddleware` é idêntico nos quatro serviços (`Api/Middleware/`): loga
`"Unexpected error"`, devolve `detail` com `Tipo: mensagem` em Development e
`"Ocorreu um erro inesperado."` fora dele, e preenche `Instance` com o path.

`DomainException` nunca chega ao middleware pelo caminho feliz: os casos de uso a
capturam e convertem em `Result.Failure`. Se escapar, vira `500`.

Consequência conhecida do desenho: "não encontrado" também responde `400`, não `404`
(ex.: `"Sala não encontrada."`, `"Reserva não encontrada."`).

Validação de entrada com FluentValidation está **registrada** em todos os serviços
(`AddFluentValidationAutoValidation()` + `AddValidatorsFromAssembly`), mas só existe um
validator implementado: `RegisterUserRequestValidator`. Ele produz `400` com
`ValidationProblemDetails` (formato diferente do erro de negócio).

## Health checks

`AddHealthChecks()` + `MapHealthChecks("/health")` em UserService, RoomService,
ReservationService e Gateway (nos três últimos com `AllowAnonymous`).
**AuthService não expõe `/health`** — e o `docker-compose.yml` coerentemente não define
healthcheck para ele (os dependentes usam `condition: service_started`).

Os healthchecks do Compose usam `curl`, instalado no estágio runtime dos Dockerfiles de
User, Room, Reservation e Gateway. O Dockerfile do AuthService não instala `curl`.

Nenhum health check verifica dependências (banco ou serviços a jusante) — todos são
"liveness" puros.

## Persistência

- **EF Core 10 + Npgsql**, um `DbContext` por serviço, mapeamento por
  `OnModelCreating` (Fluent API); sem data annotations, sem classes `IEntityTypeConfiguration`.
- **Migrations aplicadas no startup**: `db.Database.Migrate()` dentro de um escopo, logo
  após `builder.Build()` (ADR-009). Cria o banco se ele não existir.
- `Infra/db/init/00-create-service-databases.sql` cria `userdb` e `reservationdb`;
  `roomdb` não está no script e é criado pelo próprio `Migrate()`.
- **Timestamps**: colunas `timestamp with time zone`. O Npgsql exige `DateTimeKind.Utc`
  ao gravar nesse tipo — datas enviadas com offset local causam exceção em runtime
  (→ `500`). Toda data que entra no domínio deve ser convertida para UTC.
- Sem transação explícita, sem Unit of Work: cada método de repositório persiste
  isoladamente.

## Configuração

Precedência ASP.NET Core padrão: `appsettings.json` → `appsettings.{Environment}.json`
→ variáveis de ambiente (`__` = separador de seção).

| Chave | Auth | User | Room | Reservation | Gateway |
|---|---|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | — | ✔ | ✔ | ✔ | — |
| `Jwt:Issuer` / `Jwt:Audience` / `Jwt:Key` | ✔ | ✔ | ✔ | ✔ | ✔ |
| `Jwt:AccessTokenExpirationMinutes` | ✔ | — | — | — | — |
| `Services:UserServiceUrl` | ✔ | — | ✔ (definido no Compose, **não lido pelo código**) | — | — |
| `ReverseProxy:*` | — | — | — | — | ✔ |

Pontos a saber:

- `UserService/appsettings.json` **não tem seção `Jwt`** — sem `JWT_KEY` no ambiente o
  serviço lança `InvalidOperationException` no startup.
- Os `appsettings.json` versionados apontam para `Host=localhost` com senhas de
  desenvolvimento (`1234` no UserService, `postgrespw` nos demais); o Compose
  sobrescreve tudo com `Host=db`.
- O ReservationService **não** lê URLs de configuração: `RoomServiceClient` e
  `UserServiceClient` recebem `http://roomservice:5000` e `http://userservice:5000`
  fixos em `Application/DependencyInjection`. Só funciona dentro da rede do Compose.
- Todos os serviços chamam `UseHttpsRedirection()` embora os containers escutem apenas
  HTTP (`ASPNETCORE_URLS=http://+:5000`); sem porta HTTPS conhecida o middleware apenas
  emite warning e não redireciona.

## Empacotamento e execução

Dockerfile em dois estágios idêntico em todos os serviços: `sdk:10.0` copia os `.csproj`,
faz `dotnet restore`, copia o resto e `dotnet publish -c Release /p:UseAppHost=false`;
runtime `aspnet:10.0` com `ASPNETCORE_URLS=http://+:5000` e `EXPOSE 5000`.

`Infra/docker-compose.yml` sobe Postgres 15 com volume `db_data`, script de init
montado em `/docker-entrypoint-initdb.d`, e os cinco serviços com `depends_on` por
healthcheck. `global.json` fixa o SDK em `10.0.301`.

## Observabilidade

Somente `ILogger` com os níveis padrão (`Default: Information`,
`Microsoft.AspNetCore: Warning`). Não há correlação de requisição, tracing distribuído,
métricas nem log estruturado — relevante porque a criação de reserva atravessa três
serviços.
