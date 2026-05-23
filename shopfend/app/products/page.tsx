"use client";

import { useSession } from "next-auth/react";
import { Suspense, useEffect, useRef, useState } from "react";
import { useRouter, usePathname, useSearchParams } from "next/navigation";
import {
  isAbortError,
  shopbeApi,
  productResponseToListItem,
  type ProductListItem,
  type CategoryFacetDto,
} from "@/lib/shopbeApi";
import ProductCard from "../components/ProductCard";
import { errorMessage } from "@/lib/errors";

export default function ProductsPage() {
  return (
    <Suspense fallback={<div className="sb-card p-6 text-center">Loading products…</div>}>
      <ProductsPageInner />
    </Suspense>
  );
}

function ProductsPageInner() {
  const { data: session, status } = useSession();
  const [items, setItems] = useState<ProductListItem[]>([]);
  const [facets, setFacets] = useState<CategoryFacetDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);
  
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const q = searchParams.get("q") || "";
  const selectedCategoryIds = searchParams.getAll("categoryIds");

  useEffect(() => {
    if (status === "loading") return;

    abortRef.current?.abort();
    const abort = new AbortController();
    abortRef.current = abort;

    (async () => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await shopbeApi.products.list(
          {
            name: q,
            categoryIds: selectedCategoryIds,
            pageSize: 50, // Increase for better discovery
          },
          session?.accessToken,
          abort.signal
        );

        if (!abort.signal.aborted) {
          const productList = (response.products ?? []).map(productResponseToListItem);
          setItems(productList);
          setFacets(response.categoryFacets ?? []);
          setTotalCount(response.totalCount ?? 0);
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
  }, [session?.accessToken, status, q, JSON.stringify(selectedCategoryIds)]);

  const toggleCategory = (categoryId: string) => {
    const params = new URLSearchParams(searchParams.toString());
    const current = params.getAll("categoryIds");
    
    if (current.includes(categoryId)) {
      const updated = current.filter(id => id !== categoryId);
      params.delete("categoryIds");
      updated.forEach(id => params.append("categoryIds", id));
    } else {
      params.append("categoryIds", categoryId);
    }
    
    router.push(`${pathname}?${params.toString()}`);
  };

  const clearFilters = () => {
    const params = new URLSearchParams(searchParams.toString());
    params.delete("categoryIds");
    params.delete("q");
    router.push(`${pathname}?${params.toString()}`);
  };

  return (
    <div className="flex flex-col md:flex-row gap-8">
      {/* Sidebar */}
      <aside className="w-full md:w-64 flex-shrink-0 space-y-6">
        <div>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-bold text-gray-900">Categories</h2>
            {(selectedCategoryIds.length > 0 || q) && (
              <button 
                onClick={clearFilters}
                className="text-xs font-medium text-brand hover:underline"
              >
                Reset
              </button>
            )}
          </div>
          
          <div className="space-y-1">
            <button
              onClick={clearFilters}
              className={`w-full text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                selectedCategoryIds.length === 0 
                ? "bg-brand/10 text-brand font-semibold" 
                : "text-gray-600 hover:bg-gray-100"
              }`}
            >
              All Products
            </button>
            
            {facets.map((facet) => (
              <button
                key={facet.id}
                onClick={() => toggleCategory(facet.id)}
                className={`w-full flex items-center justify-between px-3 py-2 rounded-lg text-sm transition-colors ${
                  selectedCategoryIds.includes(facet.id)
                  ? "bg-brand/10 text-brand font-semibold"
                  : "text-gray-600 hover:bg-gray-100"
                }`}
              >
                <span className="truncate mr-2">{facet.name}</span>
                <span className="text-[10px] bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded-full font-bold">
                  {facet.count}
                </span>
              </button>
            ))}
            
            {!loading && facets.length === 0 && (
              <p className="text-xs text-gray-400 px-3 py-2">No categories found</p>
            )}
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-gray-900">
              {q ? "Search Results" : "Explore Products"}
            </h1>
            <div className="text-[14px] text-gray-500 mt-1">
              {q ? (
                <>
                  Results for <span className="font-semibold text-brand italic">“{q}”</span>
                </>
              ) : (
                <>Browse our curated collection</>
              )}
              {selectedCategoryIds.length > 0 && (
                <> in selected categories</>
              )}
            </div>
          </div>

          <div className="text-xs font-semibold bg-gray-100 text-gray-500 px-3 py-1.5 rounded-full uppercase tracking-widest">
            {loading ? "Loading…" : `${totalCount} Products`}
          </div>
        </div>

        {error && (
          <div className="border border-red-100 bg-red-50 text-red-600 p-4 rounded-xl text-sm font-medium">
            {error}
          </div>
        )}

        {!loading && !error && items.length === 0 && (
          <div className="sb-card p-12 text-center bg-gray-50/50">
            <p className="text-gray-500 font-medium">
              No products match your search or filters.
            </p>
            <button 
              onClick={clearFilters}
              className="mt-4 text-sm font-bold text-brand hover:underline"
            >
              Clear all filters
            </button>
          </div>
        )}

        <div className="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
          {items.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
        
        {loading && items.length > 0 && (
          <div className="fixed bottom-8 right-8 bg-white/80 backdrop-blur shadow-lg border px-4 py-2 rounded-full text-sm font-medium text-gray-500 flex items-center gap-2 animate-pulse">
            <div className="w-2 h-2 bg-brand rounded-full"></div>
            Updating results...
          </div>
        )}
      </div>
    </div>
  );
}
