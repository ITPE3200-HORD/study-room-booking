import { useState, type SubmitEvent } from 'react'
import { Alert, Button, Card, Form } from 'react-bootstrap'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { login } from './AccountService'

function LoginPage() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const location = useLocation()
    const [username, setUsername] = useState('')
    const [password, setPassword] = useState('')
    const [rememberMe, setRememberMe] = useState(false)
    const [error, setError] = useState('')
    const [isSubmitting, setIsSubmitting] = useState(false)

    const from = (location.state as { from?: { pathname: string; search?: string } } | null)?.from
    const redirectTo = from ? `${from.pathname}${from.search ?? ''}` : '/'

    async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault()
        setError('')
        setIsSubmitting(true)

        try {
            await login({ username, password, rememberMe })
            await queryClient.invalidateQueries({ queryKey: ['current-user'] })
            navigate(redirectTo, { replace: true })
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Login failed.')
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

                        <Button type="submit" className="w-100" disabled={isSubmitting}>
                            {isSubmitting ? 'Logging in…' : 'Log in'}
                        </Button>
                    </Form>
                    
                    <div className="auth-switch">
                    <p>Don´t have an account?</p>
                        <Link to="/register" className="button-outline x-100">Create an account
                        </Link>
                    </div>
                </Card.Body>
            </Card>
        </div>
    )
}

export default LoginPage
