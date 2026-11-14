import { useQuery } from '@tanstack/react-query'
import { Button, Table } from 'react-bootstrap';

const API_URL = import.meta.env.VITE_API_URL;

type Student = {
    studentId: number
    userName: string
    fullName: string
    email: string
}

const fetchStudents = async (): Promise<Student[]> => {
    const response = await fetch(`${API_URL}/api/Student`, {
        credentials: 'include',
    })

    if (!response.ok) {
        console.error(`Fetch failed with status: ${response.status}`);
        throw new Error('Network response was not ok');
    }
    return response.json() as Promise<Student[]>
};

const StudentListPage = () => {
    const { data: students = [], isLoading, error, refetch } = useQuery({
        queryKey: ['students'],
        queryFn: fetchStudents,
    });

    return (
        <main className="page-content">
            <header className="page-heading">
                <h1>Students</h1>
                <p>View and manage registered students.</p>
            </header>

            <Button className="btn-primary mb-3" onClick={() => refetch()}>
                Refresh Students
            </Button>
            {error && <p style={{ color: 'red' }}>Failed to fetch students.</p>}
            
            <div className="content-card table-scroll">
                <Table hover>
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
        </main>
    );
};

export default StudentListPage;