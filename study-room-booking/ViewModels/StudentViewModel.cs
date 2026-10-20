using study_room_booking.Models;

namespace study_room_booking.ViewModels;

public class StudentViewModel
{
    public IEnumerable<Student> Students { get; }
    public string? CurrentViewName { get; }

    public StudentViewModel(IEnumerable<Student> students, string? currentViewName)
    {
        Students = students;
        CurrentViewName = currentViewName;
    }
}
