import type { HomeRoom, HomeData, StudySession } from '../types/home'
import { handleResponse } from '../shared/handleResponse'

const API_URL = import.meta.env.VITE_API_URL;

export async function fetchHomeData(): Promise<HomeData> {
    const response = await fetch(`${API_URL}/api/Home`)

    if(!response.ok) {
        throw new Error(`Could not load home page (${response.status})`)
    }

    return response.json() as Promise<HomeData>
}