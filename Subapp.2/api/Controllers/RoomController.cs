using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.DTOs;
using study_room_booking.Models;

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

    // GET: /api/roomAPI/Table
    [HttpGet]
    [HttpGet("Table")]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomRepository.GetAll();
        if (rooms == null)
        {
            _logger.LogError("[RoomAPIController] Failed to retrieve rooms from repository");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }

        return Ok(rooms.Select(ToDto));
    }

    // GET: /api/roomAPI/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomRepository.GetRoomById(id);
        if (room == null)
        {
            _logger.LogWarning("[RoomAPIController] Room not found, RoomId: {RoomId}", id);
            return NotFound();
        }

        return Ok(ToDto(room));
    }

    // POST: /api/roomAPI
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] RoomDto roomDto)
    {
        var room = FromDto(roomDto);
        var created = await _roomRepository.Create(room);
        if (!created)
        {
            _logger.LogError("[RoomAPIController] Failed to create room: {RoomNumber}", room.RoomNumber);
            return Problem("Could not save the room. Please try again.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[RoomAPIController] Room created: {RoomId} {RoomNumber}", room.RoomId, room.RoomNumber);
        return CreatedAtAction(nameof(GetById), new { id = room.RoomId }, ToDto(room));
    }

    // PUT: /api/roomAPI/{id}
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] RoomDto roomDto)
    {
        if (id != roomDto.RoomId)
        {
            return BadRequest("The route ID must match the room ID in the request body.");
        }

        var existingRoom = await _roomRepository.GetRoomById(id);
        if (existingRoom == null)
        {
            _logger.LogWarning("[RoomAPIController] Room not found for update, RoomId: {RoomId}", id);
            return NotFound();
        }

        var room = FromDto(roomDto);
        var updated = await _roomRepository.Update(room);
        if (!updated)
        {
            _logger.LogError("[RoomAPIController] Failed to update room: {RoomId} {RoomNumber}", id, room.RoomNumber);
            return Problem("Could not update the room. Please try again.", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation("[RoomAPIController] Room updated: {RoomId} {RoomNumber}", room.RoomId, room.RoomNumber);
        return NoContent();
    }

    // DELETE: /api/roomAPI/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _roomRepository.Delete(id);
        if (!deleted)
        {
            _logger.LogWarning("[RoomAPIController] Failed to delete room, RoomId: {RoomId}", id);
            return NotFound();
        }

        _logger.LogInformation("[RoomAPIController] Room deleted, RoomId: {RoomId}", id);
        return NoContent();
    }

    private static RoomDto ToDto(Room room) => new()
    {
        RoomId = room.RoomId,
        Location = room.Location,
        RoomNumber = room.RoomNumber,
        RoomType = room.RoomType,
        Capacity = room.Capacity,
        Status = (RoomDto.Availability)room.Status
    };

    private static Room FromDto(RoomDto roomDto) => new()
    {
        RoomId = roomDto.RoomId,
        Location = roomDto.Location,
        RoomNumber = roomDto.RoomNumber,
        RoomType = roomDto.RoomType,
        Capacity = roomDto.Capacity,
        Status = (Room.Availability)roomDto.Status
    };
}
