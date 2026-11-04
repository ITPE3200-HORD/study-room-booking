import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import HomePage from './home/HomePage'
import RoomListPage from './rooms/RoomListPage'
import './App.css'


function App() {
  return (
    <>
      <Container>
        <NavMenu />
          <Router>
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/rooms" element={<RoomListPage />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </Router>
        </Container>
      </>
    )
}

export default App