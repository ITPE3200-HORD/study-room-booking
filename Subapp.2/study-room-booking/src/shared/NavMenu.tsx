//import React from 'react';
import { Nav, Navbar, NavDropdown } from 'react-bootstrap';

export default function NavMenu() {
    return (
        <Navbar expand="lg">
            <Navbar.Brand href="/">Study Room Booking</Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="me-auto">
                    <Nav.Link href="/">Home</Nav.Link>
                    <Nav.Link href="/rooms">Rooms</Nav.Link>
                    <Nav.Link href="/students">Students</Nav.Link>
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