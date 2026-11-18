import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Card, Col, Row, Spinner } from 'react-bootstrap'
import { fetchCurrentUser } from '../shared/auth'
import * as HomeService from './HomeService'

export default function HomePage() {
    const { data: user,
        isLoading: isUserLoading
    } = useQuery({
        queryKey: ['current-user'],
        queryFn: fetchCurrentUser,
        retry: false,
    })

    const {
        data,
        isLoading,
        error,
    } = useQuery({
        queryKey: ['home'],
        queryFn: HomeService.fetchHomeData,
    })

    const roomLink = !user
        ? { to: '/login', label: 'Log in' }
        : user.isAdmin
            ? { to: '/rooms', label: 'Manage rooms' }
            : { to: '/booking', label: 'Browse rooms' }
    const rooms = data?.rooms ?? []
    const sessions = data?.openStudySessions ?? []
    const featuredRoom = rooms.find(rooms => rooms.status === 0)

    if (error) {
        return (
            <main className="home-page">
                <div className="empty-state" role="alert">
                    <h2>Could not load the home page</h2>
                    <p>Please try again later.</p>
                </div>
            </main>
        )
    }

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
                    <h1>Find a study room for your group</h1>
                    <p className="home-hero-description">
                        Browse campus rooms and find open study sessions.
                    </p>

                    {!isUserLoading && (
                        <Link to={roomLink.to} className="button-primary">
                            {roomLink.label}
                        </Link>
                    )}

                {featuredRoom && (
                    <p className="featured-room">
                        <span className="availability-dot" aria-hidden="true" />
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
                    {!isUserLoading && (
                        <Link to={roomLink.to} className="hero-card-link">
                            Explore rooms <span aria-hidden="true">→</span>
                        </Link>
                    )}
                </div>
            </section>

            <section id="rooms" className="home-section">
                <header className="section-heading">
                    <div>
                        <p className="eyebrow">Find a space</p>
                        <h2>Rooms on campus</h2>
                        <p>{rooms.length} rooms in the system.</p>
                    </div>
                    {!isUserLoading && (
                        <Link to={roomLink.to} className="text-link">
                            Book a room
                        </Link>
                    )}
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
                        <p>Join a session and study with other students.</p>
                    </div>
                    {!isUserLoading && (
                        <Link to="/session" className="text-link">
                            Join a open session
                        </Link>
                    )}
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