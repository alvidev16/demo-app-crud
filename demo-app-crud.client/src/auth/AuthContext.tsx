import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { apiFetch, getToken, setToken } from '../api/client';
import type { AuthResult, UserInfo } from '../api/types';

interface AuthContextValue {
    user: UserInfo | null;
    isAuthenticated: boolean;
    loading: boolean;
    login: (email: string, password: string) => Promise<void>;
    register: (email: string, password: string) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<UserInfo | null>(null);
    const [loading, setLoading] = useState(true);

    // On first load, if a token is stored, restore the session by fetching the user.
    useEffect(() => {
        const token = getToken();
        if (!token) {
            setLoading(false);
            return;
        }
        apiFetch<UserInfo>('/api/auth/me')
            .then(setUser)
            .catch(() => {
                setToken(null);
                setUser(null);
            })
            .finally(() => setLoading(false));
    }, []);

    async function login(email: string, password: string) {
        const result = await apiFetch<AuthResult>('/api/auth/login', {
            method: 'POST',
            body: { email, password },
        });
        setToken(result.token);
        setUser(result.user);
    }

    async function register(email: string, password: string) {
        await apiFetch<UserInfo>('/api/auth/register', {
            method: 'POST',
            body: { email, password },
        });
        // Auto-login after successful registration.
        await login(email, password);
    }

    function logout() {
        setToken(null);
        setUser(null);
    }

    const value = useMemo<AuthContextValue>(
        () => ({ user, isAuthenticated: user !== null, loading, login, register, logout }),
        [user, loading],
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
    return ctx;
}
