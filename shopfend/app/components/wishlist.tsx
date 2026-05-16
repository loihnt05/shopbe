"use client";

import { useState, useEffect } from "react";
import { shopbeApi, ProductListItem, productResponseToListItem } from "@/lib/shopbeApi";
import ProductCard from "@/app/components/ProductCard";

export default function Wishlist() {
  const [wishlist, setWishlist] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchWishlist = async () => {
      try {
        const data = await shopbeApi.wishlist.get();
        // Assuming data is an array of products
        const items = (Array.isArray(data) ? data : []).map(productResponseToListItem);
        setWishlist(items);
      } catch (error) {
        console.error("Failed to fetch wishlist:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchWishlist();
  }, []);

  if (loading) {
    return (
      <div className="bg-white p-6 shadow-sm min-h-[400px] flex items-center justify-center">
        <div className="text-gray-500">Loading your wishlist...</div>
      </div>
    );
  }

  return (
    <div className="bg-white p-6 shadow-sm min-h-[400px]">
      <h1 className="text-xl font-medium text-gray-800 border-b border-gray-200 pb-4 mb-6 uppercase">
        My Wishlist
      </h1>
      {wishlist.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-gray-500 gap-4">
          <div className="text-6xl text-gray-300">♡</div>
          <p>Your wishlist is empty.</p>
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-2">
          {wishlist.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  );
}