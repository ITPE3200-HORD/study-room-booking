using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.DAL;

public class StudentRepository : IStudentRepository
{
    private readonly RoomDbContext _db;

    public StudentRepository(RoomDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Student>> GetAll()
    {
        return await _db.Students
            .Include(student => student.IdentityUser)
            .ToListAsync();
    }

    public async Task<Student?> GetById(int id)
    {
        return await _db.Students
            .Include(student => student.IdentityUser)
            .FirstOrDefaultAsync(student => student.StudentId == id);
    }
}
