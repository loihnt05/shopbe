"use client";

import React, { useState, useMemo } from "react";
import Image from "next/image";
import { formatMoney } from "@/lib/format";
import { resolveApiUrl, type ProductListItem } from "@/lib/shopbeApi";
import { XIcon } from "./icons";
import { useCart } from "./CartContext";
import { useSession } from "next-auth/react";

interface QuickAddToCartModalProps {
  product: ProductListItem;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

export default function QuickAddToCartModal({
  product,
  isOpen,
  onClose,
  onSuccess,
}: QuickAddToCartModalProps) {
  const { data: session } = useSession();
  const { addItem } = useCart();
  const [selectedAttributes, setSelectedAttributes] = useState<Record<string, string>>({});
  const [quantity, setQuantity] = useState(1);
  const [isAdding, setIsAdding] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Group attributes from all variants
  const attributeGroups = useMemo(() => {
    const groups: Record<string, Set<string>> = {};
    product.variants?.forEach((v) => {
      v.attributes?.forEach((a) => {
        if (!groups[a.name]) groups[a.name] = new Set();
        groups[a.name].add(a.value);
      });
    });
    return Object.fromEntries(
      Object.entries(groups).map(([name, values]) => [name, Array.from(values)])
    );
  }, [product.variants]);

  const attributeNames = Object.keys(attributeGroups);
  const isAllAttributesSelected = attributeNames.every((name) => selectedAttributes[name]);

  // Find the variant that matches all selected attributes
  const selectedVariant = useMemo(() => {
    if (!product.variants) return null;
    if (!isAllAttributesSelected) return null;

    return product.variants.find((v) => {
      if (!v.isActive) return false;

      return attributeNames.every((name) => {
        const selectedValue = selectedAttributes[name];
        return v.attributes?.some((a) => a.name === name && a.value === selectedValue);
      });
    });
  }, [product.variants, selectedAttributes, attributeNames, isAllAttributesSelected]);

  const outOfStock = selectedVariant ? (selectedVariant.stockQuantity ?? 0) <= 0 : false;
  const canAddToCart = isAllAttributesSelected && Boolean(selectedVariant) && !outOfStock;

  const handleAddToCart = async () => {
    if (!selectedVariant || !session?.accessToken) return;

    setIsAdding(true);
    setError(null);
    try {
      await addItem(product.id, selectedVariant.id, quantity);
      onSuccess?.();
      onClose();
    } catch (err) {
      console.error(err);
      setError("Failed to add to cart. Please try again.");
    } finally {
      setIsAdding(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-end sm:items-center justify-center p-0 sm:p-4">
      {/* Overlay */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm transition-opacity"
        onClick={onClose}
      />
      
      {/* Modal Content */}
      <div className="relative w-full max-w-lg bg-white rounded-t-2xl sm:rounded-2xl shadow-2xl overflow-hidden animate-in slide-in-from-bottom duration-300">
        <div className="p-4 sm:p-6">
          {/* Header */}
          <div className="flex justify-between items-start mb-6">
            <div className="flex gap-4">
              <div className="relative w-24 h-24 bg-slate-50 rounded-lg overflow-hidden border border-gray-100 flex-shrink-0">
                <Image
                  src={resolveApiUrl(product.primaryImageUrl) || ""}
                  alt={product.name}
                  fill
                  className="object-cover"
                  unoptimized
                />
              </div>
              <div className="flex flex-col justify-center">
                <div className="text-brand text-xl font-bold mb-1">
                  {formatMoney(selectedVariant?.price ?? product.price, product.currency)}
                </div>
                <div className="text-xs text-gray-500">
                  Stock: {selectedVariant ? selectedVariant.stockQuantity : product.totalStockQuantity}
                </div>
                {selectedVariant?.sku && (
                  <div className="text-[10px] text-gray-400 mt-1">
                    SKU: {selectedVariant.sku}
                  </div>
                )}
              </div>
            </div>
            <button 
              onClick={onClose}
              className="p-2 text-gray-400 hover:text-gray-600 transition-colors"
            >
              <XIcon className="w-6 h-6" />
            </button>
          </div>

          <div className="max-h-[60vh] overflow-y-auto pr-2 custom-scrollbar">
            {/* Attributes */}
            {attributeNames.map((name) => (
              <div key={name} className="mb-6">
                <h4 className="text-sm font-bold text-gray-900 mb-3">{name}</h4>
                <div className="flex flex-wrap gap-2">
                  {attributeGroups[name].map((value) => {
                    const isSelected = selectedAttributes[name] === value;
                    return (
                      <button
                        key={value}
                        onClick={() => setSelectedAttributes(prev => ({ ...prev, [name]: value }))}
                        className={`px-4 py-2 rounded-lg text-sm transition-all border ${
                          isSelected 
                          ? 'bg-brand/5 border-brand text-brand font-bold ring-1 ring-brand' 
                          : 'bg-white border-gray-200 text-gray-700 hover:border-brand hover:text-brand'
                        }`}
                      >
                        {value}
                      </button>
                    );
                  })}
                </div>
              </div>
            ))}

            {/* Quantity */}
            <div className="mb-8">
              <h4 className="text-sm font-bold text-gray-900 mb-3">Quantity</h4>
              <div className="flex items-center gap-4">
                <div className="flex items-center border border-gray-200 rounded-lg overflow-hidden">
                  <button 
                    onClick={() => setQuantity(Math.max(1, quantity - 1))}
                    className="px-4 py-2 hover:bg-gray-50 text-gray-600 transition-colors"
                  >
                    −
                  </button>
                  <span className="px-4 py-2 min-w-[3rem] text-center font-bold text-gray-900">
                    {quantity}
                  </span>
                  <button 
                    onClick={() => setQuantity(quantity + 1)}
                    className="px-4 py-2 hover:bg-gray-50 text-gray-600 transition-colors"
                  >
                    +
                  </button>
                </div>
              </div>
            </div>
          </div>

          {error && (
            <div className="mb-4 p-3 bg-red-50 text-red-600 text-sm rounded-lg border border-red-100">
              {error}
            </div>
          )}

          {/* Action Button */}
          <button
            onClick={handleAddToCart}
            disabled={!canAddToCart || isAdding}
            className={`w-full py-4 rounded-xl font-bold text-lg shadow-lg transition-all active:scale-[0.98] ${
              !isAllAttributesSelected
                ? 'bg-gray-100 text-gray-400 cursor-not-allowed shadow-none'
              : !selectedVariant || outOfStock
              ? 'bg-gray-200 text-gray-500 cursor-not-allowed shadow-none'
              : 'bg-brand text-white hover:bg-brand/90 hover:shadow-brand/25'
            }`}
          >
            {isAdding ? (
              <span className="flex items-center justify-center gap-2">
                <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                Processing...
              </span>
            ) : !selectedVariant || outOfStock ? (
              "Out of Stock"
            ) : isAllAttributesSelected ? (
              "Add to Cart"
            ) : (
              "Please select attributes"
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
