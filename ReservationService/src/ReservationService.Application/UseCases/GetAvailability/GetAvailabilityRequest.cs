namespace ReservationService.Application.UseCases.GetAvailability;

public record GetAvailabilityRequest(
    DateTime? Start,
    DateTime? End
);
