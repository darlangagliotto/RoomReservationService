namespace ReservationService.Application.Services
{
    public interface IUserServiceClient
    {
        Task<GetUserResponse> GetUserByIdAsync(Guid userId);
    }
}