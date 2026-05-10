"use client";

import Link from "next/link";
import ProductCard from "../components/ProductCard";

// Mock recommendations data
const RECOMMENDED_PRODUCTS = [
  {
    id: "rec-1",
    name: "Wireless Noise-Cancelling Headphones",
    price: 2990000,
    currency: "VND",
    primaryImageUrl: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&q=80",
    description: "Premium sound with active noise cancellation."
  },
  {
    id: "rec-2",
    name: "Smart Fitness Watch",
    price: 1590000,
    currency: "VND",
    primaryImageUrl: "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500&q=80",
    description: "Track your health and stay connected on the go."
  },
  {
    id: "rec-3",
    name: "Ergonomic Office Chair",
    price: 4500000,
    currency: "VND",
    primaryImageUrl: "https://images.unsplash.com/photo-1505843490538-5133c6c7d0e1?w=500&q=80",
    description: "Comfortable support for long working hours."
  },
  {
    id: "rec-4",
    name: "Mechanical Gaming Keyboard",
    price: 1250000,
    currency: "VND",
    primaryImageUrl: "https://images.unsplash.com/photo-1595225476474-87563907a212?w=500&q=80",
    description: "Tactile feedback with RGB backlighting."
  }
];

const TRENDING_PRODUCTS = [
  {
    id: "trend-1",
    name: "Minimalist Coffee Maker",
    price: 850000,
    currency: "VND",
    primaryImageUrl: "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=500&q=80",
    description: "Start your morning right with a perfect brew."
  },
  {
    id: "trend-2",
    name: "Portable Bluetooth Speaker",
    price: 650000,
    currency: "VND",
    primaryImageUrl: "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=500&q=80",
    description: "Take your music anywhere with rich bass."
  }
];

export default function RecommendationsPage() {
  return (
    <div className="space-y-10">
      <div className="sb-card p-6 sm:p-8 bg-gradient-to-br from-[var(--brand)] to-[var(--brand-2)] text-white">
        <h1 className="text-3xl sm:text-4xl font-bold tracking-tight mb-3">
          Curated Just For You
        </h1>
        <p className="text-white/80 max-w-2xl">
          Based on your browsing history and purchases, we have selected these items that we think you&apos;ll love. Discover your next favorite product today.
        </p>
      </div>

      <section>
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-semibold text-[var(--foreground)]">Top Picks for You</h2>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 sm:gap-6">
          {RECOMMENDED_PRODUCTS.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      </section>

      <section>
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-semibold text-[var(--foreground)]">Trending Now</h2>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 sm:gap-6">
          {TRENDING_PRODUCTS.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      </section>

      <div className="flex justify-center pt-4">
        <Link href="/products" className="sb-btn-outline">
          Explore all products
        </Link>
      </div>
    </div>
  );
}
