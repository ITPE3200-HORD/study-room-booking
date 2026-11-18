import type { MyPageData } from '../types/myPage'
import { handleResponse } from '../shared/handleResponse'
import { CurrentUser } from '../types/auth';
import StudentsListPage from '../students/StudentsListPage';

const API_URL = import.meta.env.VITE_API_URL;

export async function fetchMyPage(): Promise<MyPageData> {
    const response = await fetch(`${API_URL}/api/MyPage`, { credentials: 'include'})
    return handleResponse<MyPageData>(response, 'Could not load My Page')
}

export async function fetchCurrentUser(): Promise<CurrentUser | null> {
    const response = await fetch(
        `${import.meta.env.VITE_API_URL}/api/Account/Me`,
        { credentials: 'include' },
    )

    if (response.status === 401) return null
    if (!response.ok) throw new Error('Could not check sign-in status.')

    return response.json() as Promise<CurrentUser>
}