using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

public class HomeController : Controller
{
    private readonly IRoomRepository _roomRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IRoomRepository roomRepository,
        IStudySessionRepository studySessionRepository,
        ILogger<HomeController> logger)
    {
        _roomRepository = roomRepository;
        _studySessionRepository = studySessionRepository;
        _logger = logger;
    }

    // GET: /Home/Index
    // First available room is displayed on the home page
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var rooms = await _roomRepository.GetAllOrdered();
            var openStudySession = await _studySessionRepository.GetOpen(null, null);

            if (rooms == null || openStudySession == null)
            {
                _logger.LogError("Could nott load data for the homepage.");
                return StatusCode(500, "Data could nott be rettrrieved.");
            }

            var viewModel = new HomeIndexViewModel
            {
                Rooms = rooms.ToList(),
                OpenStudySessions = openStudySession.ToList()
            };

            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load tthe home page.");
            return StatusCode(500, "An unexpected error occured");
        }
    }

    // GET: /Home/Book
    [HttpGet]
    public async Task<IActionResult> Book([FromQuery] BookViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var startTime = model.Date.ToDateTime(model.StartTime);

        if (startTime < DateTime.Now)
        {
            ModelState.AddModelError(nameof(BookViewModel.StartTime), "Choose a start time in the future.");
            return View(model);
        }

        var endTime = startTime.AddMinutes(model.DurationMinutes);

        try
        {
            var availableRooms = await _roomRepository.GetAvailableRooms(
                startTime, endTime, model.GroupSize, model.RoomType);

            if (availableRooms == null)
            {
                _logger.LogError("Could not find available rooms for {StartTime} - {EndTime}.", startTime, endTime);

                return StatusCode(500, "Available rooms could not be retrieved. Please ttry again later.");
            }
            model.AvailableRooms = availableRooms.ToList();
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to find available rooms for {StartTime} - {EndTime}.", startTime, endTime);

            return StatusCode(500, "Available rooms could not be retrieved. Please try again later.");
        }
        ;
    }

    // GET: /Home/Error
    // Shown when an unexpected error occurs in production (see UseExceptionHandler in Program.cs)
    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}
