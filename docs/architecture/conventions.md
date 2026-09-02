# Convenções de código

Descritivo do padrão realmente praticado em `UserService`, `RoomService` e
`ReservationService`. Código novo deve seguir isto sem variação.

## Estrutura de um serviço

```
<Service>/
├── Dockerfile
├── <Service>.slnx                 (AuthService usa .sln clássico)
└── src/
    ├── <Service>.Domain/
    │   ├── Common/                Result.cs, DomainException.cs
    │   ├── Entities/              agregados
    │   ├── ValueObjects/          quando houver (Email)
    │   ├── Repositories/          interfaces I<Entity>Repository
    │   ├── Security/              interfaces (IPasswordHasher, ITokenGenerator)
    │   └── Services/              serviços de domínio / portas de saída
    ├── <Service>.Application/
    │   ├── DependencyInjection/DependencyInjection.cs   → AddApplication()
    │   ├── UseCases/<NomeDoCaso>/  I<Caso>UseCase, <Caso>Request, <Caso>Response, <Caso>UseCase
    │   ├── UseCases/Common/        DTOs e mappers compartilhados entre casos de uso
    │   └── Services/               clients HTTP para outros serviços (quando houver)
    ├── <Service>.Infrastructure/
    │   ├── Data/<Service>DbContext.cs
    │   ├── Migrations/
    │   ├── Repositories/           implementações EF
    │   ├── Security/               BCryptPasswordHasher, JwtTokenGenerator, JwtOptions
    │   └── DependencyInjection/DependencyInjection.cs   → AddInfrastructure(IConfiguration)
    └── <Service>.Api/
        ├── Controllers/<Entity>Controller.cs
        ├── Middleware/ExceptionMiddleware.cs
        ├── Program.cs
        └── appsettings.json
└── tests/<Service>.UnitTests/
```

## Regra de dependência

`Api → Application → Domain` e `Api → Infrastructure → Domain`.
`Domain` não referencia nada. Duas exceções reais no código, ambas indesejadas:

- `RoomService.Infrastructure` e `ReservationService.Infrastructure` referenciam
  `Application` (desnecessário — nenhum tipo de Application é usado lá).
- `ReservationService.Application` referencia `Microsoft.EntityFrameworkCore` porque
  `GetReservationsUseCase` chama `ToListAsync()` sobre o `IQueryable` devolvido por
  `IReservationRepository.Query()` — o EF vaza para a camada de aplicação.

Não replique nenhuma das duas em código novo.

## Padrão de caso de uso

Quatro arquivos por caso de uso, na pasta `UseCases/<Caso>/`:

```csharp
public interface I<Caso>UseCase
{
    Task<Result<<Caso>Response>> ExecuteAsync(<Caso>Request request);
}

public record <Caso>Request(...);    // primitivos e Guid; nunca entidades
public record <Caso>Response(...);   // record; envolve DTO Common quando existe

public class <Caso>UseCase : I<Caso>UseCase
{
    // dependências por construtor, sempre interfaces
    public async Task<Result<<Caso>Response>> ExecuteAsync(<Caso>Request request) { ... }
}
```

Regras observadas:

- Validações de negócio ficam em métodos privados `ValidateAsync(...)` que devolvem
  `Result<bool>`; o `ExecuteAsync` retorna cedo no primeiro erro.
- Construção de entidade fica em método privado `Create<Entity>(request)` envolvido em
  `try/catch (DomainException ex) → Result.Failure(ex.Message)`.
- O caso de uso nunca lança exceção para erro de negócio.

## `Result<T>`

Idêntico nos quatro serviços (`Domain/Common/Result.cs`): `IsSuccess`, `Value?`,
`Error?`, construtor `protected`, fábricas `Success(value)` / `Failure(error)`.
Sem `Result` não-genérico — validações internas usam `Result<bool>`.

## Entidades

- Propriedades com `private set`; coleções expostas como `IReadOnlyCollection<T>`
  sobre uma lista privada.
- `Id` é `Guid` gerado no construtor público (`Guid.NewGuid()`), nunca pelo banco.
- Construtor `protected` sem parâmetros para o EF Core.
- Invariantes validadas em **métodos de mutação nomeados** (`Rename`, `ChangeNumber`,
  `AssignUser`, `SchedulePeriod`, …), chamados pelo construtor público — nunca
  atribuição direta. Violação → `throw new DomainException("mensagem em inglês.")`.
- `AuthService.Domain.Entities.User` é a exceção histórica (usa um `Validate` privado
  e atribuição direta); não sirva de modelo.

## Value Objects

`sealed record` com validação no construtor e normalização do valor
(`Email` faz `Trim().ToLowerInvariant()` e valida por regex). Mapeado no EF com
`OwnsOne` + `HasColumnName`.

## Repositórios

Interface em `Domain/Repositories`, implementação EF em `Infrastructure/Repositories`,
recebendo o `DbContext` por construtor. Cada método faz seu próprio `SaveChangesAsync()`
— **não há Unit of Work nem transação explícita** em nenhum serviço.

Nome herdado do código: o método de inserção chama-se `AddSync` em User/Room/Equipment
(`AddAsync` apenas em `IReservationRepository`). Mantenha o nome do serviço que estiver
editando em vez de criar uma terceira variante.

## Injeção de dependência

Cada camada expõe uma extensão em `DependencyInjection/DependencyInjection.cs`:

- `AddApplication()` → casos de uso (`AddScoped`), validators FluentValidation por
  assembly, e `AddHttpClient<TInterface, TImpl>` quando o serviço chama outro.
- `AddInfrastructure(IConfiguration)` → `AddDbContext` com `UseNpgsql`, repositórios,
  segurança.

`Program.cs` encadeia: `builder.Services.AddApplication().AddInfrastructure(builder.Configuration);`

## Controllers

```csharp
[ApiController]
[Authorize]
[Route("api/<recurso>")]
public class <Entity>Controller : ControllerBase
```

- Um caso de uso por action, injetado por construtor.
- Corpo padrão: executa o caso de uso; se `!IsSuccess`, devolve
  `Problem(title: "Business error", detail: response.Error, statusCode: 400)`;
  senão `Ok(...)` ou `CreatedAtAction(nameof(Action), new { id = ... }, value)`.
- Atributos `[ProducesResponseType]` para o tipo de sucesso e para `400`.
- Rota de item usa restrição de tipo: `[HttpPatch("{id:guid}")]`.
- Em PATCH, o id da rota sobrescreve o do corpo: `request with { RoomId = id }`.
- Nada de lógica de negócio, mapeamento ou acesso a repositório no controller.

## Testabilidade

Todo código nasce testável: dependência entra por interface no construtor, decisão fica
separada de I/O, e nenhum caso de uso precisa de banco ou rede para ser exercitado.
**Testável não é o mesmo que testado** — escreve-se teste onde a regra é não-óbvia, onde
errar é caro, ou onde já houve defeito; perseguir cobertura total produz teste que só
repete a implementação.

Violação concreta já no repositório: `IReservationRepository.Query()` devolve
`IQueryable` e `GetReservationsUseCase` chama `ToListAsync()` sobre ele — esse caso de
uso é impossível de testar sem um Postgres real ([B11](../sdd/backlog.md)). Repositório
recebe critérios e devolve `List<T>`; não devolve consulta em aberto.

## Testes

xUnit + FluentAssertions + Moq (`AuthService.UnitTests` é a referência).
Nomenclatura `Should_<Comportamento>_When_<Condição>`, corpo em `// Arrange / // Act /
// Assert`, helper `CreateValid<Entity>()` para o caso feliz. Projeto de teste
referencia apenas `Domain` e `Application`.

`AuthService/Directory.Build.targets` define o target `Coverage`, que roda os testes
com coverlet e gera relatório HTML via `dotnet reportgenerator`
(ferramenta declarada em `.config/dotnet-tools.json`).

## Idioma

Identificadores, rotas, mensagens de erro de domínio e logs em **inglês**
(padronizado no commit `5d835f5`). Comentários existentes estão em português.
