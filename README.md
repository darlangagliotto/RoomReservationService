# RoomReservationService

Sistema de reserva de salas construído como **microsserviços em .NET 10**, com **Clean Architecture + DDD**. Permite cadastrar usuários, salas e equipamentos, e reservar salas com validação de conflito de horário.

## Arquitetura

```
Cliente (futuro: React + TS)
        │
        ▼
   API Gateway (YARP)  ── porta 5000 ── valida JWT, roteia por path
        │
   ┌────┼─────────┬─────────────┐
   ▼    ▼         ▼             ▼
AuthService  UserService  RoomService  ReservationService
 (5001)        (5002)       (5003)         (5004)
   │              │            │               │
   └──────────────┴── PostgreSQL (5432) ───────┘
```

Cada serviço de negócio (User, Room, Reservation) segue a mesma estrutura em camadas:

```
<Service>/
├── Dockerfile
└── src/
    ├── <Service>.Domain          → Entidades, regras de negócio (DomainException), Result<T>
    ├── <Service>.Application     → Casos de uso (UseCases), DTOs de request/response, clients HTTP entre serviços
    ├── <Service>.Infrastructure  → EF Core (DbContext, Migrations, Repositórios)
    └── <Service>.Api             → Controllers, Program.cs, autenticação JWT, Swagger
```

O **Gateway** é a única exceção: por não ter regra de negócio própria, tem apenas a camada `Api` (roteamento YARP + validação JWT).

Padrões usados em todos os serviços:
- **`Result<T>`** no lugar de exceptions para erros de negócio (fluxo previsível, sem custo de stack trace)
- **`DomainException`** para violar invariantes dentro da própria entidade (ex.: datas inválidas)
- Controllers sempre retornam `Problem(...)` com `400` para falhas de negócio, e deixam o `ExceptionMiddleware` cuidar de erros inesperados (`500`)

## Serviços

### API Gateway — porta `5000`
Ponto de entrada único. Roteia por prefixo de path e valida o JWT antes de repassar a requisição (o header `Authorization` é repassado intacto ao serviço de destino, que também valida o token — defesa em profundidade).

| Rota recebida          | Roteada para        |
|-------------------------|-----------------------|
| `/api/auth/*`             | AuthService             |
| `/api/users/*`              | UserService               |
| `/api/rooms/*`                | RoomService                |
| `/api/reservations/*`           | ReservationService           |

### AuthService — porta `5001`
Autentica usuários e emite o JWT usado por todo o sistema.

| Método | Rota               | Auth | Descrição                                  |
|--------|----------------------|------|-----------------------------------------------|
| POST   | `/api/auth/login`       | não  | Recebe e-mail/senha, valida contra o UserService e retorna `{ token, expiresAt }` |

### UserService — porta `5002`
Cadastro de usuários e validação interna de credenciais (usado pelo AuthService, não pelo cliente final).

| Método | Rota                            | Auth | Descrição                                   |
|--------|-----------------------------------|------|--------------------------------------------------|
| POST   | `/api/users`                        | não  | Cadastra um novo usuário (`name`, `email`, `password`) |
| POST   | `/api/users/validate-credentials`     | não  | Uso interno (chamado pelo AuthService) — valida e-mail/senha |

> ⚠️ **Limitação conhecida:** não existe endpoint `GET` para buscar usuário por Id. O `ReservationService` espera consumir `GET /api/users/id/{id}` para enriquecer a resposta da reserva com o nome do usuário — esse endpoint **ainda precisa ser criado** no `UserController`, senão essa busca sempre retorna vazio.

### RoomService — porta `5003`
Cadastro de salas e equipamentos. Um equipamento só pode estar alocado a uma sala por vez.

| Método | Rota                  | Auth | Descrição                                        |
|--------|-------------------------|------|------------------------------------------------------|
| POST   | `/api/rooms`               | sim  | Cadastra uma sala (`name`, `number`, `equipmentIds`)    |
| GET    | `/api/rooms`                 | sim  | Busca salas por query string (`?name=`, `?number=`); sem filtro, lista todas |
| PATCH  | `/api/rooms/{id}`               | sim  | Atualiza nome e/ou número de uma sala existente           |
| POST   | `/api/equipments`                 | sim  | Cadastra um equipamento (`type`, `brand`, `serialNumber`, `purchaseDate`) |

> ⚠️ **Limitação conhecida:** o `ReservationService` espera consumir `GET /api/rooms/id/{id}`, `GET /api/rooms/number/{n}` e `GET /api/rooms/name/{name}` para resolver a sala pelo identificador informado — essas rotas **ainda não existem** no `RoomController` (hoje só há a busca via query string em `GET /api/rooms`). Sem isso, a criação de reserva sempre falha com "Room not found".

### ReservationService — porta `5004`
Regras de negócio de reserva: existência de usuário/sala (via chamada HTTP aos outros serviços) e conflito de horário.

| Método | Rota                       | Auth | Descrição                                                          |
|--------|------------------------------|------|--------------------------------------------------------------------------|
| POST   | `/api/reservations`            | sim  | Cria reserva (`userId`, `roomId`, `startDate`, `endDate`); valida usuário, sala e overlap de horário |
| GET    | `/api/reservations`              | sim  | Busca reservas por `userId`, `userName`, `roomId`, `roomNumber`, `roomName`, `startDate`, `endDate` |
| DELETE | `/api/reservations/{id}`           | sim  | Cancela uma reserva (não permite cancelar reserva já iniciada)              |

> Depende de `UserService` e `RoomService` estarem disponíveis e saudáveis para criar/buscar reservas (ver limitações acima — hoje essa integração está incompleta).

## Autenticação

Todos os serviços (exceto rotas públicas como `login`, `register` e `health`) exigem JWT Bearer, validado com a mesma chave simétrica (`Jwt:Key`) compartilhada via variável de ambiente `JWT_KEY`. Fluxo:

1. `POST /api/auth/login` → retorna `{ token, expiresAt }`
2. Enviar `Authorization: Bearer <token>` nas chamadas seguintes (direto a um serviço ou através do Gateway)

## Como rodar

Pré-requisito: Docker Desktop.

```bash
cd Infra
docker compose up --build
```

Sobe, nesta ordem de dependência: Postgres → UserService → AuthService/RoomService/ReservationService → Gateway.

| Serviço             | Endereço               |
|------------------------|---------------------------|
| Gateway                  | http://localhost:5000       |
| AuthService                | http://localhost:5001         |
| UserService                  | http://localhost:5002           |
| RoomService                    | http://localhost:5003             |
| ReservationService                | http://localhost:5004               |
| Postgres                            | localhost:5432                         |

Swagger de cada serviço (não exposto pelo Gateway, ver decisão abaixo): `http://localhost:<porta>/swagger`.

### Variáveis de ambiente

Arquivo `Infra/.env` (não commitado) precisa conter:

```
JWT_KEY=<chave secreta com pelo menos 32 caracteres>
```

## Decisões de arquitetura

- **Swagger não passa pelo Gateway.** É ferramenta de desenvolvimento/debug por serviço — acessar direto na porta de cada um evita problemas de path-rewriting e base URL do OpenAPI. Padrão de mercado: o Gateway expõe só a API de negócio.
- **Defesa em profundidade no JWT.** O Gateway valida o token antes de rotear, mas cada serviço também valida — mesmo que alguém contorne o Gateway, nenhum serviço aceita requisição sem token válido.
- **Sem mensageria ainda.** Toda comunicação entre serviços hoje é síncrona via HTTP (ex.: ReservationService chamando UserService/RoomService). Mensageria (RabbitMQ) é o próximo passo do roadmap, para desacoplar eventos como criação/cancelamento de reserva.

## Roadmap

- [x] AuthService, UserService, RoomService, ReservationService (CRUD + regras de negócio)
- [x] API Gateway (YARP) com roteamento e autenticação centralizada
- [ ] Corrigir endpoints de busca por Id faltantes (UserService, RoomService) — necessários para o ReservationService funcionar de ponta a ponta
- [ ] Mensageria (RabbitMQ) para eventos entre serviços
- [ ] Frontend (React + TypeScript) com visualização gráfica das salas e seus status
