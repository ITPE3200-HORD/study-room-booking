import type { CurrentUser } from "../types/auth"

export async function fetchCurrentUser(): Promise<CurrentUser | null> {
    const response = await fetch(
        `${import.meta.env.VITE_API_URL}/api/Account/Me`,
        { credentials: 'include' },
    )

    if (response.status === 401) return null
    if (!response.ok) throw new Error('Could not check sign-in status.')

    return response.json() as Promise<CurrentUser>
}