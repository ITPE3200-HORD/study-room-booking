using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.ViewModels;
using study_room_booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace study_room_booking.Controllers;

[Authorize(Roles = "Admin")]
public class StudentController : Controller
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

    // No Create as students are created when registering as user. Admin can update student information, but not create new students.

    // The Student profile and Identity account are saved separately.
    // If the second save fails, the operation may be only partly completed.
    // Update saves the Identity account first; Delete removes the Student profile first.

    // GET: /Student/Update
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var student = await _studentRepository.GetById(id);
        if (student == null)
        {
            _logger.LogWarning("[StudentController] Student with ID {Id} not found", id);
            return NotFound();
        }

        return View(new StudentUpdateViewModel
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.IdentityUser?.Email ?? string.Empty,
            Username = student.IdentityUser?.UserName ?? string.Empty

        });
    }

    // POST: /Student/Update/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(StudentUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[StudentController] Model state is invalid for student update");
            return View(model);
        }

        var student = await _studentRepository.GetById(model.StudentId);
        if (student == null)
        {
            _logger.LogWarning("[StudentController] Student with ID {Id} not found for update", model.StudentId);
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(student.IdentityUserId);
        if (user == null)
        {
            _logger.LogWarning("[StudentController] Identity user with ID {Id} not found for student update", student.IdentityUserId);
            
            ModelState.AddModelError(string.Empty, "Associated user account not found.");
            return View(model);
        }

        user.UserName = model.Username.Trim();
        user.Email = model.Email.Trim();

        var identityResult = await _userManager.UpdateAsync(user);
        if (!identityResult.Succeeded) {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            _logger.LogWarning("[StudentController] Failed to update identity user for student with ID {Id}", model.StudentId);
            return View(model);
        }

        student.FullName = model.FullName.Trim();
        if (!await _studentRepository.Update(student))
        {
            _logger.LogError("[StudentController] Failed to update student with ID {Id} in repository", model.StudentId);
            ModelState.AddModelError(string.Empty, "Failed to update student information.");
            return View(model);
        }

        _logger.LogInformation("[StudentController] Student with ID {Id} updated successfully", model.StudentId);

        return RedirectToAction(nameof(Table));
    }

    // GET: /Student/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _studentRepository.GetById(id);
        if (student == null)
        {
            _logger.LogWarning("[StudentController] Student with ID {Id} not found for deletion", id);
            return NotFound();
        }

        return View(student);
    }

    // POST: /Student/DeleteConfirmed
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _studentRepository.GetById(id);
        if (student == null)
        {
            _logger.LogWarning("[StudentController] Student with ID {Id} not found for deletion", id);
            return NotFound();
        }

        // Delete the student before deleting the associated IdentityUser
        if (!await _studentRepository.Delete(id))
        {
            _logger.LogError("[StudentController] Failed to delete student with ID {Id} from repository", id);
            return BadRequest("Failed to delete student.");
        }

        var user = await _userManager.FindByIdAsync(student.IdentityUserId);
        if (user != null)
        {
            var identityResult = await _userManager.DeleteAsync(user);
            if (!identityResult.Succeeded)
            {
                _logger.LogError("[StudentController] Failed to delete identity user with ID {Id} for student deletion", student.IdentityUserId);
                return BadRequest("Failed to delete associated user account.");
            }
        }
        else
        {
            _logger.LogWarning("[StudentController] Identity user with ID {Id} not found for student deletion", student.IdentityUserId);
        }
        _logger.LogInformation("[StudentController] Student with ID {Id} deleted successfully", id);
        return RedirectToAction(nameof(Table));
    }
}
