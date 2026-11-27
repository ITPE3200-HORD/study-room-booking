import type { AdminBooking, BookingCreateDto, BookingUpdateDto, SearchForm, SearchResult } from '../types/booking';
import { handleResponse, safeFetch } from '../shared/handleResponse';

const API_URL = import.meta.env.VITE_API_URL;

export async function searchAvailableRooms(search: SearchForm): Promise<SearchResult> {
    const params = new URLSearchParams({
        date: search.date, 
        startTime: search.startTime,
        durationMinutes: search.durationMinutes,
        groupSize: search.groupSize,
        roomType: search.roomType,
    })

    const response = await safeFetch(
        `${API_URL}/api/Home/available-rooms?${params}`
    )

    return handleResponse<SearchResult>(response, 'Room search failed.')
}

export async function createBooking(booking: BookingCreateDto): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/Booking`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(booking),
    })
    return handleResponse<void>(response, 'Could not create the booking.')
}

export async function fetchActiveBookings(): Promise<AdminBooking[]> {
    const response = await safeFetch(`${API_URL}/api/Booking`, { credentials: 'include' })
    return handleResponse<AdminBooking[]>(response, 'Could not load bookings.')
}

export async function updateBooking(bookingId: number, booking: BookingUpdateDto): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/Booking/${bookingId}`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(booking),
    })
    return handleResponse<void>(response, 'Could not update the booking.')
}

export async function deleteBooking(bookingId: number): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/Booking/${bookingId}`, {
        method: 'DELETE',
        credentials: 'include',
    })
    return handleResponse<void>(response, 'Could not delete the booking.')
}