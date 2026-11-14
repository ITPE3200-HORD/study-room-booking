import { useState, type ChangeEvent, type SubmitEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Card, Col, Form, Row, Spinner } from 'react-bootstrap'

const API_URL = import.meta.env.VITE_API_URL;

type SearchFrom = {
    date: string
    startTime: string
    durationMinutes: string
    groupSize: string
    roomType: string
}

type SearchParams = SearchFrom & {
    requestId: number
}

type AvailableRoom = {
    roomId: number
    location: string | null
    roomNumber: string
    roomType: string
    capacity: number
}

type SearchResult = {
    availableRoomCount: number
    availableRooms: AvailableRoom[]
}

function toLocalDate(date: Date): string {
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2 ,'0')
    return `${year}-${month}-${day}`
}

const tomorrow = new Date()
tomorrow.setDate(tomorrow.getDate() + 1)

const initialForm: SearchFrom = {
    date: toLocalDate(tomorrow),
    startTime: '14:00:00',
    durationMinutes: '120',
    groupSize: '4',
    roomType: '',
}

async function searchAvailableRooms(search: SearchParams): Promise<SearchResult> {
    const params = new URLSearchParams({
        date: search.date, 
        startTime: search.startTime,
        durationMinutes: search.durationMinutes,
        groupSize: search.groupSize,
        roomType: search.roomType,
    })

    const response = await fetch(
        `${API_URL}/api/Home/available-rooms?${params}`
    )

    if (!response.ok) {
        throw new Error(`Room search failed (${response.status})`)
    }

    return response.json() as Promise<SearchResult>
}

export default function BookingPage() {
    const queryClient = useQueryClient()
    const [form, setForm] = useState<SearchFrom>(initialForm)
    const [search, setSearch] = useState<SearchParams | null>(null)

    const { data, isFetching, error } = useQuery({
        queryKey: ['availableRooms', search],
        queryFn: () => {
            if (search === null) {
                throw new Error('Search criteria are missing.')
            }
            return searchAvailableRooms(search)
        },
        enabled: search !== null,
    })

    const bookingMutation = useMutation({
        mutationFn: async ({ roomId, searchParams }: { roomId: number; searchParams: SearchParams }) => {
            const start = new Date(`${searchParams.date}T${searchParams.startTime}`)
            const end = new Date(start.getTime() + Number(searchParams.durationMinutes) * 60_000)
            const toLocalDateTime = (value: Date) =>
                `${toLocalDate(value)}T${String(value.getHours()).padStart(2, '0')}:${String(value.getMinutes()).padStart(2, '0')}:00`
            const response = await fetch(`${API_URL}/api/Booking`, {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    roomId,
                    startTime: toLocalDateTime(start),
                    endTime: toLocalDateTime(end),
                    description: '',
                }),
            })
            if (!response.ok) {
                const message = await response.text()
                throw new Error(message || 'Could not create the booking.')
            }
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['myPage'] })
            await queryClient.invalidateQueries({ queryKey: ['availableRooms'] })
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
                <div className="booking-success" role="status">Room booked successfully. You can see it under My Page.</div>
            )}
            {bookingMutation.error && (
                <Alert className="search-error" variant="danger" role="alert">
                    {bookingMutation.error.message}
                </Alert>
            )}

            {error && (
                <Alert className="search-error" variant="danger" role="alert">
                    Could not search for rooms. Check that the API is running.
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
                                                disabled={bookingMutation.isPending || !search}
                                                onClick={() => search && bookingMutation.mutate({ roomId: room.roomId, searchParams: search })}
                                            >
                                                {bookingMutation.isPending ? 'Booking…' : 'Book this room'}
                                            </Button>
                                        </Card.Footer>
                                    </Card>
                                </Col>
                            ))}
                        </Row>
                    )}
                </section>
            )}
        </main>
    )
}
