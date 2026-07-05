import './App.css';
import { useAuth } from './auth/AuthContext';
import { Login } from './components/Login';
import { Products } from './components/Products';

function App() {
    const { isAuthenticated, loading } = useAuth();

    if (loading) {
        return <div className="app-loading">Loading…</div>;
    }

    return isAuthenticated ? <Products /> : <Login />;
}

export default App;
