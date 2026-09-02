# RoomReservationService — Guia para agentes

Sistema de reserva de salas em microsserviços .NET 10 (Clean Architecture + DDD tático),
Postgres por serviço, API Gateway YARP e autenticação JWT simétrica compartilhada.

## Onde está a documentação

Toda a documentação de arquitetura vive em [`docs/`](docs/README.md) e é a **fonte de
verdade para especificação (SDD)**. Antes de qualquer implementação, leia:

| Preciso de… | Leia |
|---|---|
| Visão do sistema, portas, fluxos | [docs/architecture/overview.md](docs/architecture/overview.md) |
| Como escrever código neste repo | [docs/architecture/conventions.md](docs/architecture/conventions.md) |
| Auth, erros, config, EF, Docker | [docs/architecture/cross-cutting.md](docs/architecture/cross-cutting.md) |
| Por que está assim | [docs/architecture/decisions.md](docs/architecture/decisions.md) |
| Telas, navegação, o que falta | [docs/product/information-architecture.md](docs/product/information-architecture.md) |
| Regras de negócio e schema | [docs/domain/model.md](docs/domain/model.md) |
| Contrato de um serviço | [docs/services/](docs/services/) |
| O que fazer a seguir | [docs/sdd/backlog.md](docs/sdd/backlog.md) |
| Como conduzir uma feature | [docs/sdd/workflow.md](docs/sdd/workflow.md) |

## Regras de trabalho

1. **Spec antes de código.** Toda mudança de comportamento começa por uma spec em
   `docs/specs/` usando [docs/sdd/spec-template.md](docs/sdd/spec-template.md).
2. **Não invente arquitetura.** As convenções em `docs/architecture/conventions.md`
   são descritivas do código existente — siga-as literalmente; se precisar divergir,
   registre um ADR em `docs/architecture/decisions.md`.
3. **Um serviço por vez.** Serviços não compartilham código nem banco. Duplicação de
   `Result<T>`, `DomainException` e `Email` entre serviços é intencional (ADR-003).
4. **Atualize a doc junto com o código.** Endpoint novo → arquivo do serviço; regra de
   negócio nova → `docs/domain/model.md`; decisão estrutural → ADR.

## Comandos

Subir tudo (Docker Desktop necessário; requer `Infra/.env` com `JWT_KEY`):

```bash
docker compose -f Infra/docker-compose.yml up --build
```

Build da solution completa:

```bash
dotnet build RoomReservationService.sln
```

Testes de um serviço:

```bash
dotnet test AuthService/tests/AuthService.UnitTests/AuthService.UnitTests.csproj
```

Nova migration (exemplo RoomService):

```bash
dotnet ef migrations add <Nome> --project RoomService/src/RoomService.Infrastructure --startup-project RoomService/src/RoomService.Api
```
