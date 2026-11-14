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
        <main className="home-page">
            <section className="home-hero">
                <div className="home-hero-copy">
                    <p className="eyebrow">Study together</p>
                    <h1>Find a study rooms for your group</h1>
                    <p className="home-hero-description">
                        Browse campus rooms and find open study sessions.
                    </p>

                    <Link to="/rooms" className="button-primary">
                        Browse rooms
                    </Link>

                {featuredRoom && (
                    <p className="featured-room">
                    Available now: Room {featuredRoom.roomNumber}
                    </p>
                )}
                </div>

                <div className="home-hero-card">
                    <span className="hero-card-label">On campus</span>
                    <h2>Find your place to focus</h2>
                    <p>
                        Choose a room that works for your group and your study plan.
                    </p>
                    <Link to="/rooms" className="hero-card-link">
                        Explore rooms <span aria-hidden="true">→</span>
                    </Link>
                </div>
            </section>

            <section id="rooms" className="home-section">
                <header className="section-heading">
                    <div>
                        <p className="eyebrow">Find a space</p>
                        <h2>Rooms on campus</h2>
                        <p>{rooms.length} rooms in the system.</p>
                    </div>
                    <Link to="/rooms" className="text-link">
                        Browse all rooms
                    </Link>
                </header>

                {rooms.length === 0 ? (
                    <div className="empty-state">
                        <h3>No rooms to show</h3>
                        <p>Available rooms will appear here.</p>
                    </div>
                ) : (
                    <Row className="g-3">
                        {rooms.map(room => (
                            <Col key={room.roomId} xs={12} md={6} lg={4}>
                                <Card className="info-card h-100">
                                    <Card.Body>
                                        <div className="card-topline">
                                            <span className="card-label">
                                                {room.roomType}
                                            </span>
                                            <span className={`status-badge ${
                                                room.status === 0 ? 'is-available' : 'is-unavailable'
                                            }`}>
                                                {room.status === 0
                                                ? 'Available'
                                                : 'Unavailable'}
                                            </span>
                                        </div>

                                        <Card.Title>
                                            Room {room.roomNumber}
                                        </Card.Title>

                                        <Card.Text>
                                            <span>{room.location || 'Location not listed'}</span>
                                            <span>Capacity: {room.capacity}</span>
                                        </Card.Text>
                                    </Card.Body>
                                </Card>
                            </Col>
                        ))}
                    </Row>
                )}
            </section>

            <section id="sessions" className="home-section">
                <header className="section-heading">
                    <div>
                        <p className="eyebrow">Learn together</p>
                        <h2>Open study sessions</h2>
                        <p>Join a sessions and study with other students.</p>
                    </div>
                </header>

                {sessions.length === 0 ? (
                    <div className="empty-state">
                        <h3>No open study sessions right now. </h3>
                        <p>Check back later to see newly listed sessions.</p>
                    </div>
                ) : (
                    <Row className="g-3">
                        {sessions.map(session => (
                            <Col key={session.studySessionId} xs={12} md={6}>
                                <Card className="info-card session-card h-100">
                                    <Card.Body>
                                        <span className="card-label">
                                        {session.subject || 'Study session'}
                                        </span>
                                        <Card.Title>{session.title}</Card.Title>

                                        <Card.Text>
                                            <span>Room {session.roomNumber || '—'}</span>
                                            <span>
                                                Starts: {new Date(session.startTime)
                                                    .toLocaleString('en-GB')}
                                            </span>
                                            <span>
                                                Places left: {Math.max(
                                                    0,
                                                    session.maxParticipants -
                                                    session.participantCount
                                                )}
                                            </span>
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