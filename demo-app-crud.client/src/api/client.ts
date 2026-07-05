import type { ApiProblem } from './types';

const TOKEN_KEY = 'demo.token';

export function getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null): void {
    if (token) localStorage.setItem(TOKEN_KEY, token);
    else localStorage.removeItem(TOKEN_KEY);
}

/** Error carrying the parsed ProblemDetails payload and HTTP status. */
export class ApiError extends Error {
    status: number;
    problem?: ApiProblem;

    constructor(status: number, message: string, problem?: ApiProblem) {
        super(message);
        this.status = status;
        this.problem = problem;
    }
}

interface RequestOptions {
    method?: string;
    body?: unknown;
}

/**
 * Thin fetch wrapper that injects the JWT, serializes JSON and surfaces API
 * errors as typed {@link ApiError}s.
 */
export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const headers: Record<string, string> = { 'Content-Type': 'application/json' };
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const response = await fetch(path, {
        method: options.method ?? 'GET',
        headers,
        body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    });

    if (response.status === 204) return undefined as T;

    const isJson = response.headers.get('content-type')?.includes('json');
    const payload = isJson ? await response.json() : undefined;

    if (!response.ok) {
        const problem = payload as ApiProblem | undefined;
        const message = problem?.detail || problem?.title || `Request failed (${response.status})`;
        throw new ApiError(response.status, message, problem);
    }

    return payload as T;
}
