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
  const [recentlyViewed, setRecentlyViewed] = useState<LoadState<ProductListItem[]>>(
    () => ({ data: [], loading: false, error: null })
  );

  const [selectedSeedId, setSelectedSeedId] = useState<string | null>(null);
  const [simulating, setSimulating] = useState(false);
  const abortRef = useRef<AbortController | null>(null);

  // Load recently viewed
  useEffect(() => {
    if (!session?.accessToken) return;
    const abort = new AbortController();
    
    (async () => {
      try {
        setRecentlyViewed(s => ({ ...s, loading: true, error: null }));
        const data = await shopbeApi.recommendations.recentlyViewed(session.accessToken!, 10, abort.signal);
        const mapped = (data ?? []).map(productResponseToListItem);
        if (!abort.signal.aborted) {
          setRecentlyViewed({ data: mapped, loading: false, error: null });
        }
      } catch (e) {
        if (!isAbortError(e) && !abort.signal.aborted) {
          setRecentlyViewed({ data: [], loading: false, error: errorMessage(e, "Failed to load recently viewed") });
        }
      }
    })();
    
    return () => abort.abort();
  }, [session?.accessToken]);

  const runSimulation = async () => {
    if (!session?.accessToken) {
      alert("Please sign in to run the simulation.");
      return;
    }
    try {
      setSimulating(true);
      const res = await shopbeApi.simulation.run(session.accessToken);
      alert(`${res.message}\nProfile built for: ${res.profileCategories.join(", ")}`);
      window.location.reload();
    } catch (e) {
      alert(errorMessage(e, "Simulation failed"));
    } finally {
      setSimulating(false);
    }
  };

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

    if (!session?.accessToken) return;

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
    if (!selectedSeedId) return;

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

  // If the user is signed out, hide personalized results (without mutating state in an effect).
  const personalizedDisplay: LoadState<ProductListItem[]> = useMemo(() => {
    if (!session) return { data: [], loading: false, error: null };
    return personalized;
  }, [personalized, session]);

  // If no seed is selected, hide similar results (without mutating state in an effect).
  const similarDisplay: LoadState<ProductListItem[]> = useMemo(() => {
    if (!selectedSeedId) return { data: [], loading: false, error: null };
    return similar;
  }, [similar, selectedSeedId]);

  const seedOptions = useMemo(() => {
    return topSelling.data.map((p) => ({ id: p.id, name: p.name }));
  }, [topSelling.data]);

  return (
    <div className="space-y-6">
      <div className="sb-card p-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="text-sm text-(--muted)">Personalized Engine</div>
            <h1 className="text-2xl font-bold">Product Recommendations</h1>
            <p className="mt-1 text-sm text-(--muted) max-w-xl">
              Recommendations update in real-time based on your clicks, views, and purchases.
            </p>
          </div>
          <button 
            onClick={runSimulation} 
            disabled={simulating}
            className="sb-btn-primary whitespace-nowrap"
          >
            {simulating ? "Generating Data..." : "🧪 Run Behavior Simulation"}
          </button>
        </div>
      </div>

      {session && recentlyViewed.data.length > 0 && (
        <section className="space-y-3">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold flex items-center gap-2">
              <span className="text-xl">🕒</span> Recently Viewed
            </h2>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
            {recentlyViewed.data.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold flex items-center gap-2">
            <span className="text-xl">✨</span> Recommended For You
          </h2>
          <div className="text-sm text-(--muted)">
            {!session
              ? "Sign in for personal picks"
              : personalizedDisplay.loading
                ? "Loading…"
                : `${personalizedDisplay.data.length} items`}
          </div>
        </div>

        {!session ? (
          <div className="sb-card p-4 text-sm text-(--muted) bg-slate-50/50 border-dashed">
            Sign in to see products matched to your browsing and purchase history.
          </div>
        ) : personalizedDisplay.error ? (
          <div className="sb-card p-4 border border-red-300 bg-red-50 text-sm">
            {personalizedDisplay.error}
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
            {personalizedDisplay.data.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        )}
      </section>

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold flex items-center gap-2">
            <span className="text-xl">🔥</span> Trending Now
          </h2>
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

      <section className="space-y-3 pt-6 border-t border-slate-100">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
          <h2 className="text-lg font-semibold flex items-center gap-2">
            <span className="text-xl">🔍</span> Explore Similar To...
          </h2>
          <div className="flex items-center gap-2">
            <select
              id="seed"
              className="sb-input text-xs h-9 py-0"
              value={selectedSeedId ?? ""}
              onChange={(e) => setSelectedSeedId(e.target.value || null)}
              disabled={seedOptions.length === 0}
            >
              {seedOptions.length === 0 ? (
                <option value="">(load trending first)</option>
              ) : null}
              {seedOptions.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        {similarDisplay.error ? (
          <div className="sb-card p-4 border border-red-300 bg-red-50 text-sm">
            {similarDisplay.error}
          </div>
        ) : null}

        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
          {similarDisplay.data.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      </section>
    </div>
  );
}
