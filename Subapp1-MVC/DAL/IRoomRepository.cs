// Here we define the interface for the Room repository, 
// which outlines the methods for interacting with the Room data in the database. 
// This includes methods for retrieving all rooms, getting a room by its ID, 
// creating a new room, updating an existing room, and deleting a room by its ID.

using study_room_booking.Models;

namespace study_room_booking.DAL;

// Defines the database operations available for rooms
public interface IRoomRepository
{
    Task<IEnumerable<Room>?> GetAll();
    Task<Room?> GetRoomById(int id);
    Task<bool> Create(Room room);
    Task<bool> Update(Room room);
    Task<bool> Delete(int id);
}