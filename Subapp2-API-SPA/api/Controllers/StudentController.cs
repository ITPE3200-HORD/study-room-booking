using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using StudentEntity = study_room_booking.Models.Student;
using StudentResponseDto = study_room_booking.DTOs.Student;
using StudentUpdateDto = study_room_booking.DTOs.StudentUpdateDto;

namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StudentController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        IStudentRepository studentRepository,
        UserManager<IdentityUser> userManager,
        ILogger<StudentController> logger)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: /api/Student
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _studentRepository.GetAll();

        if (students == null)
        {
            _logger.LogError("[StudentController] Failed to retrieve students");
            return Problem("Failed to retrieve students.");
        }
        return Ok(students.Select(ToDto));
    }

    // GET: /api/Student/{id} 
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var student = await _studentRepository.GetById(id);

        if (student == null)
        {
            return NotFound("Student not found.");
        }

        return Ok(ToDto(student));
    }

    // PUT: /api/Student/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update (int id, [FromBody] StudentUpdateDto model)
    {
        if (id != model.StudentId)
        {
            return BadRequest("The route ID must match StudentId in the request body.");
        }

        var student = await _studentRepository.GetById(id);

        if (student == null)
        {
            return NotFound("Student not found.");
        }

        var user = await _userManager.FindByIdAsync(student.IdentityUserId);

        if (user == null)
        {
            _logger.LogWarning("[StudentController] Identity user not foundfor student {StudentId}", id);

            return NotFound("The student´s user account was not found.");
        }

        user.UserName = model.UserName.Trim();
        user.Email = model.Email.Trim();

        var identityResult = await _userManager.UpdateAsync(user);

        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(error => error.Description);

            return BadRequest(new { errors });
        }

        student.FullName = model.FullName.Trim();

        if (!await _studentRepository.Update(student))
        {
            _logger.LogError("[StudentController] Failed to update student {StudentId}", id);

            return Problem("Failed to update student information.");
        }

        _logger.LogInformation("[StudentController] Student {StudentId} updated", id);

        return NoContent();
    }

    // DELETE: /api/Student/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _studentRepository.GetById(id);
        if (student == null)
        {
            return NotFound("Student not found.");
        }

        // delete studentprofile first 
        if (!await _studentRepository.Delete(id))
        {
            _logger.LogError("[StudentController] Failed to delete student {StudentId}.", id);
            return Problem("Failed to delete student.");
        }

        var user = await _userManager.FindByIdAsync(student.IdentityUserId);
        if (user != null)
        {
            var identityResult = await _userManager.DeleteAsync(user);

            if(!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.Select(error => error.Description);
                _logger.LogError("[StudentController] Failed to delete Identity user for student {StudentId}.", id);

                return Problem(detail: string.Join("; ", errors), statusCode: StatusCodes.Status500InternalServerError);
            }
        }
        _logger.LogInformation("[StudentController] Student {StudentId} deleted.", id);
        return NoContent();
    }

    private static StudentResponseDto ToDto(StudentEntity student)
    {
        return new StudentResponseDto
        {
           StudentId = student.StudentId,
           UserName = student.IdentityUser?.UserName ?? string.Empty,
           FullName = student.FullName,
           Email = student.IdentityUser?.Email ?? string.Empty
        };
    }
}