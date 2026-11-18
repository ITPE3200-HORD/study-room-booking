export type Room = {
    roomId: number
    location: string | null
    roomNumber: string
    roomType: string
    capacity: number
    status: number
}

export type RoomForm = Omit<Room, 'roomId'>