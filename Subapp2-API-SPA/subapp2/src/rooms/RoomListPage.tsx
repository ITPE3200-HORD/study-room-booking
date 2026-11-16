import { useState, type SubmitEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Form, Modal, Table } from 'react-bootstrap'

const API_URL = import.meta.env.VITE_API_URL

type Room = {
    roomId: number
    location: string | null
    roomNumber: string
    roomType: string
    capacity: number
    status: number
}

type RoomForm = Omit<Room, 'roomId'>

const emptyRoom: RoomForm = {
    location: '',
    roomNumber: '',
    roomType: 'Group Study Room',
    capacity: 4,
    status: 0,
}

async function fetchRooms(): Promise<Room[]> {
    const response = await fetch(`${API_URL}/api/RoomAPI`, { credentials: 'include' })
    if (!response.ok) throw new Error('Could not load rooms.')
    return response.json() as Promise<Room[]>
}

async function saveRoom(room: Room | RoomForm) {
    const isUpdate = 'roomId' in room
    const response = await fetch(
        `${API_URL}/api/RoomAPI${isUpdate ? `/${room.roomId}` : ''}`,
        {
            method: isUpdate ? 'PUT' : 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(room),
        },
    )
    if (!response.ok) throw new Error('Could not save room. Check your details and permissions.')
}

async function deleteRoom(roomId: number) {
    const response = await fetch(`${API_URL}/api/RoomAPI/${roomId}`, {
        method: 'DELETE',
        credentials: 'include',
    })
    if (!response.ok) throw new Error('Could not delete room. It may have existing bookings.')
}

export default function RoomListPage() {
    const queryClient = useQueryClient()
    const { data: rooms = [], isLoading, error } = useQuery({
        queryKey: ['rooms'],
        queryFn: fetchRooms,
    })
    const [showEditor, setShowEditor] = useState(false)
    const [editingRoom, setEditingRoom] = useState<Room | null>(null)
    const [form, setForm] = useState<RoomForm>(emptyRoom)
    const [actionError, setActionError] = useState('')

    const saveMutation = useMutation({
        mutationFn: saveRoom,
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['rooms'] })
            setShowEditor(false)
            setActionError('')
        },
        onError: error => setActionError(error.message),
    })
    const deleteMutation = useMutation({
        mutationFn: deleteRoom,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['rooms'] }),
        onError: error => setActionError(error.message),
    })

    function openCreate() {
        setEditingRoom(null)
        setForm(emptyRoom)
        setActionError('')
        setShowEditor(true)
    }

    function openEdit(room: Room) {
        setEditingRoom(room)
        setForm({
            location: room.location ?? '',
            roomNumber: room.roomNumber,
            roomType: room.roomType,
            capacity: room.capacity,
            status: room.status,
        })
        setActionError('')
        setShowEditor(true)
    }

    function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        setActionError('')
        saveMutation.mutate(editingRoom ? { ...form, roomId: editingRoom.roomId } : form)
    }

    return (
        <main className="page-content">
            <header className="page-heading page-heading-actions">
                <div>
                    <p className="eyebrow">Administration</p>
                    <h1>Rooms</h1>
                    <p>View and manage campus rooms.</p>
                </div>
                <Button className="btn-primary" onClick={openCreate}>Add room</Button>
            </header>

            {(error || actionError) && (
                <div className="search-error" role="alert">
                    {actionError || 'Could not load rooms. Check that the API is running and you are signed in as an admin.'}
                </div>
            )}

            {isLoading ? (
                <p role="status">Loading rooms…</p>
            ) : error ? null : rooms.length === 0 ? (
                <div className="empty-state">
                    <h3>No rooms found</h3>
                    <p>Add a room to get started.</p>
                </div>
            ) : (
                <div className="content-card table-scroll">
                    <Table hover>
                        <thead>
                            <tr>
                                <th scope="col">Room</th>
                                <th scope="col">Location</th>
                                <th scope="col">Type</th>
                                <th scope="col">Capacity</th>
                                <th scope="col">Status</th>
                                <th scope="col">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rooms.map(room => (
                                <tr key={room.roomId}>
                                    <td>{room.roomNumber}</td>
                                    <td>{room.location || '—'}</td>
                                    <td>{room.roomType}</td>
                                    <td>{room.capacity}</td>
                                    <td>{room.status === 0 ? 'Available' : 'Unavailable'}</td>
                                    <td>
                                        <div className="table-actions">
                                            <Button size="sm" variant="outline-success" onClick={() => openEdit(room)}>Edit</Button>
                                            <Button
                                                size="sm"
                                                variant="outline-danger"
                                                disabled={deleteMutation.isPending}
                                                onClick={() => {
                                                    if (window.confirm(`Delete room ${room.roomNumber}?`)) {
                                                        setActionError('')
                                                        deleteMutation.mutate(room.roomId)
                                                    }
                                                }}
                                            >Delete</Button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </div>
            )}

            <Modal show={showEditor} onHide={() => setShowEditor(false)} centered>
                <Form onSubmit={handleSubmit}>
                    <Modal.Header closeButton>
                        <Modal.Title>{editingRoom ? 'Edit room' : 'Add room'}</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        {actionError && <Alert variant="danger">{actionError}</Alert>}
                        <Form.Group className="mb-3" controlId="roomNumber">
                            <Form.Label>Room number</Form.Label>
                            <Form.Control required maxLength={10} value={form.roomNumber}
                                onChange={event => setForm({ ...form, roomNumber: event.target.value })} />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="location">
                            <Form.Label>Location</Form.Label>
                            <Form.Control required value={form.location ?? ''}
                                onChange={event => setForm({ ...form, location: event.target.value })} />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="roomType">
                            <Form.Label>Room type</Form.Label>
                            <Form.Select value={form.roomType}
                                onChange={event => setForm({ ...form, roomType: event.target.value })}>
                                <option>Group Study Room</option>
                                <option>Class Room</option>
                                <option>Auditorium</option>
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="capacity">
                            <Form.Label>Capacity</Form.Label>
                            <Form.Control type="number" min={1} max={250} required value={form.capacity}
                                onChange={event => setForm({ ...form, capacity: Number(event.target.value) })} />
                        </Form.Group>
                        <Form.Group controlId="status">
                            <Form.Label>Status</Form.Label>
                            <Form.Select value={form.status}
                                onChange={event => setForm({ ...form, status: Number(event.target.value) })}>
                                <option value={0}>Available</option>
                                <option value={1}>Unavailable</option>
                            </Form.Select>
                        </Form.Group>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="outline-secondary" onClick={() => setShowEditor(false)}>Cancel</Button>
                        <Button type="submit" disabled={saveMutation.isPending}>
                            {saveMutation.isPending ? 'Saving…' : 'Save room'}
                        </Button>
                    </Modal.Footer>
                </Form>
            </Modal>
        </main>
    )
}
