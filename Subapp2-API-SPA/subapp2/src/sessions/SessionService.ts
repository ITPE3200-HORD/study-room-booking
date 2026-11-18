import { handleResponse, safeFetch } from '../shared/handleResponse';
import type { SessionFilter, StudySession, StudySessionCreateDto, StudySessionUpdateDto } from '../types/session';

const API_URL = import.meta.env.VITE_API_URL

// Open study session. Only filter with a value are sent to the API
export async function fetchSessions(filter: SessionFilter): Promise<StudySession[]> {
    const params = new URLSearchParams()
    if (filter.subject.trim()) params.set('subject', filter.subject.trim())
    if (filter.date) params.set('date', filter.date)

    // Credentials is needed to the API knows who you are (isOwner / hasJoined)
    const response = await safeFetch(`${API_URL}/api/StudySession?${params}`, { credentials: 'include' })
    return handleResponse<StudySession[]>(response, 'Could not load study sessions.')
}

export async function createSession(dto: StudySessionCreateDto): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/StudySession`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto),
    })
    return handleResponse<void>(response, 'Could not create the study session.')
}

export async function updateSession(id: number, dto: StudySessionUpdateDto): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/StudySession/${id}`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto),
    })
    return handleResponse<void>(response, 'Could not update the study session.')
}

export async function deleteSession(id: number): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/StudySession/${id}`, {
        method: 'DELETE',
        credentials: 'include'
    })
    return handleResponse<void>(response, 'Could not delete the study session.')
}

export async function joinSession(id: number): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/StudySession/${id}/join`, {
        method: 'POST',
        credentials: 'include',
    })
    return handleResponse<void>(response, 'Could not join the study session.')
}

export async function leaveSession(id: number): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/StudySession/${id}/join`, {
        method: 'DELETE',
        credentials: 'include',
    })
    return handleResponse<void>(response, 'Could not leave the study session.')
}
