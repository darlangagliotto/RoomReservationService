using ReservationService.Domain.Common;

namespace ReservationService.Domain.Entities
{
    public class Reservation
    {
        public Guid Id {get; private set;}
        public Guid UserId {get; private set;}
        public Guid RoomId {get; private set;}
        public DateTime StartTime {get; private set;}
        public DateTime EndTime {get; private set;}

        protected Reservation() { }

        public Reservation(
            Guid userId, 
            Guid roomId, 
            DateTime startTime,
            DateTime endTime)
        {
            Id = Guid.NewGuid();
            AssignUser(userId);
            AssignRoom(roomId);
            SchedulePeriod(startTime, endTime);            
        }

        public void AssignUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new DomainException("Informe o usuário.");
            }
            UserId = userId;
        }

        public void AssignRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
            {
                throw new DomainException("Informe a sala.");
            }
            RoomId = roomId;
        }

        public void SchedulePeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.UtcNow)
            {
                throw new DomainException("O horário de início precisa estar no futuro.");
            }

            if (startDate >= endDate)
            {
                throw new DomainException("O horário de início precisa ser anterior ao de término.");
            }

            if (startDate == DateTime.MinValue)
            {
                throw new DomainException("Informe o horário de início.");
            }
            
            if (endDate == DateTime.MinValue)
            {
                throw new DomainException("Informe o horário de término.");
            }

            StartTime = startDate;
            EndTime = endDate;
        }
    }
}