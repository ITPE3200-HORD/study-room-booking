using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using System.Security.Claims;
using BookingEntity = study_room_booking.Models.Booking;
using study_room_booking.Models;
using study_room_booking.DTOs;

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
    // Shows all bookings in a table
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await _bookingRepository.GetAll();

        if (bookings == null)
        {
            _logger.LogError("[BookingController] Booking list not found while executing GetAll()");
            return NotFound("Booking list not found");
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

        return Ok(booking.ToDto());
    }

    //GET: /api/Booking/creation-options?roomId=1&date=2026-11-10&startTime=09:00:00&durationMinutes=60
    //Gets information about booking and validates starttime and availability
    [HttpGet("creation-options")]
    [Authorize]
    public async Task<IActionResult> GetCreationOptions(
        [FromQuery] int roomId,
        [FromQuery] DateTime date,
        [FromQuery] TimeSpan startTime,
        [FromQuery] int durationMinutes)
    {
        if (roomId <= 0 || durationMinutes <= 0 || startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1))
        {
            return BadRequest("Room, start time or duration is invalid.");
        }

        var start = date.Date.Add(startTime);

        if (start <= DateTime.Now)
        {
            return BadRequest("The booking must start in the future.");
        }

        var room = await _roomRepository.GetRoomById(roomId);

        if (room == null)
        {
            return NotFound("Room not found.");
        }

        var end = start.AddMinutes(durationMinutes);

        var proposedBooking = new BookingEntity
        {
            RoomId = roomId,
            StartTime = start,
            EndTime = end
        };

        var isAvailable = !await _bookingRepository.IsOverlapping(proposedBooking);

        return Ok(new BookingOptionsDto
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            StartTime = start,
            EndTime = end,
            DurationMinutes = durationMinutes,
            IsAvailable = isAvailable
        });
    }

    // POST: /api/Booking/Create
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
            return Forbid(); //Only students can book
        }

        var booking = new BookingEntity
        {
            RoomId = bookingDto.RoomId,
            StudentId = studentId.Value,
            Description = bookingDto.Description?.Trim(),
            StartTime = bookingDto.StartTime,
            EndTime = bookingDto.EndTime
        };

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

        _logger.LogInformation("[BookingController] Booking created: {BookingId}", booking.BookingId);

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

        var currentStudentId = await
            GetCurrentStudentId();
        var isOwner = currentStudentId != null && booking.StudentId == currentStudentId;

        if (!isOwner)
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

        var currentStudentId = await
            GetCurrentStudentId();
        var isOwner = currentStudentId != null && booking.StudentId == currentStudentId;

        if (!isOwner)
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
        if (userId == null) return null;

        var student = await
        _studentRepository.GetByIdentityUserId(userId);
        return student?.StudentId;
    }
}