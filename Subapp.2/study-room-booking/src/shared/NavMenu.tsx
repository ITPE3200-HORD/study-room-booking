import { Nav, Navbar, NavDropdown } from 'react-bootstrap';
import { useQuery } from '@tanstack/react-query';
import { fetchCurrentUser } from './auth'

export default function NavMenu() {
    const { data: user } = useQuery({
        queryKey: ['current-user'],
        queryFn: fetchCurrentUser,
        retry: false,
    })

    const isAdmin = user?.isAdmin === true
    const isStudent = user !== null && user !== undefined && !isAdmin

    return (
        <Navbar expand="lg" className="site-navbar">
            <Navbar.Brand href="/">Study Room Booking</Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="me-auto">
                    {isStudent && (
                        <>
                            <Nav.Link href="/booking">Book a room</Nav.Link>
                            <Nav.Link href="/session">Join a session</Nav.Link>
                        </>
                    )}

                    {isAdmin && (
                        <>
                            <Nav.Link href="/rooms">Manage rooms</Nav.Link>
                            <Nav.Link href="/students">Manage students</Nav.Link>
                            <Nav.Link href="/sessions">Manage sessions</Nav.Link>
                        </>
                    )}
                </Nav>

                <Nav className="ms-auto navbar-right">
                    {!user ? (
                        <>
                            <Nav.Link href="/login">Log in</Nav.Link>
                            <Nav.Link href="/register">Register</Nav.Link>
                        </>
                    ) : (
                        <NavDropdown title="Account" id="account-dropdown" align="end">
                            {isStudent && (
                                <NavDropdown.Item href="/my-page">My page</NavDropdown.Item>
                            )}
                            <NavDropdown.Item href="/logout">Log out</NavDropdown.Item>
                        </NavDropdown>
                    )}
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    );
};
