namespace study_room_booking.DTOs;

public class AvailableRoomsDto
{
    public int AvailableRoomCount { get; set; }
    public List<RoomDto> AvailableRooms { get; set; } = new();
}