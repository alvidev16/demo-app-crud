import { useState, type FormEvent } from 'react';
import { useAuth } from '../auth/AuthContext';
import { ApiError } from '../api/client';

const DEMO_EMAIL = 'admin@demo.com';
const DEMO_PASSWORD = 'Admin123!';

export function Login() {
    const { login, register } = useAuth();
    const [mode, setMode] = useState<'login' | 'register'>('login');
    const [email, setEmail] = useState(DEMO_EMAIL);
    const [password, setPassword] = useState(DEMO_PASSWORD);
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setError(null);
        setBusy(true);
        try {
            if (mode === 'login') await login(email, password);
            else await register(email, password);
        } catch (err) {
            setError(err instanceof ApiError ? err.message : 'Something went wrong.');
        } finally {
            setBusy(false);
        }
    }

    return (
        <div className="auth-screen">
            <form className="card auth-card" onSubmit={handleSubmit}>
                <h1 className="auth-title">Inventory Admin</h1>
                <p className="auth-subtitle">
                    {mode === 'login' ? 'Sign in to manage your products' : 'Create a new account'}
                </p>

                <label className="field">
                    <span>Email</span>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        autoComplete="username"
                        required
                    />
                </label>

                <label className="field">
                    <span>Password</span>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
                        required
                    />
                </label>

                {error && <div className="alert alert-error">{error}</div>}

                <button className="btn btn-primary btn-block" type="submit" disabled={busy}>
                    {busy ? 'Please wait…' : mode === 'login' ? 'Sign in' : 'Register'}
                </button>

                <button
                    type="button"
                    className="link-button"
                    onClick={() => {
                        setError(null);
                        setMode(mode === 'login' ? 'register' : 'login');
                    }}
                >
                    {mode === 'login' ? "Don't have an account? Register" : 'Already have an account? Sign in'}
                </button>

                {mode === 'login' && (
                    <div className="demo-hint">
                        Demo credentials: <code>{DEMO_EMAIL}</code> / <code>{DEMO_PASSWORD}</code>
                    </div>
                )}
            </form>
        </div>
    );
}
