using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using System.Security.Claims;
using BookingEntity = study_room_booking.Models.Booking;
using study_room_booking.Models;
using study_room_booking.DTOs;
using Microsoft.VisualBasic;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IStudentRepository studentRepository,
        ILogger<BookingController> logger)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    // GET: /api/Booking
    // Admin only: all bookings that have not ended yet
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetActive()
    {
        var bookings = await _bookingRepository.GetActive();

        if (bookings == null)
        {
            _logger.LogError("[BookingController] Failed to retrieve active bookings");
            return Problem("Bookings could not be retrieved.", statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok(bookings.Select(booking => booking.ToDto()));
    }

    // GET: /api/Booking/{id}
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingRepository.GetBookingById(id);

        if (booking == null)
        {
            return NotFound("Booking not found.");
        }

        var currentStudentId = await GetCurrentStudentId();
        var isOwner = currentStudentId != null && booking.StudentId == currentStudentId;
        var isAdmin = User.IsInRole("Admin");

        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }

        return Ok(booking.ToDto());
    }

    // POST: /api/Booking
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] BookingCreateDto bookingDto)
    {
        var room = await _roomRepository.GetRoomById(bookingDto.RoomId);

        if (room == null)
        {
            return NotFound("Room not found.");
        }

        var studentId = await GetCurrentStudentId();

        if (studentId == null)
        {
            return Forbid(); // Only students can create bookings, admins cannot be owners
        }
        //Rule: a study session can't have more participants than the room has seats
        if (bookingDto.CreateStudySession && bookingDto.SessionMaxParticipants > room.Capacity) 
        {
            _logger.LogWarning("[BookingController] MaxParticipants {Max} exceeds capacity {Capacity} for room {RoomId}", bookingDto.SessionMaxParticipants, room.Capacity, room.RoomId);
            return BadRequest($"Room {room.RoomNumber} only has room for {room.Capacity} people");
        }

        var booking = new BookingEntity
        {
            RoomId = bookingDto.RoomId,
            StudentId = studentId.Value,
            Description = bookingDto.Description?.Trim(),
            StartTime = bookingDto.StartTime,
            EndTime = bookingDto.EndTime
        };

        // The booking and its study session are saved together in one SaveChangesAsync()
        if (bookingDto.CreateStudySession)
        {
            booking.StudySession = new StudySession
            {
                Title = bookingDto.SessionTitle!.Trim(),
                Subject = string.IsNullOrWhiteSpace(bookingDto.SessionSubject) ? null : bookingDto.SessionSubject.Trim(),
                MaxParticipants = bookingDto.SessionMaxParticipants!.Value,
                IsOpen = true, 
                // The student who books the room joins the session automatically
                Participants = { new SessionParticipant { StudentId = studentId.Value } }
            };
        }

        if (await _bookingRepository.IsOverlapping(booking))
        {
            _logger.LogWarning("[BookingController] Booking conflict for RoomId {RoomId}", booking.RoomId);
            return Conflict("The room is already booked for that time.");
        }

        if (!await _bookingRepository.Create(booking))
        {
            _logger.LogError("[BookingController] Failed to create booking for RoomId {RoomId}", booking.RoomId);

            return Problem("Something went wrong while saving the booking.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[BookingController] Booking created: {BookingId} (study session: {HasSession})", booking.BookingId, bookingDto.CreateStudySession);

        return CreatedAtAction(nameof(GetById), new { id = booking.BookingId }, booking.ToDto());
    }

    // PUT: /api/Booking/{id}
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] BookingUpdateDto bookingDto)
    {
        var booking = await _bookingRepository.GetBookingById(id);

        if (booking == null)
        {
            return NotFound("Booking not found.");
        }

        var currentStudentId = await GetCurrentStudentId();
        var isOwner = currentStudentId != null && booking.StudentId == currentStudentId;
        var isAdmin = User.IsInRole("Admin");

        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }

        var room = await _roomRepository.GetRoomById(bookingDto.RoomId);

        if (room == null)
        {
            return NotFound("Room not found.");
        }

        // update the current entity, so bookingId and userdata is kept
        booking.RoomId = bookingDto.RoomId;
        booking.Description = bookingDto.Description?.Trim();
        booking.StartTime = bookingDto.StartTime;
        booking.EndTime = bookingDto.EndTime;

        if (await _bookingRepository.IsOverlapping(booking))
        {
            _logger.LogWarning("[BookingController] Booking conflict when updating booking {BookingId}", id);
            return Conflict("The room is already booked for that time.");
        }

        if (!await _bookingRepository.Update(booking))
        {
            _logger.LogError(
                "[BookingController] Failed to update booking {BookingId}", id);

            return Problem(
                "Something went wrong while updating the booking.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation(
           "[BookingController] Booking updated: {BookingId}", id);

        return NoContent();
    }

    // DELETE: /api/Booking/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _bookingRepository.GetBookingById(id);

        if (booking == null)
        {
            return NotFound("Booking not found.");
        }

        var currentStudentId = await GetCurrentStudentId();
        var isOwner = currentStudentId != null && booking.StudentId == currentStudentId;
        var isAdmin = User.IsInRole("Admin");

        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }

        if (!await _bookingRepository.Delete(id))
        {
            _logger.LogError(
                "[BookingController] Booking deletion failed, BookingId: {BookingId}",
                id);

            return Problem(
                 "Something went wrong while deleting the booking.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation(
            "[BookingController] Booking deleted: {BookingId}",
            id);

        return NoContent();
    }

    private async Task<int?> GetCurrentStudentId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null || User.IsInRole ("Admin"))
        {
            return null;
        }

        var fullName = 
        User.FindFirstValue(ClaimTypes.Email) ??
        User.Identity?.Name ??
        userId;

        var student = await _studentRepository.GetOrCreateByIdentityUserId(
            userId, 
            fullName);
        
        return student?.StudentId;
    }
}