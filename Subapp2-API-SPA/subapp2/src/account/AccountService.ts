import type { MyPageData } from '../types/myPage'
import { handleResponse, safeFetch } from '../shared/handleResponse'
import type { CurrentUser, LoginDto, RegisterDto } from '../types/auth';

const API_URL = import.meta.env.VITE_API_URL;

// Gets the bookings and study sessions for the logged-in user
export async function fetchMyPage(): Promise<MyPageData> {
    const response = await safeFetch(`${API_URL}/api/MyPage`, { credentials: 'include'})
    return handleResponse<MyPageData>(response, 'Could not load My Page')
}

// Returns the logged-in user, or null if nobody is logged in
export async function fetchCurrentUser(): Promise<CurrentUser | null> {
    const response = await safeFetch(`${API_URL}/api/Account/Me`, { credentials: 'include' })
    if (response.status === 401) return null
    return handleResponse<CurrentUser>(response, 'Could not check sign-in status.')
}

// Logs the user in. Throws an error with a readable message if it fails
export async function login(credentials: LoginDto): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/Account/Login`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(credentials),
    })
    return handleResponse<void>(response, 'Login failed.')
}

// Creates a new account. Throws an error with a readable message if it fails
export async function register(account: RegisterDto): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/Account/Register`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(account),
    })
    return handleResponse<void>(response, 'Failed to register.')
}
