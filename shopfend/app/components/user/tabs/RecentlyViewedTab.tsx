"use client";

import { useState, useEffect } from "react";
import { useSession } from "next-auth/react";
import { Clock, Trash2, ShoppingCart } from "lucide-react";
import { shopbeApi, type ProductListItem, productResponseToListItem } from "@/lib/shopbeApi";
import ProductCard from "../../ProductCard";

export default function RecentlyViewedTab() {
  const { data: session } = useSession();
  const [items, setItems] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (session?.accessToken) {
      shopbeApi.recommendations.recentlyViewed(session.accessToken, 10)
        .then(res => setItems((Array.isArray(res) ? res : []).map(productResponseToListItem)))
        .catch(err => console.error(err))
        .finally(() => setLoading(false));
    }
  }, [session?.accessToken]);

  return (
    <div className="p-8 md:p-10 space-y-10">
      <div className="flex items-center justify-between border-b border-slate-50 pb-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">Recently Viewed</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">Pick up right where you left off.</p>
        </div>
        <button className="flex items-center gap-2 px-6 py-2.5 rounded-xl text-xs font-bold text-rose-500 hover:bg-rose-50 transition-all">
          <Trash2 size={16} />
          Clear History
        </button>
      </div>

      {loading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
          {[1, 2, 3, 4, 5].map(i => <div key={i} className="aspect-[3/4] bg-slate-50 animate-pulse rounded-3xl" />)}
        </div>
      ) : items.length === 0 ? (
        <div className="py-20 text-center bg-slate-50/50 rounded-3xl border-2 border-dashed border-slate-100">
           <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
            <Clock className="text-slate-200" size={32} />
          </div>
          <p className="text-slate-500 font-bold">No history found</p>
          <p className="text-xs text-slate-400 mt-1">Start browsing products to see them here.</p>
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
          {items.map((item) => (
            <div key={item.id} className="relative group">
              <ProductCard product={item} />
              <button className="absolute top-2 right-2 p-1.5 bg-white/80 backdrop-blur-sm rounded-full opacity-0 group-hover:opacity-100 transition-all hover:bg-brand hover:text-white shadow-sm">
                <ShoppingCart size={14} />
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
