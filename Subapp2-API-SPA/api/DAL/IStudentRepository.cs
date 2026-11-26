// Interface for the Student repository.
// Defines which database operations the controllers can use for students,
// including finding the student profile linked to a logged-in IdentityUser.

using study_room_booking.Models;

namespace study_room_booking.DAL;

public interface IStudentRepository
{
    Task<IEnumerable<Student>?> GetAll();
    Task<Student?> GetById(int id);
    Task<Student?> GetByIdentityUserId(string userId);
    Task<bool> Create(Student student);
    Task<bool> Update(Student student);
    Task<bool> Delete(int id);
}

