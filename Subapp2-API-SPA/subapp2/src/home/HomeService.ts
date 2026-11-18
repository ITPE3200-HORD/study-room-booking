import type { HomeData } from '../types/home'
import { handleResponse } from '../shared/handleResponse'

const API_URL = import.meta.env.VITE_API_URL;

export async function fetchHomeData(): Promise<HomeData> {
    const response = await fetch(`${API_URL}/api/Home`)

    return handleResponse<HomeData>(response, 'Could not load the home page.')
}