"use client";

import { useSession } from "next-auth/react";
import { Suspense, useEffect, useRef, useState, useMemo } from "react";
import { useRouter, usePathname, useSearchParams } from "next/navigation";
import {
  isAbortError,
  shopbeApi,
  productResponseToListItem,
  type ProductListItem,
  type CategoryFacetDto,
  type BrandFacetDto,
} from "@/lib/shopbeApi";
import ProductCard from "../components/ProductCard";
import { errorMessage } from "@/lib/errors";
import { FilterIcon, XIcon, CheckIcon, SearchIcon, StarIcon } from "../components/icons";
import { formatMoney } from "@/lib/format";

export default function ProductsPage() {
  return (
    <Suspense fallback={<div className="sb-card p-12 text-center animate-pulse">Loading products…</div>}>
      <ProductsPageInner />
    </Suspense>
  );
}

function ProductsPageInner() {
  const { data: session, status } = useSession();
  const [items, setItems] = useState<ProductListItem[]>([]);
  const [categoryFacets, setCategoryFacets] = useState<CategoryFacetDto[]>([]);
  const [brandFacets, setBrandFacets] = useState<BrandFacetDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);
  const [showMobileFilters, setShowMobileFilters] = useState(false);
  
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const q = searchParams.get("q") || "";
  const selectedCategoryIds = useMemo(() => {
    const ids = searchParams.getAll("categoryIds");
    const single = searchParams.get("category");
    if (single && !ids.includes(single)) {
      return [...ids, single];
    }
    return ids;
  }, [searchParams]);

  const selectedBrandIds = useMemo(() => searchParams.getAll("brandIds"), [searchParams]);
  const selectedCategorySlugs = useMemo(() => searchParams.getAll("categorySlugs"), [searchParams]);
  const minPrice = searchParams.get("minPrice") || "";
  const maxPrice = searchParams.get("maxPrice") || "";
  const minRating = searchParams.get("minRating") || "";
  const sortBy = searchParams.get("sortBy") || "newest";

  const [localMinPrice, setLocalMinPrice] = useState(minPrice);
  const [localMaxPrice, setLocalMaxPrice] = useState(maxPrice);

  useEffect(() => {
    setLocalMinPrice(minPrice);
    setLocalMaxPrice(maxPrice);
  }, [minPrice, maxPrice]);

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
            categorySlugs: selectedCategorySlugs,
            brandIds: selectedBrandIds,
            minBasePrice: minPrice ? parseInt(minPrice) : undefined,
            maxBasePrice: maxPrice ? parseInt(maxPrice) : undefined,
            minRating: minRating ? parseInt(minRating) : undefined,
            sortBy: sortBy,
            pageSize: 50,
          },
          session?.accessToken,
          abort.signal
        );

        if (!abort.signal.aborted) {
          const productList = (response.products ?? []).map(productResponseToListItem);
          setItems(productList);
          setCategoryFacets(response.categoryFacets ?? []);
          setBrandFacets(response.brandFacets ?? []);
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session?.accessToken, status, q, JSON.stringify(selectedCategoryIds), JSON.stringify(selectedCategorySlugs), JSON.stringify(selectedBrandIds), minPrice, maxPrice, minRating, sortBy]);

  const toggleCategory = (categoryId: string) => {
    const params = new URLSearchParams(searchParams.toString());
    params.delete("category");
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

  const toggleBrand = (brandId: string) => {
    const params = new URLSearchParams(searchParams.toString());
    const current = params.getAll("brandIds");
    if (current.includes(brandId)) {
      const updated = current.filter(id => id !== brandId);
      params.delete("brandIds");
      updated.forEach(id => params.append("brandIds", id));
    } else {
      params.append("brandIds", brandId);
    }
    router.push(`${pathname}?${params.toString()}`);
  };

  const setSortBy = (val: string) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set("sortBy", val);
    router.push(`${pathname}?${params.toString()}`);
  };

  const setMinRating = (val: number) => {
    const params = new URLSearchParams(searchParams.toString());
    if (val > 0) params.set("minRating", val.toString());
    else params.delete("minRating");
    router.push(`${pathname}?${params.toString()}`);
  };

  const applyPriceFilter = () => {
    const params = new URLSearchParams(searchParams.toString());
    if (localMinPrice) params.set("minPrice", localMinPrice);
    else params.delete("minPrice");
    if (localMaxPrice) params.set("maxPrice", localMaxPrice);
    else params.delete("maxPrice");
    router.push(`${pathname}?${params.toString()}`);
    setShowMobileFilters(false);
  };

  const clearFilters = () => {
    const params = new URLSearchParams();
    if (q) params.set("q", q);
    router.push(`${pathname}?${params.toString()}`);
    setShowMobileFilters(false);
  };

  const removeFilter = (key: string, value?: string) => {
    const params = new URLSearchParams(searchParams.toString());
    params.delete("category");
    if (key === "categoryIds" && value) {
      const current = params.getAll("categoryIds");
      const updated = current.filter(id => id !== value);
      params.delete("categoryIds");
      updated.forEach(id => params.append("categoryIds", id));
    } else if (key === "categorySlugs" && value) {
      const current = params.getAll("categorySlugs");
      const updated = current.filter(s => s !== value);
      params.delete("categorySlugs");
      updated.forEach(s => params.append("categorySlugs", s));
    } else if (key === "brandIds" && value) {
      const current = params.getAll("brandIds");
      const updated = current.filter(id => id !== value);
      params.delete("brandIds");
      updated.forEach(id => params.append("brandIds", id));
    } else {
      params.delete(key);
    }
    router.push(`${pathname}?${params.toString()}`);
  };

  const FiltersContent = () => (
    <div className="space-y-8 pb-10">
      <div>
        <h3 className="sb-label mb-4">Brands</h3>
        <div className="space-y-1 max-h-[200px] overflow-y-auto pr-2 sb-scrollbar-hide">
          {brandFacets.map((facet) => {
            const isActive = selectedBrandIds.includes(facet.id);
            return (
              <button
                key={facet.id}
                onClick={() => toggleBrand(facet.id)}
                className={`w-full flex items-center group transition-all py-1.5 rounded-lg ${
                  isActive ? "text-brand" : "text-gray-600 hover:text-brand"
                }`}
              >
                <div className={`w-4 h-4 rounded border mr-3 flex items-center justify-center transition-colors ${
                  isActive ? "bg-brand border-brand" : "border-gray-300 group-hover:border-brand"
                }`}>
                  {isActive && <CheckIcon className="w-3 h-3 text-white" />}
                </div>
                <span className={`flex-1 text-left text-sm truncate ${isActive ? "font-bold" : "font-medium"}`}>
                  {facet.name}
                </span>
                <span className="text-[10px] bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded-full font-bold ml-2">
                  {facet.count}
                </span>
              </button>
            );
          })}
          {!loading && brandFacets.length === 0 && (
            <p className="text-xs text-gray-400">No brands matching criteria</p>
          )}
        </div>
      </div>

      <div>
        <h3 className="sb-label mb-4">Price Range</h3>
        <div className="space-y-4">
          <div className="flex items-center gap-2">
            <input
              type="number"
              placeholder="Min"
              value={localMinPrice}
              onChange={(e) => setLocalMinPrice(e.target.value)}
              className="w-full text-sm px-3 py-2 bg-slate-50 border border-gray-100 rounded-lg focus:ring-2 focus:ring-brand/20 focus:border-brand outline-none"
            />
            <span className="text-gray-300">—</span>
            <input
              type="number"
              placeholder="Max"
              value={localMaxPrice}
              onChange={(e) => setLocalMaxPrice(e.target.value)}
              className="w-full text-sm px-3 py-2 bg-slate-50 border border-gray-100 rounded-lg focus:ring-2 focus:ring-brand/20 focus:border-brand outline-none"
            />
          </div>
          <button
            onClick={applyPriceFilter}
            className="w-full py-2 bg-brand text-white text-sm font-bold rounded-lg hover:bg-brand-hover transition-colors shadow-sm active:scale-95"
          >
            Apply Range
          </button>
        </div>
      </div>

      <div>
        <h3 className="sb-label mb-4">Rating</h3>
        <div className="space-y-2">
          {[5, 4, 3, 2, 1].map((star) => (
            <button
              key={star}
              onClick={() => setMinRating(star)}
              className={`w-full flex items-center gap-2 px-2 py-1.5 rounded-lg transition-colors ${
                minRating === star.toString() ? "bg-brand/5 text-brand" : "hover:bg-gray-50 text-gray-600"
              }`}
            >
              <div className="flex items-center gap-0.5">
                {[...Array(5)].map((_, i) => (
                  <StarIcon
                    key={i}
                    className={`w-3.5 h-3.5 ${i < star ? "fill-yellow-400 text-yellow-400" : "fill-gray-200 text-gray-200"}`}
                  />
                ))}
              </div>
              <span className="text-xs font-medium">
                {star === 5 ? "" : "& up"}
              </span>
              {minRating === star.toString() && <CheckIcon className="w-3.5 h-3.5 ml-auto" />}
            </button>
          ))}
          {minRating && (
            <button
              onClick={() => setMinRating(0)}
              className="text-[11px] font-bold text-gray-400 hover:text-brand mt-1 ml-2"
            >
              Clear rating
            </button>
          )}
        </div>
      </div>

      {(selectedCategoryIds.length > 0 || selectedCategorySlugs.length > 0 || selectedBrandIds.length > 0 || minPrice || maxPrice || minRating) && (
        <button
          onClick={clearFilters}
          className="w-full py-2 text-xs font-bold text-gray-500 hover:text-brand border border-gray-200 rounded-lg transition-colors"
        >
          Clear All Filters
        </button>
      )}
    </div>
  );

  return (
    <div className="relative">
      {/* Mobile Filter Header */}
      <div className="md:hidden flex items-center justify-between mb-6 bg-white p-4 rounded-xl shadow-sm border border-gray-100">
        <div className="flex items-center gap-2">
          <FilterIcon className="w-5 h-5 text-brand" />
          <span className="font-bold text-gray-900">Filters</span>
          {(selectedCategoryIds.length > 0 || selectedCategorySlugs.length > 0 || selectedBrandIds.length > 0 || minRating) && (
            <span className="bg-brand text-white text-[10px] w-5 h-5 flex items-center justify-center rounded-full">
              {selectedCategoryIds.length + selectedCategorySlugs.length + selectedBrandIds.length + (minRating ? 1 : 0)}
            </span>
          )}
        </div>
        <button
          onClick={() => setShowMobileFilters(true)}
          className="text-sm font-bold text-brand"
        >
          Adjust Filters
        </button>
      </div>

      <div className="flex flex-col md:flex-row gap-8">
        {/* Desktop Sidebar */}
        <aside className="hidden md:block w-64 flex-shrink-0">
          <div className="sticky top-24">
            <div className="flex items-center gap-2 mb-6">
              <FilterIcon className="w-5 h-5 text-brand" />
              <h2 className="text-lg font-bold text-gray-900">Search Filter</h2>
            </div>
            <FiltersContent />
          </div>
        </aside>

        {/* Main Content */}
        <div className="flex-1 min-w-0">
          <div className="mb-6 space-y-6">
            <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
              <div>
                <h1 className="text-3xl font-extrabold tracking-tight text-gray-900">
                  {q ? "Search Results" : "Explore Products"}
                </h1>
                <div className="text-sm text-gray-500 mt-1 flex items-center gap-1 flex-wrap">
                  {q ? (
                    <>
                      Results for <span className="font-bold text-brand italic">“{q}”</span>
                    </>
                  ) : (
                    <>Browse our curated collection</>
                  )}
                  <span className="text-gray-300 mx-1">•</span>
                  <span className="font-medium">
                    {loading && items.length === 0 ? "Finding items..." : `${totalCount} items found`}
                  </span>
                </div>
              </div>
            </div>

            {/* Shopee-style Horizontal Category Bar */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
              <div className="flex items-center gap-2 overflow-x-auto p-4 sb-scrollbar-hide">
                <button
                  onClick={() => {
                    const params = new URLSearchParams(searchParams.toString());
                    params.delete("categoryIds");
                    params.delete("category");
                    router.push(`${pathname}?${params.toString()}`);
                  }}
                  className={`flex-shrink-0 px-4 py-2 rounded-full text-sm font-bold transition-all ${
                    selectedCategoryIds.length === 0
                      ? "bg-brand text-white shadow-md shadow-brand/20"
                      : "bg-gray-50 text-gray-600 hover:bg-gray-100"
                  }`}
                >
                  All Categories
                </button>
                {categoryFacets.map((facet) => {
                  const isActive = selectedCategoryIds.includes(facet.id);
                  return (
                    <button
                      key={facet.id}
                      onClick={() => toggleCategory(facet.id)}
                      className={`flex-shrink-0 px-4 py-2 rounded-full text-sm font-bold transition-all flex items-center gap-2 ${
                        isActive
                          ? "bg-brand text-white shadow-md shadow-brand/20"
                          : "bg-gray-50 text-gray-600 hover:bg-gray-100"
                      }`}
                    >
                      {facet.name}
                      <span className={`text-[10px] px-1.5 py-0.5 rounded-full font-bold ${
                        isActive ? "bg-white/20 text-white" : "bg-gray-200 text-gray-500"
                      }`}>
                        {facet.count}
                      </span>
                    </button>
                  );
                })}
              </div>
            </div>

            {/* Shopee-style Sorting Bar */}
            <div className="bg-gray-100/50 rounded-xl p-3 flex flex-wrap items-center gap-4">
              <span className="text-sm text-gray-500 font-medium ml-2">Sort by:</span>
              <div className="flex flex-wrap items-center gap-2">
                {[
                  { label: "Newest", value: "newest" },
                  { label: "Popular", value: "popular" },
                  { label: "Top Rated", value: "rating" },
                ].map((sort) => (
                  <button
                    key={sort.value}
                    onClick={() => setSortBy(sort.value)}
                    className={`px-4 py-2 rounded-lg text-sm font-bold transition-all ${
                      sortBy === sort.value
                        ? "bg-white text-brand shadow-sm"
                        : "text-gray-600 hover:bg-white/50"
                    }`}
                  >
                    {sort.label}
                  </button>
                ))}
                
                <div className="relative group">
                  <select
                    value={sortBy.startsWith("price_") ? sortBy : ""}
                    onChange={(e) => setSortBy(e.target.value)}
                    className={`appearance-none pl-4 pr-10 py-2 rounded-lg text-sm font-bold bg-white border-none outline-none cursor-pointer transition-all ${
                      sortBy.startsWith("price_") ? "text-brand ring-1 ring-brand/20" : "text-gray-600"
                    }`}
                  >
                    <option value="" disabled>Price</option>
                    <option value="price_asc">Price: Low to High</option>
                    <option value="price_desc">Price: High to Low</option>
                  </select>
                  <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
                    <svg className="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7" />
                    </svg>
                  </div>
                </div>
              </div>
            </div>

            {/* Active Filter Chips */}
            {(selectedCategoryIds.length > 0 || selectedCategorySlugs.length > 0 || selectedBrandIds.length > 0 || minPrice || maxPrice || minRating) && (
              <div className="flex items-center gap-2 flex-wrap">
                <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest mr-1">Active:</span>
                
                {selectedCategoryIds.map(id => {
                  const facet = categoryFacets.find(f => f.id === id);
                  return (
                    <button
                      key={id}
                      onClick={() => removeFilter("categoryIds", id)}
                      className="inline-flex items-center gap-1.5 px-3 py-1 bg-brand/5 border border-brand/10 text-brand rounded-full text-xs font-bold hover:bg-brand/10 transition-colors"
                    >
                      {facet?.name || id}
                      <XIcon className="w-3 h-3" />
                    </button>
                  );
                })}

                {selectedCategorySlugs.map(slug => {
                  return (
                    <button
                      key={slug}
                      onClick={() => removeFilter("categorySlugs", slug)}
                      className="inline-flex items-center gap-1.5 px-3 py-1 bg-brand/5 border border-brand/10 text-brand rounded-full text-xs font-bold hover:bg-brand/10 transition-colors"
                    >
                      {slug.replace(/-/g, ' ')}
                      <XIcon className="w-3 h-3" />
                    </button>
                  );
                })}

                {selectedBrandIds.map(id => {
                  const facet = brandFacets.find(f => f.id === id);
                  return (
                    <button
                      key={id}
                      onClick={() => removeFilter("brandIds", id)}
                      className="inline-flex items-center gap-1.5 px-3 py-1 bg-brand/5 border border-brand/10 text-brand rounded-full text-xs font-bold hover:bg-brand/10 transition-colors"
                    >
                      {facet?.name || id}
                      <XIcon className="w-3 h-3" />
                    </button>
                  );
                })}

                {(minPrice || maxPrice) && (
                  <button
                    onClick={() => {
                      removeFilter("minPrice");
                      removeFilter("maxPrice");
                    }}
                    className="inline-flex items-center gap-1.5 px-3 py-1 bg-brand/5 border border-brand/10 text-brand rounded-full text-xs font-bold hover:bg-brand/10 transition-colors"
                  >
                    {minPrice && maxPrice ? `${formatMoney(parseInt(minPrice))} - ${formatMoney(parseInt(maxPrice))}` : 
                     minPrice ? `> ${formatMoney(parseInt(minPrice))}` : `< ${formatMoney(parseInt(maxPrice))}`}
                    <XIcon className="w-3 h-3" />
                  </button>
                )}

                {minRating && (
                  <button
                    onClick={() => removeFilter("minRating")}
                    className="inline-flex items-center gap-1.5 px-3 py-1 bg-brand/5 border border-brand/10 text-brand rounded-full text-xs font-bold hover:bg-brand/10 transition-colors"
                  >
                    <div className="flex items-center gap-0.5">
                      <StarIcon className="w-3 h-3 fill-current" />
                      {minRating}+
                    </div>
                    <XIcon className="w-3 h-3" />
                  </button>
                )}

                <button
                  onClick={clearFilters}
                  className="text-xs font-bold text-gray-400 hover:text-brand underline decoration-gray-300 underline-offset-4 px-2"
                >
                  Clear all
                </button>
              </div>
            )}
          </div>

          {error && (
            <div className="border border-red-100 bg-red-50 text-red-600 p-4 rounded-xl text-sm font-medium animate-in slide-in-from-bottom-10">
              {error}
            </div>
          )}

          {!loading && !error && items.length === 0 && (
            <div className="sb-card p-16 text-center bg-gray-50/50 border-dashed border-2">
              <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <SearchIcon className="w-8 h-8 text-gray-400" />
              </div>
              <p className="text-gray-900 font-bold text-lg">No matches found</p>
              <p className="text-gray-500 mt-1 mb-6">
                Try adjusting your filters or search terms to find what you're looking for.
              </p>
              <button 
                onClick={clearFilters}
                className="sb-btn-primary"
              >
                Reset all filters
              </button>
            </div>
          )}

          <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-4 animate-in fade-in duration-500">
            {items.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
            
            {loading && items.length === 0 && Array.from({ length: 12 }).map((_, i) => (
              <div key={i} className="sb-card aspect-[3/4] animate-pulse bg-gray-100/50" />
            ))}
          </div>
          
          {loading && items.length > 0 && (
            <div className="fixed bottom-8 left-1/2 -translate-x-1/2 bg-white/90 backdrop-blur shadow-xl border px-6 py-3 rounded-full text-sm font-bold text-brand flex items-center gap-3 animate-in slide-in-from-bottom-10">
              <div className="w-2 h-2 bg-brand rounded-full animate-ping"></div>
              Refreshing Results...
            </div>
          )}
        </div>
      </div>

      {/* Mobile Filter Drawer */}
      {showMobileFilters && (
        <div className="fixed inset-0 z-[100] md:hidden">
          <div 
            className="absolute inset-0 bg-black/50 backdrop-blur-sm animate-in fade-in"
            onClick={() => setShowMobileFilters(false)}
          />
          <div className="absolute right-0 top-0 bottom-0 w-[300px] bg-white shadow-2xl animate-in slide-in-from-right flex flex-col">
            <div className="flex items-center justify-between p-4 border-b">
              <h2 className="text-lg font-bold">Filters</h2>
              <button onClick={() => setShowMobileFilters(false)}>
                <XIcon className="w-6 h-6" />
              </button>
            </div>
            <div className="flex-1 overflow-y-auto p-6">
              <FiltersContent />
            </div>
            <div className="p-4 border-t bg-gray-50">
              <button
                onClick={() => setShowMobileFilters(false)}
                className="w-full sb-btn-primary"
              >
                Show Results
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
