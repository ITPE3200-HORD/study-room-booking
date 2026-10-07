// Controller for bookings. Handles requests from the user and uses the repositories
// to access the database. Failed operations are logged instead of crashing the app.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using study_room_booking.DAL;
using study_room_booking.Models;

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

    // GET: /Booking/Create
    // Shows the empty form for creating a new booking
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateRooms();
        return View();
    }

    // POST: /Booking/Create
    // Validates the input, checks for overlapping bookings and saves the booking
    [HttpPost]
    public async Task<IActionResult> Create(Booking booking)
    {
        // Business rule: a room cannot be double-booked
        if (ModelState.IsValid && await _bookingRepository.IsOverlapping(booking))
        {
            ModelState.AddModelError("", "Rommet er allerede booket i dette tidsrommet");
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
            ModelState.AddModelError("", "Noe gikk galt ved lagring av bookingen. Prøv igjen.");
        }

        _logger.LogWarning("[BookingController] Booking creation failed {@booking}", booking);
        await PopulateRooms();
        return View(booking);
    }

    // Fills the room dropdown in the Create and Update views
    private async Task PopulateRooms()
    {
        var rooms = await _roomRepository.GetAll() ?? new List<Room>();
        ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomName");
    }
}