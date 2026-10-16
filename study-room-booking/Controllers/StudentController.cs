using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

public class StudentController : Controller
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        IStudentRepository studentRepository,
        ILogger<StudentController> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    // GET: /Student/Table
    public async Task<IActionResult> Table()
    {
        var students = await _studentRepository.GetAll();
        if (students == null)
        {
            _logger.LogError("[StudentController] Failed to retrieve students from repository");
            return StatusCode(500, "Internal server error");
        }

        return View(new StudentViewModel(students, "Table"));
    }
}
