import type { Room, RoomForm } from '../types/room'
import { handleResponse } from '../shared/handleResponse'

const API_URL = import.meta.env.VITE_API_URL

export async function fetchRooms(): Promise<Room[]> {
    const response = await fetch(`${API_URL}/api/RoomAPI`, { credentials: 'include' })
    return handleResponse<Room[]>(response, 'Could not load rooms.')
}

export async function createRoom(room: RoomForm): Promise<void> {
    const response = await fetch(`${API_URL}/api/RoomAPI`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(room),
    })
    return handleResponse<void>(response, 'Could not save room. Check your details and permissions.')
}

export async function updateRoom(room: Room): Promise<void> {
    const response = await fetch(`${API_URL}/api/RoomAPI/${room.roomId}`, {
            method: 'PUT',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(room),
    })
    return handleResponse<void>(response, 'Could not update room. Check your details and permissions.')
}

export async function deleteRoom(roomId: number): Promise<void> {
    const response = await fetch(`${API_URL}/api/RoomAPI/${roomId}`, {
        method: 'DELETE',
        credentials: 'include',
    })
    return handleResponse<void>(response, 'Could not delete room. It may have existing bookings.')
}