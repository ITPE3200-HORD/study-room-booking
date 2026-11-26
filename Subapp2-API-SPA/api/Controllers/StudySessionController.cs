using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.DTOs;
using study_room_booking.Models;
using StudentEntity = study_room_booking.Models.Student;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudySessionController : ControllerBase
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudySessionController> _logger;

    public StudySessionController(
        IStudySessionRepository studySessionRepository,
        IBookingRepository bookingRepository,
        IStudentRepository studentRepository,
        ILogger<StudySessionController> logger)
    {
        _studySessionRepository = studySessionRepository;
        _bookingRepository = bookingRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    // GET: /api/StudySession?subject=math&date=2026-11-20
    // Open sessions, optionally filtered. Open to everyone.
    [HttpGet]
    public async Task<IActionResult> GetOpen([FromQuery] string? subject, [FromQuery] DateTime? date)
    {
        var sessions = await _studySessionRepository.GetOpen(subject, date);
        if (sessions == null)
        {
            _logger.LogError("[StudySessionController] Failed to retrieve study session");
            return Problem("Study session could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var currentStudentId = await GetCurrentStudentId();
        return Ok(sessions.Select(session => ToDto(session, currentStudentId)));
    }

    // GET /api/StudySession/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var session = await _studySessionRepository.GetById(id);
        if (session == null)
        {
            _logger.LogWarning("[StudySessionController] Study session not found, id: {Id}", id);
            return NotFound("Study session not found.");
        }

        return Ok(ToDto(session, await GetCurrentStudentId()));
    }

    // POST: /api/StudySession
    // Opens one of your own bookings as a study session
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] StudySessionCreateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var booking = await _bookingRepository.GetBookingById(dto.BookingId);
        if (booking == null) return NotFound("Booking not found.");

        var student = await _studentRepository.GetByIdentityUserId(userId!);
        if (student == null)
            return BadRequest("You need a student profile to create a study session.");

        // rule: you can only create a session for your own booking
        if (booking.StudentId != student.StudentId)
        {
            _logger.LogWarning("[StudySessionController] Student {StudentId} tried to create a session for booking {BookingId} they do not own", student.StudentId, dto.BookingId);
            return Forbid();
        }

        // Rule: one booking can only have one session
        if (await _studySessionRepository.ExistsForBooking(dto.BookingId))
        {
            _logger.LogWarning("[StudySessionController] Booking {BookingId} already has a study session", dto.BookingId);
            return Conflict("This booking already has a study session.");
        }

        // Rule: the booking must be in the future
        if (booking.StartTime <= DateTime.Now)
            return BadRequest("You can only create study sessions for future bookings.");

        // Rule: there can't be more participants than the room has seats
        if (booking.Room != null && dto.MaxParticipants > booking.Room.Capacity)
            return BadRequest($"Room {booking.Room.RoomNumber} only has room for {booking.Room.Capacity} people.");
        
        var session = new StudySession
        {
            BookingId = dto.BookingId,
            Title = dto.Title.Trim(),
            Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim(),
            MaxParticipants = dto.MaxParticipants,
            IsOpen = true,
            // The creator joins their own session automatically
            Participants = { new SessionParticipant { StudentId = student.StudentId } }
        };

        if (!await _studySessionRepository.Create(session))
        {
            _logger.LogError("[StudySessionController] Failed to create study session for booking {BookingId}", dto.BookingId);
            return Problem("Could not create the study session.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[StudySessionController] Study session created: {Id}", session.StudySessionId);

        var created = await _studySessionRepository.GetById(session.StudySessionId);
        return CreatedAtAction(nameof(GetById), new { id = session.StudySessionId }, ToDto(created!, student.StudentId));
    }

    // PUT: /api/StudySession/{id}
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] StudySessionUpdateDto dto)
    {
        var session = await _studySessionRepository.GetById(id);
        if (session == null) return NotFound("Study session not found.");

        var currentStudentId = await
        GetCurrentStudentId();

        // Rule: only the owner or an admin can edit
        if (!CanManage(session, currentStudentId))
        {
            _logger.LogWarning("[StudySessionController] User tried to update study session {Id} without permission", id);
            return Forbid();
        }

        // Rule: you can't lower the limit below the number of people already joined
        if (dto.MaxParticipants < session.Participants.Count)
            return BadRequest($"The session already has {session.Participants.Count} participants.");

        if (session.Booking.Room != null && dto.MaxParticipants > session.Booking.Room.Capacity)
            return BadRequest($"The room only has room for {session.Booking.Room.Capacity} people.");

        session.Title = dto.Title.Trim();
        session.Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim();
        session.MaxParticipants = dto.MaxParticipants;
        session.IsOpen = dto.IsOpen;

        if (!await _studySessionRepository.Update(session))
        {
            _logger.LogError("[StudySessionController] Failed to update study session {Id}", id);
            return Problem("Could not update the study session.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[StudySessionController] Study session updated: {Id}", id);
        return NoContent();
    }

    // DELETE: /api/StudySession/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var session = await _studySessionRepository.GetById(id);
        if (session == null) return NotFound("Study session not found.");

        var currentStudentId = await
        GetCurrentStudentId();

        if (!CanManage(session, currentStudentId))
        {
            _logger.LogWarning("[StudySessionController] User tried to delete study session {Id} without permission", id);
            return Forbid();
        }

        // Participants are deleted automatically (cascade delete in the database)
        if (!await _studySessionRepository.Delete(id))
        {
            _logger.LogError("[StudySessionController] Failed to delete study session {Id}", id);
            return Problem("Could not delete the study session.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[StudySessionController] Study session deleted: {Id}", id);
        return NoContent();
    }

    // POST: /api/StudySession/{id}/join
    // Creates a SessionParticipant: the many-to-many link between Student and StudySession
    [HttpPost("{id:int}/join")]
    [Authorize]
    public async Task<IActionResult> Join(int id)
    {
        var session = await _studySessionRepository.GetById(id);
        if (session == null) return NotFound("Study session not found.");

        var student = await GetCurrentStudent();
        if (student == null) return BadRequest("Only students can join study sessions.");

        // Domain rules for joining
        if (!session.IsOpen)
        {
            _logger.LogWarning("[StudySessionController] Student {StudentId} tried to join closed session {Id}", student.StudentId, id);
            return Conflict("This study session is closed.");
        }
        if (session.Booking.StartTime <= DateTime.Now)
        {
            _logger.LogWarning("[StudySessionController] Student {StudentId} tried to join session {Id} that has already started", student.StudentId, id);
            return Conflict("This study session has already started.");
        }
        if (session.Participants.Any(participant => participant.StudentId == student.StudentId))
        {
            _logger.LogWarning("[StudySessionController] Student {StudentId} has already joined session {Id}", student.StudentId, id);
            return Conflict("You have already joined this study session.");
        }
        if (session.Participants.Count >= session.MaxParticipants)
        {
            _logger.LogWarning("[StudySessionController] Student {StudentId} tried to join full session {Id}", student.StudentId, id);
            return Conflict("This study session is full.");
        }

        var participant = new SessionParticipant { StudySessionId = id, StudentId = student.StudentId };
        if (!await _studySessionRepository.AddParticipant(participant))
        {
            _logger.LogError("[StudySessionController] Student {StudentId} could not join session {Id}", student.StudentId, id);
            return Problem("Could not join the study session.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[StudySessionController] Student {StudentId} joined session {Id}", student.StudentId, id);
        return NoContent();
    }

    // DELETE: /api/StudySession/{id}/join
    [HttpDelete("{id:int}/join")]
    [Authorize]
    public async Task<IActionResult> Leave(int id)
    {
        var session = await _studySessionRepository.GetById(id);
        if (session == null) return NotFound("Study session not found.");

        var student = await GetCurrentStudent();
        if (student == null || !session.Participants.Any(participant => participant.StudentId == student.StudentId))
            return BadRequest("You are not a participant in this study session.");

        // Rule: the owner can't leave their own session, they must delete it instead
        if (IsOwner(session, student.StudentId))
            return BadRequest("You own this session. Delete it instead of leaving.");

        if (!await _studySessionRepository.RemoveParticipant(id, student.StudentId))
        {
            _logger.LogError("[StudySessionController] Student {StudentId} could not leave session {Id}", student.StudentId, id);
            return Problem("Could not leave the study session.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[StudySessionController] Student {StudentId} left session {Id}", student.StudentId, id);
        return NoContent();
    }

    // ---------- Helpers ----------

    private async Task<StudentEntity?> GetCurrentStudent()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId == null ? null : await _studentRepository.GetByIdentityUserId(userId);
    }

    private async Task<int?> GetCurrentStudentId() => (await GetCurrentStudent())?.StudentId;

    private static bool IsOwner(StudySession session, int? currentStudentId) =>
        currentStudentId != null && session.Booking.StudentId == currentStudentId;

    private bool CanManage(StudySession session, int? currentStudentId) => 
        IsOwner(session, currentStudentId) || User.IsInRole("Admin");

    private StudySessionDto ToDto(StudySession session, int? currentStudentId) => new()
    {
        StudySessionId = session.StudySessionId,
        Title = session.Title,
        Subject = session.Subject,
        MaxParticipants = session.MaxParticipants,
        ParticipantCount = session.Participants.Count,
        IsOpen = session.IsOpen,
        BookingId = session.BookingId,
        RoomNumber = session.Booking.Room?.RoomNumber ?? string.Empty,
        StartTime = session.Booking.StartTime,
        EndTime = session.Booking.EndTime,
        OwnerUserName = session.Booking.Student?.FullName ?? string.Empty,
        IsOwner = IsOwner(session, currentStudentId),
        HasJoined = currentStudentId != null &&
            session.Participants.Any(participant => participant.StudentId == currentStudentId)
    };
}