# Gateway

Ponto de entrada único do sistema. Porta host **5000**.

## Responsabilidade

Autenticar a requisição (JWT) e encaminhá-la ao serviço dono do recurso, sem
transformar path nem corpo. Não tem regra de negócio — por isso é o único componente
com um só projeto (`Gateway.Api`), sem `Domain`/`Application`/`Infrastructure`.

## Stack

`Yarp.ReverseProxy 2.3.0`, `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.5`,
.NET 10. Configuração de roteamento 100% declarativa em `appsettings.json`
(`builder.Services.AddReverseProxy().LoadFromConfig(...)`).

## Tabela de rotas

| Path de entrada | Cluster | Destino |
|---|---|---|
| `/api/auth/{**catch-all}` | `auth-cluster` | `http://authservice:5000` |
| `/api/users/{**catch-all}` | `users-cluster` | `http://userservice:5000` |
| `/api/rooms/{**catch-all}` | `rooms-cluster` | `http://roomservice:5000` |
| `/api/reservations/{**catch-all}` | `reservations-cluster` | `http://reservationservice:5000` |

Sem transformação de path: o prefixo chega íntegro ao serviço de destino. Sem
balanceamento (um destino por cluster), sem health check ativo do YARP, sem rate limit,
sem CORS.

## Endpoints próprios

| Método | Rota | Auth | Resposta |
|---|---|---|---|
| GET | `/` | anônimo | `"Gateway is running"` |
| GET | `/health` | anônimo | health check padrão |

## Autorização

`FallbackPolicy = RequireAuthenticatedUser()` — aplica-se a **todo endpoint sem
metadados de autorização próprios**, inclusive às rotas do reverse proxy. `/` e
`/health` escapam por `AllowAnonymous()` explícito.

## Configuração

`Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key` (via `JWT_KEY`) e a seção `ReverseProxy`.
Detalhes de validação do token em [cross-cutting.md](../architecture/cross-cutting.md#autenticação-e-autorização).

## Lacunas conhecidas

- **`/api/auth/login` fica inacessível pelo Gateway.** A rota `auth-route` não declara
  `AuthorizationPolicy`, então a `FallbackPolicy` exige token — e o cliente ainda não
  tem token nenhum quando vai fazer login. Correção: `"AuthorizationPolicy": "anonymous"`
  na rota (ver [backlog B2](../sdd/backlog.md#b2)). O mesmo vale para
  `POST /api/users` (cadastro).
- **`/api/equipments` não é roteado**: não há rota para esse prefixo, então o endpoint de
  cadastro de equipamento só existe na porta direta do RoomService (5003).
- `appsettings.Development.json` do Gateway está versionado (os demais serviços têm esse
  arquivo ignorado pelo `.gitignore`) e contém apenas configuração de log.
- Sem `X-Forwarded-*`, sem propagação de correlação, sem timeout configurado por cluster.
