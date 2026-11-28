import { Alert, Nav, Navbar, NavDropdown } from 'react-bootstrap';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { fetchCurrentUser, logout } from '../account/AccountService';

export default function NavMenu() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [logoutError, setLogoutError] = useState('')
    const [isLoggingOut, setIsLoggingOut] = useState(false)
    const { data: user } = useQuery({
        queryKey: ['current-user'],
        queryFn: fetchCurrentUser,
        retry: false,
    })

    async function handleLogout() {
        setLogoutError('')
        setIsLoggingOut(true)

        try {
            await logout()
            // Removes all cached data from the previous user (My Page, sessions, bookings)
            queryClient.clear()
            queryClient.setQueryData(['current-user'], null)
            navigate('/', { replace: true })
        }
        catch (error) {
            setLogoutError(error instanceof Error ? error.message : 'Could not log out.')
        }
        finally {
            setIsLoggingOut(false)
        }
    }

    const isAdmin = user?.isAdmin === true
    const isStudent = user !== null && user !== undefined && !isAdmin

    return (
        <Navbar expand="lg" className="site-navbar" collapseOnSelect>
            <Navbar.Brand as={Link} to="/">Study Room Booking</Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="me-auto">
                    {isStudent && (
                        <>
                            <Nav.Link as={Link} to="/booking" eventKey="/booking">Book a room</Nav.Link>
                            <Nav.Link as={Link} to="/sessions" eventKey="/sessions">Join a session</Nav.Link>
                        </>
                    )}

                    {isAdmin && (
                        <>
                            <Nav.Link as={Link} to="/rooms" eventKey="/rooms">Manage rooms</Nav.Link>
                            <Nav.Link as={Link} to="/bookings" eventKey="/bookings">Manage bookings</Nav.Link>
                            <Nav.Link as={Link} to="/students" eventKey="/students">Manage students</Nav.Link>
                            <Nav.Link as={Link} to="/sessions" eventKey="/sessions">Manage sessions</Nav.Link>
                        </>
                    )}
                </Nav>

                <Nav className="ms-auto navbar-right">
                    {!user ? (
                        <>
                            <Nav.Link as={Link} to="/login" eventKey="/login">Log in</Nav.Link>
                            <Nav.Link as={Link} to="/register" eventKey="/register">Register</Nav.Link>
                        </>
                    ) : (
                        <NavDropdown title="Account" id="account-dropdown" align="end">
                            {isStudent && (
                                <NavDropdown.Item as={Link} to="/my-page" eventKey="/my-page">My page</NavDropdown.Item>
                            )}
                            <NavDropdown.Item
                                as="button"
                                type="button"
                                disabled={isLoggingOut}
                                onClick={handleLogout}
                            >
                                {isLoggingOut ? 'Logging out…' : 'Log out'}
                            </NavDropdown.Item>
                            {logoutError && (
                                <Alert variant="danger" role="alert" className="m-2">
                                    {logoutError}
                                </Alert>
                            )}
                        </NavDropdown>
                    )}
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    );
};
