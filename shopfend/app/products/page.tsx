"use client";

import { useSession } from "next-auth/react";
import { Suspense, useEffect, useRef, useState } from "react";
import { useSearchParams } from "next/navigation";
import {
  isAbortError,
  shopbeApi,
  type ProductListItem,
} from "@/lib/shopbeApi";
import ProductCard from "../components/ProductCard";
import { errorMessage } from "@/lib/errors";

export default function ProductsPage() {
  return (
    <Suspense fallback={<div className="sb-card p-6">Loading…</div>}>
      <ProductsPageInner />
    </Suspense>
  );
}

function ProductsPageInner() {
  const { data: session, status } = useSession();
  const [items, setItems] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);
  const searchParams = useSearchParams();
  const q = (searchParams.get("q") ?? "").trim().toLowerCase();

  useEffect(() => {
    if (status === "loading") return;

    abortRef.current?.abort();
    const abort = new AbortController();
    abortRef.current = abort;

    (async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await shopbeApi.products.list(
          session?.accessToken,
          abort.signal
        );
        if (!abort.signal.aborted) {
          setItems(data ?? []);
        }
      } catch (e: unknown) {
        if (isAbortError(e)) return;
        setError(errorMessage(e, "Failed to load products"));
      } finally {
        if (!abort.signal.aborted) {
          setLoading(false);
        }
      }
    })();

    return () => abort.abort();
  }, [session?.accessToken, status]);

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold">Products</h1>
          <div className="text-sm text-slate-600">
            {q ? (
              <>
                Results for <span className="font-medium">“{q}”</span>
              </>
            ) : (
              <>Browse what’s available</>
            )}
          </div>
        </div>

        <div className="text-sm text-slate-500">
          {loading ? "Loading…" : `${items.length} items loaded`}
        </div>
      </div>

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

      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
        {(q
          ? items.filter((p) => {
              const hay = `${p.name} ${p.description ?? ""}`.toLowerCase();
              return hay.includes(q);
            })
          : items
        ).map((p) => (
          <ProductCard key={p.id} product={p} />
        ))}
      </div>
    </div>
  );
}
