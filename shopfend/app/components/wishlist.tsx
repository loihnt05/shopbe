"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useSession } from "next-auth/react";
import { shopbeApi, type ProductListItem, productResponseToListItem } from "@/lib/shopbeApi";
import WishlistItemCard from "./WishlistItemCard";
import QuickAddToCartModal from "./QuickAddToCartModal";
import { ChevronDownIcon, TrashIcon, CartIcon, FilterIcon, CheckIcon } from "./icons";
import { useCart } from "./CartContext";

export default function Wishlist() {
  const { data: session, status } = useSession();
  const { addItem } = useCart();
  
  const [items, setItems] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [sortBy, setSortBy] = useState("Recent");
  const [inStockOnly, setInStockOnly] = useState(false);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  
  const [modalProduct, setModalProduct] = useState<ProductListItem | null>(null);
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" } | null>(null);

  const showToast = useCallback((message: string, type: "success" | "error" = "success") => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3000);
  }, []);

  const performFetch = useCallback(async (pageNum: number, isLoadMore: boolean) => {
    if (!session?.accessToken) return;

    try {
      setLoading(true);
      const data = await shopbeApi.wishlist.get(session.accessToken, {
        sortBy,
        inStockOnly,
        pageNumber: pageNum,
        pageSize: 12,
      });

      const newItems = (Array.isArray(data) ? data : []).map(productResponseToListItem);
      
      if (isLoadMore) {
        setItems(prev => [...prev, ...newItems]);
        setPage(pageNum);
      } else {
        setItems(newItems);
        setPage(1);
      }
      
      setHasMore(newItems.length === 12);
    } catch (error) {
      console.error("Failed to fetch wishlist:", error);
      showToast("Failed to load wishlist", "error");
    } finally {
      setLoading(false);
    }
  }, [session?.accessToken, sortBy, inStockOnly, showToast]);

  useEffect(() => {
    if (status === "authenticated") {
      performFetch(1, false);
    }
  }, [status, sortBy, inStockOnly, performFetch]); // Refetch when filters change

  const handleLoadMore = () => {
    performFetch(page + 1, true);
  };

  const handleRemove = async (productId: string) => {
    if (!session?.accessToken) return;
    try {
      await shopbeApi.wishlist.remove(session.accessToken, productId);
      setItems(prev => prev.filter(i => i.id !== productId));
      setSelectedIds(prev => {
        const next = new Set(prev);
        next.delete(productId);
        return next;
      });
      showToast("Item removed from wishlist");
    } catch (error) {
      console.error(error);
      showToast("Failed to remove item", "error");
    }
  };

  const handleBulkRemove = async () => {
    if (!session?.accessToken || selectedIds.size === 0) return;
    try {
      const idsArray = Array.from(selectedIds);
      await shopbeApi.wishlist.bulkRemove(session.accessToken, idsArray);
      setItems(prev => prev.filter(i => !selectedIds.has(i.id)));
      setSelectedIds(new Set());
      showToast(`${idsArray.length} items removed from wishlist`);
    } catch (error) {
      console.error(error);
      showToast("Failed to remove items", "error");
    }
  };

  const handleMoveToCart = async (product: ProductListItem) => {
    if (!session?.accessToken) {
      showToast("Please login to add items to cart", "error");
      return;
    }

    if (product.variants && product.variants.length > 1) {
      setModalProduct(product);
    } else if (product.variants && product.variants.length === 1) {
      const variant = product.variants[0];
      if (variant.stockQuantity && variant.stockQuantity > 0) {
        try {
          await addItem(product.id, variant.id, 1);
          showToast("Added to cart successfully");
        } catch (error) {
          console.error(error);
          showToast("Failed to add to cart", "error");
        }
      } else {
        showToast("Product is out of stock", "error");
      }
    } else {
      showToast("Product information incomplete", "error");
    }
  };

  const handleBulkAddToCart = async () => {
    if (!session?.accessToken || selectedIds.size === 0) return;
    
    const selectedItems = items.filter(i => selectedIds.has(i.id));
    let successCount = 0;
    
    for (const item of selectedItems) {
      if (item.totalStockQuantity === 0) continue;
      
      if (item.variants && item.variants.length === 1) {
        try {
          await addItem(item.id, item.variants[0].id, 1);
          successCount++;
        } catch (err) {
          console.error(`Failed to add ${item.name} to cart`, err);
        }
      } else {
        setModalProduct(item);
        showToast("Please select options for multi-variant items", "success");
        return;
      }
    }
    
    if (successCount > 0) {
      showToast(`Added ${successCount} items to cart`);
      setSelectedIds(new Set());
    }
  };

  const toggleSelectAll = () => {
    if (selectedIds.size === items.length) {
      setSelectedIds(new Set());
    } else {
      setSelectedIds(new Set(items.map(i => i.id)));
    }
  };

  const toggleSelect = (id: string, selected: boolean) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (selected) next.add(id);
      else next.delete(id);
      return next;
    });
  };

  if (status === "unauthenticated") {
    return (
      <div className="bg-white min-h-[60vh] flex flex-col items-center justify-center p-6 text-center">
        <div className="w-24 h-24 bg-gray-50 rounded-full flex items-center justify-center mb-6">
          <FilterIcon className="w-10 h-10 text-gray-300" />
        </div>
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Your wishlist is waiting</h2>
        <p className="text-gray-500 mb-8 max-w-sm">
          Login to see the products you&apos;ve saved and sync them across all your devices.
        </p>
        <button 
          onClick={() => window.location.href = '/api/auth/signin'}
          className="bg-brand text-white px-10 py-3 rounded-full font-bold shadow-lg hover:bg-brand/90 transition-all active:scale-95"
        >
          Login Now
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      {/* Page Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-8 border-b border-gray-100 pb-8">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight mb-1">My Wishlist</h1>
          <p className="text-gray-500 text-sm">{items.length} items saved in your favorites</p>
        </div>

        {/* Toolbar */}
        <div className="flex flex-wrap items-center gap-3">
          {/* Sorting */}
          <div className="relative group">
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
              className="appearance-none bg-white border border-gray-200 px-4 py-2.5 pr-10 rounded-xl text-sm font-bold text-gray-700 focus:outline-none focus:ring-2 focus:ring-brand/20 focus:border-brand cursor-pointer shadow-sm transition-all"
            >
              <option value="Recent">Recently Added</option>
              <option value="PriceAsc">Price: Low to High</option>
              <option value="PriceDesc">Price: High to Low</option>
              <option value="Discount">Highest Discount</option>
            </select>
            <ChevronDownIcon className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          </div>

          {/* Filter */}
          <button
            onClick={() => setInStockOnly(!inStockOnly)}
            className={`flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold transition-all border shadow-sm ${
              inStockOnly 
              ? 'bg-brand/5 border-brand text-brand' 
              : 'bg-white border-gray-200 text-gray-700 hover:border-brand/30'
            }`}
          >
            <CheckIcon className={`w-4 h-4 ${inStockOnly ? 'opacity-100' : 'opacity-30'}`} />
            In Stock Only
          </button>
        </div>
      </div>

      {/* Bulk Actions Bar */}
      {selectedIds.size > 0 && (
        <div className="sticky top-4 z-40 mb-8 bg-white border border-brand/20 shadow-xl shadow-brand/10 rounded-2xl p-4 flex items-center justify-between animate-in fade-in slide-in-from-top-4 duration-300">
          <div className="flex items-center gap-4">
            <label className="flex items-center gap-3 cursor-pointer group">
              <input
                type="checkbox"
                checked={selectedIds.size === items.length}
                onChange={toggleSelectAll}
                className="w-5 h-5 rounded border-gray-300 text-brand focus:ring-brand shadow-sm"
              />
              <span className="text-sm font-bold text-gray-700">
                {selectedIds.size} {selectedIds.size === 1 ? 'item' : 'items'} selected
              </span>
            </label>
          </div>
          <div className="flex items-center gap-3">
            <button
              onClick={handleBulkAddToCart}
              className="flex items-center gap-2 px-6 py-2 rounded-lg bg-brand text-white text-sm font-bold shadow-md hover:bg-brand/90 transition-all active:scale-95"
            >
              <CartIcon className="w-4 h-4" />
              <span>Add to Cart</span>
            </button>
            <div className="w-px h-6 bg-gray-200 mx-1 hidden sm:block" />
            <button
              onClick={handleBulkRemove}
              className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-bold text-red-500 hover:bg-red-50 transition-colors"
            >
              <TrashIcon className="w-4 h-4" />
              <span className="hidden sm:inline">Remove Selected</span>
            </button>
          </div>
        </div>
      )}

      {/* Grid State Handling */}
      {loading && items.length === 0 ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-6">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="aspect-[3/4] bg-gray-50 rounded-2xl animate-pulse" />
          ))}
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 px-4 text-center bg-gray-50/50 rounded-3xl border-2 border-dashed border-gray-200">
          <div className="w-20 h-20 bg-white rounded-full flex items-center justify-center mb-6 shadow-sm">
            <CartIcon className="w-8 h-8 text-gray-200" />
          </div>
          <h3 className="text-xl font-bold text-gray-800 mb-2">No products found</h3>
          <p className="text-gray-500 mb-8 max-w-xs">
            {inStockOnly ? "Try turning off 'In Stock Only' or change your filters." : "Your wishlist is empty. Start exploring our collections today!"}
          </p>
          <button 
            onClick={() => window.location.href = '/'}
            className="bg-brand text-white px-8 py-3 rounded-full font-bold shadow-lg hover:bg-brand/90 transition-all active:scale-95"
          >
            Start Shopping
          </button>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-6">
            {items.map((product) => (
              <WishlistItemCard 
                key={product.id} 
                product={product}
                isSelected={selectedIds.has(product.id)}
                onSelect={(selected) => toggleSelect(product.id, selected)}
                onRemove={() => handleRemove(product.id)}
                onMoveToCart={() => handleMoveToCart(product)}
              />
            ))}
          </div>

          {hasMore && (
            <div className="mt-12 flex justify-center">
              <button
                onClick={handleLoadMore}
                disabled={loading}
                className="px-10 py-3 rounded-full bg-white border border-gray-200 text-sm font-bold text-gray-700 hover:border-brand hover:text-brand transition-all shadow-sm active:scale-95 flex items-center gap-3 disabled:opacity-50"
              >
                {loading ? (
                  <>
                    <div className="w-4 h-4 border-2 border-brand/30 border-t-brand rounded-full animate-spin" />
                    Loading...
                  </>
                ) : (
                  "Show More Items"
                )}
              </button>
            </div>
          )}
        </>
      )}

      {/* Modal & Toast */}
      {modalProduct && (
        <QuickAddToCartModal
          product={modalProduct}
          isOpen={!!modalProduct}
          onClose={() => setModalProduct(null)}
          onSuccess={() => showToast("Added to cart successfully")}
        />
      )}

      {toast && (
        <div className={`fixed bottom-8 left-1/2 -translate-x-1/2 z-[200] px-6 py-4 rounded-2xl shadow-2xl flex items-center gap-3 animate-in fade-in slide-in-from-bottom-8 duration-300 ${
          toast.type === 'success' ? 'bg-gray-900 text-white' : 'bg-red-500 text-white'
        }`}>
          {toast.type === 'success' && <CheckIcon className="w-5 h-5 text-green-400" />}
          <span className="text-sm font-bold">{toast.message}</span>
        </div>
      )}
    </div>
  );
}
