import { useQuery } from '@tanstack/react-query'
import { Alert, Spinner, Table } from 'react-bootstrap'
import { Link } from 'react-router-dom'

const API_URL = import.meta.env.VITE_API_URL;

type MyBooking = {
    bookingId: number
    roomId: number
    roomNumber: string | null
    startTime: string
    endTime: string
    description: string | null
}

type MyStudySession = {
    studySessionId: number
    title: string
    subject: string | null
    isOpen: boolean
    maxParticipants: number
    participantCount: number
    bookingId: number
    roomId: number
    roomNumber: string | null
    startTime: string
    endTime: string
}

type MyPageData = {
    hasStudentProfile: boolean
    bookings: MyBooking[]
    studySessions: MyStudySession[]
}

type ApiError = Error & {
    status?: number
}

async function fetchMyPage(): Promise<MyPageData> {
    const response = await fetch(`${API_URL}/api/MyPage`, {
        credentials: 'include',
    })

    if (!response.ok) {
        const error = Object.assign(
            new Error(
            response.status === 401
                ? 'Log in to see you booking and study sessions.'
                : `Could not load My Page (${response.status}).`
            ),
            { status: response.status }
        )
        throw error
    }

    return response.json()
}

function formatDate(value: string): string {
    return new Date(value).toLocaleString('nb-NO')
}

function MyPagePage() {
    const { data, isLoading, error } = useQuery<MyPageData, ApiError>({
    queryKey: ['myPage'],
    queryFn: fetchMyPage,
    })

    if (isLoading) {
        return (
            <div className="py-5 text-center" role="status">
                <Spinner animation="border" />
                <p className="mt-3">Loading your page...</p>
            </div>
        )
    }

    if (error?.status === 401) {
        return (
            <Alert variant="warning">
                {error.message} <Link to="/login">Log in</Link>
            </Alert>
        )
    }

    if (error) {
        return <Alert variant="danger">{error.message}</Alert>
    }

    const bookings = data?.bookings ?? []
    const sessions = data?.studySessions ?? []

    return (
        <main>
            <h1>My page</h1>
            <p>Here you can see your bookings and study sessions.</p>

            <section className="my-5">
                <h2>My bookings</h2>

                {bookings.length === 0 ? (
                    <p>You have no bookings.</p>
                ) : (
                    <Table responsive striped hover>
                        <thead>
                            <tr>
                                <th>Room</th>
                                <th>Description</th>
                                <th>From</th>
                                <th>To</th>
                            </tr>
                        </thead>
                        <tbody>
                            {bookings.map(booking => (
                                <tr key={booking.bookingId}>
                                    <td>{booking.roomNumber ?? '—'}</td>
                                    <td>{booking.description || '—'}</td>
                                    <td>{formatDate(booking.startTime)}</td>
                                    <td>{formatDate(booking.endTime)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                )}
            </section>

            <section className="my-5">
                <h2>My study sessions</h2>

                {!data?.hasStudentProfile ? (
                    <p>You do not have a student profile yet.</p>
                ) : sessions.length === 0 ? (
                    <p>You have no study sessions.</p>
                ) : (
                    <Table responsive striped hover>
                        <thead>
                            <tr>
                                <th>Session</th>
                                <th>Subject</th>
                                <th>Room</th>
                                <th>From</th>
                                <th>To</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sessions.map(session => (
                                <tr key={session.studySessionId}>
                                    <td>{session.title}</td>
                                    <td>{session.subject || '—'}</td>
                                    <td>{session.roomNumber || '—'}</td>
                                    <td>{formatDate(session.startTime)}</td>
                                    <td>{formatDate(session.endTime)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                )}
            </section>
        </main>
    )
}

export default MyPagePage

