import type { BookingCreateDto, SearchForm, SearchResult } from "../types/booking";
import { handleResponse } from "../shared/handleResponse";

const API_URL = import.meta.env.VITE_API_URL;

export async function searchAvailableRooms(search: SearchForm): Promise<SearchResult> {
    const params = new URLSearchParams({
        date: search.date, 
        startTime: search.startTime,
        durationMinutes: search.durationMinutes,
        groupSize: search.groupSize,
        roomType: search.roomType,
    })

    const response = await fetch(
        `${API_URL}/api/Home/available-rooms?${params}`
    )
    
    return handleResponse<SearchResult>(response, 'Room search failed.')
}

export async function createBooking(booking: BookingCreateDto): Promise<void> {
    const response = await fetch(`${API_URL}/api/Booking`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(booking),
    })
    return handleResponse<void>(response, 'Could not create the booking.')
}