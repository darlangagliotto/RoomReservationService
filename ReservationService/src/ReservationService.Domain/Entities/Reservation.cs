using RoomService.Domain.Common;

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
                throw new DomainException("User is required.");
            }
            UserId = userId;
        }

        public void AssignRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
            {
                throw new DomainException("Room is required.")
            }
            RoomId = roomId;
        }

        public void SchedulePeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.UtcNow)
            {
                throw new DomainException("Start time must be in the future.");
            }

            if (startDate >= endDate)
            {
                throw new DomainException("Start time must be before end time.");
            }

            if (startDate == DateTime.MinValue)
            {
                throw new DomainException("Start time is required.");
            }
            
            if (endDate == DateTime.MinValue)
            {
                throw new DomainException("End time is required.");
            }

            StartTime = startDate;
            EndTime = endTime;
        }
    }
}