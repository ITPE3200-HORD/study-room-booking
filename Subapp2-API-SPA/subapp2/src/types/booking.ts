export type SearchForm = {
    date: string
    startTime: string
    durationMinutes: string
    groupSize: string
    roomType: string
}

export type SearchParams = SearchForm & {
    requestId: number
}

export type AvailableRoom = {
    roomId: number
    location: string | null
    roomNumber: string
    roomType: string
    capacity: number
}

export type SearchResult = {
    availableRoomCount: number
    availableRooms: AvailableRoom[]
}

export interface BookingCreateDto {
    roomId: number
    startTime: string
    endTime: string
    description: string
}

export type AdminBooking = {
    bookingId: number
    roomId: number
    roomNumber: string | null
    studentId: number
    studentName: string
    studySessionId: number | null
    description: string | null
    startTime: string
    endTime: string
}

export interface BookingUpdateDto {
    roomId: number
    startTime: string
    endTime: string
    description: string
}