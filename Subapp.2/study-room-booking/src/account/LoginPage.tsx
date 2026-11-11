import { useState, type SubmitEvent } from 'react'
import { Alert, Button, Card, Form } from 'react-bootstrap'
import { useNavigate } from 'react-router-dom'

const API_URL = import.meta.env.VITE_API_URL;

type LoginErrorResponse = {
    message?: string
}

function LoginPage() {
    const navigate = useNavigate()
    const [username, setUsername] = useState('')
    const [password, setPassword] = useState('')
    const [rememberMe, setRememberMe] = useState(false)
    const [error, setError] = useState('')
    const [isSubmitting, setIsSubmitting] = useState(false)

    async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        setError('')
        setIsSubmitting(true)

        try {
            const response = await fetch(`${API_URL}/api/Account/Login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include', 
                body: JSON.stringify({ username, password, rememberMe }),
            })

            const result = await response.json().catch(() => null)

            if (!response.ok) {
                throw new Error(result?.message || 'Login failed.')
            }

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
                    <h1 className="h2 mb-4">Log in</h1>

                    {error && <Alert variant="danger">{error}</Alert>}

                    <Form onSubmit={handleSubmit}>
                        <Form.Group className="mb-3" controlId="username">
                            <Form.Label>Username</Form.Label>
                            <Form.Control
                                autoComplete="username"
                                value={username}
                                onChange={event => setUsername(event.target.value)}
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

                        <Form.Check
                            className="mb-3"
                            type="checkbox"
                            label="Remember me"
                            checked={rememberMe}
                            onChange={event => setRememberMe(event.target.checked)}
                        />

                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? 'Logging in…' : 'Log in'}
                        </Button>
                    </Form>
                </Card.Body>
            </Card>
        </div>
    )
}

export default LoginPage