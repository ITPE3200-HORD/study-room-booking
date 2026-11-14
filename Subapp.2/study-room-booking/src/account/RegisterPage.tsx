import { useState, type SubmitEvent } from 'react'
import { Alert, Button, Card, Form } from 'react-bootstrap'
import { useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'

const API_URL = import.meta.env.VITE_API_URL;

type RegisterApiError = {
    message?: string
    detail?: string
    title?: string
    errors?: string [] | Record<string, string[]>
}

function RegisterPage() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [userName, setUserName] = useState('')
    const [email, setEmail] = useState('')
    const [fullName, setFullName] = useState('')
    const [password, setPassword] = useState('')
    const [confirmPassword, setConfirmPassword] = useState('')
    const [error, setError] = useState('')
    const [isSubmitting, setIsSubmitting] = useState(false)

    async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        setError('')
        setIsSubmitting(true)

        try {
            const response = await fetch(`${API_URL}/api/Account/Register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include', 
                body: JSON.stringify({ userName, email, fullName, password, confirmPassword }),
            })

            const result = await response.json().catch(() => null) as RegisterApiError | null

            if (!response.ok) {
                const validationErrors = result?.errors
                    ? Array.isArray(result.errors)
                        ? result.errors
                        : Object.values(result.errors).flat()
                    : []

                throw new Error(
                    validationErrors.join(' ') ||
                    result?.detail ||
                    result?.message ||
                    result?.title ||
                    'Failed to register.'
                )         
            }

            await queryClient.invalidateQueries({ queryKey: ['current-user'] })
            navigate('/')
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : 'Could not connect to the API.'
    )
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className="mx-auto my-5" style={{ maxWidth: '480px' }}>
            <Card>
                <Card.Body className="p-4">
                    <h1 className="h2 mb-4">Register account</h1>

                    {error && <Alert variant="danger">{error}</Alert>}

                    <Form onSubmit={handleSubmit}>
                        <Form.Group className="mb-3" controlId="userName">
                            <Form.Label>Username</Form.Label>
                            <Form.Control
                                autoComplete="userName"
                                value={userName}
                                onChange={event => setUserName(event.target.value)}
                                required
                            />
                        </Form.Group>

                         <Form.Group className="mb-3" controlId="email">
                            <Form.Label>Email</Form.Label>
                            <Form.Control
                                type="email"
                                autoComplete="email"
                                value={email}
                                onChange={event => setEmail(event.target.value)}
                                required
                            />
                        </Form.Group>

                         <Form.Group className="mb-3" controlId="fullName">
                            <Form.Label>Full name</Form.Label>
                            <Form.Control
                                autoComplete="name"
                                value={fullName}
                                onChange={event => setFullName(event.target.value)}
                                required
                            />
                        </Form.Group>

                        <Form.Group className="mb-3" controlId="password">
                            <Form.Label>Password</Form.Label>
                            <Form.Control
                                type="password"
                                autoComplete="current-password"
                                value={password}
                                onChange={event => setPassword(event.target.value)}
                                required
                            />
                        </Form.Group>

                        <Form.Group className="mb-3" controlId="confirmPassword">
                            <Form.Label>Confirm password</Form.Label>
                            <Form.Control
                                type="password"
                                autoComplete="new-password"
                                value={confirmPassword}
                                onChange={event => setConfirmPassword(event.target.value)}
                                required
                            />
                        </Form.Group>

                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? 'Registering…' : 'Register'}
                        </Button>
                    </Form>
                </Card.Body>
            </Card>
        </div>
    )
}

export default RegisterPage
