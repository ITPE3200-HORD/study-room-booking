using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.DAL;

public class UserRepository : IUserRepository
{
    private readonly RoomDbContext _db;
    

    public UserRepository(RoomDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        return await _db.Users.ToListAsync();
    }

    public async Task<User?> GetById(int id)
    {
        return await _db.Users.FindAsync(id);
    }
}