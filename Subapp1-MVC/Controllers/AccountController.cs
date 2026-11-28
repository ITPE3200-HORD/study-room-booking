using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;
using study_room_booking.ViewModels;
using study_room_booking.DAL;

namespace study_room_booking.Controllers;

public class AccountController : Controller
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

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email
        };


        var identityResult = await _userManager.CreateAsync(user, model.Password);

        if (!identityResult.Succeeded)
        {
            _logger.LogWarning("[AccountController] Registration failed for user {Username}", model.Username);
            foreach (var error in identityResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // Create a new Student entity and link it to the IdentityUser
        var student = new Student
        {
            FullName = model.FullName,
            IdentityUserId = user.Id
        };

        var studentSaved = await _studentRepository.Create(student);

        if (!studentSaved)
        {
            _logger.LogError("[AccountController] Failed to create student profile for user {UserId}", user.Id);
            // Remove the account so registration doesn't leave an unlinked user behind.
            var rollbackResult = await _userManager.DeleteAsync(user);

            if (!rollbackResult.Succeeded)
            {
                _logger.LogError("[AccountController] Failed to roll back user {UserId}", user.Id);
                ModelState.AddModelError(
                    string.Empty,
                    "The student profile could not be saved, and the account could not be removed.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Could not save the student profile. Please try again.");
            }

            return View(model);
        }

        // Add the user to the "Student" role
        var roleResult = await _userManager.AddToRoleAsync(user, "Student");

        // If adding the user to the role fails, roll back the student and user creation
        if (!roleResult.Succeeded)
        {
            _logger.LogError("[AccountController] Failed to add user {UserId} to the Student role", user.Id);
            foreach (var error in roleResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            // Roll back the profile and account
            await _studentRepository.Delete(student.StudentId);
            await _userManager.DeleteAsync(user);

            return View(model);
        }

        _logger.LogInformation("[AccountController] New user registered: {Username}", user.UserName);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Default to home page if ReturnUrl is null or empty
        string targetUrl = !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
            ? model.ReturnUrl
            : Url.Action("Index", "Home") ?? "~/";

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Redirect(targetUrl);
            }
            _logger.LogWarning("[AccountController] Failed login attempt for user {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
};
