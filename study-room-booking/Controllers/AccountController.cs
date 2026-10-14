using Microsoft.AspNetCore.Mvc;
using study_room_booking.Models;

namespace study_room_booking.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }


};
