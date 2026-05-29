"use client";

import React from "react";
import Link from "next/link";
import Image from "next/image";
import { formatMoney } from "@/lib/format";
import { StarIcon, TrashIcon, CartIcon } from "./icons";
import type { ProductListItem } from "@/lib/shopbeApi";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/+$/, "") ||
  "http://localhost:5072";

function resolveImageSrc(value?: string | null): string | undefined {
  if (!value) return undefined;
  if (/^https?:\/\//i.test(value)) return value;

  try {
    return new URL(value, API_BASE_URL).toString();
  } catch {
    return value;
  }
}

interface WishlistItemCardProps {
  product: ProductListItem;
  isSelected: boolean;
  onSelect: (selected: boolean) => void;
  onRemove: () => void;
  onMoveToCart: () => void;
}

export default function WishlistItemCard({
  product,
  isSelected,
  onSelect,
  onRemove,
  onMoveToCart,
}: WishlistItemCardProps) {
  const hasDiscount = product.discountPrice != null && product.price != null && product.discountPrice < product.price;
  const displayPrice = hasDiscount ? product.discountPrice : product.price;
  const originalPrice = hasDiscount ? product.price : null;
  
  const discountPercentage = hasDiscount 
    ? Math.round((1 - (product.discountPrice! / product.price!)) * 100) 
    : 0;

  const thumbnailSrc = resolveImageSrc(product.primaryImageUrl ?? "");
  const outOfStock = product.totalStockQuantity === 0;

  return (
    <div className={`group relative bg-white rounded-xl overflow-hidden border transition-all duration-300 flex flex-col h-full ${isSelected ? 'border-brand ring-1 ring-brand' : 'border-gray-100 hover:shadow-xl hover:scale-[1.01]'}`}>
      {/* Selection Checkbox */}
      <div className="absolute top-2 left-2 z-20">
        <input
          type="checkbox"
          checked={isSelected}
          onChange={(e) => onSelect(e.target.checked)}
          className="w-5 h-5 rounded border-gray-300 text-brand focus:ring-brand cursor-pointer shadow-sm"
        />
      </div>

      {/* Remove Button */}
      <button
        onClick={onRemove}
        className="absolute top-2 right-2 z-20 p-2 bg-white/80 backdrop-blur-sm rounded-full text-gray-400 hover:text-red-500 hover:bg-white shadow-sm transition-colors"
        title="Remove from wishlist"
      >
        <TrashIcon className="w-4 h-4" />
      </button>

      {/* Product Image Link */}
      <Link href={`/products/${product.id}`} className="block">
        <div className="relative aspect-square w-full bg-slate-50 overflow-hidden">
          {thumbnailSrc ? (
            <Image
              src={thumbnailSrc}
              alt={product.name}
              fill
              sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 20vw"
              className={`object-cover transition-transform duration-500 group-hover:scale-110 ${outOfStock ? 'grayscale opacity-60' : ''}`}
              unoptimized
            />
          ) : (
            <div className="flex items-center justify-center h-full text-slate-400 text-xs">No image</div>
          )}
          
          {outOfStock && (
            <div className="absolute inset-0 flex items-center justify-center">
              <span className="bg-black/60 text-white text-[10px] font-bold px-3 py-1.5 rounded-full uppercase tracking-widest">
                Out of Stock
              </span>
            </div>
          )}

          {hasDiscount && !outOfStock && (
            <div className="absolute top-2 right-2 bg-yellow-300 text-brand text-[10px] font-bold px-2 py-1 rounded-full shadow-sm flex items-center gap-0.5">
              <span>{discountPercentage}%</span>
              <span className="uppercase text-[8px] opacity-80">Off</span>
            </div>
          )}
        </div>
      </Link>

      <div className="p-3 flex flex-col flex-1">
        <div className="flex items-center gap-1 mb-1">
           {product.brandName && (
            <span className="text-[10px] bg-brand/10 text-brand px-1.5 py-0.5 rounded font-bold uppercase">
              {product.brandName}
            </span>
          )}
          {product.categoryName && (
            <div className="text-[10px] text-gray-400 font-medium uppercase truncate">
              {product.categoryName}
            </div>
          )}
        </div>
        
        <Link href={`/products/${product.id}`} className="block">
          <div className="line-clamp-2 text-sm text-gray-800 mb-2 min-h-[40px] leading-snug group-hover:text-brand transition-colors font-medium">
            {product.name}
          </div>
        </Link>

        <div className="mt-auto">
          <div className="flex flex-col gap-0.5 mb-2">
            <div className="flex items-center gap-2">
              <div className="text-brand text-lg font-bold">
                {formatMoney(displayPrice, product.currency)}
              </div>
              {originalPrice && (
                <div className="text-xs text-gray-400 line-through">
                  {formatMoney(originalPrice, product.currency)}
                </div>
              )}
            </div>
          </div>
          
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center gap-1">
              {product.averageRating && product.averageRating > 0 ? (
                <div className="flex items-center gap-0.5 text-yellow-500">
                  <StarIcon className="w-3 h-3 fill-current" />
                  <span className="text-[11px] font-bold">{product.averageRating.toFixed(1)}</span>
                </div>
              ) : null}
              <span className="text-[10px] text-gray-400">
                {product.soldCount && product.soldCount > 0 
                  ? `${product.soldCount >= 1000 ? (product.soldCount / 1000).toFixed(1) + 'k' : product.soldCount} sold`
                  : "0 sold"}
              </span>
            </div>
          </div>

          <button
            onClick={onMoveToCart}
            disabled={outOfStock}
            className={`w-full py-2 rounded-lg flex items-center justify-center gap-2 text-sm font-bold transition-all ${
              outOfStock 
              ? 'bg-gray-100 text-gray-400 cursor-not-allowed' 
              : 'bg-brand text-white hover:bg-brand/90 active:scale-95 shadow-md hover:shadow-brand/20'
            }`}
          >
            <CartIcon className="w-4 h-4" />
            <span>Add to Cart</span>
          </button>
        </div>
      </div>
    </div>
  );
}
