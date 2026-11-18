import type { Student } from '../types/student'
import { handleResponse, safeFetch } from '../shared/handleResponse'

const API_URL = import.meta.env.VITE_API_URL

export async function fetchStudents(): Promise<Student[]> {
    const response = await safeFetch(`${API_URL}/api/Student`, { credentials: 'include' })
    return handleResponse<Student[]>(response, 'Could not load students.')
}

export async function updateStudent(student: Student): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/Student/${student.studentId}`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(student),
    })
    return handleResponse<void>(response, 'Could not update student. Check the details and your permissions.')
}

export async function deleteStudent(studentId: number): Promise<void> {
    const response = await safeFetch(`${API_URL}/api/Student/${studentId}`, {
        method: 'DELETE',
        credentials: 'include',
    })
    return handleResponse<void>(response, 'Could not delete student. Check the details and your permissions.')
}