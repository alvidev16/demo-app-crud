import { apiFetch } from './client';
import type { Product, ProductInput } from './types';

export const productsApi = {
    getAll: () => apiFetch<Product[]>('/api/products'),
    getById: (id: string) => apiFetch<Product>(`/api/products/${id}`),
    create: (input: ProductInput) => apiFetch<Product>('/api/products', { method: 'POST', body: input }),
    update: (id: string, input: ProductInput) =>
        apiFetch<Product>(`/api/products/${id}`, { method: 'PUT', body: input }),
    remove: (id: string) => apiFetch<void>(`/api/products/${id}`, { method: 'DELETE' }),
};
