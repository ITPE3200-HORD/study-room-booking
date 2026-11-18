import { useQuery } from '@tanstack/react-query'
import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { fetchCurrentUser } from '../account/AccountService'

export default function AdminRoute({ children }: { children: ReactNode }) {
    const { data: user, isLoading } = useQuery({
        queryKey: ['current-user'],
        queryFn: fetchCurrentUser,
        retry: false,
    })

    if (isLoading) return <p role="status">Checking access…</p>
    if (!user?.isAdmin) return <Navigate to="/" replace />

    return children
}