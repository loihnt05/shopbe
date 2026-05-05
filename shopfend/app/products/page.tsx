"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { isAbortError, shopbeApi, type ProductListItem } from "../../lib/shopbeApi";

export default function ProductsPage() {
  const [items, setItems] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const abort = useMemo(() => new AbortController(), []);

  useEffect(() => {
    (async () => {
      try {
        setLoading(true);
        const data = await shopbeApi.products.list(abort.signal);
        setItems(data ?? []);
      } catch (e: any) {
        if (isAbortError(e)) return;
        setError(e?.message ?? "Failed to load products");
      } finally {
        setLoading(false);
      }
    })();

    return () => abort.abort();
  }, [abort]);

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Products</h1>

      {loading && <p>Loading…</p>}
      {error && (
        <div className="border border-red-300 bg-red-50 p-3 rounded text-sm">
          {error}
        </div>
      )}

      {!loading && !error && items.length === 0 && (
        <p className="opacity-70">
          No products yet. Start backend and ensure DB seeding ran.
        </p>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {items.map((p) => (
          <Link
            key={p.id}
            href={`/products/${p.id}`}
            className="border rounded p-4 hover:shadow-sm transition"
          >
            <div className="font-medium">{p.name}</div>
            {p.description ? (
              <div className="text-sm opacity-75 line-clamp-3 mt-1">
                {p.description}
              </div>
            ) : null}
            <div className="mt-3 text-sm">
              {p.discountPrice != null ? (
                <>
                  <span className="font-semibold">
                    {p.discountPrice} {p.currency ?? ""}
                  </span>
                  {p.price != null ? (
                    <span className="opacity-60 line-through ml-2">
                      {p.price}
                    </span>
                  ) : null}
                </>
              ) : p.price != null ? (
                <span className="font-semibold">
                  {p.price} {p.currency ?? ""}
                </span>
              ) : (
                <span className="opacity-70">View details</span>
              )}
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}


