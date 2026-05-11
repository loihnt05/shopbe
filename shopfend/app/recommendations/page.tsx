"use client";

import { useSession } from "next-auth/react";
import { useEffect, useMemo, useRef, useState } from "react";

import ProductCard from "../components/ProductCard";
import { errorMessage } from "@/lib/errors";
import {
  isAbortError,
  productResponseToListItem,
  shopbeApi,
  type ProductListItem,
} from "@/lib/shopbeApi";

type LoadState<T> = {
  data: T;
  loading: boolean;
  error: string | null;
};

function initLoadState<T>(data: T): LoadState<T> {
  return { data, loading: true, error: null };
}

export default function RecommendationsPage() {
  const { data: session, status } = useSession();

  const [topSelling, setTopSelling] = useState<LoadState<ProductListItem[]>>(
    () => initLoadState([])
  );
  const [personalized, setPersonalized] = useState<LoadState<ProductListItem[]>>(
    () => ({ data: [], loading: false, error: null })
  );
  const [similar, setSimilar] = useState<LoadState<ProductListItem[]>>(() => ({
    data: [],
    loading: false,
    error: null,
  }));

  const [selectedSeedId, setSelectedSeedId] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  // Load top-selling (anonymous endpoint)
  useEffect(() => {
    abortRef.current?.abort();
    const abort = new AbortController();
    abortRef.current = abort;

    (async () => {
      try {
        setTopSelling((s) => ({ ...s, loading: true, error: null }));
        const data = await shopbeApi.recommendations.topSelling(10, abort.signal);
        const mapped = (data ?? []).map(productResponseToListItem);
        if (!abort.signal.aborted) {
          setTopSelling({ data: mapped, loading: false, error: null });
          setSelectedSeedId((prev) => prev ?? mapped[0]?.id ?? null);
        }
      } catch (e: unknown) {
        if (isAbortError(e)) return;
        if (!abort.signal.aborted) {
          setTopSelling({
            data: [],
            loading: false,
            error: errorMessage(e, "Failed to load top-selling recommendations"),
          });
        }
      }
    })();

    return () => abort.abort();
  }, []);

  // Load personalized (requires auth)
  useEffect(() => {
    if (status === "loading") return;

    if (!session?.accessToken) {
      setPersonalized({ data: [], loading: false, error: null });
      return;
    }

    const abort = new AbortController();

    (async () => {
      try {
        setPersonalized((s) => ({ ...s, loading: true, error: null }));
        const data = await shopbeApi.recommendations.me(
          session.accessToken!,
          10,
          abort.signal
        );
        const mapped = (data ?? []).map(productResponseToListItem);
        if (!abort.signal.aborted) {
          setPersonalized({ data: mapped, loading: false, error: null });
        }
      } catch (e: unknown) {
        if (isAbortError(e)) return;
        if (!abort.signal.aborted) {
          setPersonalized({
            data: [],
            loading: false,
            error: errorMessage(e, "Failed to load personalized recommendations"),
          });
        }
      }
    })();

    return () => abort.abort();
  }, [session?.accessToken, status]);

  // Load similar (anonymous endpoint)
  useEffect(() => {
    if (!selectedSeedId) {
      setSimilar({ data: [], loading: false, error: null });
      return;
    }

    const abort = new AbortController();

    (async () => {
      try {
        setSimilar((s) => ({ ...s, loading: true, error: null }));
        const data = await shopbeApi.recommendations.similar(
          selectedSeedId,
          8,
          abort.signal
        );
        const mapped = (data ?? []).map(productResponseToListItem);
        if (!abort.signal.aborted) {
          setSimilar({ data: mapped, loading: false, error: null });
        }
      } catch (e: unknown) {
        if (isAbortError(e)) return;
        if (!abort.signal.aborted) {
          setSimilar({
            data: [],
            loading: false,
            error: errorMessage(e, "Failed to load similar products"),
          });
        }
      }
    })();

    return () => abort.abort();
  }, [selectedSeedId]);

  const seedOptions = useMemo(() => {
    return topSelling.data.map((p) => ({ id: p.id, name: p.name }));
  }, [topSelling.data]);

  return (
    <div className="space-y-6">
      <div className="sb-card p-6">
        <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3">
          <div>
            <div className="text-sm text-[var(--muted)]">Presentation demo</div>
            <h1 className="text-2xl font-semibold">Recommendations</h1>
            <p className="mt-1 text-sm text-[var(--muted)] max-w-3xl">
              This page calls the backend endpoints in <code>Shopbe.Web</code>:
              <code className="ml-2">/api/recommendations/top-selling</code>,
              <code className="ml-2">/api/recommendations/me</code>, and
              <code className="ml-2">/api/recommendations/products/{"{id}"}/similar</code>.
            </p>
          </div>
        </div>
      </div>

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Top selling</h2>
          <div className="text-sm text-[var(--muted)]">
            {topSelling.loading ? "Loading…" : `${topSelling.data.length} items`}
          </div>
        </div>

        {topSelling.error ? (
          <div className="sb-card p-4 border border-red-300 bg-red-50 text-sm">
            {topSelling.error}
          </div>
        ) : null}

        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
          {topSelling.data.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      </section>

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">For you (personalized)</h2>
          <div className="text-sm text-[var(--muted)]">
            {!session
              ? "Sign in to enable"
              : personalized.loading
                ? "Loading…"
                : `${personalized.data.length} items`}
          </div>
        </div>

        {!session ? (
          <div className="sb-card p-4 text-sm text-[var(--muted)]">
            Personalized recommendations require authentication because the backend
            uses your browsing / cart / purchase events.
          </div>
        ) : personalized.error ? (
          <div className="sb-card p-4 border border-red-300 bg-red-50 text-sm">
            {personalized.error}
          </div>
        ) : null}

        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
          {personalized.data.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      </section>

      <section className="space-y-3">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
          <h2 className="text-lg font-semibold">Similar products</h2>
          <div className="flex items-center gap-2">
            <label className="text-sm text-[var(--muted)]" htmlFor="seed">
              Seed product
            </label>
            <select
              id="seed"
              className="sb-input max-w-xs"
              value={selectedSeedId ?? ""}
              onChange={(e) => setSelectedSeedId(e.target.value || null)}
              disabled={seedOptions.length === 0}
            >
              {seedOptions.length === 0 ? (
                <option value="">(load top-selling first)</option>
              ) : null}
              {seedOptions.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        {similar.error ? (
          <div className="sb-card p-4 border border-red-300 bg-red-50 text-sm">
            {similar.error}
          </div>
        ) : null}

        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
          {similar.data.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      </section>
    </div>
  );
}
