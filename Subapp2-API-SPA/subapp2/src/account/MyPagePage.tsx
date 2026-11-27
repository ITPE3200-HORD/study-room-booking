import { useState, type SubmitEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Form, Modal, Spinner, Table } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import type { MyPageData, MyStudySession } from '../types/myPage'
import * as AccountService from './AccountService'
import * as SessionService from '../sessions/SessionService'
import { ApiError } from '../shared/handleResponse'

// The modal is used both to create a new session for a booking and to edit an existing session
type SessionModal = 
    | { mode: 'create'; bookingId: number; roomNumber: string | null }
    | { mode: 'edit'; session: MyStudySession }

type SessionForm = {
    title: string
    subject: string
    maxParticipants: number
    isOpen: boolean
}

const emptySessionForm: SessionForm = { title: '', subject: '', maxParticipants: 4, isOpen: true }

function formatDate(value: string): string {
    return new Date(value).toLocaleString('nb-NO', {dateStyle: 'short', timeStyle: 'short'})
}

function MyPagePage() {
    const queryClient = useQueryClient()
    const [modal, setModal] = useState<SessionModal | null>(null)
    const [form, setForm] = useState<SessionForm>(emptySessionForm)
    const [modalError, setModalError] = useState('')
    const [actionError, setActionError] = useState('')

    const { data, isLoading, error } = useQuery<MyPageData, ApiError>({
    queryKey: ['myPage'],
    queryFn: AccountService.fetchMyPage,
    })

    // Sessions are shown on My page, the sessions page and the home page
    async function refreshSessions() {
        await Promise.all([
            queryClient.invalidateQueries({ queryKey: ['myPage'] }),
            queryClient.invalidateQueries({ queryKey: ['sessions'] }),
            queryClient.invalidateQueries({ queryKey: ['home'] }),
        ])
    }

    // Create or update, depending on which mode the modal is in
    const saveMutation = useMutation({
        mutationFn: ({ target, values }: { target: SessionModal; values: SessionForm }) =>
            target.mode === 'create'
                ? SessionService.createSession({
                    bookingId: target.bookingId,
                    title: values.title,
                    subject: values.subject,
                    maxParticipants: values.maxParticipants,
                })
                : SessionService.updateSession(target.session.studySessionId, values),
        onSuccess: async () => {
            await refreshSessions()
            setModal(null)
        },
        // The API's message (e.g. "Room 101 only has room for 6 people.") is shown in the modal
        onError: err => setModalError(err.message),
    })

    const deleteMutation = useMutation({
        mutationFn: SessionService.deleteSession,
        onSuccess: refreshSessions,
        onError: err => setActionError(err.message),
    })

    const leaveMutation = useMutation({
        mutationFn: SessionService.leaveSession,
        onSuccess: refreshSessions,
        onError: err => setActionError(err.message),
    })

    function openCreate(bookingId: number, roomNumber: string | null) {
        setModal({ mode: 'create', bookingId, roomNumber })
        setForm(emptySessionForm)
        setModalError('')
    }

    function openEdit(session: MyStudySession) {
        setModal({ mode: 'edit', session })
        setForm({
            title: session.title,
            subject: session.subject ?? '',
            maxParticipants: session.maxParticipants,
            isOpen: session.isOpen,
        })
        setModalError('')
    }

    function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        if (!modal) return
        setModalError('')
        saveMutation.mutate({ target: modal, values: form })
    }

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
    const now = new Date()
    const isBusy = deleteMutation.isPending || leaveMutation.isPending

    return (
        <main className="page-content">
            <header className="page-heading">
                <h1>My page</h1>
                <p>Here you can see your bookings and study sessions.</p>
            </header>

            {actionError && (
                <Alert variant="danger" dismissible onClose={() => setActionError('')}>
                    {actionError}
                </Alert>
            )}

            <section className="my-5">
                <h2>My bookings</h2>

                {bookings.length === 0 ? (
                    <p>You have no bookings. <Link to="/booking">Book a room</Link></p>
                ) : (
            <div className="content-card table-scroll">
                    <Table hover>
                        <thead>
                            <tr>
                                <th>Room</th>
                                <th>Description</th>
                                <th>From</th>
                                <th>To</th>
                                <th>Study session</th>
                            </tr>
                        </thead>
                        <tbody>
                            {bookings.map(booking => {
                                const isFuture = new Date(booking.startTime) > now

                                return (
                                <tr key={booking.bookingId}>
                                    <td>{booking.roomNumber ?? '—'}</td>
                                    <td>{booking.description || '—'}</td>
                                    <td>{formatDate(booking.startTime)}</td>
                                    <td>{formatDate(booking.endTime)}</td>
                                    <td>
                                        {booking.studySessionId !== null ? (
                                            <span className="status-badge is-available">Study session</span>
                                        ) : isFuture && data?.hasStudentProfile ? (
                                    <div className="table.actions">
                                                <Button
                                                    size="sm"
                                                    variant="outline-success"
                                                    onClick={() => openCreate(booking.bookingId, booking.roomNumber)}
                                                >
                                                    Open as study session
                                                </Button>
                                        </div>
                                        ) : (
                                            '-'
                                        )}
                                    </td>
                                </tr>
                                )
                            })}
                        </tbody>
                    </Table>
                </div>
                )}
            </section>

            <section className="my-5">
                <h2>My study sessions</h2>

                {!data?.hasStudentProfile ? (
                    <p>You do not have a student profile yet.</p>
                ) : sessions.length === 0 ? (
                    <p>You have no study sessions.</p>
                ) : (
            <div className="content-card table-scroll">
                    <Table hover>
                        <thead>
                            <tr>
                                <th>Session</th>
                                <th>Subject</th>
                                <th>Room</th>
                                <th>From</th>
                                <th>To</th>
                                <th>Participant</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sessions.map(session => (
                                <tr key={session.studySessionId}>
                                    <td>
                                        {session.title}
                                        {!session.isOpen && <span className="text-muted"> (closed)</span>}
                                    </td>
                                    <td>{session.subject || '—'}</td>
                                    <td>{session.roomNumber || '—'}</td>
                                    <td>{formatDate(session.startTime)}</td>
                                    <td>{formatDate(session.endTime)}</td>
                                    <td>{session.participantCount} / {session.maxParticipants}</td>
                                    <td>
                                        <div className="table-actions">
                                            {session.isOwner ? (
                                                <>
                                                    <Button size="sm" variant="outline-success" onClick={() => openEdit(session)}>
                                                        Edit
                                                    </Button>
                                                    <Button
                                                        size="sm"
                                                        variant="outline-danger"
                                                        disabled={isBusy}
                                                        onClick={() => {
                                                            if (window.confirm(`Delete the study session "${session.title}"? The participants will be removed.`)) {
                                                                setActionError('')
                                                                deleteMutation.mutate(session.studySessionId)
                                                            }
                                                        }}
                                                    >
                                                        Delete
                                                    </Button>
                                                </>
                                            ) : (
                                                <Button
                                                    size="sm"
                                                    variant="outline-secondary"
                                                    disabled={isBusy}
                                                    onClick={() => {
                                                        setActionError('')
                                                        leaveMutation.mutate(session.studySessionId)
                                                    }}
                                                >
                                                    Leave
                                                </Button>
                                            )}
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </div>
                )}
            </section>

            <Modal show={modal !== null} onHide={() => setModal(null)} centered>
                <Form onSubmit={handleSubmit}>
                    <Modal.Header closeButton>
                        <Modal.Title>
                            {modal?.mode === 'create'
                                ? `Open room ${modal.roomNumber ?? ''} as a study session`
                                : 'Edit study session'}
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        {modalError && <Alert variant="danger">{modalError}</Alert>}

                        <Form.Group className="mb-3" controlId="sessionTitle">
                            <Form.Label>Title</Form.Label>
                            <Form.Control
                                required
                                maxLength={100}
                                placeholder="e.g. Exam preparation"
                                value={form.title}
                                onChange={event => setForm({ ...form, title: event.target.value })}
                            />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="sessionSubject">
                            <Form.Label>Subject (optional)</Form.Label>
                            <Form.Control
                                maxLength={100}
                                placeholder="e.g. ITPE3200"
                                value={form.subject}
                                onChange={event => setForm({ ...form, subject: event.target.value })}
                            />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="sessionMaxParticipants">
                            <Form.Label>Maximum participants (including you)</Form.Label>
                            <Form.Control
                                type="number"
                                required
                                min={2}
                                max={50}
                                value={form.maxParticipants}
                                onChange={event => setForm({ ...form, maxParticipants: Number(event.target.value) })}
                            />
                        </Form.Group>

                        {modal?.mode === 'edit' && (
                            <Form.Check
                                id="sessionIsOpen"
                                type="switch"
                                label="Open for new participants"
                                checked={form.isOpen}
                                onChange={event => setForm({ ...form, isOpen: event.target.checked })}
                            />
                        )}
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="outline-secondary" onClick={() => setModal(null)}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={saveMutation.isPending}>
                            {saveMutation.isPending ? 'Saving…' : 'Save'}
                        </Button>
                    </Modal.Footer>
                </Form>
            </Modal>
        </main>
    )
}

export default MyPagePage

