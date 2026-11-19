import { useState, type ChangeEvent, type SubmitEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from 'react-router-dom'
import { Alert, Button, Card, Col, Form, Row, Spinner } from "react-bootstrap";
import * as SessionService from './SessionService'
import { fetchCurrentUser } from "../account/AccountService";
import type { SessionFilter, StudySession } from "../types/session";

const emptyFilter: SessionFilter = { subject: '', date: '' }

function formatSessionTime(start: string, end: string): string {
    const startDate = new Date(start)
    const endDate = new Date(end)
    const day = startDate.toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric', month: 'short' })
    const time = (date: Date) => date.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
    return `${day}, ${time(startDate)}–${time(endDate)}`
}

export default function SessionsListPage() {
    const queryClient = useQueryClient()

    // form = what the user is typing, filter = what was searched for.
    // The search only runs when the user clicks "Search", not on every key press.
    const [form, setForm] = useState<SessionFilter>(emptyFilter)
    const [filter, setFilter] = useState<SessionFilter>(emptyFilter)
    const [actionError, setActionError] = useState('')

    const { data: user } = useQuery({
        queryKey: ['current-user'],
        queryFn: fetchCurrentUser,
        retry: false,
    })

    // The filter is part of the query key, so React Query fetches again when it changes
    const { data: sessions = [], isLoading, error } = useQuery({
        queryKey: ['sessions', filter],
        queryFn: () => SessionService.fetchSessions(filter),
    })

    // After join/leave/delete, the session lists on all pages are out of date
    async function refreshSessions() {
        setActionError('')
        await Promise.all([
            queryClient.invalidateQueries({ queryKey: ['sessions'] }),
            queryClient.invalidateQueries({ queryKey: ['myPage'] }),
            queryClient.invalidateQueries({ queryKey: ['home'] }),
        ])
    }

    // The API's message (e.g. "This study session is full.") is shown to the user
    function showError(err: Error) {
        setActionError(err.message)
    }

    const joinMutation = useMutation({
        mutationFn: SessionService.joinSession,
        onSuccess: refreshSessions,
        onError: showError,
    })
    const leaveMutation = useMutation({
        mutationFn: SessionService.leaveSession,
        onSuccess: refreshSessions,
        onError: showError,
    })
    const deleteMutation = useMutation({
        mutationFn: SessionService.deleteSession,
        onSuccess: refreshSessions,
        onError: showError,
    })

    const isBusy = joinMutation.isPending || leaveMutation.isPending || deleteMutation.isPending
    const hasFilter = filter.subject.trim() !== '' || filter.date !== ''

    function handleChange(event: ChangeEvent<HTMLInputElement>) {
        const { name, value } = event.target
        setForm(current => ({ ...current, [name]: value }))
    }

    function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        setActionError('')
        setFilter(form)
    }

    function clearFilter() {
        setForm(emptyFilter)
        setFilter(emptyFilter)
    }

    // Which buttons a session card shows depends on who is looking at it
    function renderActions(session: StudySession) {
        if (!user) {
            return (
                <Link to="/login" className="btn btn-outline-success btn-sm">
                    Log in to join
                </Link>
            )
        }

        const isFull = session.participantCount >= session.maxParticipants
        const canDelete = session.isOwner || user.isAdmin

        return (
            <div className="table-actions">
                {session.isOwner ? (
                    <span className="status-badge is-available">Your session</span>
                ) : session.hasJoined ? (
                    <Button
                        size="sm"
                        variant="outline-secondary"
                        disabled={isBusy}
                        onClick={() => leaveMutation.mutate(session.studySessionId)}
                    >
                        Leave
                    </Button>
                ) : !user.isAdmin && (
                    <Button
                        size="sm"
                        disabled={isBusy || isFull}
                        onClick={() => joinMutation.mutate(session.studySessionId)}
                    >
                        {isFull ? 'Full' : 'Join'}
                    </Button>
                )}

                {canDelete && (
                    <Button
                        size="sm"
                        variant="outline-danger"
                        disabled={isBusy}
                        onClick={() => {
                            if (window.confirm(`Delete the study session "${session.title}"?`)) {
                                deleteMutation.mutate(session.studySessionId)
                            }
                        }}
                    >
                        Delete
                    </Button>
                )}
            </div>
        )
    }
    return (
        <main className="page-content">
            <header className="page-heading">
                <p className="eyebrow">Learn together</p>
                <h1>Study sessions</h1>
                <p>Find an open study session and join other students.</p>
            </header>

            <Form onSubmit={handleSubmit} className="room-search-panel">
                <Row className="g-3 align-items-end">
                    <Col xs={12} md={5}>
                        <Form.Label htmlFor="subject">Subject or title</Form.Label>
                        <Form.Control
                            id="subject"
                            name="subject"
                            placeholder="e.g. Web applications"
                            maxLength={100}
                            value={form.subject}
                            onChange={handleChange}
                        />
                    </Col>
                    <Col xs={12} md={4}>
                        <Form.Label htmlFor="date">Date</Form.Label>
                        <Form.Control
                            id="date"
                            name="date"
                            type="date"
                            value={form.date}
                            onChange={handleChange}
                        />
                    </Col>
                    <Col xs={12} md={3} className="d-flex gap-2">
                        <Button type="submit">Search</Button>
                        {hasFilter && (
                            <Button variant="outline-secondary" onClick={clearFilter}>
                                Clear
                            </Button>
                        )}
                    </Col>
                </Row>
            </Form>

            {actionError && (
                <Alert variant="danger" role="alert" dismissible onClose={() => setActionError('')}>
                    {actionError}
                </Alert>
            )}

            {isLoading ? (
                <div className="text-center py-5" role="status">
                    <Spinner animation="border" />
                    <p className="mt-3">Loading study sessions…</p>
                </div>
            ) : error ? (
                <div className="search-error" role="alert">
                    {error.message}
                </div>
            ) : sessions.length === 0 ? (
                <div className="empty-state">
                    <h3>{hasFilter ? 'No sessions match your search' : 'No open study sessions right now'}</h3>
                    <p>
                        {hasFilter
                            ? 'Try another subject or date.'
                            : 'Book a room and open it as a study session from My page.'}
                    </p>
                </div>
            ) : (
                <Row className="g-3" aria-live="polite">
                    {sessions.map(session => {
                        const spotsLeft = Math.max(0, session.maxParticipants - session.participantCount)

                        return (
                            <Col key={session.studySessionId} xs={12} md={6} lg={4}>
                                <Card className="info-card h-100">
                                    <Card.Body>
                                        <div className="card-topline">
                                            <span className="card-label">
                                                {session.subject || 'Study session'}
                                            </span>
                                            <span className={`status-badge ${spotsLeft > 0 ? 'is-available' : 'is-unavailable'}`}>
                                                {spotsLeft > 0 ? `${spotsLeft} spots left` : 'Full'}
                                            </span>
                                        </div>

                                        <Card.Title>{session.title}</Card.Title>

                                        <Card.Text>
                                            <span>Room {session.roomNumber || '—'}</span>
                                            <span>{formatSessionTime(session.startTime, session.endTime)}</span>
                                            <span>
                                                {session.participantCount} of {session.maxParticipants} participants
                                            </span>
                                            <span>Hosted by {session.ownerUserName}</span>
                                        </Card.Text>
                                    </Card.Body>

                                    <Card.Footer className="booking-card-footer">
                                        {renderActions(session)}
                                    </Card.Footer>
                                </Card>
                            </Col>
                        )
                    })}
                </Row>
            )}
        </main>
    )
}