// Controller for bookings. Handles requests from the user and uses the repositories
// to access the database. Failed operations are logged instead of crashing the app.
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
    private readonly ILogger<BookingController> _logger;

    public BookingController(IBookingRepository bookingRepository, IRoomRepository roomRepository, ILogger<BookingController> logger)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    // GET: /Booking/Table
    // Shows all bookings in a table
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

    //GET: /Booking/Creation
    //Confirmation page shown after the user picks a room from the search results
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Creation (int roomId, DateTime date, TimeSpan startTime, int DurationMinutes)
    {
        var room = await _roomRepository.GetRoomById(roomId);
        if (room == null) return NotFound();

        var model = new BookingCreationViewModel
        {
            RoomId = roomId,
            RoomNumber = room.RoomNumber,
            Date = date,
            StartTime = startTime,
            DurationMinutes = DurationMinutes
        };
        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Creation (BookingCreationViewModel model)
    {
        var start = model.Date.Date + model.StartTime;

        var booking = new Booking
        {
            RoomId = model.RoomId,
            Description = model.Description,
            StartTime = start,
            EndTime = start.AddMinutes(model.DurationMinutes)
        };

        if (ModelState.IsValid && await _bookingRepository.IsOverlapping(booking))
            ModelState.AddModelError ("","The room is already booked");

        if (ModelState.IsValid)
        {
            if(await _bookingRepository.Create(booking))
            {
                _logger.LogInformation("[BookingController] Booking create for room number {roomId}", booking.RoomId);
                return RedirectToAction(nameof(Table)); //Bytte ut med egen bekreftelsesside???
            }
            ModelState.AddModelError("", "Something went wrong while saving the booking. Please try again.");
        }   
        return View(model);
    }

/*TRENGER VI DETTE???
    // GET: /Booking/Create
    // Shows the empty form for creating a new booking
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create(int? roomId, DateTime? date, TimeSpan? startTime, int? durationMinutes)
    {
        await PopulateRooms();

        var booking = new Booking();

        if(roomId.HasValue)
            booking.RoomId = roomId.Value;

        if(date.HasValue && startTime.HasValue)
        {
            var start = date.Value.Date + startTime.Value;
            booking.StartTime = start;
            booking.EndTime = start.AddMinutes(durationMinutes ?? 60);
        }
        return View(booking);
    }

    // POST: /Booking/Create
    // Validates the input, checks for overlapping bookings and saves the booking
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Booking booking)
    {
        // Business rule: a room cannot be double-booked
        if (ModelState.IsValid && await _bookingRepository.IsOverlapping(booking))
        {
            ModelState.AddModelError("", "The room is already booked in this time period");
        }

        if (ModelState.IsValid)
        {
            bool created = await _bookingRepository.Create(booking);
            if (created)
            {
                _logger.LogInformation("[BookingController] Booking created: {id} for RoomId {roomId}", booking.BookingId, booking.RoomId);
                return RedirectToAction(nameof(Table));
            }

            // The repository failed (database error) – tell the user instead of failing silently
            ModelState.AddModelError("", "Something went wrong while saving the booking. Please try again.");
        }

        _logger.LogWarning("[BookingController] Booking creation failed {@booking}", booking);
        await PopulateRooms();
        return View(booking);
    }
    */

        // GET: /Booking/Update/5
    // Shows the form filled with the existing booking
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Update(int id)
    {
        var booking = await _bookingRepository.GetBookingById(id);
        if (booking == null)
        {
            _logger.LogError("[BookingController] Booking not found when updating BookingId {id}", id);
            return BadRequest("Booking not found for the BookingId");
        }
        await PopulateRooms();
        return View(booking);
    }

    // POST: /Booking/Update
    // Validates the changes, checks for overlap and saves
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Update(Booking booking)
    {
        if (ModelState.IsValid && await _bookingRepository.IsOverlapping(booking))
        {
            ModelState.AddModelError("", "The room is already booked in this time period");
        }

        if (ModelState.IsValid)
        {
            bool updated = await _bookingRepository.Update(booking);
            if (updated)
            {
                _logger.LogInformation("[BookingController] Booking updated: {id}", booking.BookingId);
                return RedirectToAction(nameof(Table));
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
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _bookingRepository.GetBookingById(id);
        if (booking == null)
        {
            _logger.LogError("[BookingController] Booking not found for BookingId {id}", id);
            return BadRequest("Booking not found for the BookingId");
        }
        return View(booking);
    }

    // POST: /Booking/DeleteConfirmed
    // Deletes the booking after the user has confirmed
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool deleted = await _bookingRepository.Delete(id);
        if (!deleted)
        {
            _logger.LogError("[BookingController] Booking deletion failed for BookingId {id}", id);
            return BadRequest("Booking deletion failed");
        }
        _logger.LogInformation("[BookingController] Booking deleted: {id}", id);
        return RedirectToAction(nameof(Table));
    }

    // Fills the room dropdown in the Create and Update views
    private async Task PopulateRooms()
    {
        var rooms = await _roomRepository.GetAll() ?? new List<Room>();
        ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber");
    }
}