using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;
using study_room_booking.DAL;
using study_room_booking.DTOs;


namespace study_room_booking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IStudentRepository studentRepository,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    // POST: /api/Account/Register
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        try
        {
            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // Invalid input (e.g. username taken, weak password) is a user error, so Warning is used
                _logger.LogWarning("[AccountController] Registration failed for user {Username}", model.Username);
                return BadRequest(new
                {
                    errors = result.Errors.Select(error => error.Description)
                });
            }

            var student = new Student
            {
                FullName = model.FullName.Trim(),
                IdentityUserId = user.Id
            };

            if (!await _studentRepository.Create(student))
            {
                var rollbackResult = await _userManager.DeleteAsync(user);

                _logger.LogError(
                    "[AccountController] Failed to create student profile for user {UserId}",
                    user.Id);

                if (!rollbackResult.Succeeded)
                {
                    _logger.LogError(
                        "[AccountController] Failed to roll back user {UserId}",
                        user.Id);
                }

                return Problem(
                    "Could not create the student profile.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return Ok(new AuthResponseDto
            {
                Message = "Account created.",
                Username = user.UserName ?? string.Empty
            });
        }
        catch (Exception e)
        {
            // Unexpected errors (e.g. database unavailable) are logged with full details,
            // while the user only receives a general message without technical information
            _logger.LogError(e, "[AccountController] Registration failed unexpectedly for user {Username}", model.Username);
            return Problem("Registration could not be completed.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // POST: /api/Account/Login
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                // Wrong username or password is a user error, not a system error, so Warning is used
                _logger.LogWarning("[AccountController] Failed login attempt for user {Username}", model.Username);
                return Unauthorized(new { message = "Invalid username or password." });
            }
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "Invalid username or password" });
            }
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var student = await _studentRepository.GetOrCreateByIdentityUserId(user.Id, user.UserName ?? user.Email ?? model.Username);

                if (student == null)
                {
                    await _signInManager.SignOutAsync();
                    return Problem("The student profile could not be created", statusCode: StatusCodes.Status500InternalServerError);
                }
            }

            return Ok(new AuthResponseDto
            {
                Message = "Logged in.",
                Username = model.Username
            });
        }
        catch (Exception e)
        {
            // Unexpected errors (e.g. database unavailable) are logged with full details,
            // while the user only receives a general message without technical information
            _logger.LogError(e, "[AccountController] Login failed for user {Username}", model.Username);
            return Problem("Login could not be completed.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // GET: /api/Account/Me
    [HttpGet("Me")]
    public IActionResult Me()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserDto
        {
            Username = User.Identity.Name ?? string.Empty,
            UserId = _userManager.GetUserId(User) ?? string.Empty,
            IsAdmin = User.IsInRole("Admin")
        });
    }

    // POST: /api/Account/Logout
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }
}