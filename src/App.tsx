import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Toaster } from 'sonner';
import { Navigation } from './components/layout/Navigation';
import Home from './pages/Home';
import Dashboard from './pages/Dashboard';
import Cards from './pages/Cards';
import Players from './pages/Players';
import Balance from './pages/Balance';
import Export from './pages/Export';
import ApiDocs from './pages/ApiDocs';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-50">
        <Navigation />
        <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/cards" element={<Cards />} />
            <Route path="/players" element={<Players />} />
            <Route path="/balance" element={<Balance />} />
            <Route path="/export" element={<Export />} />
            <Route path="/api-docs" element={<ApiDocs />} />
            <Route path="/other" element={<div className="p-8"><h1 className="text-2xl font-bold">Coming Soon</h1></div>} />
          </Routes>
        </main>
        <Toaster />
      </div>
    </Router>
  );
}

export default App;
