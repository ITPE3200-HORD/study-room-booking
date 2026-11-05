// Here we define the controller for rooms, which handles requests from the browser
// and decides which view to show. All database access goes through the Room repository.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;
using study_room_booking.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomAPIController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<RoomAPIController> _logger;

    public RoomAPIController(IRoomRepository roomRepository, ILogger<RoomAPIController> logger)
    {
        _roomRepository = roomRepository;
        _logger = logger;
    }

    // GET: /Room/Table
    // Shows all rooms in a table
    [HttpGet("Table")]
    public async Task<IActionResult> Table()
    {
        var rooms = await _roomRepository.GetAll();
        if (rooms == null)
        {
            // Log an error if the repository failed to retrieve rooms
            _logger.LogError("[RoomAPIController] Failed to retrieve rooms from repository");
            return StatusCode(500, "Internal server error");
        }
        var roomDtos = rooms.Select(room => new RoomDto
        {
            RoomId = room.RoomId,
            Location = room.Location,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            Capacity = room.Capacity,
            Status = (RoomDto.Availability)room.Status
        });
        return Ok(roomDtos);
    }

    // GET: /Room/Create
    // Shows the empty form for creating a new room
    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Room/Create
    // Saves the new room to the database if the form is valid
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Room room)
    {
        if (ModelState.IsValid)
        {
            bool returnOk = await _roomRepository.Create(room);
            if (!returnOk)
            {
                // Log an error and show a message to the user if the repository failed to save the room
                _logger.LogError("[RoomController] Failed to create room in repository: {RoomNumber}", room.RoomNumber); // Terminal log for debugging
                ModelState.AddModelError(string.Empty, "Could not save the room. Please try again."); // Message to user
                return View(room);
            }

            // Log that the room was successfully created
            _logger.LogInformation("[RoomController] Room created: {RoomId} {RoomNumber}", room.RoomId, room.RoomNumber);
            return RedirectToAction(nameof(Table));
        }

        // Log a warning if the form input was invalid, so the error can be traced
        _logger.LogWarning("[RoomController] Room creation failed, ModelState invalid {RoomNumber}", room.RoomNumber);
        return View(room);
    }

    // GET: /Room/Update/{id}
    // Shows the form for updating an existing room
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Update(int id)
    {
        var room = await _roomRepository.GetRoomById(id);
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
    [Authorize]
    public async Task<IActionResult> Update(Room room)
    {
        if (ModelState.IsValid)
        {
            bool returnOk = await _roomRepository.Update(room);
            if (!returnOk)
            {
                // Log an error and show a message to the user if the repository failed to update the room
                _logger.LogError("[RoomController] Failed to update room in repository: {RoomId} {RoomNumber}", room.RoomId, room.RoomNumber); // Terminal log for debugging
                ModelState.AddModelError(string.Empty, "Could not update the room. Please try again."); // Message to user
                return View(room);
            }
            // Log that the room was successfully updated
            _logger.LogInformation("[RoomController] Room updated: {RoomId} {RoomNumber}", room.RoomId, room.RoomNumber);
            return RedirectToAction(nameof(Table));
        }

        // Log a warning if the form input was invalid, so the error can be traced
        _logger.LogWarning("[RoomController] Room update failed, ModelState invalid {RoomNumber} {RoomId}", room.RoomNumber, room.RoomId);
        return View(room);
    }

    // GET: /Room/Delete/{id}
    // Shows the confirmation page for deleting a room
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _roomRepository.GetRoomById(id);
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
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool returnOk = await _roomRepository.Delete(id);
        if (!returnOk)
        {
            // Log an error if the repository failed to delete the room (not found or database error)
            _logger.LogError("[RoomController] Failed to delete room in repository, RoomId: {RoomId}", id);
            return BadRequest("Room deletion failed");
        }

        // Log that the room was successfully deleted
        _logger.LogInformation("[RoomController] Room deleted, RoomId: {RoomId}", id);
        return RedirectToAction(nameof(Table));
    }
}