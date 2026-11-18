export type Student = {
    studentId: number
    userName: string
    fullName: string
    email: string
}

export type StudentForm = Omit<Student, 'studentId'>