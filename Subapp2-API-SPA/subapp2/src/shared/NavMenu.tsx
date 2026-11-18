import { Alert, Nav, Navbar, NavDropdown } from 'react-bootstrap';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchCurrentUser, logout } from '../account/AccountService';

export default function NavMenu() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [logoutError, setLogoutError] = useState('')
    const [isLoggingOut, setIsLoggingOut] = useState(false)
    const { data: user } = useQuery({
        queryKey: ['current-user'],
        queryFn: AccountService.fetchCurrentUser,
        retry: false,
    })

    async function handleLogout() {
        setLogoutError('')
        setIsLoggingOut(true)

        try {
            // The API call and error handling are done in AccountService
            await logout()
            queryClient.setQueryData(['current-user'], null)
            navigate('/ { replace: true }')
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
