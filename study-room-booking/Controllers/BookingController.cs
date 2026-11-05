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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Creation (BookingCreationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var start = model.Date.Date + model.StartTime;
        var booking = new Booking
        {
            RoomId = model.RoomId,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = User.Identity?.Name ?? "",
            Description = model.Description,
            StartTime = start,
            EndTime = start.AddMinutes(model.DurationMinutes)
        };

        if (await _bookingRepository.IsOverlapping(booking))
        {
            ModelState.AddModelError("", "The room is already booked.");
            return View(model);
        }
            if(!await _bookingRepository.Create(booking))
            {
                ModelState.AddModelError("", "Something went wrong while saving the booking. Please try again.");
                return View(model);
            }
            _logger.LogInformation("[BookingController] Bokking created for Room {roomId}", booking.RoomId);
            return RedirectToAction(nameof(Table));
        }
    
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