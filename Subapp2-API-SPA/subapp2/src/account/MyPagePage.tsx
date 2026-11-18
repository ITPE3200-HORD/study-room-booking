import { useQuery } from '@tanstack/react-query'
import { Alert, Spinner, Table } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import type { MyPageData } from '../types/myPage'
import * as AccountService from './AccountService'
import { ApiError } from '../shared/handleResponse'

function formatDate(value: string): string {
    return new Date(value).toLocaleString('nb-NO')
}

function MyPagePage() {
    const { data, isLoading, error } = useQuery<MyPageData, ApiError>({
    queryKey: ['myPage'],
    queryFn: AccountService.fetchMyPage,
    })

    if (isLoading) {
        return (
            <div className="py-5 text-center" role="status">
                <Spinner animation="border" />
                <p className="mt-3">Loading your page...</p>
            </div>
        )
    }

    if (error instanceof ApiError && error.status === 401) {
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

