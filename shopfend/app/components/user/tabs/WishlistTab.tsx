"use client";

import { useState, useEffect } from "react";
import { useSession } from "next-auth/react";
import { Heart, Search, Filter, Trash2, ShoppingCart } from "lucide-react";
import { shopbeApi, type ProductListItem, productResponseToListItem } from "@/lib/shopbeApi";
import WishlistItemCard from "../../WishlistItemCard";

export default function WishlistTab() {
  const { data: session } = useSession();
  const [items, setItems] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    if (session?.accessToken) {
      shopbeApi.wishlist.get(session.accessToken)
        .then(res => setItems((Array.isArray(res) ? res : []).map(productResponseToListItem)))
        .catch(err => console.error(err))
        .finally(() => setLoading(false));
    }
  }, [session?.accessToken]);

  const toggleSelect = (id: string, selected: boolean) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (selected) next.add(id);
      else next.delete(id);
      return next;
    });
  };

  const handleRemove = async (id: string) => {
    if (!session?.accessToken) return;
    try {
      await shopbeApi.wishlist.remove(session.accessToken, id);
      setItems(prev => prev.filter(i => i.id !== id));
      setSelectedIds(prev => {
        const next = new Set(prev);
        next.delete(id);
        return next;
      });
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="p-8 md:p-10 space-y-8">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">My Wishlist</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">{items.length} items saved for later.</p>
        </div>
        
        <div className="flex items-center gap-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
            <input 
              type="text" 
              placeholder="Search wishlist..." 
              className="pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold outline-none focus:ring-2 focus:ring-brand/10 transition-all w-full md:w-48"
            />
          </div>
          <button className="p-2.5 bg-slate-50 text-slate-600 rounded-xl hover:bg-slate-100 transition-all">
            <Filter size={18} />
          </button>
        </div>
      </div>

      {selectedIds.size > 0 && (
        <div className="bg-brand/5 border border-brand/10 rounded-2xl p-4 flex items-center justify-between animate-in slide-in-from-top-2">
          <span className="text-sm font-bold text-brand">{selectedIds.size} items selected</span>
          <div className="flex items-center gap-3">
            <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-white text-rose-500 text-xs font-bold shadow-sm hover:bg-rose-50 transition-all">
              <Trash2 size={14} />
              Remove
            </button>
            <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-brand text-white text-xs font-bold shadow-sm hover:bg-brand-hover transition-all">
              <ShoppingCart size={14} />
              Add to Cart
            </button>
          </div>
        </div>
      )}

      {loading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="aspect-[3/4] bg-slate-50 animate-pulse rounded-3xl" />
          ))}
        </div>
      ) : items.length === 0 ? (
        <div className="py-20 text-center bg-slate-50/50 rounded-3xl border-2 border-dashed border-slate-100">
          <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
            <Heart className="text-slate-200" size={32} />
          </div>
          <p className="text-slate-500 font-bold">Your wishlist is empty</p>
          <p className="text-xs text-slate-400 mt-1">Add items you love to find them easily later.</p>
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
          {items.map((item) => (
            <WishlistItemCard 
              key={item.id}
              product={item}
              isSelected={selectedIds.has(item.id)}
              onSelect={(sel) => toggleSelect(item.id, sel)}
              onRemove={() => handleRemove(item.id)}
              onMoveToCart={() => {}} // Integration point
            />
          ))}
        </div>
      )}
    </div>
  );
}
