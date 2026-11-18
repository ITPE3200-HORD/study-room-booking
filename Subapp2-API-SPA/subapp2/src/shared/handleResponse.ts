// Error with the HTTP status code, so pages can react to e.g. 401
export class ApiError extends Error {
    status: number
    constructor(message: string, status: number) {
        super(message)
        this.status = status
    }
}

export async function handleResponse<T>(response: Response, errorMessage: string): Promise<T> {
    const text = await response.text() // empty for 204 No content
    let data: unknown = undefined
    if (text) {
        try { data = JSON.parse(text) } catch { data = text } // some errors are plain text
    }

    if (!response.ok) {
        throw new ApiError(readErrorMessage(data) ?? errorMessage, response.status)
    }
    return data as T
}

function readErrorMessage(data: unknown): string | undefined {
    if (typeof data === 'string') return data || undefined
    if (typeof data !== 'object' || data === null) return undefined

    const body = data as { message?: string; detail?: string; title?: string; errors?: string[] | Record<string, string[]> }

    if (body.errors) {
        const messages = Array.isArray(body.errors) ? body.errors : Object.values(body.errors).flat()
        if (messages.length > 0) return messages.join(' ')
    }
    return body.detail || body.message || body.title
}