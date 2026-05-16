"use client";

import { useState, useEffect } from "react";
import { shopbeApi, ProductListItem, productResponseToListItem } from "@/lib/shopbeApi";
import ProductCard from "@/app/components/ProductCard";
import Link from "next/link";

export default function Home() {
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const data = await shopbeApi.products.list();
        const productList = data.map(productResponseToListItem);
        setProducts(productList);
      } catch (error) {
        console.error("Failed to fetch products:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  return (
    <div className="space-y-6">
      {/* Top Banner Section */}
      <div className="flex gap-2 h-[235px]">
        {/* Main Banner Slider (Placeholder) */}
        <div className="w-2/3 bg-white rounded-sm overflow-hidden relative">
          <div className="w-full h-full bg-gradient-to-r from-orange-400 to-[#ee4d2d] flex items-center justify-center text-white text-2xl font-bold">
            Big Sale - Up to 50% Off
          </div>
        </div>
        {/* Side Banners (Placeholder) */}
        <div className="w-1/3 flex flex-col gap-2">
          <div className="h-1/2 bg-blue-100 rounded-sm overflow-hidden flex items-center justify-center text-blue-800 font-bold">
            Free Shipping
          </div>
          <div className="h-1/2 bg-purple-100 rounded-sm overflow-hidden flex items-center justify-center text-purple-800 font-bold">
            Shopee Mall Brands
          </div>
        </div>
      </div>

      {/* Categories */}
      <div className="bg-white rounded-sm shadow-sm pb-4 pt-2">
        <div className="px-4 py-4 text-gray-500 font-medium bg-white">CATEGORIES</div>
        <div className="grid grid-cols-5 md:grid-cols-10 gap-x-2 gap-y-4 px-4">
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
              className="flex flex-col items-center justify-start gap-2 hover:-translate-y-[1px] transition-transform"
            >
              <div className="w-[70px] h-[70px] rounded-full border border-gray-100 flex items-center justify-center text-3xl bg-gray-50">
                {cat.icon}
              </div>
              <span className="text-[14px] text-center text-gray-700 leading-tight">
                {cat.name}
              </span>
            </Link>
          ))}
        </div>
      </div>

      {/* Daily Discover */}
      <div>
        <div className="bg-white sticky top-[118px] z-40 border-b-4 border-[#ee4d2d] mb-4">
          <div className="text-[#ee4d2d] font-medium text-center py-4 bg-white uppercase">
            Daily Discover
          </div>
        </div>

        {loading ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-2">
            {[...Array(12)].map((_, i) => (
              <div key={i} className="bg-white h-[280px] animate-pulse rounded-sm"></div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-2">
            {products.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        )}

        {!loading && products.length > 0 && (
          <div className="flex justify-center mt-6">
            <button className="bg-white border border-gray-300 text-gray-600 px-10 py-2 rounded-sm hover:bg-gray-50 transition-colors w-[390px]">
              See More
            </button>
          </div>
        )}
      </div>
    </div>
  );
}