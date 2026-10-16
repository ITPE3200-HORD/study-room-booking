using study_room_booking.Models;

namespace study_room_booking.DAL;

public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAll();
    Task<Student?> GetById(int id);
}
