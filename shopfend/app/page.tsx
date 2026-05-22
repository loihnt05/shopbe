"use client";

import { useState, useEffect } from "react";
import { shopbeApi, ProductListItem, productResponseToListItem } from "@/lib/shopbeApi";
import ProductCard from "@/app/components/ProductCard";
import Link from "next/link";

export default function Home() {
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);

  const fetchInitialProducts = async () => {
    try {
      setLoading(true);
      const data = await shopbeApi.products.discover(20);
      const productList = (data ?? []).map(productResponseToListItem);
      setProducts(productList);
      setHasMore(productList.length === 20);
    } catch (error) {
      console.error("Failed to fetch initial products:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchInitialProducts();
  }, []);

  const handleDiscoverMore = async () => {
    if (loadingMore) return;
    
    try {
      setLoadingMore(true);
      const excludeIds = products.map(p => p.id);
      const data = await shopbeApi.products.discover(10, excludeIds);
      const newProducts = (data ?? []).map(productResponseToListItem);
      
      if (newProducts.length === 0) {
        setHasMore(false);
      } else {
        setProducts(prev => [...prev, ...newProducts]);
        if (newProducts.length < 10) {
          setHasMore(false);
        }
      }
    } catch (error) {
      console.error("Failed to fetch more products:", error);
    } finally {
      setLoadingMore(false);
    }
  };

  return (
    <div className="space-y-8 pb-10">
      {/* Top Banner Section */}
      <div className="flex gap-4 h-[280px]">
        {/* Main Banner Slider (Placeholder) */}
        <div className="w-2/3 bg-white rounded-xl overflow-hidden relative shadow-lg group cursor-pointer">
          <div className="w-full h-full bg-gradient-to-br from-orange-500 via-brand to-red-600 flex flex-col items-center justify-center text-white text-center p-8 transition-transform duration-700 group-hover:scale-105">
            <h2 className="text-4xl font-extrabold mb-4 tracking-tight drop-shadow-md">SUMMER MEGA SALE</h2>
            <p className="text-white/90 text-lg font-medium max-w-md">Up to 70% off on all electronics and fashion. Limited time only!</p>
            <div className="mt-6 bg-white text-brand px-8 py-2.5 rounded-full font-bold shadow-lg hover:bg-orange-50 transition-colors">Shop Now</div>
          </div>
        </div>
        {/* Side Banners (Placeholder) */}
        <div className="w-1/3 flex flex-col gap-4">
          <div className="h-1/2 bg-gradient-to-tr from-blue-500 to-cyan-400 rounded-xl overflow-hidden flex flex-col items-center justify-center text-white p-4 shadow-md hover:shadow-lg transition-all cursor-pointer">
            <span className="text-2xl mb-1">🚚</span>
            <span className="font-bold text-lg">Free Shipping</span>
            <span className="text-white/80 text-xs">For orders over ₫500k</span>
          </div>
          <div className="h-1/2 bg-gradient-to-tr from-purple-600 to-pink-500 rounded-xl overflow-hidden flex flex-col items-center justify-center text-white p-4 shadow-md hover:shadow-lg transition-all cursor-pointer">
            <span className="text-2xl mb-1">💎</span>
            <span className="font-bold text-lg">Premium Mall</span>
            <span className="text-white/80 text-xs">100% Authentic Brands</span>
          </div>
        </div>
      </div>

      {/* Categories */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-5 border-b border-gray-50 flex items-center justify-between">
          <h3 className="text-gray-900 font-bold tracking-wide uppercase text-sm">Shop by Category</h3>
          <Link href="/products" className="text-brand text-xs font-semibold hover:underline">View All</Link>
        </div>
        <div className="grid grid-cols-5 md:grid-cols-10 gap-x-2 gap-y-6 p-6">
          {[
            { id: "electronics", name: "Electronics", icon: "💻" },
            { id: "clothing", name: "Men's Clothing", icon: "👕" },
            { id: "books", name: "Books", icon: "📚" },
            { id: "beauty", name: "Beauty", icon: "💄" },
            { id: "sports", name: "Sports", icon: "⚽" },
            { id: "home", name: "Home & Living", icon: "🏠" },
            { id: "toys", name: "Toys", icon: "🧸" },
            { id: "automotive", name: "Automotive", icon: "🚗" },
            { id: "health", name: "Health", icon: "💊" },
            { id: "grocery", name: "Grocery", icon: "🛒" },
          ].map((cat) => (
            <Link
              key={cat.id}
              href={`/products?category=${cat.id}`}
              className="flex flex-col items-center justify-start gap-3 hover:-translate-y-1 transition-all duration-300 group"
            >
              <div className="w-16 h-16 rounded-2xl border border-gray-100 flex items-center justify-center text-2xl bg-slate-50 group-hover:bg-orange-50 group-hover:border-brand/20 transition-colors shadow-sm">
                {cat.icon}
              </div>
              <span className="text-[13px] text-center text-gray-600 font-medium group-hover:text-brand transition-colors leading-tight">
                {cat.name}
              </span>
            </Link>
          ))}
        </div>
      </div>

      {/* Daily Discover */}
      <div>
        <div className="bg-white sticky top-[110px] z-40 border-b-2 border-brand/10 mb-6 rounded-t-xl">
          <div className="text-brand font-bold text-center py-5 bg-white uppercase tracking-widest text-sm flex items-center justify-center gap-2">
            <span className="w-8 h-px bg-brand/30"></span>
            Daily Discover
            <span className="w-8 h-px bg-brand/30"></span>
          </div>
        </div>

        {loading ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {[...Array(12)].map((_, i) => (
              <div key={i} className="bg-white aspect-[3/4] animate-pulse rounded-xl shadow-sm"></div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {products.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
            {loadingMore && [...Array(6)].map((_, i) => (
              <div key={i} className="bg-white aspect-[3/4] animate-pulse rounded-xl shadow-sm"></div>
            ))}
          </div>
        )}

        {!loading && hasMore && (
          <div className="flex justify-center mt-10">
            <button 
              onClick={handleDiscoverMore}
              disabled={loadingMore}
              className="bg-white border-2 border-brand/20 text-brand px-12 py-3 rounded-xl font-bold hover:bg-brand hover:text-white transition-all duration-300 shadow-sm hover:shadow-md disabled:opacity-50"
            >
              {loadingMore ? "Loading..." : "Discover More"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}