export type StudySession = {
    studySessionId: number
    title: string
    subject: string | null
    maxParticipants: number
    participantCount: number
    isOpen: boolean
    bookingId: number
    roomNumber: string
    startTime: string
    endTime: string
    ownerUserName: string
    isOwner: boolean
    hasJoined: boolean
}

export type StudySessionCreateDto = { bookingId: number; title: string; subject: string; maxParticipants: number }
export type StudySessionUpdateDto = { title: string; subject: string; maxParticipants: number; isOpen: boolean }
export type SessionFilter = { subject: string; date: string }
