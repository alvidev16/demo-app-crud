import { useEffect, useState } from 'react';
import { productsApi } from '../api/products';
import type { Product, ProductInput } from '../api/types';
import { ApiError } from '../api/client';
import { useAuth } from '../auth/AuthContext';
import { ProductForm } from './ProductForm';

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

export function Products() {
    const { user, logout } = useAuth();
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [editing, setEditing] = useState<Product | null>(null);
    const [creating, setCreating] = useState(false);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            setProducts(await productsApi.getAll());
        } catch (err) {
            setError(err instanceof ApiError ? err.message : 'Failed to load products.');
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    async function handleCreate(input: ProductInput) {
        await productsApi.create(input);
        setCreating(false);
        await load();
    }

    async function handleUpdate(input: ProductInput) {
        if (!editing) return;
        await productsApi.update(editing.id, input);
        setEditing(null);
        await load();
    }

    async function handleDelete(product: Product) {
        if (!confirm(`Delete "${product.name}"? This cannot be undone.`)) return;
        try {
            await productsApi.remove(product.id);
            await load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : 'Failed to delete product.');
        }
    }

    return (
        <div className="app-shell">
            <header className="topbar">
                <div className="brand">
                    <span className="brand-mark">📦</span>
                    <span>Inventory Admin</span>
                </div>
                <div className="topbar-right">
                    <span className="user-badge">{user?.email}</span>
                    <button className="btn btn-ghost" onClick={logout}>Sign out</button>
                </div>
            </header>

            <main className="content">
                <div className="page-head">
                    <div>
                        <h1>Products</h1>
                        <p className="muted">{products.length} item{products.length === 1 ? '' : 's'} in catalog</p>
                    </div>
                    <button className="btn btn-primary" onClick={() => setCreating(true)}>+ New product</button>
                </div>

                {error && <div className="alert alert-error">{error}</div>}

                {loading ? (
                    <div className="empty">Loading products…</div>
                ) : products.length === 0 ? (
                    <div className="empty">No products yet. Create your first one!</div>
                ) : (
                    <div className="table-wrap card">
                        <table className="table">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>SKU</th>
                                    <th>Category</th>
                                    <th className="num">Price</th>
                                    <th className="num">Stock</th>
                                    <th className="actions-col">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {products.map((p) => (
                                    <tr key={p.id}>
                                        <td className="strong">{p.name}</td>
                                        <td><code>{p.sku}</code></td>
                                        <td><span className="chip">{p.category}</span></td>
                                        <td className="num">{currency.format(p.price)}</td>
                                        <td className="num">
                                            <span className={p.stock === 0 ? 'stock stock-out' : 'stock'}>{p.stock}</span>
                                        </td>
                                        <td className="actions-col">
                                            <button className="btn btn-sm" onClick={() => setEditing(p)}>Edit</button>
                                            <button className="btn btn-sm btn-danger" onClick={() => handleDelete(p)}>Delete</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </main>

            {creating && <ProductForm onSubmit={handleCreate} onCancel={() => setCreating(false)} />}
            {editing && <ProductForm initial={editing} onSubmit={handleUpdate} onCancel={() => setEditing(null)} />}
        </div>
    );
}
