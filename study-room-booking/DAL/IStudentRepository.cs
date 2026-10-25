using study_room_booking.Models;

namespace study_room_booking.DAL;

public interface IStudentRepository
{
    Task<IEnumerable<Student>?> GetAll();
    Task<Student?> GetById(int id);
    Task<bool> Create(Student student);
    Task<bool> Update(Student student);
    Task<bool> Delete(int id);
}
