using Microsoft.EntityFrameworkCore;
using study_room_booking.Models;

namespace study_room_booking.DAL;

public class StudentRepository : IStudentRepository
{
    private readonly RoomDbContext _db;
    private readonly ILogger<StudentRepository> _logger;

    public StudentRepository(
        RoomDbContext db,
        ILogger<StudentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Student>?> GetAll()
    {
        try
        {
            return await _db.Students
            .Include(student => student.IdentityUser)
            .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[StudentRepository] GetAll() failed, error message: {e}", e.Message);
            return null;
        }
    }

    public async Task<Student?> GetById(int id)
    {
        try
        {
            return await _db.Students
            .Include(student => student.IdentityUser)
            .FirstOrDefaultAsync(student => student.StudentId == id);
        }
        catch (Exception e)
        {
            _logger.LogError("[StudentRepository] GetStudentById() failed for StudentId {id}, error message: {e}", id, e.Message);
            return null;
        }
    }

    public async Task<bool> Create(Student student)
    {
        try
        {
            _db.Students.Add(student);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudentRepository] Create() failed for student {@student}, error message: {e}", student, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(Student student)
    {
        try
        {
            _db.Students.Update(student);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudentRepository] Update() failed for student {@student}, error message: {e}", student, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var student = await _db.Students.FindAsync(id);
            if (student == null)
            {
                _logger.LogError("[StudentRepository] Student not found for StudentId {id}", id);
                return false;
            }
            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[StudentRepository] Delete() failed for StudentId {id}, error message: {e}", id, e.Message);
            return false;
        }
    }
}
