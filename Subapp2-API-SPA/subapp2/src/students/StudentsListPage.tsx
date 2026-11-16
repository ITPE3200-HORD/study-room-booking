import { useState, type SubmitEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Form, Modal, Table } from 'react-bootstrap'

const API_URL = import.meta.env.VITE_API_URL

type Student = {
    studentId: number
    userName: string
    fullName: string
    email: string
}

type StudentForm = Omit<Student, 'studentId'>

async function fetchStudents(): Promise<Student[]> {
    const response = await fetch(`${API_URL}/api/Student`, { credentials: 'include' })
    if (!response.ok) throw new Error('Could not load students.')
    return response.json() as Promise<Student[]>
}

async function updateStudent(student: Student) {
    const response = await fetch(`${API_URL}/api/Student/${student.studentId}`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(student),
    })
    if (!response.ok) throw new Error('Could not update student. Check the details and your permissions.')
}

async function deleteStudent(studentId: number) {
    const response = await fetch(`${API_URL}/api/Student/${studentId}`, {
        method: 'DELETE',
        credentials: 'include',
    })
    if (!response.ok) throw new Error('Could not delete student.')
}

export default function StudentsListPage() {
    const queryClient = useQueryClient()
    const { data: students = [], isLoading, error } = useQuery({
        queryKey: ['students'],
        queryFn: fetchStudents,
    })
    const [editingStudent, setEditingStudent] = useState<Student | null>(null)
    const [form, setForm] = useState<StudentForm>({ userName: '', fullName: '', email: '' })
    const [actionError, setActionError] = useState('')

    const updateMutation = useMutation({
        mutationFn: updateStudent,
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['students'] })
            setEditingStudent(null)
            setActionError('')
        },
        onError: error => setActionError(error.message),
    })
    const deleteMutation = useMutation({
        mutationFn: deleteStudent,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['students'] }),
        onError: error => setActionError(error.message),
    })

    function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        if (!editingStudent) return
        setActionError('')
        updateMutation.mutate({ ...form, studentId: editingStudent.studentId })
    }

    return (
        <main className="page-content">
            <header className="page-heading">
                <p className="eyebrow">Administration</p>
                <h1>Students</h1>
                <p>View and manage registered students.</p>
            </header>

            {(error || actionError) && (
                <div className="search-error" role="alert">
                    {actionError || 'Could not load students. Check that the API is running and you are signed in as an admin.'}
                </div>
            )}

            {isLoading ? (
                <p role="status">Loading students…</p>
            ) : error ? null : students.length === 0 ? (
                <div className="empty-state">
                    <h3>No students found</h3>
                    <p>Student profiles are created when students register.</p>
                </div>
            ) : (
                <div className="content-card table-scroll">
                    <Table hover>
                        <thead>
                            <tr>
                                <th scope="col">Student ID</th>
                                <th scope="col">Email</th>
                                <th scope="col">Username</th>
                                <th scope="col">Full name</th>
                                <th scope="col">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {students.map(student => (
                                <tr key={student.studentId}>
                                    <td>{student.studentId}</td>
                                    <td>{student.email}</td>
                                    <td>{student.userName}</td>
                                    <td>{student.fullName}</td>
                                    <td>
                                        <div className="table-actions">
                                            <Button size="sm" variant="outline-success" onClick={() => {
                                                setEditingStudent(student)
                                                setForm({ userName: student.userName, fullName: student.fullName, email: student.email })
                                                setActionError('')
                                            }}>Edit</Button>
                                            <Button
                                                size="sm"
                                                variant="outline-danger"
                                                disabled={deleteMutation.isPending}
                                                onClick={() => {
                                                    if (window.confirm(`Delete ${student.fullName} and their account?`)) {
                                                    setActionError('')
                                                    deleteMutation.mutate(student.studentId)
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

            <Modal show={editingStudent !== null} onHide={() => setEditingStudent(null)} centered>
                <Form onSubmit={handleSubmit}>
                    <Modal.Header closeButton>
                        <Modal.Title>Edit student</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        {actionError && <Alert variant="danger">{actionError}</Alert>}
                        <Form.Group className="mb-3" controlId="studentFullName">
                            <Form.Label>Full name</Form.Label>
                            <Form.Control required maxLength={100} value={form.fullName}
                                onChange={event => setForm({ ...form, fullName: event.target.value })} />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="studentEmail">
                            <Form.Label>Email</Form.Label>
                            <Form.Control type="email" required value={form.email}
                                onChange={event => setForm({ ...form, email: event.target.value })} />
                        </Form.Group>
                        <Form.Group controlId="studentUserName">
                            <Form.Label>Username</Form.Label>
                            <Form.Control required maxLength={50} value={form.userName}
                                onChange={event => setForm({ ...form, userName: event.target.value })} />
                        </Form.Group>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="outline-secondary" onClick={() => setEditingStudent(null)}>Cancel</Button>
                        <Button type="submit" disabled={updateMutation.isPending}>
                            {updateMutation.isPending ? 'Saving…' : 'Save student'}
                        </Button>
                    </Modal.Footer>
                </Form>
            </Modal>
        </main>
    )
}
