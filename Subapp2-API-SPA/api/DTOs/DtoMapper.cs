using study_room_booking.Models;

namespace study_room_booking.DTOs;

//Converts entities to DTOs, so controllers never send entities to the client
public static class DtoMapper
{
    public static RoomDto ToDto(this Room room) => new()
    {
        RoomId = room.RoomId,
        Location = room.Location,
        RoomNumber = room.RoomNumber,
        RoomType = room.RoomType,
        Capacity = room.Capacity,
        Status = (RoomDto.Availability)room.Status
    };
    public static BookingDto ToDto(this Booking booking) => new()
    {
        BookingId = booking.BookingId,
        RoomId = booking.RoomId,
        RoomNumber = booking.Room?.RoomNumber,
        StudentId = booking.StudentId,
        StudySessionId = booking.StudySession?.StudySessionId,
        Description = booking.Description,
        StartTime = booking.StartTime,
        EndTime = booking.EndTime
    };
    public static StudySessionDto ToDto(this StudySession session, int? currentStudentId) => new()
    {
        StudySessionId = session.StudySessionId,
        Title = session.Title,
        Subject = session.Subject,
        MaxParticipants = session.MaxParticipants,
        ParticipantCount = session.Participants.Count,
        IsOpen = session.IsOpen,
        BookingId = session.BookingId,
        RoomId = session.Booking.RoomId,
        RoomNumber = session.Booking.Room?.RoomNumber ?? string.Empty,
        StartTime = session.Booking.StartTime,
        EndTime = session.Booking.EndTime,
        OwnerUserName = session.Booking.Student?.FullName ?? string.Empty,
        IsOwner = currentStudentId != null && session.Booking.StudentId == currentStudentId,
        HasJoined = currentStudentId != null &&
            session.Participants.Any(participant => participant.StudentId == currentStudentId)
    };
}