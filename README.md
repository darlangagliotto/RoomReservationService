RoomReservationSystem/
└── AuthService/
    ├── AuthService.sln
    │
    ├── AuthService.Api/
    │   ├── Controllers/
    │   │   └── AuthController.cs
    │   │
    │   ├── Contracts/
    │   │   ├── Requests/
    │   │   │   ├── RegisterRequest.cs
    │   │   │   └── LoginRequest.cs
    │   │   │
    │   │   └── Responses/
    │   │       ├── RegisterResponse.cs
    │   │       └── LoginResponse.cs
    │   │
    │   ├── Program.cs
    │   └── appsettings.json
    │
    ├── AuthService.Application/
    │   ├── UseCases/
    │   │   ├── RegisterUser/
    │   │   │   ├── RegisterUserInput.cs
    │   │   │   ├── RegisterUserOutput.cs
    │   │   │   └── RegisterUserUseCase.cs
    │   │   │
    │   │   └── LoginUser/
    │   │       ├── LoginUserInput.cs
    │   │       ├── LoginUserOutput.cs
    │   │       └── LoginUserUseCase.cs
    │   │
    │   ├── Abstractions/
    │   │   └── IJwtTokenGenerator.cs
    │   │
    │   └── DependencyInjection.cs
    │
    ├── AuthService.Domain/
    │   ├── Entities/
    │   │   └── User.cs
    │   │
    │   ├── ValueObjects/
    │   │   └── Email.cs
    │   │
    │   ├── Repositories/
    │   │   └── IUserRepository.cs
    │   │
    │   ├── Security/
    │   │   └── IPasswordHasher.cs
    │   │
    │   └── DomainExceptions/
    │       └── InvalidCredentialsException.cs
    │
    └── AuthService.Infrastructure/
        ├── Persistence/
        │   ├── AuthDbContext.cs
        │   └── Repositories/
        │       └── UserRepository.cs
        │
        ├── Security/
        │   ├── BCryptPasswordHasher.cs
        │   └── JwtTokenGenerator.cs
        │
        └── DependencyInjection.cs

API Gateway → Porta de entrada única que roteia requisições, valida JWT e aplica políticas como rate limiting e logging.

Auth Service → Responsável por autenticação, emissão de JWT e controle de permissões.

User Service → Gerencia dados e perfis dos usuários do sistema.

Room Service → Gerencia cadastro e informações das salas (capacidade, recursos, localização).

Reservation Service → Executa regras de negócio de reserva, valida conflitos de horário e determina status das salas.

Notification Service → Processa eventos (via RabbitMQ) para enviar e-mails e outras notificações assíncronas.