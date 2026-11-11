import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Alert, Button, Card, Col, Row, Spinner } from 'react-bootstrap'

const API_URL = import.meta.env.VITE_API_URL;

type Room = {
    roomId: number
    location: string | null
    roomNumber: string
    roomType: string
    capacity: number
    status: 0 | 1
}

type StudySession = {
    studySessionId: number
    title: string
    subject: string | null
    maxParticipants: number
    participantCount: number
    roomId: number
    roomNumber: string 
    startTime: string
    endTime: string
}

type HomeData = {
    rooms: Room[]
    openStudySessions: StudySession[]
}

async function fetchHomeData(): Promise<HomeData> {
    const response = await fetch(`${API_URL}/api/Home`)

    if(!response.ok) {
        throw new Error(`Could not load home page (${response.status})`)
    }

    return response.json() as Promise<HomeData>
}

export default function HomePage() {
    const { data, isLoading, error } = useQuery({
        queryKey: ['home'],
        queryFn: fetchHomeData,
    })

    const rooms = data?.rooms ?? []
    const sessions = data?.openStudySessions ?? []
    const featuredRoom = rooms.find(rooms => rooms.status == 0)

    if (isLoading) {
        return (
        <div className="text-center py-5" role="status">
            <Spinner animation="border" />
            <p className="mt-3">Loading rooms and study sessions...</p>
        </div>
        )
    }

    return (
        <main>
            <section className="bg-light rounded p-4 p-md-5 md-5">
                <p className="text-uppercase text-secondary mb-2">
                    Book rooms and study together
                </p>
                <h1>Find a study rooms for your group</h1>
                <p className="lead">Browse campus rooms and find open study sessions.</p>

                <Button className="btn btn-primary mb-3" href="/rooms">Browse rooms</Button>

                {featuredRoom && (
                    <p className="mt-3 mb-0">Available now: Room {featuredRoom.roomNumber}</p>
                )}
            </section>

            <section id="rooms" className="mb-5">
                <h2>Rooms on campus</h2>
                <p>{rooms.length} rooms in the system.</p>

                {rooms.length === 0 ? (
                    <p>No rooms are available to show.</p>
                ) : (
                    <Row className="g-3 mt-1">
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
                                            Capacity: {room.capacity}
                                            <br />
                                            Status: {room.status === 0
                                                ? 'Available'
                                                : 'Not available'}
                                        </Card.Text>
                                    </Card.Body>
                                </Card>
                            </Col>
                        ))}
                    </Row>
                )}
            </section>

            <section id="sessions" className="mb-5">
                <h2>Open study sessions</h2>

                {sessions.length === 0 ? (
                    <p>Np open study sessions right now.</p>
                ) : (
                    <Row className="g-3 mt-1">
                        {sessions.map(session => (
                            <Col key={session.studySessionId} xs={12} md={6}>
                                <Card className="h-100">
                                    <Card.Body>
                                        <Card.Title>{session.title}</Card.Title>
                                        <Card.Text>
                                            Subject: {session.subject || '—'}
                                            <br />
                                            Room: {session.roomNumber || '—'}
                                            <br />
                                            Starts: {new Date(
                                                session.startTime
                                            ).toLocaleString('en-GB')}
                                            <br />
                                            Places left: {
                                                session.maxParticipants -
                                                session.participantCount
                                            }
                                        </Card.Text>
                                    </Card.Body>
                                </Card>
                            </Col>
                        ))}
                    </Row>
                )}
            </section>
        </main>
    )    
}