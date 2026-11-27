import { useState, type ChangeEvent, type SubmitEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Card, Col, Form, Modal, Row, Spinner } from 'react-bootstrap'
import type { AvailableRoom, SearchForm, SearchParams } from '../types/booking'
import * as BookingService from './BookingService'

function toLocalDate(date: Date): string {
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    return `${year}-${month}-${day}`
}

const tomorrow = new Date()
tomorrow.setDate(tomorrow.getDate() + 1)

const initialForm: SearchForm = {
    date: toLocalDate(tomorrow),
    startTime: '14:00:00',
    durationMinutes: '120',
    groupSize: '4',
    roomType: '',
}

type BookingDetails = {
    description: string
    createStudySession: boolean
    sessionTitle: string
    sessionSubject: string
    sessionMaxParticipants: string
}

const emptyDetails: BookingDetails = {
    description: '',
    createStudySession: false,
    sessionTitle: '',
    sessionSubject: '',
    sessionMaxParticipants: '2',
}

export default function BookingPage() {
    const queryClient = useQueryClient()
    const [form, setForm] = useState<SearchForm>(initialForm)
    const [search, setSearch] = useState<SearchParams | null>(null)
    const [selectedRoom, setSelectedRoom] = useState<AvailableRoom | null>(null)
    const [details, setDetails] = useState<BookingDetails>(emptyDetails)

    const { data, isFetching, error } = useQuery({
        queryKey: ['availableRooms', search],
        queryFn: () => {
            if (search === null) {
                throw new Error('Search criteria are missing.')
            }
            return BookingService.searchAvailableRooms(search)
        },
        enabled: search !== null,
    })

    const bookingMutation = useMutation({
        mutationFn: ({ roomId, searchParams, details }: { roomId: number; searchParams: SearchParams; details: BookingDetails }) => {
            const start = new Date(`${searchParams.date}T${searchParams.startTime}`)
            const end = new Date(start.getTime() + Number(searchParams.durationMinutes) * 60_000)
            const toLocalDateTime = (value: Date) =>
                `${toLocalDate(value)}T${String(value.getHours()).padStart(2, '0')}:${String(value.getMinutes()).padStart(2, '0')}:00`
            return BookingService.createBooking({
                roomId,
                startTime: toLocalDateTime(start),
                endTime: toLocalDateTime(end),
                description: details.description.trim(),
                createStudySession: details.createStudySession,
                sessionTitle: details.createStudySession ? details.sessionTitle.trim() : undefined,
                sessionSubject: details.createStudySession ? details.sessionSubject.trim() : undefined,
                sessionMaxParticipants: details.createStudySession ? Number(details.sessionMaxParticipants) : undefined,
            })
        },
        onSuccess: async () => {
            setSelectedRoom(null)
            // A new session also appears on the sessions page and the home page
            await Promise.all([
                queryClient.invalidateQueries({ queryKey: ['myPage'] }),
                queryClient.invalidateQueries({ queryKey: ['availableRooms'] }),
                queryClient.invalidateQueries({ queryKey: ['sessions'] }),
                queryClient.invalidateQueries({ queryKey: ['home'] }),
            ])
        },
    })

    function handleChange(
        event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
    ) {
        const { name, value } = event.target
        setForm(current => ({ ...current, [name]: value }))
    }

    function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        bookingMutation.reset()
        setSearch({ ...form, requestId: Date.now() })
    }

    // Opens the confirmation modal for the chosen room
    function openBooking(room: AvailableRoom) {
        if (!search) return
        bookingMutation.reset()
        // Suggest the group size as max participants, but never more than the room has seats
        const suggested = Math.min(Math.max(Number(search.groupSize), 2), room.capacity, 50)
        setDetails({ ...emptyDetails, sessionMaxParticipants: String(suggested) })
        setSelectedRoom(room)
    }

    function handleDetailsChange(event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) {
        const { name, value, type, checked } = event.target as HTMLInputElement
        setDetails(current => ({ ...current, [name]: type === 'checkbox' ? checked : value }))
    }

    const rooms = data?.availableRooms ?? []

    return (
        <main className="booking-page">
            <header className="booking-heading">
                <p className="eyebrow">Find a space</p>
                <h1>Find a room</h1>
                <p>Choose when you need a room and how many people are in your group.</p>
            </header>

            <Form onSubmit={handleSubmit} className="room-search-panel">
                <Row className="g-3">
                    <Col xs={12} sm={6} lg={3}>
                        <Form.Label htmlFor="date">Date</Form.Label>
                        <Form.Control
                            id="date"
                            name="date"
                            type="date"
                            min={toLocalDate(new Date())}
                            value={form.date}
                            onChange={handleChange}
                            required
                        />
                    </Col>

                    <Col xs={12} sm={6} lg={3}>
                        <Form.Label htmlFor="startTime">Start time</Form.Label>
                        <Form.Select
                            id="startTime"
                            name="startTime"
                            value={form.startTime}
                            onChange={handleChange}
                        >
                            {Array.from({ length: 17 }, (_, index) => index + 6).map(hour => {
                                const time = `${String(hour).padStart(2, '0')}:00:00`
                                return (
                                    <option key={time} value={time}>
                                        {time.slice(0, 5)}
                                    </option>
                                )
                            })}
                        </Form.Select>
                    </Col>

                    <Col xs={12} sm={6} lg={2}>
                        <Form.Label htmlFor="durationMinutes">Duration</Form.Label>
                        <Form.Select
                            id="durationMinutes"
                            name="durationMinutes"
                            value={form.durationMinutes}
                            onChange={handleChange}
                        >
                            <option value="60">1 hour</option>
                            <option value="90">1.5 hours</option>
                            <option value="120">2 hours</option>
                            <option value="180">3 hours</option>
                        </Form.Select>
                    </Col>

                    <Col xs={12} sm={6} lg={2}>
                        <Form.Label htmlFor="groupSize">Group size</Form.Label>
                        <Form.Select
                            id="groupSize"
                            name="groupSize"
                            value={form.groupSize}
                            onChange={handleChange}
                        >
                            {[1, 2, 4, 6, 8, 10, 15, 20, 25].map(size => (
                                <option key={size} value={size}>
                                    {size} {size === 1 ? 'person' : 'people'}
                                </option>
                            ))}
                        </Form.Select>
                    </Col>

                    <Col xs={12} sm={6} lg={2}>
                        <Form.Label htmlFor="roomType">Room type</Form.Label>
                        <Form.Select
                            id="roomType"
                            name="roomType"
                            value={form.roomType}
                            onChange={handleChange}
                        >
                            <option value="">All types</option>
                            <option value="Group Study Room">Group Study Room</option>
                            <option value="Class Room">Class Room</option>
                            <option value="Auditorium">Auditorium</option>
                        </Form.Select>
                    </Col>

                    <Col xs={12}>
                        <Button type="submit" disabled={isFetching}>
                            {isFetching ? 'Searching…' : 'Search available rooms'}
                        </Button>
                    </Col>
                </Row>
            </Form>

            {bookingMutation.isSuccess && (
                <div className="booking-success" role="status">
                    {bookingMutation.variables?.details.createStudySession
                        ? 'Room booked and opened as a study session. You can see it under My Page.'
                        : 'Room booked successfully. You can see it under My Page.'}
                </div>
            )}
            {bookingMutation.error && !selectedRoom && (
                <Alert className="search-error" variant="danger" role="alert">
                    {bookingMutation.error.message}
                </Alert>
            )}

            {error && (
                <Alert className="search-error" variant="danger" role="alert">
                    {error.message}
                </Alert>
            )}

            {isFetching && (
                <div className="booking-page" role="status">
                    <Spinner size="sm" animation="border" className="me-2" />
                    Searching for available rooms…
                </div>
            )}

            {data && !isFetching && (
                <section className="room-results" aria-live="polite">
                    <header className="room-results-heading">
                        <div>
                            <p className="eyebrow">Your search</p>
                            <h2>Available rooms</h2>
                            <p>{data.availableRoomCount} rooms match your search.</p>
                        </div>
                    </header>

                    {rooms.length === 0 ? (
                        <div className="empty-state">
                            <h3>No rooms found for these details</h3>
                            <p>Try another time, group size, or room type.</p>
                        </div>
                    ) : (
                        <Row className="g-3">
                            {rooms.map(room => (
                                <Col key={room.roomId} xs={12} md={6} lg={4}>
                                    <Card className="info-card h-100">
                                        <Card.Body>
                                            <span className="card-label">{room.roomType}</span>
                                            <Card.Title>Room {room.roomNumber}</Card.Title>
                                            <Card.Text>
                                                <span>{room.location || 'Location not listed'}</span>
                                                <span>Capacity: {room.capacity} people</span>
                                            </Card.Text>
                                        </Card.Body>
                                        <Card.Footer className="booking-card-footer">
                                            <Button
                                                disabled={!search}
                                                onClick={() => openBooking(room)}
                                            >
                                                Book this room
                                            </Button>
                                        </Card.Footer>
                                    </Card>
                                </Col>
                            ))}
                        </Row>
                    )}
                </section>
            )}
            <Modal show={selectedRoom != null} onHide={() => setSelectedRoom(null)}>
                <Form onSubmit={event => {
                    event.preventDefault()
                    if (selectedRoom && search) {
                        bookingMutation.mutate({ roomId: selectedRoom.roomId, searchParams: search, details })
                    }
                }}>
                    <Modal.Header closeButton>
                        <Modal.Title>Book room {selectedRoom?.roomNumber}</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        {search && (
                            <p>
                                {search.date}, {search.startTime.slice(0, 5)}, {Number(search.durationMinutes) / 60} hours</p>
                        )}
                        {bookingMutation.error && (
                            <Alert variant="danger">
                                {bookingMutation.error.message}</Alert>
                        )}

                        <Form.Group className="mb-3" controlId="description">
                            <Form.Label>Description (optional)</Form.Label>
                            <Form.Control as="textarea" rows={2} maxLength={200} name="description" value={details.description} onChange={handleDetailsChange} />
                        </Form.Group>
                        <Form.Check className="mb-3" type="switch" id="createStudySession"
                            name="createStudySession" label="Open this booking as a study session"
                            checked={details.createStudySession} onChange={handleDetailsChange} />
                        {details.createStudySession && (
                            <>
                                <Form.Group className="mb-3" controlId="sessionTitle">
                                    <Form.Label>Title</Form.Label>
                                    <Form.Control required maxLength={100} name="sessionTitle"
                                        value={details.sessionTitle} onChange={handleDetailsChange} />
                                </Form.Group>
                                <Form.Group className="mb-3" controlId="sessionSubject">
                                    <Form.Label>Subject (optional)</Form.Label>
                                    <Form.Control maxLength={100} name="sessionSubject"
                                        value={details.sessionSubject} onChange={handleDetailsChange} />
                                </Form.Group>
                                <Form.Group controlId="sessionMaxParticipants">
                                    <Form.Label>Max participants (including you)</Form.Label>
                                    <Form.Control type="number" required min={2}
                                        max={Math.min(selectedRoom?.capacity ?? 50, 50)}
                                        name="sessionMaxParticipants"
                                        value={details.sessionMaxParticipants} onChange={handleDetailsChange} />
                                    <Form.Text>The room has {selectedRoom?.capacity} seats.</Form.Text>
                                </Form.Group>
                            </>
                        )}

                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="secondary" onClick={() => setSelectedRoom(null)}>Cancel</Button>
                        <Button type="submit" disabled={bookingMutation.isPending}>
                            {bookingMutation.isPending ? 'Booking…' : 'Confirm booking'}
                        </Button>
                    </Modal.Footer>
                </Form>
            </Modal>
        </main>
    )
}
