using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Controllers;

[Authorize(Roles = "Student")]
public class StudySessionController : Controller
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudySessionController> _logger;

    public StudySessionController(
        IStudySessionRepository studySessionRepository,
        IStudentRepository studentRepository,
        ILogger<StudySessionController> logger)
    {
        _studySessionRepository = studySessionRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    // GET: /StudySession/Update/5
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var session = await _studySessionRepository.GetById(id);

        if (session == null)
            return NotFound();

        if (!IsOwner(session))
            return Forbid();

        var model = new StudySessionUpdateViewModel
        {
            Title = session.Title,
            Subject = session.Subject,
            MaxParticipants = session.MaxParticipants,
            IsOpen = session.IsOpen,
            StudySessionId = session.StudySessionId
        };

        return View(model);
    }

    // POST: /StudySession/Update/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int id,
        StudySessionUpdateViewModel model)
    {
        model.StudySessionId = id; // Keep the route ID when redisplaying the form

        var session = await _studySessionRepository.GetById(id);

        if (session == null)
            return NotFound();

        if (!IsOwner(session))
            return Forbid();

        if (ModelState.IsValid &&
            model.MaxParticipants < session.Participants.Count)
        {
            ModelState.AddModelError(
                nameof(model.MaxParticipants),
                $"The session already has {session.Participants.Count} participants.");
        }

        if (ModelState.IsValid &&
            session.Booking.Room != null &&
            model.MaxParticipants > session.Booking.Room.Capacity)
        {
            ModelState.AddModelError(
                nameof(model.MaxParticipants),
                $"The room only has room for {session.Booking.Room.Capacity} people.");
        }

        if (!ModelState.IsValid)
            return View(model);

        session.Title = model.Title.Trim();
        session.Subject = string.IsNullOrWhiteSpace(model.Subject)
            ? null
            : model.Subject.Trim();
        session.MaxParticipants = model.MaxParticipants;
        session.IsOpen = model.IsOpen;

        if (!await _studySessionRepository.Update(session))
        {
            _logger.LogError(
                "[StudySessionController] Failed to update session {SessionId}",
                id);

            ModelState.AddModelError("", "Could not update the study session.");
            return View(model);
        }

        return RedirectToAction("MyPage", "MyPage");
    }

    // POST: /StudySession/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var session = await _studySessionRepository.GetById(id);

        if (session == null)
            return NotFound();

        if (!IsOwner(session))
            return Forbid();

        if (!await _studySessionRepository.Delete(id))
        {
            _logger.LogError(
                "[StudySessionController] Failed to delete session {SessionId}",
                id);

            return StatusCode(500, "Could not delete the study session.");
        }

        return RedirectToAction("MyPage", "MyPage");
    }

    private bool IsOwner(StudySession session)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return userId != null &&
               session.Booking.Student?.IdentityUserId == userId;
    }

    // POST: /StudySession/Join/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int id)
    {
        var session = await _studySessionRepository.GetById(id);
        if (session == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var student = userId == null
            ? null
            : await _studentRepository.GetByIdentityUserId(userId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "You need a student profile to join.";
            return RedirectToAction("Index", "Home");
        }

        string? error = null;

        if (!session.IsOpen)
            error = "This study session is closed.";
        else if (session.Booking.StartTime <= DateTime.Now)
            error = "This study session has already started.";
        else if (session.Participants.Any(p => p.StudentId == student.StudentId))
            error = "You have already joined this study session.";
        else if (session.Participants.Count >= session.MaxParticipants)
            error = "This study session is full.";

        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToAction("Index", "Home");
        }

        var added = await _studySessionRepository.AddParticipant(
            new SessionParticipant
            {
                StudySessionId = id,
                StudentId = student.StudentId
            });

        if (!added)
        {
            TempData["ErrorMessage"] = "Could not join the study session.";
            return RedirectToAction("Index", "Home");
        }

        TempData["SuccessMessage"] = "You joined the study session.";
        return RedirectToAction("Index", "Home");
    }

    // POST: /StudySession/Leave/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var session = await _studySessionRepository.GetById(id);

        if (session == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var student = userId == null
            ? null
            : await _studentRepository.GetByIdentityUserId(userId);

        if (student == null ||
            !session.Participants.Any(p => p.StudentId == student.StudentId))
        {
            TempData["ErrorMessage"] = "You are not a participant in this study session.";
            return RedirectToAction("MyPage", "MyPage");
        }

        if (IsOwner(session))
        {
            TempData["ErrorMessage"] = "You own this session. Delete it instead of leaving.";
            return RedirectToAction("MyPage", "MyPage");
        }

        if (!await _studySessionRepository.RemoveParticipant(id, student.StudentId))
        {
            _logger.LogError(
                "[StudySessionController] Student {StudentId} could not leave session {SessionId}",
                student.StudentId,
                id);

            TempData["ErrorMessage"] = "Could not leave the study session.";
            return RedirectToAction("MyPage", "MyPage");
        }

        _logger.LogInformation(
            "[StudySessionController] Student {StudentId} left session {SessionId}",
            student.StudentId,
            id);

        TempData["SuccessMessage"] = "You left the study session.";
        return RedirectToAction("MyPage", "MyPage");
    }
}