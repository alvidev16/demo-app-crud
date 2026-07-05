export interface Product {
    id: string;
    name: string;
    sku: string;
    price: number;
    stock: number;
    category: string;
    createdAt: string;
    updatedAt: string;
}

export interface ProductInput {
    name: string;
    sku: string;
    price: number;
    stock: number;
    category: string;
}

export interface UserInfo {
    id: string;
    email: string;
    role: string;
    createdAt: string;
}

export interface AuthResult {
    token: string;
    expiresAt: string;
    user: UserInfo;
}

/** Shape of the RFC-7807 ProblemDetails returned by the API on errors. */
export interface ApiProblem {
    title?: string;
    detail?: string;
    status?: number;
    errors?: Record<string, string>;
}
