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
            _logger.LogError(e, "[StudySessionRepository] GetOpen() failed");
            return null;
        }
    }

    public async Task<StudySession?> GetById(int id)
    {
        try
        {
            return await _db.StudySessions
                    .Include(session => session.Booking).ThenInclude(booking => booking.Room)
                    .Include(session => session.Participants)
                    .FirstOrDefaultAsync(session => session.StudySessionId == id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StudySessionRepository] GetById() failed for StudySessionId {Id}", id);
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
            _logger.LogError(e, "[StudySessionRepository] Create() failed for booking {BookingId}", studySession.BookingId);
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
            _logger.LogError(e, "[StudySessionRepository] Update() failed for StudySessionId {Id}", studySession.StudySessionId);
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
                _logger.LogWarning("[StudySessionRepository] Study session not found for StudySessionId {Id}", id);
                return false;
            }
            _db.StudySessions.Remove(studySession);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StudySessionRepository] Delete() failed for StudySessionId {Id}", id);
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
            _logger.LogError(e, "[StudySessionRepository] ExistsForBooking() failed for BookingId {BookingId}", bookingId);
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
            _logger.LogError(e, "[StudySessionRepository] AddParticipant() failed for session {SessionId}, student {StudentId}",
                participant.StudySessionId, participant.StudentId);
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
            _logger.LogError(e, "[StudySessionRepository] RemoveParticipant() failed for session {SessionId}, student {StudentId}",
              studySessionId, studentId);
            return false;
        }
    }
}