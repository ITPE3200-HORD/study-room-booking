import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import Container from 'react-bootstrap/Container'
import HomePage from './home/HomePage.js'
import RoomListPage from './rooms/RoomListPage.js'
import BookingPage from './booking/BookingPage.js'
import NavMenu from './shared/NavMenu.js'
import MyPagePage from './account/MyPagePage.js'
import LoginPage from './account/LoginPage.js'
import SessionsListPage from './sessions/SessionsListPage.js'
import './App.css'
import StudentListPage from './students/StudentsListPage.js'
import AdminRoute from './shared/AdminRoute.js'
import RegisterPage from './account/RegisterPage.js'


const queryClient = new QueryClient()

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <NavMenu />
        <Container>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/rooms" element={<AdminRoute><RoomListPage /></AdminRoute>}/>
            <Route path="/booking" element={<BookingPage />} />
            <Route path="/students" element={<AdminRoute><StudentListPage /></AdminRoute>} />
            <Route path="/sessions" element={<SessionsListPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/logout" element={<Navigate to="/" replace />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/my-page" element={<MyPagePage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Container>
      </Router>
    </QueryClientProvider>
  )
}
