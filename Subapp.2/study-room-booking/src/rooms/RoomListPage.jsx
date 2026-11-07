//import React from 'react';
import { useQuery } from '@tanstack/react-query'
import { Table } from 'react-bootstrap';

const API_URL = "http://localhost:5234"

const fetchRooms = async () => {
    const response = await fetch(`${API_URL}/api/roomapi/roomlist`);
    if (!response.ok) {
        console.error(`Fetch failed with status: ${response.status}`);
        throw new Error('Network response was not ok');
    }
    const data = await response.json();
    console.log('Fetched rooms successfully:', data);
    return data; 
};

const RoomListPage = () => {
    const { data: rooms = [], isLoading, error, refetch } = useQuery({
        queryKey: ['rooms'],
        queryFn: fetchRooms,
    });

    return (
        <div className="text-start">
            <h1>Rooms</h1>
            <Button onClick={() => refetch()} className="btn btn-primary mb-3" disabled={isLoading}>
                {isLoading ? 'Loading...' : 'Refresh Rooms'}
            </Button>
            {error && <p style={{ color: 'red' }}>Failed to fetch rooms.</p>}
            <Table striped bordered hover>
                <thead>
                    <tr>
                        <th>RoomID</th>
                        <th>Location</th>
                        <th>Room number</th>
                        <th>Room type</th>
                        <th>Capacity</th>
                    </tr>
                </thead>
                <tbody>
                    {rooms.map(room => (
                        <tr key={room.roomId}>
                            <td>{room.roomId}</td>
                            <td>{room.location}</td>
                            <td>{room.roomNumber}</td>
                            <td>{room.roomType}</td>
                            <td>{room.capacity}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
};

export default RoomListPage;