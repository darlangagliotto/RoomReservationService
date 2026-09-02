# 002 — Consulta por id e autenticação serviço-a-serviço

| Campo | Valor |
|---|---|
| Status | Rascunho |
| Serviços afetados | UserService, RoomService, ReservationService, Gateway |
| Depende de | — |
| Autor / data | Darlan · 2026-09-02 |

## 1. Problema

O `ReservationService` chama quatro endpoints que **não existem**
([B1](../sdd/backlog.md#b1)), e além disso **não propaga o header `Authorization`** — os
destinos exigem JWT, então as chamadas seriam rejeitadas com `401` mesmo se as rotas
existissem.

Efeito hoje: `POST /api/reservations` falha sempre com `"User not found."`, e
`GET /api/reservations` devolve `userName` e `roomName` vazios e `roomNumber: 0`.
Reserva — o coração do produto — não funciona de ponta a ponta.

## 2. Resultado esperado

Criar e consultar reservas funciona: a resposta traz nome do usuário, nome e número da
sala, e a existência de ambos é de fato validada antes de gravar.

## 3. Escopo

**Incluído**

- `GET /api/users/{id}` no UserService
- `GET /api/rooms/{id}` no RoomService
- Propagação do `Authorization` nas chamadas do ReservationService
- Reescrita dos clients do ReservationService para consumir os endpoints acima e, para
  busca por nome/número, o `GET /api/rooms?name=&number=` **que já existe**
- URLs dos serviços movidas para configuração ([B10](../sdd/backlog.md))

**Fora de escopo**

- Listagem de usuários, edição, bloqueio
- Índices únicos ([B5](../sdd/backlog.md)) e concorrência de overlap ([B4](../sdd/backlog.md))
- Correção de B2 no Gateway

## 4. Contrato

### `GET /api/users/{id:guid}` — autenticado

`200 OK`:
```json
{ "id": "9f3...", "name": "João Silva", "email": "joao@email.com" }
```

`404 Not Found` quando não existe.

**Nunca** devolve `passwordHash` nem `isBlocked`. É a primeira rota do UserService que
exige autenticação — as duas existentes são `[AllowAnonymous]`.

### `GET /api/rooms/{id:guid}` — autenticado

`200 OK`: mesmo `RoomResponse` já usado pelo `GET /api/rooms`, incluindo `equipments`.

`404 Not Found` quando não existe.

### Rotas descartadas

`/api/rooms/number/{n}` e `/api/rooms/name/{n}`, que o client atual chama, **não serão
criadas**. `GET /api/rooms?number=` e `?name=` já resolvem, e duplicar a busca em três
formatos é dívida gratuita. O client é que muda.

### `404` em vez de `400`

Estas duas rotas usam `404` para ausência, e não o `400` de negócio do ADR-011. Motivo:
são consultas de recurso por identificador, onde `404` é a resposta correta e é o que o
client precisa distinguir de erro real. O `Result<T>` continua valendo para as demais
operações.

## 5. Regras de negócio

### Autenticação serviço-a-serviço

O ReservationService **propaga o token do chamador** nas chamadas ao UserService e ao
RoomService — via `IHttpContextAccessor` e um `DelegatingHandler` que copia o header
`Authorization` para o `HttpClient` tipado.

Alternativas descartadas e por quê:

| Opção | Descartada porque |
|---|---|
| Token de serviço próprio | Introduz um segredo novo e uma identidade sem dono; ganho nulo enquanto a autorização é binária |
| Rotas internas anônimas | Depende de a rede ser confiável — o oposto da defesa em profundidade do ADR-004 |

Consequência aceita: o ReservationService age **como o usuário**, herdando exatamente as
permissões dele. Isso é adequado hoje porque não há papéis; se papéis existirem, revisar.

Esta escolha vira ADR na entrega.

Sem token no contexto (chamada de fundo, futura mensageria), a chamada falha como `401` e
o caso de uso devolve o erro de negócio correspondente — não silencia.

### Configuração

`Services:UserServiceUrl` e `Services:RoomServiceUrl` passam a ser lidos da configuração e
declarados no `docker-compose`. Hoje estão fixos no código
(`Application/DependencyInjection`), o que impede rodar fora da rede do Compose.

### Nulabilidade

`IUserServiceClient` e `IRoomServiceClient` passam a declarar retorno anulável
(`Task<GetRoomResponse?>`), alinhando assinatura e implementação
([B7](../sdd/backlog.md)). `IReservationRepository.GetReservationById` idem.

## 6. Impacto em dados

Nenhuma migration. Nenhuma entidade alterada.

## 7. Impacto entre serviços

Ordem de implantação: **UserService e RoomService antes do ReservationService** — o
consumidor só funciona depois que os provedores expõem as rotas.

Indisponibilidade de qualquer provedor mantém o comportamento atual: o caso de uso devolve
`"User not found."` / `"Room not found."`. A UI não consegue distinguir ausência de
indisponibilidade, e isso permanece assim nesta spec.

`GET /api/users/{id}` e `GET /api/rooms/{id}` já são cobertos pelas rotas
`/api/users/*` e `/api/rooms/*` do Gateway. Nenhuma mudança de roteamento.

## 8. Critérios de aceite

- [ ] Dado um usuário existente, quando consulto `GET /api/users/{id}` com token válido, então recebo `200` com id, nome e e-mail — e nenhum campo de senha.
- [ ] Dado id inexistente, então recebo `404`.
- [ ] Dado nenhum token, quando consulto `GET /api/users/{id}`, então recebo `401`.
- [ ] Dado uma sala existente, quando consulto `GET /api/rooms/{id}`, então recebo `200` com a sala e seus equipamentos.
- [ ] Dado usuário e sala válidos e horário livre, quando crio uma reserva, então recebo `201` com `userName`, `roomName` e `roomNumber` preenchidos.
- [ ] Dado `userId` inexistente, quando crio uma reserva, então recebo `400` com `"User not found."`.
- [ ] Dado uma reserva existente, quando listo reservas, então cada item traz nome do usuário e dados da sala.
- [ ] Dado que o token do chamador expirou, quando o ReservationService chama outro serviço, então a falha aparece como erro, nunca como "não encontrado" silencioso.
- [ ] As URLs dos serviços vêm de configuração; nenhum host fixo permanece no código.
- [ ] `dotnet build` sem erros; testes dos casos de uso afetados passando com clients dublados.

## 9. Decisões em aberto

Nenhuma.
