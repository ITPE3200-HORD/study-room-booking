export type MyBooking = {
    bookingId: number
    roomId: number
    roomNumber: string | null
    startTime: string
    endTime: string
    description: string | null
}

export type MyStudySession = {
    studySessionId: number
    title: string
    subject: string | null
    isOpen: boolean
    maxParticipants: number
    participantCount: number
    bookingId: number
    roomId: number
    roomNumber: string | null
    startTime: string
    endTime: string
}

export type MyPageData = {
    hasStudentProfile: boolean
    bookings: MyBooking[]
    studySessions: MyStudySession[]
}