import { useState, type FormEvent } from 'react';
import type { Product, ProductInput } from '../api/types';
import { ApiError } from '../api/client';

interface Props {
    initial?: Product | null;
    onSubmit: (input: ProductInput) => Promise<void>;
    onCancel: () => void;
}

export function ProductForm({ initial, onSubmit, onCancel }: Props) {
    const [name, setName] = useState(initial?.name ?? '');
    const [sku, setSku] = useState(initial?.sku ?? '');
    const [price, setPrice] = useState(initial?.price?.toString() ?? '');
    const [stock, setStock] = useState(initial?.stock?.toString() ?? '');
    const [category, setCategory] = useState(initial?.category ?? '');
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setError(null);
        setBusy(true);
        try {
            await onSubmit({
                name,
                sku,
                price: Number(price),
                stock: Number(stock),
                category,
            });
        } catch (err) {
            setError(err instanceof ApiError ? err.message : 'Could not save the product.');
        } finally {
            setBusy(false);
        }
    }

    return (
        <div className="modal-backdrop" onClick={onCancel}>
            <div className="modal card" onClick={(e) => e.stopPropagation()}>
                <h2 className="modal-title">{initial ? 'Edit product' : 'New product'}</h2>
                <form onSubmit={handleSubmit}>
                    <label className="field">
                        <span>Name</span>
                        <input value={name} onChange={(e) => setName(e.target.value)} required minLength={3} maxLength={100} />
                    </label>

                    <div className="field-row">
                        <label className="field">
                            <span>SKU</span>
                            <input value={sku} onChange={(e) => setSku(e.target.value)} required />
                        </label>
                        <label className="field">
                            <span>Category</span>
                            <input value={category} onChange={(e) => setCategory(e.target.value)} required />
                        </label>
                    </div>

                    <div className="field-row">
                        <label className="field">
                            <span>Price</span>
                            <input type="number" step="0.01" min="0.01" value={price} onChange={(e) => setPrice(e.target.value)} required />
                        </label>
                        <label className="field">
                            <span>Stock</span>
                            <input type="number" step="1" min="0" value={stock} onChange={(e) => setStock(e.target.value)} required />
                        </label>
                    </div>

                    {error && <div className="alert alert-error">{error}</div>}

                    <div className="modal-actions">
                        <button type="button" className="btn btn-ghost" onClick={onCancel} disabled={busy}>
                            Cancel
                        </button>
                        <button type="submit" className="btn btn-primary" disabled={busy}>
                            {busy ? 'Saving…' : initial ? 'Save changes' : 'Create'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
