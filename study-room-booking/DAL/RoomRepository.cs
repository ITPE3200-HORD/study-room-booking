// Here we implement the Room repository, which handles all database access for rooms.
// Each method wraps the database call in try/catch, so errors are logged
// instead of crashing the application.

using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.DAL;

public class RoomRepository : IRoomRepository
{
    private readonly RoomDbContext _db;
    private readonly ILogger<RoomRepository> _logger;

    public RoomRepository(RoomDbContext db, ILogger<RoomRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Room>?> GetAll()
    {
        try
        {
            return await _db.Rooms.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[RoomRepository] GetAll() failed, error message: {e}", e.Message);
            return null;
        }
    }

    public async Task<Room?> GetRoomById(int id)
    {
        try
        {
            return await _db.Rooms.FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError("[RoomRepository] GetRoomById() failed for RoomId {id}, error message: {e}", id, e.Message);
            return null;
        }
    }

    public async Task<bool> Create(Room room)
    {
        try
        {
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[RoomRepository] Create() failed for room {@room}, error message: {e}", room, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(Room room)
    {
        try
        {
            _db.Rooms.Update(room);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[RoomRepository] Update() failed for room {@room}, error message: {e}", room, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
            {
                _logger.LogError("[RoomRepository] Room not found for RoomId {id}", id);
                return false;
            }
            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[RoomRepository] Delete() failed for RoomId {id}, error message: {e}", id, e.Message);
            return false;
        }
    }
}