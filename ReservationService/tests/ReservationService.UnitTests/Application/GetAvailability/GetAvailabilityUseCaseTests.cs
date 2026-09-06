using FluentAssertions;
using Moq;
using ReservationService.Application.Services;
using ReservationService.Application.UseCases.GetAvailability;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Repositories;
using Xunit;

namespace ReservationService.UnitTests.Application.GetAvailability
{
    public class GetAvailabilityUseCaseTests
    {
        private static readonly Guid RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        // Relativo a hoje: a entidade Reservation exige inicio no futuro, entao
        // uma data fixa faria a suite apodrecer sozinha com o tempo.
        private static readonly DateTime Day = DateTime.UtcNow.Date.AddDays(1);

        private readonly Mock<IReservationRepository> _repository = new();
        private readonly Mock<IRoomServiceClient> _roomClient = new();

        private static DateTime At(int hour, int minute = 0) => Day.AddHours(hour).AddMinutes(minute);

        private static Reservation ReservationFrom(int startHour, int endHour, int startMinute = 0, int endMinute = 0)
            => new(Guid.NewGuid(), RoomId, At(startHour, startMinute), At(endHour, endMinute));

        private GetAvailabilityUseCase CreateUseCase(params Reservation[] reservations)
        {
            _roomClient.Setup(c => c.GetAllRoomsAsync())
                .ReturnsAsync([new GetRoomResponse(RoomId, "Sala Azul", 101)]);

            _repository.Setup(r => r.GetOverlappingOrLaterSameDayAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(reservations.ToList());

            return new GetAvailabilityUseCase(_repository.Object, _roomClient.Object);
        }

        private async Task<RoomAvailabilityResponse> QueryAsync(
            GetAvailabilityUseCase useCase, int startHour, int endHour, int startMinute = 0, int endMinute = 0)
        {
            var result = await useCase.ExecuteAsync(
                new GetAvailabilityRequest(At(startHour, startMinute), At(endHour, endMinute)));

            result.IsSuccess.Should().BeTrue();
            return result.Value!.Single();
        }

        [Fact]
        public async Task Should_Report_EmUso_When_Reservation_Overlaps_The_Window()
        {
            // Reserva 14:00-15:30, consulta 14:30-15:00
            var useCase = CreateUseCase(ReservationFrom(14, 15, endMinute: 30));

            var room = await QueryAsync(useCase, 14, 15, startMinute: 30);

            room.Status.Should().Be("EmUso");
            room.BusyUntil.Should().Be(At(15, 30));
            room.NextReservationAt.Should().BeNull();
        }

        [Fact]
        public async Task Should_Report_Disponivel_When_Nothing_Is_Booked_Later_That_Day()
        {
            var useCase = CreateUseCase(ReservationFrom(14, 15, endMinute: 30));

            var room = await QueryAsync(useCase, 16, 17);

            room.Status.Should().Be("Disponivel");
            room.BusyUntil.Should().BeNull();
            room.NextReservationAt.Should().BeNull();
        }

        [Fact]
        public async Task Should_Report_Reservada_When_Free_Now_But_Booked_Later_Same_Day()
        {
            var useCase = CreateUseCase(ReservationFrom(17, 18));

            var room = await QueryAsync(useCase, 9, 10);

            room.Status.Should().Be("Reservada");
            room.NextReservationAt.Should().Be(At(17));
            room.BusyUntil.Should().BeNull();
        }

        [Fact]
        public async Task Should_Chain_Back_To_Back_Reservations_Into_One_Block()
        {
            // 14-15 seguida de 15-16: ocupada ate 16, nao ate 15.
            var useCase = CreateUseCase(ReservationFrom(14, 15), ReservationFrom(15, 16));

            var room = await QueryAsync(useCase, 14, 15, startMinute: 30);

            room.BusyUntil.Should().Be(At(16));
        }

        [Fact]
        public async Task Should_Not_Count_A_Reservation_That_Only_Touches_The_Window_Edge()
        {
            // Reserva comeca exatamente quando a consulta termina.
            var useCase = CreateUseCase(ReservationFrom(15, 16));

            var room = await QueryAsync(useCase, 14, 15);

            room.Status.Should().Be("Reservada");
            room.NextReservationAt.Should().Be(At(15));
        }

        [Fact]
        public async Task Should_Report_Disponivel_When_The_Only_Reservation_Is_Another_Day()
        {
            var nextDay = new Reservation(Guid.NewGuid(), RoomId, Day.AddDays(1).AddHours(10), Day.AddDays(1).AddHours(11));
            var useCase = CreateUseCase(nextDay);

            var room = await QueryAsync(useCase, 9, 10);

            room.Status.Should().Be("Disponivel");
        }

        [Fact]
        public async Task Should_Return_Every_Room_Even_Without_Reservations()
        {
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new GetAvailabilityRequest(At(9), At(10)));

            result.Value.Should().HaveCount(1);
            result.Value![0].Status.Should().Be("Disponivel");
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_No_Rooms_Exist()
        {
            _roomClient.Setup(c => c.GetAllRoomsAsync()).ReturnsAsync([]);
            _repository.Setup(r => r.GetOverlappingOrLaterSameDayAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync([]);
            var useCase = new GetAvailabilityUseCase(_repository.Object, _roomClient.Object);

            var result = await useCase.ExecuteAsync(new GetAvailabilityRequest(At(9), At(10)));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_Reject_End_Before_Start()
        {
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new GetAvailabilityRequest(At(10), At(9)));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("O início precisa ser anterior ao fim.");
        }

        [Fact]
        public async Task Should_Reject_Missing_Bounds()
        {
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new GetAvailabilityRequest(null, At(9)));

            result.Error.Should().Be("Informe o início e o fim do período.");
        }

        [Fact]
        public async Task Should_Fail_Whole_Request_When_RoomService_Is_Down()
        {
            // Planta com salas faltando e pior que planta que nao carrega.
            _roomClient.Setup(c => c.GetAllRoomsAsync()).ThrowsAsync(new HttpRequestException("down"));
            var useCase = new GetAvailabilityUseCase(_repository.Object, _roomClient.Object);

            var result = await useCase.ExecuteAsync(new GetAvailabilityRequest(At(9), At(10)));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Não foi possível carregar as salas.");
        }

        [Fact]
        public async Task Should_Query_Rooms_And_Database_Once_Each()
        {
            var useCase = CreateUseCase(ReservationFrom(14, 15));

            await useCase.ExecuteAsync(new GetAvailabilityRequest(At(9), At(10)));

            _roomClient.Verify(c => c.GetAllRoomsAsync(), Times.Once);
            _repository.Verify(r => r.GetOverlappingOrLaterSameDayAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }
    }
}
