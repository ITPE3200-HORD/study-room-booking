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
    // Tests that Table returns a view containing the bookings provided by the repository.
    // Arrange: substitute the repositories and logger, and configure GetAll() to return sample bookings.
    // Act: call the Table action.
    // Assert: verify the result is a ViewResult with the expected bookings, and that GetAll() was called once.
    [Fact]
    public async Task Table_WhenBookingsExist_ReturnsViewWithBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new()
            {
                BookingId = 1,
                RoomId = 2,
                UserName = "Alex",
                StartTime = new DateTime(2030, 5, 1, 10, 0, 0),
                EndTime = new DateTime(2030, 5, 1, 11, 0, 0)
            }
        };

        var bookingRepository = Substitute.For<IBookingRepository>();
        bookingRepository.GetAll()
            .Returns(Task.FromResult<IEnumerable<Booking>?>(bookings));

        var roomRepository = Substitute.For<IRoomRepository>();
        var logger = Substitute.For<ILogger<BookingController>>();

        var controller = new BookingController(
            bookingRepository,
            roomRepository,
            logger);

        // Act
        var result = await controller.Table();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Booking>>(viewResult.Model);
        Assert.Equal(bookings, model);

        await bookingRepository.Received(1).GetAll();
    }

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

        var controller = new BookingController(
            bookingRepository,
            roomRepository,
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
}