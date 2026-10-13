using study_room_booking.Models;

namespace study_room_booking.ViewModels
{
    public class UserViewModel
    {
        public IEnumerable<User> Users;
        public string? CurrentViewName;

        public UserViewModel(IEnumerable<User> users, string? currentViewName)
        {
            Users = users;
            CurrentViewName = currentViewName;
        }
    }
}