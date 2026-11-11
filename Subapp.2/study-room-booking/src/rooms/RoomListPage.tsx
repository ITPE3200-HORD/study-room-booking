import { useState, type ChangeEvent, type SubmitEvent } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Alert, Button, Card, Col, Form, Row, Spinner } from 'react-bootstrap'

const API_URL = import.meta.env.VITE_API_URL;

type searchFrom = {
    date: string
    startTime: string
    durationMinutes: string
    groupSize: string
    roomType: string
}

type SearchParams = searchFrom & {
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

const initialForm: searchFrom = {
    date: toLocalDate(tomorrow),
    startTime: '14:00:00',
    durationMinutes: '120',
    groupSize: '4',
    roomType: '',
}

async function SearchAvailableRooms(search: SearchParams): Promise<SearchResult> {
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

function RoomListPage() {
    const [form, setForm] = useState<searchFrom>(initialForm)
    const [search, setSearch] = useState<SearchParams | null>(null)

    const { data, isFetching, error } = useQuery({
        queryKey: ['availableRooms', search],
        queryFn: () => {
            if (search === null) {
                throw new Error('Search criteria are missing.')
            }
            return SearchAvailableRooms(search)
        },
        enabled: search !== null,
    })

    function handleChange(
        event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
    ) {
        const { name, value } = event.target
        setForm(current => ({ ...current, [name]: value }))
    }

    function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        setSearch({ ...form, requestId: Date.now() })
    }

    const rooms = data?.availableRooms ?? []

    return (
        <main>
            <header className="mb-4">
                <h1>Find a room</h1>
                <p>Choose when you need a room and how many people are in your group.</p>
            </header>

            <Form onSubmit={handleSubmit} className="bg-light rounded p-3 p-md-4 mb-4">
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

            {error && (
                <Alert variant="danger" role="alert">
                    Could not search for rooms. Check that the API is running.
                </Alert>
            )}

            {isFetching && (
                <div className="py-3" role="status">
                    <Spinner size="sm" animation="border" className="me-2" />
                    Searching for available rooms…
                </div>
            )}

            {data && !isFetching && (
                <section aria-live="polite">
                    <h2>Available rooms</h2>
                    <p>{data.availableRoomCount} rooms match your search.</p>

                    {rooms.length === 0 ? (
                        <p>Try another time, group size, or room type.</p>
                    ) : (
                        <Row className="g-3">
                            {rooms.map(room => (
                                <Col key={room.roomId} xs={12} md={6} lg={4}>
                                    <Card className="h-100">
                                        <Card.Body>
                                            <Card.Title>
                                                Room {room.roomNumber}
                                            </Card.Title>
                                            <Card.Text>
                                                Location: {room.location || '—'}
                                                <br />
                                                Type: {room.roomType}
                                                <br />
                                                Capacity: {room.capacity} people
                                            </Card.Text>
                                        </Card.Body>
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

export default RoomListPage;