// Here we define the controller for rooms, which handles requests from the browser
// and decides which view to show. All database access goes through the Room repository.
 
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;
 
namespace study_room_booking.Controllers;
 
public class RoomController : Controller
{
    private readonly RoomDbContext _roomDbContext; // temporary, will be removed when all methods use the repository
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<RoomController> _logger;
 
    public RoomController(RoomDbContext roomDbContext, IRoomRepository roomRepository, ILogger<RoomController> logger)
    {
        _roomDbContext = roomDbContext;
        _roomRepository = roomRepository;
        _logger = logger;
    }
 
    // GET: /Room/Table
    // Shows all rooms in a table
    public async Task<IActionResult> Table()
    {
        var rooms = await _roomRepository.GetAll();
        if (rooms == null)
        {
            // Log an error if the repository failed to retrieve rooms
            _logger.LogError("[RoomController] Failed to retrieve rooms from repository");
            return StatusCode(500, "Internal server error");
        }
        var roomListViewModel = new RoomsViewModel(rooms.ToList(), "Table");
        return View(roomListViewModel);
    }
 
    // GET: /Room/Create
    // Shows the empty form for creating a new room
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
 
    // POST: /Room/Create
    // Saves the new room to the database if the form is valid
    [HttpPost]
    public async Task<IActionResult> Create(Room room)
    {
        if (ModelState.IsValid)
        {
            _roomDbContext.Rooms.Add(room);
            _roomDbContext.SaveChanges();
 
            // Log that the room was successfully created
            _logger.LogInformation("[RoomController] Room created: {RoomId} {RoomName}", room.RoomId, room.RoomName);
            return RedirectToAction(nameof(Table));
        }
 
        // Log a warning if the form input was invalid, so the error can be traced
        _logger.LogWarning("[RoomController] Room creation failed, ModelState invalid {RoomName}", room.RoomName);
        return View(room);
    }
 
    // GET: /Room/Update/{id}
    // Shows the form for updating an existing room
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var room = _roomDbContext.Rooms.Find(id);
        if (room == null)
        {
            // Log a warning if no room with the given id exists
            _logger.LogWarning("[RoomController] Room not found for update, RoomId: {RoomId}", id);
            return NotFound();
        }
        return View(room);
    }
 
    // POST: /Room/Update
    // Updates an existing room in the database if the form is valid
    [HttpPost]
    public async Task<IActionResult> Update(Room room)
    {
        if (ModelState.IsValid)
        {
            _roomDbContext.Rooms.Update(room);
            _roomDbContext.SaveChanges();
 
            // Log that the room was successfully updated
            _logger.LogInformation("[RoomController] Room updated: {RoomId} {RoomName}", room.RoomId, room.RoomName);
            return RedirectToAction(nameof(Table));
        }
 
        // Log a warning if the form input was invalid, so the error can be traced
        _logger.LogWarning("[RoomController] Room update failed, ModelState invalid {RoomName} {RoomId}", room.RoomName, room.RoomId);
        return View(room);
    }
 
    // GET: /Room/Delete/{id}
    // Shows the confirmation page for deleting a room
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var room = _roomDbContext.Rooms.Find(id);
        if (room == null)
        {
            // Log a warning if no room with the given id exists
            _logger.LogWarning("[RoomController] Room not found for deletion, RoomId: {RoomId}", id);
            return NotFound();
        }
        return View(room);
    }
 
    // POST: /Room/DeleteConfirmed/{id}
    // Deletes a room from the database
    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
        var room = _roomDbContext.Rooms.Find(id);
        if (room == null)
        {
            // Log a warning if no room with the given id exists
            _logger.LogWarning("[RoomController] Room not found when confirming deletion, RoomId: {RoomId}", id);
            return NotFound();
        }
        _roomDbContext.Rooms.Remove(room);
        _roomDbContext.SaveChanges();
 
        // Log that the room was successfully deleted
        _logger.LogInformation("[RoomController] Room deleted: {RoomId} {RoomName}", room.RoomId, room.RoomName);
        return RedirectToAction(nameof(Table));
    }
}