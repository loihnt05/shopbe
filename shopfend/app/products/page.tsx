"use client";

import { useSession } from "next-auth/react";
import { Suspense, useEffect, useRef, useState } from "react";
import { useSearchParams } from "next/navigation";
import {
  isAbortError,
  shopbeApi,
  productResponseToListItem,
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
          const productList = (data ?? []).map(productResponseToListItem);
          setItems(productList);
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
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">All Products</h1>
          <div className="text-[14px] text-gray-500 mt-1">
            {q ? (
              <>
                Search results for <span className="font-semibold text-brand italic">“{q}”</span>
              </>
            ) : (
              <>Explore our full collection</>
            )}
          </div>
        </div>

        <div className="text-xs font-semibold bg-gray-100 text-gray-500 px-3 py-1.5 rounded-full uppercase tracking-widest">
          {loading ? "Loading…" : `${items.length} Products`}
        </div>
      </div>

      {error && (
        <div className="border border-red-100 bg-red-50 text-red-600 p-4 rounded-xl text-sm font-medium">
          {error}
        </div>
      )}

      {!loading && !error && items.length === 0 && (
        <div className="sb-card p-12 text-center">
          <p className="text-gray-400 font-medium">
            No products found. Start backend and ensure DB seeding ran.
          </p>
        </div>
      )}

      <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
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
