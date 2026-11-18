import type { MyPageData } from '../types/myPage'
import { handleResponse, safeFetch } from '../shared/handleResponse'
import type { CurrentUser } from '../types/auth';

const API_URL = import.meta.env.VITE_API_URL;

export async function fetchMyPage(): Promise<MyPageData> {
    const response = await safeFetch(`${API_URL}/api/MyPage`, { credentials: 'include'})
    return handleResponse<MyPageData>(response, 'Could not load My Page')
}

export async function fetchCurrentUser(): Promise<CurrentUser | null> {
    const response = await safeFetch(`${API_URL}/api/Account/Me`, { credentials: 'include' })
    if (response.status === 401) return null
    return handleResponse<CurrentUser>(response, 'Could not check sign-in status.')
}