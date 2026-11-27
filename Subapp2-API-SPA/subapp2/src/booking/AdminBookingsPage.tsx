import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Form, Modal, Spinner, Table } from 'react-bootstrap'
import * as BookingService from './BookingService'
import type { AdminBooking } from '../types/booking'

// "2026-11-27T10:00:00" -> "2026-11-27T10:00", the format <input type="datetime-local"> expects
const toInputValue = (value: string) => value.slice(0, 16)

const formatDate = (value: string) =>
    new Date(value).toLocaleString('nb-NO', { dateStyle: 'short', timeStyle: 'short' })

export default function AdminBookingsPage() {
    const queryClient = useQueryClient()
    const [editing, setEditing] = useState<AdminBooking | null>(null)
    const [form, setForm] = useState({ startTime: '', endTime: '', description: '' })

    const { data: bookings, isLoading, error } = useQuery({
        queryKey: ['adminBookings'],
        queryFn: BookingService.fetchActiveBookings,
    })

    const refresh = () => queryClient.invalidateQueries({ queryKey: ['adminBookings'] })

    const updateMutation = useMutation({
        mutationFn: (booking: AdminBooking) =>
            BookingService.updateBooking(booking.bookingId, {
                roomId: booking.roomId,
                startTime: form.startTime,
                endTime: form.endTime,
                description: form.description,
            }),
        onSuccess: async () => {
            setEditing(null)
            await refresh()
        },
    })

    const deleteMutation = useMutation({
        mutationFn: BookingService.deleteBooking,
        onSuccess: refresh,
    })

    function openEdit(booking: AdminBooking) {
        updateMutation.reset()
        setForm({
            startTime: toInputValue(booking.startTime),
            endTime: toInputValue(booking.endTime),
            description: booking.description ?? '',
        })
        setEditing(booking)
    }

    function handleDelete(booking: AdminBooking) {
        const warning = booking.studySessionId !== null
            ? ' This also deletes the study session for this booking.'
            : ''
        if (window.confirm(`Delete booking for ${booking.studentName} in room ${booking.roomNumber}?${warning}`)) {
            deleteMutation.mutate(booking.bookingId)
        }
    }

    if (isLoading) return <Spinner animation="border" />
    if (error) return <Alert variant="danger">{error.message}</Alert>

    const now = new Date()

    return (
        <main>
            <h1>Manage bookings</h1>
            <p>All bookings that have not ended yet.</p>

            {deleteMutation.error && <Alert variant="danger">{deleteMutation.error.message}</Alert>}

            {bookings?.length === 0 ? (
                <p>There are no active bookings.</p>
            ) : (
                <Table striped hover responsive>
                    <thead>
                        <tr>
                            <th>Room</th>
                            <th>Student</th>
                            <th>Start</th>
                            <th>End</th>
                            <th>Description</th>
                            <th>Session</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {bookings?.map(booking => (
                            <tr key={booking.bookingId}>
                                <td>{booking.roomNumber ?? '—'}</td>
                                <td>{booking.studentName}</td>
                                <td>{formatDate(booking.startTime)}</td>
                                <td>{formatDate(booking.endTime)}</td>
                                <td>{booking.description || '—'}</td>
                                <td>{booking.studySessionId !== null ? 'Yes' : '—'}</td>
                                <td className="text-nowrap">
                                    {/* Bookings that have started cannot be edited (the API requires a future start time) */}
                                    {new Date(booking.startTime) > now && (
                                        <Button size="sm" variant="outline-success" className="me-2"
                                            onClick={() => openEdit(booking)}>
                                            Edit
                                        </Button>
                                    )}
                                    <Button size="sm" variant="outline-danger"
                                        disabled={deleteMutation.isPending}
                                        onClick={() => handleDelete(booking)}>
                                        Delete
                                    </Button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </Table>
            )}

            <Modal show={editing !== null} onHide={() => setEditing(null)}>
                <Form onSubmit={event => {
                    event.preventDefault()
                    if (editing) updateMutation.mutate(editing)
                }}>
                    <Modal.Header closeButton>
                        <Modal.Title>
                            Edit booking: room {editing?.roomNumber}, {editing?.studentName}
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        {updateMutation.error && <Alert variant="danger">{updateMutation.error.message}</Alert>}

                        <Form.Group className="mb-3" controlId="startTime">
                            <Form.Label>Start</Form.Label>
                            <Form.Control type="datetime-local" required value={form.startTime}
                                onChange={event => setForm({ ...form, startTime: event.target.value })} />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="endTime">
                            <Form.Label>End</Form.Label>
                            <Form.Control type="datetime-local" required value={form.endTime}
                                onChange={event => setForm({ ...form, endTime: event.target.value })} />
                        </Form.Group>
                        <Form.Group controlId="description">
                            <Form.Label>Description</Form.Label>
                            <Form.Control maxLength={200} value={form.description}
                                onChange={event => setForm({ ...form, description: event.target.value })} />
                        </Form.Group>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="secondary" onClick={() => setEditing(null)}>Cancel</Button>
                        <Button type="submit" disabled={updateMutation.isPending}>Save</Button>
                    </Modal.Footer>
                </Form>
            </Modal>
        </main>
    )
}