using Microsoft.EntityFrameworkCore;

namespace study_room_booking.DTOs
{
    [PrimaryKey(nameof(StudySessionId), nameof(StudentId))]
    public class SessionParticipant
    {
        public int StudySessionId { get; set; }
        public virtual StudySession? StudySession { get; set; }

        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
