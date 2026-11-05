//import React from 'react';
import { Table } from 'react-bootstrap';

const RoomListPage = () => {
    //mock data for rooms
    const rooms = [
        {
            RoomId: 1,
            Location: 'P35',
            RoomNumber: '101',
            RoomType: 'Auditorium',
            Capacity: 50,
            Availability: true,
        },
        {
            RoomId: 2,
            Location: 'P32',
            RoomNumber: '202',
            RoomType: 'Group study room',
            Capacity: 4,
            Availability: false,
        }
    ];

    return (
        <div>
            <h1>Rooms</h1>
            <Table striped bordered hover>
                <thead>
                    <tr>
                        <th>Room ID</th>
                        <th>Location</th>
                        <th>Room Number</th>
                        <th>Room Type</th>
                        <th>Capacity</th>
                        <th>Availability</th>
                    </tr>
                </thead>
                <tbody>
                    {rooms.map((room) => (
                        <tr key={room.RoomId}>
                            <td>{room.RoomId}</td>
                            <td>{room.Location}</td>
                            <td>{room.RoomNumber}</td>
                            <td>{room.RoomType}</td>
                            <td>{room.Capacity}</td>
                            <td>{room.Availability ? 'Available' : 'Not Available'}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
};

export default RoomListPage;