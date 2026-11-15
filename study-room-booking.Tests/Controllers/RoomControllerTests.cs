using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using study_room_booking.Controllers;
using study_room_booking.DAL;
using study_room_booking.Models;
using study_room_booking.ViewModels;

namespace study_room_booking.Tests.Controllers;

public class RoomControllerTests
{
    // Tests that a valid room is saved and the controller redirects to the room table.
    // Arrange: configure the repository substitute to successfully save the room.
    // Act: submit a valid room through the controller.
    // Assert: verify the room details were sent to the repository and the action redirects to Table.
    [Fact]
    public async Task Create_WithValidRoom_SavesRoomAndRedirectsToTable()
    {
        // Arrange
        var roomRepository = Substitute.For<IRoomRepository>();
        roomRepository.Create(Arg.Any<Room>()).Returns(true);

        var logger = Substitute.For<ILogger<RoomController>>();
        var controller = new RoomController(roomRepository, logger);

        var room = new Room
        {
            Location = "P35",
            RoomNumber = "101",
            RoomType = "Group Study Room",
            Capacity = 6,
            Status = Room.Availability.Available
        };

        // Act
        var result = await controller.Create(room);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Table", redirect.ActionName);

        await roomRepository.Received(1).Create(
            Arg.Is<Room>(savedRoom =>
                savedRoom.Location == "P35" &&
                savedRoom.RoomNumber == "101" &&
                savedRoom.RoomType == "Group Study Room" &&
                savedRoom.Capacity == 6 &&
                savedRoom.Status == Room.Availability.Available));
    }

    // Tests that a valid room update is saved and redirects to the room table.
    // Arrange: configure the repository substitute to successfully update the room.
    // Act: submit the updated room through the controller.
    // Assert: verify the updated details were sent to the repository and the action redirects to Table.
    [Fact]
    public async Task Update_WithValidRoom_UpdatesRoomAndRedirectsToTable()
    {
        // Arrange
        var roomRepository = Substitute.For<IRoomRepository>();
        roomRepository.Update(Arg.Any<Room>()).Returns(true);

        var logger = Substitute.For<ILogger<RoomController>>();
        var controller = new RoomController(roomRepository, logger);

        var room = new Room
        {
            RoomId = 1,
            Location = "P35",
            RoomNumber = "102",
            RoomType = "Group Study Room",
            Capacity = 8,
            Status = Room.Availability.Available
        };

        // Act
        var result = await controller.Update(room);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Table", redirect.ActionName);

        await roomRepository.Received(1).Update(
            Arg.Is<Room>(updatedRoom =>
                updatedRoom.RoomId == 1 &&
                updatedRoom.Location == "P35" &&
                updatedRoom.RoomNumber == "102" &&
                updatedRoom.RoomType == "Group Study Room" &&
                updatedRoom.Capacity == 8 &&
                updatedRoom.Status == Room.Availability.Available));
    }
}