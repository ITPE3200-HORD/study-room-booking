// Controller for bookings. Handles requests from the user and uses the repositories
// to access the database. Failed operations are logged instead of crashing the app.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

public class BookingController : Controller
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

    // GET: /Booking/Table
    // Shows all bookings in a table. Only accessible to admins
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Table()
    {
        var bookings = await _bookingRepository.GetAll();
        if (bookings == null)
        {
            _logger.LogError("[BookingController] Booking list not found while executing GetAll()");
            return NotFound("Booking list not found");
        }
        return View(bookings);
    }

    // GET: /Booking/Create
    // Confirmation page shown after the user picks a room from the search results
    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Create(
        int roomId,
        DateTime date,
        TimeSpan startTime,
        int DurationMinutes)
    {
        var room = await _roomRepository.GetRoomById(roomId);
        if (room == null) return NotFound();

        var model = new BookingCreateViewModel
        {
            RoomId = roomId,
            RoomNumber = room.RoomNumber,
            Date = date,
            StartTime = startTime,
            DurationMinutes = DurationMinutes
        };

        return View(model);
    }

    // POST: /Booking/Create
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Find the Student linked to the logged-in IdentityUser (created on first booking)
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null) return Challenge();

        var student = await _studentRepository.GetByIdentityUserId(identityUserId);
        if (student == null)
        {
            student = new Student
            {
                IdentityUserId = identityUserId,
                FullName = User.Identity?.Name ?? ""
            };

            if (!await _studentRepository.Create(student))
            {
                _logger.LogError(
                    "[BookingController] Could not create Student for IdentityUser {IdentityUserId}",
                    identityUserId);

                ModelState.AddModelError(
                    "",
                    "Could not find or create your student profile. Please try again.");

                return View(model);
            }
        }

        // Check that the room can hold all participants in the study session.
        if (model.CreateStudySession)
        {
            var room = await _roomRepository.GetRoomById(model.RoomId);

            if (room == null) return NotFound("Room not found.");

            if (model.SessionMaxParticipants > room.Capacity)
            {
                ModelState.AddModelError(
                    nameof(model.SessionMaxParticipants),
                    $"Room {room.RoomNumber} only has room for {room.Capacity} people.");

                return View(model);
            }
        }

        var start = model.Date.Date + model.StartTime;

        var booking = new Booking
        {
            RoomId = model.RoomId,
            StudentId = student.StudentId,
            Description = model.Description,
            StartTime = start,
            EndTime = start.AddMinutes(model.DurationMinutes)
        };

        if (model.CreateStudySession)
        {
            booking.StudySession = new StudySession
            {
                Title = model.SessionTitle!.Trim(),
                Subject = model.SessionSubject,
                MaxParticipants = model.SessionMaxParticipants,
                IsOpen = true,
                Participants = new List<SessionParticipant>
            {
                new() { StudentId = student.StudentId }
            }
            };
        }

        if (await _bookingRepository.IsOverlapping(booking))
        {
            ModelState.AddModelError("", "The room is already booked.");
            return View(model);
        }

        // Prevent the user from booking two rooms at the same time
        if (await _bookingRepository.UserHasOverlappingBooking(booking))
        {
            ModelState.AddModelError(
                "",
                "You already have another booking in this time period. " +
                "You can't be in two rooms at the same time, so please choose a different time.");

            _logger.LogWarning(
                "[BookingController] Student {StudentId} tried to double book at {StartTime}",
                booking.StudentId,
                booking.StartTime);

            return View(model);
        }

        if (!await _bookingRepository.Create(booking))
        {
            ModelState.AddModelError(
                "",
                "Something went wrong while saving the booking. Please try again.");

            return View(model);
        }

        _logger.LogInformation(
            "[BookingController] Booking created for Room {roomId}",
            booking.RoomId);

        return RedirectToAction("MyPage", "MyPage");
    }

    // GET: /Booking/Update/5
    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Update(int id)
    {
        var booking = await _bookingRepository.GetBookingById(id);

        if (booking == null)
        {
            _logger.LogError("[BookingController] Booking not found when updating BookingId {id}", id);
            return NotFound("Booking not found for the BookingId");
        }
        await PopulateRooms();
        return View(booking);
    }


    // POST: /Booking/Update
    // Validates the changes, checks for overlap and saves
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int bookingId, string? Description, string startTime, string EndTime)
    {
        var booking = await _bookingRepository.GetBookingById(bookingId);
        if (booking == null) return NotFound();
        // Only the owner or admin can update the booking
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOwner = booking.Student?.IdentityUserId == userId;

        if (!isOwner)
        {
            return Forbid();
        }

        //Parse time from drop-down 
        if (!TimeOnly.TryParse(startTime, out var start) || !TimeOnly.TryParse(EndTime, out var end))
        {
            ModelState.AddModelError("", "Invalid time selected.");
        }
        else if (end <= start)
        {
            ModelState.AddModelError("", "End time must be after start time");
        }
        else
        {
            var date = booking.StartTime.Date;
            booking.StartTime = date + start.ToTimeSpan();
            booking.EndTime = date + end.ToTimeSpan();
            booking.Description = Description;
        }

        if (ModelState.IsValid && await _bookingRepository.IsOverlapping(booking))
        {
            ModelState.AddModelError("", "The room is already booked in this time period");
        }

        // Prevent the user from having two bookings at the same time
        if (ModelState.IsValid && await _bookingRepository.UserHasOverlappingBooking(booking))
        {
            ModelState.AddModelError("",
            "You already have another booking in this time period. " +
            "You can't be in two rooms at the same time, so please choose a different time.");
            _logger.LogWarning("[BookingController] Student {StudentId} tried to double book at {StartTime}", booking.StudentId, booking.StartTime);
        }

        if (ModelState.IsValid)
        {
            bool updated = await _bookingRepository.Update(booking);
            if (updated)
            {
                _logger.LogInformation("[BookingController] Booking updated: {id}", booking.BookingId);
                return RedirectToAction("MyPage", "MyPage");
            }

            ModelState.AddModelError("", "Something went wrong while updating the booking. Please try again.");
        }

        _logger.LogWarning("[BookingController] Booking update failed {@booking}", booking);
        await PopulateRooms();
        return View(booking);
    }

    // GET: /Booking/Delete/5
    // Shows a confirmation page before deleting
    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _bookingRepository.GetBookingById(id);
        if (booking == null)
        {
            _logger.LogError("[BookingController] Booking not found for BookingId {id}", id);
            return NotFound("Booking not found for the BookingId");
        }
        return View(booking);
    }

    // POST: /Booking/DeleteConfirmed
    // Deletes the booking after the user has confirmed
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _bookingRepository.GetBookingById(id);
        if (booking == null) return NotFound();
        // Only the owner or admin can delete the booking
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOwner = booking.Student?.IdentityUserId == userId;

        if (!isOwner)
        {
            return Forbid();
        }

        bool deleted = await _bookingRepository.Delete(id);
        if (!deleted)
        {
            _logger.LogError("[BookingController] Booking deletion failed for BookingId {id}", id);
            return BadRequest("Booking deletion failed");
        }
        _logger.LogInformation("[BookingController] Booking deleted: {id}", id);
        return RedirectToAction("MyPage", "MyPage");
    }

    // Fills the room dropdown in the Create and Update views
    private async Task PopulateRooms()
    {
        var rooms = await _roomRepository.GetAll() ?? new List<Room>();
        ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber");
    }
}