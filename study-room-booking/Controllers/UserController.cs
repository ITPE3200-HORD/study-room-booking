using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository userRepository, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    // GET: /User/Table
    // Shows all users in a table
    public async Task<IActionResult> Table()
    {
        var users = await _userRepository.GetAll();
        if (users == null)
        {
            // Log an error if the repository failed to retrieve users
            _logger.LogError("[UserController] Failed to retrieve users from repository");
            return StatusCode(500, "Internal server error");
        }
        var userListViewModel = new UserViewModel(users.ToList(), "Table");
        return View(userListViewModel);
    }
}