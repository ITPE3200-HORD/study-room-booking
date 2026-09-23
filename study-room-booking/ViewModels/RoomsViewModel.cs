using study_room_booking.Models;

namespace study_room_booking.ViewModels
{
    public class RoomsViewModel
    {
        public IEnumerable<Room> Rooms;
        public string? CurrentViewName;

        public RoomsViewModel(IEnumerable<Room> rooms, string? currentViewName)
        {
            Rooms = rooms;
            CurrentViewName = currentViewName;
        }
    }
}