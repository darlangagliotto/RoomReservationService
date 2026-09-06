# Decisões de arquitetura (ADRs)

Registro do que já está decidido e materializado no código. Toda divergência futura
exige um ADR novo aqui (nunca editar um ADR aceito — superá-lo).

| # | Decisão | Estado |
|---|---|---|
| 001 | Microsserviços por subdomínio | Aceita |
| 002 | Clean Architecture em 4 projetos por serviço | Aceita |
| 003 | Zero código compartilhado entre serviços | Aceita |
| 004 | JWT simétrico compartilhado, validado em profundidade | Aceita |
| 005 | Um banco lógico por serviço, em uma instância Postgres | Aceita |
| 006 | Comunicação síncrona HTTP entre serviços | Aceita, com substituição planejada |
| 007 | API Gateway com YARP | Aceita |
| 008 | Swagger não passa pelo Gateway | Aceita |
| 009 | Migrations aplicadas no startup da API | Aceita, com ressalva |
| 010 | Cancelamento de reserva é exclusão física | Aceita, a revisar |
| 011 | `Result<T>` em vez de exceções para erro de negócio | Aceita |
| 012 | Propagação do token do chamador entre serviços | Aceita |
| 013 | `404` para consulta por identificador | Aceita |

---

## ADR-001 — Microsserviços por subdomínio

**Contexto**: quatro capacidades com ciclos de vida distintos (autenticação, cadastro
de pessoas, patrimônio/salas, agendamento).
**Decisão**: um serviço por capacidade, com processo, imagem e banco próprios.
**Consequências**: independência de deploy; em troca, consistência entre serviços passa
a ser eventual e cada consulta enriquecida exige chamada remota (ver ADR-006).

## ADR-002 — Clean Architecture em 4 projetos

**Decisão**: `Domain` (regras e portas), `Application` (casos de uso), `Infrastructure`
(EF, HTTP, segurança), `Api` (controllers e composição). Dependências apontam para dentro.
**Consequências**: domínio testável sem infraestrutura — o projeto de testes referencia
só `Domain` e `Application`. Custo: quatro projetos por serviço para funcionalidades
pequenas.

## ADR-003 — Zero código compartilhado

**Contexto**: `Result<T>`, `DomainException` e `Email` são idênticos em vários serviços.
**Decisão**: duplicar em vez de extrair pacote comum.
**Justificativa**: um pacote compartilhado acopla os ciclos de release e reintroduz o
monólito pela porta dos fundos.
**Consequências**: mudança nesses tipos precisa ser replicada manualmente; é aceitável
que evoluam de forma divergente.

## ADR-004 — JWT simétrico compartilhado com validação em profundidade

**Decisão**: HS256 com a mesma `JWT_KEY` no emissor (AuthService) e em todos os
validadores, incluindo o Gateway.
**Consequências**: simples e sem dependência de JWKS, mas **todo serviço detém a chave
de assinatura** — qualquer um deles pode forjar tokens, e a rotação é global e
simultânea. Migrar para chave assimétrica (RS256 + endpoint JWKS) é o caminho natural
quando houver mais de um emissor ou requisito de isolamento.
A validação repetida no Gateway e no serviço garante que contornar o Gateway não dá acesso.

## ADR-005 — Um banco por serviço, uma instância Postgres

**Decisão**: `userdb`, `roomdb` e `reservationdb` isolados logicamente, na mesma
instância `db` do Compose.
**Consequências**: sem joins nem FKs entre serviços — `Reservation.UserId` e
`Reservation.RoomId` são referências não validadas pelo banco, verificadas só por HTTP
no caso de uso. A instância única é conveniência de desenvolvimento e continua sendo um
ponto único de falha.

## ADR-006 — Comunicação síncrona HTTP

**Decisão**: chamadas HTTP diretas entre serviços (`HttpClient` tipado registrado via
`AddHttpClient`).
**Consequências**: acoplamento temporal — criar reserva falha se UserService ou
RoomService estiverem fora do ar; latência acumulada; risco de N+1 remoto (a listagem
de reservas faz duas chamadas HTTP **por reserva** retornada).
**Substituição planejada**: RabbitMQ para eventos de reserva (`ReservationCreated`,
`ReservationCancelled`) e cache local dos dados de leitura de usuário/sala.

## ADR-007 — API Gateway com YARP

**Decisão**: entrada única na porta 5000, roteando por prefixo de path para clusters
com destino único, configurada por `appsettings.json` (sem código).
**Consequências**: clientes conhecem um só host; os serviços permanecem acessíveis
diretamente nas portas 5001–5004 (apenas para desenvolvimento). O Gateway não faz
rewrite de path — o prefixo chega íntegro ao serviço, por isso as rotas dos controllers
são `api/users`, `api/rooms`, etc.

## ADR-008 — Swagger não passa pelo Gateway

**Decisão**: `/swagger` só é acessível na porta direta de cada serviço, e apenas em
Development.
**Justificativa**: Swagger é ferramenta de desenvolvimento; expô-lo via Gateway exigiria
rewrite de base URL do documento OpenAPI sem benefício para o cliente da API.

## ADR-009 — Migrations aplicadas no startup

**Decisão**: `db.Database.Migrate()` no `Program.cs` de User, Room e Reservation.
**Consequências**: ambiente de desenvolvimento sobe sozinho, sem passo manual.
**Ressalva**: em produção com múltiplas réplicas, instâncias concorrentes aplicando
migrations é uma condição de corrida; a migração deveria virar um passo de pipeline ou
um init container antes do rollout.

## ADR-010 — Cancelamento é exclusão física

**Decisão**: `CancelReservationUseCase` chama `DeleteAsync`; não existe campo de status
na entidade `Reservation`.
**Consequências**: sem histórico, sem auditoria, sem distinção entre "cancelada" e
"nunca existiu"; a regra "não cancelar reserva já iniciada" é a única proteção contra
apagar histórico. **A revisar** quando entrar mensageria — um evento
`ReservationCancelled` precisa de um agregado que ainda exista.

## ADR-011 — `Result<T>` em vez de exceções

**Decisão**: erro de negócio é valor de retorno; exceção (`DomainException`) só para
violação de invariante dentro da entidade, sempre capturada no caso de uso.
**Consequências**: fluxo explícito e sem custo de stack unwinding; o controller decide o
status HTTP em um único ponto. Custo: todo erro de negócio vira `400`, inclusive
"não encontrado" — a granularidade de status HTTP se perde.

## ADR-012 — Propagação do token do chamador entre serviços

**Contexto**: o ReservationService precisa consultar UserService e RoomService, que exigem
JWT. Até a spec 002 ele não enviava header algum, e as chamadas seriam rejeitadas com `401`.

**Decisão**: propagar o `Authorization` da requisição em curso, via `DelegatingHandler`
registrado nos `HttpClient` tipados. A leitura do `HttpContext` fica na camada `Api`, atrás
da porta `IAccessTokenProvider` declarada em `Application` — a camada de aplicação não
conhece ASP.NET.

**Alternativas descartadas**:

| Opção | Descartada porque |
|---|---|
| Token de serviço próprio | Introduz segredo novo e uma identidade sem dono; ganho nulo enquanto a autorização é binária |
| Rotas internas anônimas | Depende de a rede ser confiável — o oposto da defesa em profundidade do ADR-004 |

**Consequências**: o ReservationService age **como o usuário**, herdando exatamente as
permissões dele — adequado hoje porque não há papéis; **revisar quando houver**. Sem token
no contexto (chamada de fundo, futura mensageria), a chamada sai sem header e o destino
responde `401`: falha visível, nunca silenciosa.

O provider é registrado como **singleton** de propósito: os `DelegatingHandler` do
`HttpClientFactory` são reaproveitados entre requisições, e um provider `Scoped` viraria
dependência capturada de um escopo morto. `IHttpContextAccessor` já resolve a requisição
corrente via `AsyncLocal`.

## ADR-013 — `404` para consulta por identificador

**Contexto**: o ADR-011 manda todo erro de negócio virar `400`, inclusive "não encontrado".
Isso funciona para operações, mas quebra quem consulta um recurso por id: o chamador não
consegue distinguir "não existe" de "a chamada falhou".

**Decisão**: `GET /api/users/{id}` e `GET /api/rooms/{id}` respondem `404` quando o recurso
não existe. Demais operações seguem o ADR-011 sem mudança.

**Consequências**: os clients do ReservationService tratam `404` como ausência e
`EnsureSuccessStatusCode` para o resto — um `401` por token expirado deixa de ser
confundido com "usuário não encontrado", que era o comportamento anterior. A regra vale
apenas para leitura endereçada por identificador; buscas por filtro continuam devolvendo
`400` com `"No … found."`, tratado como coleção vazia pelo frontend.
