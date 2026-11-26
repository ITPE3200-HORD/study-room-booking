using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.DAL;

public class StudySessionRepository : IStudySessionRepository
{
    private readonly RoomDbContext _db;
    private readonly ILogger<StudySessionRepository> _logger;

    public StudySessionRepository(RoomDbContext db, ILogger<StudySessionRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<StudySession>?> GetOpen(string? subject, DateTime? date)
    {
        try
        {
            var query = _db.StudySessions
                .Include(session => session.Booking).ThenInclude(booking => booking.Room)
                .Include(session => session.Booking).ThenInclude(booking => booking.Student)
                .Include(session => session.Participants)
                .Where(session => session.IsOpen && session.Booking.StartTime >= DateTime.Now);

            if (!string.IsNullOrWhiteSpace(subject))
            {
                var search = subject.Trim().ToLower();
                query = query.Where(session =>
                    session.Title.ToLower().Contains(search) ||
                    (session.Subject != null && session.Subject.ToLower().Contains(search)));
            }

            if (date.HasValue)
            {
                var day = date.Value.Date;
                query = query.Where(session =>
                    session.Booking.StartTime >= day && session.Booking.StartTime < day.AddDays(1));
            }

            return await query.OrderBy(session => session.Booking.StartTime).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] GetOpen() failed, error message: {e}", e.Message);
            return null;
        }
    }

    public async Task<StudySession?> GetById(int id)
    {
        try
        {
            return await _db.StudySessions
                    .Include(session => session.Booking).ThenInclude(booking => booking.Room)
                    .Include(session => session.Booking).ThenInclude(booking => booking.Student)
                    .Include(session => session.Participants)
                    .FirstOrDefaultAsync(session => session.StudySessionId == id);
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] GetById() failed for StudySessionId {id}, error message: {e}", id, e.Message);
            return null;
        }
    }

    public async Task<bool> Create(StudySession studySession)
    {
        try
        {
            _db.StudySessions.Add(studySession);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] Create() failed for studysession {@studySession}, error message: {e}", studySession, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(StudySession studySession)
    {
        try
        {
            _db.StudySessions.Update(studySession);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] Update() failed for study session {@studySession}, error message: {e}", studySession, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var studySession = await _db.StudySessions.FindAsync(id);
            if (studySession == null)
            {
                _logger.LogError("[StudySessionRepository] Studysession not found for StudySessionId {id}", id);
                return false;
            }
            _db.StudySessions.Remove(studySession);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] Delete() failed for StudySessionId {id}, error message: {e}", id, e.Message);
            return false;
        }
    }

    // A booking can only have one study session (one-to-one)
    public async Task<bool> ExistsForBooking(int bookingId)
    {
        try
        {
            return await _db.StudySessions.AnyAsync(session => session.BookingId == bookingId);
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] ExistsForBooking() failed for BookingId {bookingId}, error message: {e}", bookingId, e.Message);
            return true; // treat errors as "exists" so we never create duplicates
        }
    }
    
    public async Task<bool> AddParticipant(SessionParticipant participant)
    {
        try
        {
            _db.SessionParticipants.Add(participant);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] AddParticipant() failed for session {SessionId}, student {StudentId}, error message: {e}",
                participant.StudySessionId, participant.StudentId, e.Message);
            return false;
        }    
    }

    public async Task<bool> RemoveParticipant(int studySessionId, int studentId)
    {
        try
        {
            // SessionParticipant has a composite key (StudySessionId + StudentId), 
            // so FindAsync needs both values, in the same order as in [PrimaryKey(...)]
            var participant = await _db.SessionParticipants.FindAsync(studySessionId, studentId);
            if (participant == null)
            {
                _logger.LogWarning("[StudySessionRepository] Participant not found: session {SessionId}, student {StudentId}",
                    studySessionId, studentId);
                return false;
            }

            _db.SessionParticipants.Remove(participant);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudySessionRepository] RemoveParticipant() failed for session {SessionId}, student {StudentId}, error message: {e}",
                studySessionId, studentId, e.Message);
            return false;
        }
    }
}