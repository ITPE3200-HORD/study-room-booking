import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import Container from 'react-bootstrap/Container'
import HomePage from './home/HomePage.js'
import RoomListPage from './rooms/RoomListPage.js'
import NavMenu from './shared/NavMenu.js'
import MyPagePage from './account/MyPagePage.js'
import LoginPage from './account/LoginPage.js'
import './App.css'

const queryClient = new QueryClient()

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>

      <Container>
        <NavMenu />
        <Router>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/rooms" element={<RoomListPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/my-page" element={<MyPagePage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Router>
      </Container>
    </QueryClientProvider>
  )
}
