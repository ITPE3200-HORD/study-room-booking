import type { MyPageData } from '../types/myPage'
import { handleResponse } from '../shared/handleResponse'
import type { CurrentUser, LoginDto, RegisterDto } from '../types/auth'

const API_URL = import.meta.env.VITE_API_URL;

export async function fetchMyPage(): Promise<MyPageData> {
    const response = await fetch(`${API_URL}/api/MyPage`, { credentials: 'include'})
    return handleResponse<MyPageData>(response, 'Could not load My Page.')
}

export async function fetchCurrentUser(): Promise<CurrentUser | null> {
    const response = await fetch(`${API_URL}/api/Account/Me`, { credentials: 'include' })
    if (response.status === 401) return null
    return handleResponse<CurrentUser>(response, 'Could not check sign-in status.')
}

export async function login(account: LoginDto): Promise<void> {
    const response = await fetch(`${API_URL}/api/Account/Login`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(account),
    })
    return handleResponse<void>(response, 'Login failed.')
}

export async function register(account: RegisterDto): Promise<void> {
    const response = await fetch(`${API_URL}/api/Account/Register`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(account),
    })
    return handleResponse<void>(response, 'Failed to register.')
}

export async function logout(): Promise<void> {
    const response = await fetch(`${API_URL}/api/Account/Logout`, {
        method: 'POST',
        credentials: 'include',
    })
    return handleResponse<void>(response, 'Could not log out.')
}