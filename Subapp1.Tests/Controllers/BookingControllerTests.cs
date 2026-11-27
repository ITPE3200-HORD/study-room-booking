using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using study_room_booking.Controllers;
using study_room_booking.DAL;
using study_room_booking.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using study_room_booking.ViewModels;

namespace study_room_booking.Tests.Controllers;

public class BookingControllerTests
{
    // Tests that a booking is rejected when the selected room is already booked.
    // Arrange: configure the repository to report a time conflict and create a valid booking request.
    // Act: submit the booking through the controller.
    // Assert: verify the form is returned with a conflict error and the repository does not save the booking.
    [Fact]
    public async Task Create_WhenRoomIsAlreadyBooked_DoesNotSaveBooking()
    {
        // Arrange
        var bookingRepository = Substitute.For<IBookingRepository>();
        bookingRepository.IsOverlapping(Arg.Any<Booking>())
            .Returns(true);

        var roomRepository = Substitute.For<IRoomRepository>();
        var logger = Substitute.For<ILogger<BookingController>>();

        var studentRepository = Substitute.For<IStudentRepository>();
        studentRepository.GetByIdentityUserId("student-123")
            .Returns(new Student
            {
                StudentId = 1,
                IdentityUserId = "student-123",
                FullName = "Test Student"
            });

        var controller = new BookingController(
            bookingRepository,
            roomRepository,
            studentRepository,
            logger);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "student-123") },
                authenticationType: "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var model = new BookingCreateViewModel
        {
            RoomId = 1,
            RoomNumber = "101",
            Date = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(10, 0, 0),
            DurationMinutes = 60,
            CreateStudySession = false
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage == "The room is already booked.");

        await bookingRepository.DidNotReceive()
            .Create(Arg.Any<Booking>());
    }

    // Tests that a valid booking is saved for the signed-in user.
    // Arrange: configure the repository to report no conflicts and a successful save.
    // Act: submit the booking through the controller.
    // Assert: verify it is saved with the expected details and the user is redirected to MyPage.
    [Fact]
    public async Task Create_WithValidBooking_SavesBookingAndRedirectsToMyPage()
    {
        // Arrange
        var bookingRepository = Substitute.For<IBookingRepository>();
        bookingRepository.IsOverlapping(Arg.Any<Booking>()).Returns(false);
        bookingRepository.UserHasOverlappingBooking(Arg.Any<Booking>()).Returns(false);
        bookingRepository.Create(Arg.Any<Booking>()).Returns(true);

        var roomRepository = Substitute.For<IRoomRepository>();
        var logger = Substitute.For<ILogger<BookingController>>();

        var studentRepository = Substitute.For<IStudentRepository>();
    studentRepository.GetByIdentityUserId("student-123")
        .Returns(new Student
        {
            StudentId = 1,
            IdentityUserId = "student-123",
            FullName = "Test Student"
        });

        var controller = new BookingController(
        bookingRepository,
        roomRepository,
        studentRepository,
        logger);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "student-123") },
                authenticationType: "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var model = new BookingCreateViewModel
        {
            RoomId = 1,
            RoomNumber = "101",
            Date = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(10, 0, 0),
            DurationMinutes = 60,
            CreateStudySession = false
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("MyPage", redirect.ActionName);
        Assert.Equal("MyPage", redirect.ControllerName);

        await bookingRepository.Received(1).Create(
            Arg.Is<Booking>(booking =>
                booking.RoomId == model.RoomId &&
                booking.StudentId == 1 &&
                booking.StartTime == model.Date.Date + model.StartTime &&
                booking.EndTime == model.Date.Date + model.StartTime
                    .Add(TimeSpan.FromMinutes(model.DurationMinutes))));
    }

    // Tests that a user cannot create another booking during an existing booking.
    // Arrange: configure the repository to report no room conflict but a user booking conflict.
    // Act: submit a valid booking request.
    // Assert: verify the form shows an error and the booking is not saved.
    [Fact]
    public async Task Create_WhenUserHasOverlappingBooking_DoesNotSaveBooking()
    {
        // Arrange
        var bookingRepository = Substitute.For<IBookingRepository>();
        bookingRepository.IsOverlapping(Arg.Any<Booking>()).Returns(false);
        bookingRepository.UserHasOverlappingBooking(Arg.Any<Booking>()).Returns(true);

        var roomRepository = Substitute.For<IRoomRepository>();
        var logger = Substitute.For<ILogger<BookingController>>();

        var studentRepository = Substitute.For<IStudentRepository>();
        studentRepository.GetByIdentityUserId("student-123")
            .Returns(new Student
            {
                StudentId = 1,
                IdentityUserId = "student-123",
                FullName = "Test Student"
            });

        var controller = new BookingController(
             bookingRepository,
             roomRepository,
            studentRepository,
             logger);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "student-123") },
                authenticationType: "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var model = new BookingCreateViewModel
        {
            RoomId = 1,
            RoomNumber = "101",
            Date = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(10, 0, 0),
            DurationMinutes = 60,
            CreateStudySession = false
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Contains(
            controller.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage!.Contains("You already have another booking"));

        await bookingRepository.DidNotReceive()
            .Create(Arg.Any<Booking>());
    }

    // Tests that creating a study session also creates its associated booking.
    // Arrange: configure the repository to report no conflicts and successfully save.
    // Act: submit a valid booking request with study-session details.
    // Assert: verify the booking and its study session are sent to the repository and the user is redirected to MyPage.
    [Fact]
    public async Task Create_WithStudySession_CreatesBookingAndSession()
    {
        // Arrange
        var bookingRepository = Substitute.For<IBookingRepository>();
        bookingRepository.IsOverlapping(Arg.Any<Booking>()).Returns(false);
        bookingRepository.UserHasOverlappingBooking(Arg.Any<Booking>()).Returns(false);
        bookingRepository.Create(Arg.Any<Booking>()).Returns(true);

        var roomRepository = Substitute.For<IRoomRepository>();
        var logger = Substitute.For<ILogger<BookingController>>();
       
        var studentRepository = Substitute.For<IStudentRepository>();
    studentRepository.GetByIdentityUserId("student-123")
        .Returns(new Student
        {
            StudentId = 1,
            IdentityUserId = "student-123",
            FullName = "Test Student"
        });
       
        var controller = new BookingController(
        bookingRepository,
        roomRepository,
        studentRepository,
        logger);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "student-123") },
                authenticationType: "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var model = new BookingCreateViewModel
        {
            RoomId = 1,
            RoomNumber = "101",
            Date = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(10, 0, 0),
            DurationMinutes = 60,
            CreateStudySession = true,
            SessionTitle = "  Study group  ",
            SessionSubject = "Statistics",
            SessionMaxParticipants = 5
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("MyPage", redirect.ActionName);
        Assert.Equal("MyPage", redirect.ControllerName);

        await bookingRepository.Received(1).Create(
            Arg.Is<Booking>(booking =>
                booking.RoomId == model.RoomId &&
                booking.StudentId == 1 &&
                booking.StudySession != null &&
                booking.StudySession.Title == "Study group" &&
                booking.StudySession.Subject == "Statistics" &&
                booking.StudySession.MaxParticipants == 5 &&
                booking.StudySession.IsOpen));
    }
}