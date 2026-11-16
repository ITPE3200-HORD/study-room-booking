using System.ComponentModel.DataAnnotations;
using study_room_booking.ViewModels;

namespace study_room_booking.Tests.ViewModels;

public class BookingCreateViewModelTests
{
    // Tests that booking validation rejects a start time in the past.
    // Arrange: create a booking request with a past date and otherwise valid values.
    // Act: run the model's data annotation and custom validation.
    // Assert: verify validation fails with the expected error.
    [Fact]
    public void Validate_WhenStartTimeIsInPast_ReturnsValidationError()
    {
        // Arrange
        var model = new BookingCreateViewModel
        {
            RoomId = 1,
            RoomNumber = "101",
            Date = DateTime.Today.AddDays(-1),
            StartTime = new TimeSpan(10, 0, 0),
            DurationMinutes = 60,
            CreateStudySession = false
        };

        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            validationResults,
            validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(
            validationResults,
            result => result.ErrorMessage == "You cannot book in the past.");
    }

    // Tests that booking validation rejects durations below 60 or above 180 minutes.
    // Arrange: create booking requests with durations outside the allowed range.
    // Act: validate each request.
    // Assert: verify each fails with the duration validation error.
    [Fact]
    public void Validate_WhenDurationIsOutsideAllowedRange_ReturnsValidationError()
    {
        // Arrange / Act / Assert
        foreach (var duration in new[] { 30, 240 })
        {
            var model = new BookingCreateViewModel
            {
                RoomId = 1,
                RoomNumber = "101",
                Date = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(10, 0, 0),
                DurationMinutes = duration,
                CreateStudySession = false
            };

            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(
                validationResults,
                result => result.ErrorMessage ==
                    "The booking must last between 1 and 3 hours.");
        }
    }
}