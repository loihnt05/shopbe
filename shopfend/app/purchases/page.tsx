"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { useSession, signIn } from "next-auth/react";
import Link from "next/link";
import Image from "next/image";
import {
  isAbortError,
  resolveApiUrl,
  shopbeApi,
  type PurchasedProductDto,
} from "@/lib/shopbeApi";
import { errorMessage } from "@/lib/errors";

function formatPurchasedAt(value: string) {
  // Backend returns ISO string.
  const dt = new Date(value);
  if (Number.isNaN(dt.getTime())) return value;
  return dt.toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export default function PurchasesPage() {
  const { data: session, status } = useSession();

  const [items, setItems] = useState<PurchasedProductDto[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [onlyNotReviewed, setOnlyNotReviewed] = useState(false);

  const abortRef = useRef<AbortController | null>(null);

  const getSignal = () => {
    abortRef.current?.abort();
    abortRef.current = new AbortController();
    return abortRef.current.signal;
  };

  const load = async () => {
    if (!session?.accessToken) return;
    try {
      setLoading(true);
      setError(null);
      const data = await shopbeApi.orders.purchasedProducts(session.accessToken, {
        onlyNotReviewed,
        signal: getSignal(),
      });
      setItems(data);
    } catch (e: unknown) {
      if (isAbortError(e)) return;
      setError(errorMessage(e, "Failed to load purchase history"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (status === "authenticated") load();
    return () => abortRef.current?.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status, onlyNotReviewed]);

  const grouped = useMemo(() => {
    if (!items) return [] as PurchasedProductDto[];
    // Ensure deterministic ordering in UI.
    return [...items].sort((a, b) => (a.purchasedAt < b.purchasedAt ? 1 : -1));
  }, [items]);

  if (status === "loading") return <p>Loading session…</p>;

  if (!session) {
    return (
      <div className="space-y-3">
        <h1 className="text-2xl font-semibold">Purchase history</h1>
        <p className="opacity-80">Sign in to view your purchased products.</p>
        <button onClick={() => signIn("keycloak")} className="sb-btn-primary">
          Sign in
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold">Purchase history</h1>
          <div className="text-sm text-slate-600">
            Products from your paid orders.
          </div>
        </div>

        <div className="flex items-center gap-3">
          <label className="text-sm flex items-center gap-2 select-none">
            <input
              type="checkbox"
              checked={onlyNotReviewed}
              onChange={(e) => setOnlyNotReviewed(e.target.checked)}
            />
            Only not reviewed
          </label>

          <button className="sb-btn-outline" onClick={load} disabled={loading}>
            {loading ? "Loading…" : "Refresh"}
          </button>
        </div>
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 p-3 rounded text-sm text-red-800">
          {error}
        </div>
      )}

      {!items ? (
        <div className="sb-card p-6">{loading ? "Loading…" : "No data yet."}</div>
      ) : grouped.length === 0 ? (
        <div className="sb-card p-6 space-y-2">
          <div className="font-semibold">No purchases found</div>
          <div className="text-sm text-slate-600">
            Once an order is paid, the products will show up here.
          </div>
          <Link href="/products" className="sb-btn-primary">
            Browse products
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {grouped.map((it) => {
            const imgSrc = resolveApiUrl(it.productImageUrl);
            return (
              <div key={`${it.orderId}:${it.productId}`} className="sb-card overflow-hidden">
                <div className="relative aspect-[4/3] bg-linear-to-br from-slate-50 to-slate-100">
                  {imgSrc ? (
                    <Image
                      src={imgSrc}
                      alt={it.productName}
                      fill
                      sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
                      className="object-cover"
                      unoptimized
                    />
                  ) : (
                    <div className="absolute inset-0 grid place-items-center text-slate-400 text-xs">
                      No image
                    </div>
                  )}
                </div>

                <div className="p-4 space-y-2">
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <div className="font-medium text-slate-900 line-clamp-2">
                        {it.productName}
                      </div>
                      <div className="text-xs text-slate-500 mt-1">
                        Purchased: {formatPurchasedAt(it.purchasedAt)}
                      </div>
                    </div>

                    <span
                      className={`sb-badge ${
                        it.isReviewed ? "sb-badge-muted" : "sb-badge-brand"
                      }`}
                      title={it.isReviewed ? "Reviewed" : "Not reviewed yet"}
                    >
                      {it.isReviewed ? "Reviewed" : "To review"}
                    </span>
                  </div>

                  <div className="flex items-center justify-between gap-2 pt-2">
                    <Link href={`/products/${it.productId}`} className="sb-btn-outline">
                      View
                    </Link>
                  </div>

                  <div className="text-[11px] text-slate-500">
                    <span className="opacity-70">Order:</span>{" "}
                    <span className="font-mono break-all">{it.orderId}</span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

