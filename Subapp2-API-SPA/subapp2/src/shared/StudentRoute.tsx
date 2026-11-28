// shared/StudentRoute.tsx
import { useQuery } from '@tanstack/react-query'
import { Navigate, useLocation } from 'react-router-dom'
import type { ReactNode } from 'react'
import { fetchCurrentUser } from '../account/AccountService'

export default function StudentRoute({ children }: { children: ReactNode }) {
    const location = useLocation()
    const { data: user, isLoading } = useQuery({
        queryKey: ['current-user'],
        queryFn: fetchCurrentUser,
        retry: false,
    })

    if (isLoading) return <p role="status">Checking access…</p>
  
    if (!user) return <Navigate to="/login" state={{ from: location }} replace />

    if (user.isAdmin) return <Navigate to="/" replace />

    return children
}