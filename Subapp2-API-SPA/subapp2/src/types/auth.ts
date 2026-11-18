export type CurrentUser = {
    username: string
    userId: string
    isAdmin: boolean
}


export interface LoginDto { username: string; password: string; rememberMe: boolean }
export interface RegisterDto { userName: string; email: string; fullName: string; password: string; confirmPassword: string }