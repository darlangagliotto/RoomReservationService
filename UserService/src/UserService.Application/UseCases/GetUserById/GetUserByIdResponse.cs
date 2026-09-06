namespace UserService.Application.UseCases.GetUserById;

// Deliberadamente sem PasswordHash e sem IsBlocked: este contrato e consumido
// pelo ReservationService apenas para exibir o nome de quem reservou.
public record GetUserByIdResponse(
    Guid Id,
    string Name,
    string Email
);
