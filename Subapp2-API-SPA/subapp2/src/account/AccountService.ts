import type { MyPageData } from '../types/myPage'
import { handleResponse } from '../shared/handleResponse'

const API_URL = import.meta.env.VITE_API_URL;

export async function fetchMyPage(): Promise<MyPageData> {
    const response = await fetch(`${API_URL}/api/MyPage`, { credentials: 'include'})
    return handleResponse<MyPageData>(response, 'Could not load My Page')
}