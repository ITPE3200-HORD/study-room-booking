import { useQuery } from '@tanstack/react-query'
import { Table } from 'react-bootstrap';

const API_URL = "http://localhost:5003"

const fetchStudents = async () => {
    const response = await fetch(`${API_URL}/api/Student`, {
        credentials: 'include',
    })

    if (!response.ok) {
        console.error(`Fetch failed with status: ${response.status}`);
        throw new Error('Network response was not ok');
    }
    return response.json()
};

const StudentListPage = () => {
    const { data: students = [], isLoading, error, refetch } = useQuery({
        queryKey: ['students'],
        queryFn: fetchStudents,
    });

    return (
        <div className="text-start">
            <h1>Students</h1>
            <Button onClick={() => refetch()} className="btn btn-primary mb-3" disabled={isLoading}>
                {isLoading ? 'Loading...' : 'Refresh Students'}
            </Button>
            {error && <p style={{ color: 'red' }}>Failed to fetch students.</p>}
            <Table striped bordered hover>
                <thead>
                    <tr>
                        <th>StudentID</th>
                        <th>Email</th>
                        <th>Username</th>
                        <th>Full name</th>
                    </tr>
                </thead>
                <tbody>
                    {students.map(student => (
                        <tr key={student.studentId}>
                            <td>{student.studentId}</td>
                            <td>{student.email}</td>
                            <td>{student.userName}</td>
                            <td>{student.fullName}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
};

export default StudentListPage;