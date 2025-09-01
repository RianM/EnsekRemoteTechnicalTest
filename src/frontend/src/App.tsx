import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './components/AuthProvider';
import Header from './components/Layout/Header';
import TabNavigation from './components/Layout/TabNavigation';
import AccountsList from './components/Accounts/AccountsList';
import MeterReadingsList from './components/MeterReadings/MeterReadingsList';
// Styling provided by Bootstrap via index.html

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="d-flex flex-column min-vh-100">
          <Header />
          <TabNavigation />
          <main className="container my-4 flex-grow-1">
            <Routes>
              <Route path="/" element={<Navigate to="/accounts" replace />} />
              <Route path="/accounts" element={<AccountsList />} />
              <Route path="/meter-readings" element={<MeterReadingsList />} />
            </Routes>
          </main>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App
