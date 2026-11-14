//import React from 'react';
import { Nav, Navbar, NavDropdown } from 'react-bootstrap';
import { useQuery } from '@tanstack/react-query';

const API_URL = import.meta.env.VITE_API_URL;

export default function NavMenu() {
    const { data: isAdmin = false } = useQuery({
        queryKey: ['current-user-is-admin'],
        queryFn: async () => {
            const response = await fetch(`${API_URL}/api/Account/Me`, {
                credentials: 'include',
            });
            if (!response.ok) return false;
            const user = await response.json() as { isAdmin: boolean };
            return user.isAdmin;
        },
        retry: false,
    });

    return (
        <Navbar expand="lg" className="site-navbar">
            <Navbar.Brand href="/">Study Room Booking</Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="me-auto">
                    <Nav.Link href="/">Home</Nav.Link>
                    <Nav.Link href="/booking">Book a room</Nav.Link>
                    {isAdmin && <Nav.Link href="/rooms">Manage rooms</Nav.Link>}
                    {isAdmin && <Nav.Link href="/students">Students</Nav.Link>}
                    <NavDropdown title="More" id="basic-nav-dropdown">
                        <NavDropdown.Item href="/my-page">My Page</NavDropdown.Item>
                        <NavDropdown.Item href="/login">Log in</NavDropdown.Item>
                        <NavDropdown.Item href="#action/3.3">Something</NavDropdown.Item>
                        <NavDropdown.Divider />
                        <NavDropdown.Item href="#action/3.4">Log out</NavDropdown.Item>
                    </NavDropdown>
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    );
};
