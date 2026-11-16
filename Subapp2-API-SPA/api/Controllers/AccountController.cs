using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;
using study_room_booking.DAL;
using RegisterRequestDto = study_room_booking.DTOs.RegisterRequestDto;
using LoginRequestDto = study_room_booking.DTOs.LoginRequestDto;


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
        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
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

        return Ok(new
        {
            message = "Account created.",
            username = user.UserName
        });
    }

    // POST: /api/Account/Login
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        var result = await _signInManager.PasswordSignInAsync(
            model.Username,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid username or password."});
        }
        return Ok(new
        {
            message = "Logged in.",
            username = model.Username
        });
    }

    // GET: /api/Account/Me
    [HttpGet("Me")]
    public IActionResult Me()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            username = User.Identity.Name,
            userId = _userManager.GetUserId(User),
            isAdmin = User.IsInRole("Admin")
        });
    }

    // POST: /api/Account/Logout
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }
};
