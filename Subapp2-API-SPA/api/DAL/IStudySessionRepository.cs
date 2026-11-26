using study_room_booking.Models;

namespace study_room_booking.DAL;
public interface IStudySessionRepository
{
    Task<IEnumerable<StudySession>?> GetOpen(string? subject, DateTime? date);  // search + filter
    Task<IEnumerable<StudySession>?> GetForStudent(int studentId); // sessions the student has joined
    Task<StudySession?> GetById(int id);              // Include Booking.Room and Participants
    Task<bool> Create(StudySession session);
    Task<bool> Update(StudySession session);
    Task<bool> Delete(int id);
    Task<bool> ExistsForBooking(int bookingId);
    Task<bool> AddParticipant(SessionParticipant participant);     // join
    Task<bool> RemoveParticipant(int studySessionId, int studentId); // leave
}