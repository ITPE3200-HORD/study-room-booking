using study_room_booking.Models;

namespace study_room_booking.DAL;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAll();
    Task<User?> GetById(int id);
}