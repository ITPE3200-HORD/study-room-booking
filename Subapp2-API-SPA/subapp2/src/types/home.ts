export type HomeRoom = {
    roomId: number
    location: string | null
    roomNumber: string
    roomType: string
    capacity: number
    status: 0 | 1
}

export type StudySession = {
    studySessionId: number
    title: string
    subject: string | null
    maxParticipants: number
    participantCount: number
    roomId: number
    roomNumber: string 
    startTime: string
    endTime: string
}

export type HomeData = {
    rooms: HomeRoom[]
    openStudySessions: StudySession[]
}