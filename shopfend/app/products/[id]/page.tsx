"use client";

import { useSession, signIn } from "next-auth/react";
import { use, useEffect, useState, useMemo, useRef } from "react";
import {
  isAbortError,
  shopbeApi,
  BehaviorType,
  productResponseToListItem,
  type ProductDetail,
  type ProductListItem,
  type ProductVariantDto,
  type WishlistItem,
} from "@/lib/shopbeApi";
import Link from "next/link";
import { formatMoney } from "@/lib/format";
import { useCart } from "../../components/CartContext";
import { errorMessage } from "@/lib/errors";
import Image from "next/image";
import ProductCard from "../../components/ProductCard";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/+$/, "") ||
  "http://localhost:5072";

const COLOR_MAP: Record<string, string> = {
  "Red": "#EF4444",
  "Blue": "#3B82F6",
  "Green": "#10B981",
  "Black": "#000000",
  "White": "#FFFFFF",
  "Silver": "#C0C0C0",
  "Gold": "#FFD700",
  "Pink": "#EC4899",
  "Purple": "#8B5CF6",
  "Yellow": "#F59E0B",
  "Orange": "#F97316",
  "Grey": "#6B7280",
  "Brown": "#78350F",
  "Obsidian Black": "#0F0F0F",
  "Steel Gray": "#4A4A4A",
};

function resolveImageSrc(value?: string | null): string | undefined {
  if (!value) return undefined;
  if (/^https?:\/\//i.test(value)) return value;

  try {
    return new URL(value, API_BASE_URL).toString();
  } catch {
    return value;
  }
}

export default function ProductDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: session } = useSession();
  const { refreshCart, openDrawer } = useCart();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [similarProducts, setSimilarProducts] = useState<ProductListItem[]>([]);
  const [boughtTogether, setBoughtTogether] = useState<ProductListItem[]>([]);

  // Selection state
  const [selectedAttributes, setSelectedAttributes] = useState<Record<string, string>>({});
  const [selectedVariant, setSelectedVariant] = useState<ProductVariantDto | null>(null);
  const initializedRef = useRef<string | null>(null);

  useEffect(() => {
    const abort = new AbortController();
    (async () => {
      try {
        setError(null);
        const data = await shopbeApi.products.getById(
          id,
          session?.accessToken,
          abort.signal
        );
        setProduct(data);

        // Track view
        shopbeApi.tracking.track({
          behaviorType: BehaviorType.ProductView,
          productId: id,
          categoryId: data.categoryId,
        }, session?.accessToken).catch(() => {}); // fire and forget

        // Load similar
        const similar = await shopbeApi.recommendations.similar(id, 5, abort.signal);
        setSimilarProducts((similar ?? []).map(productResponseToListItem));

        // Load bought together
        const together = await shopbeApi.recommendations.frequentlyBoughtTogether(id, 5, abort.signal);
        setBoughtTogether((together ?? []).map(productResponseToListItem));

      } catch (e: unknown) {
        if (isAbortError(e)) return;
        setError(errorMessage(e, "Failed to load product"));
      }
    })();

    return () => abort.abort();
  }, [id, session?.accessToken]);

  // Group attributes dynamically
  const attributeGroups = useMemo(() => {
    const groups: Record<string, Set<string>> = {};
    product?.variants?.forEach((v) => {
      v.attributes?.forEach((attr) => {
        if (!groups[attr.name]) groups[attr.name] = new Set();
        groups[attr.name].add(attr.value);
      });
    });
    return Object.entries(groups).map(([name, values]) => ({
      name,
      values: Array.from(values).sort(),
    }));
  }, [product]);

  const requiredAttributeNames = useMemo(
    () => attributeGroups.map((group) => group.name),
    [attributeGroups]
  );

  const isAllAttributesSelected = useMemo(
    () => requiredAttributeNames.every((name) => Boolean(selectedAttributes[name])),
    [requiredAttributeNames, selectedAttributes]
  );

  // Set default selection to first available variant
  useEffect(() => {
    if (product?.variants?.length && initializedRef.current !== id) {
      const firstActive = product.variants.find(v => v.isActive) || product.variants[0];
      const initial: Record<string, string> = {};
      firstActive.attributes?.forEach((attr) => {
        initial[attr.name] = attr.value;
      });
      setSelectedAttributes(initial);
      setSelectedVariant(firstActive);
      initializedRef.current = id;
    }
  }, [product, id]);

  // Match variant based on selection
  useEffect(() => {
    if (!product?.variants || initializedRef.current !== id) return;
    const match = product.variants.find((v) => {
      if (!v.isActive) return false;

      if (requiredAttributeNames.length === 0) {
        return !v.attributes || v.attributes.length === 0;
      }

      if (!isAllAttributesSelected) return false;

      return requiredAttributeNames.every((name) =>
        v.attributes?.some(
          (attr) => attr.name === name && attr.value === selectedAttributes[name]
        )
      );
    });
    setSelectedVariant(match || null);
  }, [selectedAttributes, product, id, requiredAttributeNames, isAllAttributesSelected]);

  const isOptionAvailable = (attrName: string, value: string) => {
    if (!product?.variants) return false;
    
    return product.variants.some((v) => {
      if (!v.isActive) return false;

      // Does this variant have the target attribute value?
      const hasTarget = v.attributes?.some(a => a.name === attrName && a.value === value);
      if (!hasTarget) return false;

      // Does it also match all OTHER currently selected attributes?
      return v.attributes?.every(a => {
        if (a.name === attrName) return true; 
        if (selectedAttributes[a.name]) {
          return a.value === selectedAttributes[a.name];
        }
        return true;
      });
    });
  };

  const handleAttributeSelect = (attrName: string, value: string) => {
    setSelectedAttributes(prev => {
      const next = { ...prev };
      if (next[attrName] === value) {
        delete next[attrName];
      } else {
        next[attrName] = value;
      }
      return next;
    });
  };

  // Update image based on selection heuristic
  useEffect(() => {
    if (!product?.images || Object.keys(selectedAttributes).length === 0) return;
    
    const selectedValues = Object.values(selectedAttributes);
    for (const val of selectedValues) {
      if (!val) continue;
      const match = product.images.find(img => 
        img.altText?.toLowerCase().includes(val.toLowerCase()) || 
        img.imageUrl.toLowerCase().includes(val.toLowerCase())
      );
      if (match) {
        setSelectedImage(resolveImageSrc(match.imageUrl) ?? null);
        return;
      }
    }
  }, [selectedAttributes, product]);

  const hasDiscount = product?.discountPrice != null && product?.price != null && product.discountPrice < product.price;
  const displayPrice = selectedVariant?.price ?? (hasDiscount ? product?.discountPrice : (product?.price ?? product?.variants?.[0]?.price));
  const originalPrice = hasDiscount ? product?.price : null;
  const displayCurrency = selectedVariant?.currency ?? (product?.currency ?? product?.variants?.[0]?.currency);
  const primaryImageSrc = selectedImage ?? resolveImageSrc(
    product?.primaryImageUrl ?? product?.images?.[0]?.imageUrl
  );

  const [isWishlisted, setIsWishlisted] = useState(false);
  const [wishlistBusy, setWishlistBusy] = useState(false);

  useEffect(() => {
    if (session?.accessToken && id) {
      shopbeApi.wishlist.get(session.accessToken).then((items: WishlistItem[]) => {
        setIsWishlisted(items.some((i) => i.productId === id));
      }).catch(() => {});
    }
  }, [session?.accessToken, id]);

  const toggleWishlist = async () => {
    if (!session) {
      signIn("keycloak");
      return;
    }

    if (!session.accessToken || !id) return;

    setWishlistBusy(true);
    try {
      if (isWishlisted) {
        await shopbeApi.wishlist.remove(session.accessToken, id);
        setIsWishlisted(false);
      } else {
        await shopbeApi.wishlist.add(session.accessToken, id);
        setIsWishlisted(true);
      }
    } catch (e) {
      console.error("Wishlist toggle failed", e);
    } finally {
      setWishlistBusy(false);
    }
  };

  const addToCart = async () => {
    setError(null);

    if (!session?.accessToken) {
      setError("Please sign in to add items to cart.");
      return;
    }
    
    const variantId = selectedVariant?.id;
    if (!isAllAttributesSelected) {
      setError("Please select every option before adding this item to cart.");
      return;
    }

    if (!variantId) {
      setError("Please select a valid combination of options.");
      return;
    }

    try {
      setBusy(true);
      await shopbeApi.cart.addItem(
        session.accessToken,
        { productId: id, productVariantId: variantId, quantity },
        undefined
      );

      // Track AddToCart
      shopbeApi.tracking.track({
        behaviorType: BehaviorType.AddToCart,
        productId: id,
        categoryId: product?.categoryId,
        quantity: quantity,
        value: (selectedVariant?.price ?? product?.discountPrice ?? product?.price) ? (selectedVariant?.price ?? product!.discountPrice ?? product!.price) * (quantity || 1) : undefined,
        currency: displayCurrency ?? "VND"
      }, session.accessToken).catch(() => {});

      await refreshCart();
      openDrawer();
    } catch (e: unknown) {
      setError(errorMessage(e, "Failed to add to cart"));
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <div className="text-sm text-slate-500">Product detail</div>
          <h1 className="text-2xl font-semibold">{product?.name ?? "Product"}</h1>
        </div>
        <Link href="/products" className="text-sm text-slate-600 hover:underline">
          ← Back to products
        </Link>
      </div>

      {error && (
        <div className="border border-red-300 bg-white text-red-800 p-3 rounded text-sm">
          {error}
        </div>
      )}

      {!product ? (
        <div className="sb-card p-6">Loading…</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
          <div className="lg:col-span-5 space-y-4">
            <div className="sb-card overflow-hidden">
              <div className="aspect-square bg-linear-to-br from-slate-50 to-slate-100 grid place-items-center">
                {primaryImageSrc ? (
                  <Image
                    src={primaryImageSrc}
                    alt={product.name}
                    width={700}
                    height={700}
                    className="h-full w-full object-cover"
                    unoptimized
                  />
                ) : (
                  <div className="text-slate-400 text-sm">No image</div>
                )}
              </div>
            </div>

            {/* Thumbnail Gallery */}
            {product.images && product.images.length > 1 && (
              <div className="flex gap-2 overflow-x-auto pb-2 scrollbar-hide">
                {product.images.map((img) => (
                  <div 
                    key={img.id} 
                    onClick={() => setSelectedImage(resolveImageSrc(img.imageUrl) ?? null)}
                    className={`shrink-0 w-20 h-20 rounded-lg border-2 overflow-hidden cursor-pointer transition-all ${
                      resolveImageSrc(img.imageUrl ?? "") === primaryImageSrc ? 'border-brand' : 'border-transparent opacity-70 hover:opacity-100'
                    }`}
                  >
                    <Image
                      src={resolveImageSrc(img.imageUrl) ?? ""}
                      alt={product.name}
                      width={80}
                      height={80}
                      className="w-full h-full object-cover"
                      unoptimized
                    />
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="lg:col-span-7 space-y-4">
            <div className="sb-card p-5">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    {product.categoryName && (
                      <span className="text-[10px] bg-brand/10 text-brand font-bold px-2 py-0.5 rounded-full uppercase tracking-wider">
                        {product.categoryName}
                      </span>
                    )}
                    {product.brandName && (
                      <span className="text-[10px] bg-slate-100 text-slate-600 font-bold px-2 py-0.5 rounded-full uppercase tracking-wider">
                        {product.brandName}
                      </span>
                    )}
                  </div>
                  <div className="text-xl font-semibold text-slate-900">
                    {product.name}
                  </div>
                  <div className="flex items-center gap-2 mt-1">
                    <div className="text-xs text-slate-500">
                      {product.soldCount && product.soldCount > 0 ? (
                        <span className="bg-slate-100 px-2 py-0.5 rounded-full font-medium">
                          {product.soldCount >= 1000 
                            ? `${(product.soldCount / 1000).toFixed(1)}k` 
                            : product.soldCount} sold
                        </span>
                      ) : (
                        <span className="text-slate-400">No sales yet</span>
                      )}
                    </div>
                    {selectedVariant && (
                      <div className="text-[10px] text-slate-400 font-mono">
                        SKU: {selectedVariant.sku}
                      </div>
                    )}
                  </div>
                  {product.description ? (
                    <p className="text-sm text-slate-600 mt-2 line-clamp-3">
                      {product.description}
                    </p>
                  ) : null}
                </div>
                <div className="text-right shrink-0">
                  <div className="text-xs text-slate-500 mb-1">Price</div>
                  <div className="flex flex-col items-end">
                    <div className="text-2xl font-bold text-(--brand)">
                      {formatMoney(displayPrice ?? null, displayCurrency)}
                    </div>
                    {originalPrice && (
                      <div className="text-sm text-slate-400 line-through">
                        {formatMoney(originalPrice, displayCurrency)}
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Dynamic Attribute Selection Zones (Shopee Style) */}
              <div className="mt-10 space-y-12">
                {attributeGroups.map((group) => {
                  const isColor = group.name.toLowerCase() === "color";
                  const selectedValue = selectedAttributes[group.name];
                  
                  return (
                    <div key={group.name} className="flex flex-col gap-4">
                      {/* Zone Header */}
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <div className="w-1 h-4 bg-brand rounded-full"></div>
                          <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest">
                            {group.name}
                          </h3>
                        </div>
                        {selectedValue && (
                          <span className="text-[10px] font-bold text-brand bg-brand/5 px-2 py-1 rounded">
                            {selectedValue}
                          </span>
                        )}
                      </div>
                      
                      {/* Zone Values (Chips) */}
                      <div className="flex flex-wrap gap-3">
                        {group.values.map((val) => {
                          const isSelected = selectedAttributes[group.name] === val;
                          const isAvailable = isOptionAvailable(group.name, val);
                          const colorHex = isColor ? (COLOR_MAP[val] || val) : null;
                          
                          return (
                            <button
                              key={val}
                              disabled={!isAvailable}
                              onClick={() => handleAttributeSelect(group.name, val)}
                              className={`
                                relative flex items-center gap-3 px-6 py-2.5 text-sm rounded-xl border-2 transition-all duration-300
                                ${isSelected 
                                  ? "border-brand bg-white text-brand font-bold ring-4 ring-brand/10 shadow-sm" 
                                  : isAvailable 
                                    ? "border-slate-100 text-slate-600 hover:border-brand/40 bg-white" 
                                    : "border-slate-50 text-slate-200 bg-slate-50/50 cursor-not-allowed border-dashed opacity-40"}
                              `}
                            >
                              {isColor && colorHex && (
                                <span 
                                  className="w-4 h-4 rounded-full border border-black/10 shadow-inner shrink-0"
                                  style={{ backgroundColor: colorHex }}
                                />
                              )}
                              <span>{val}</span>
                              
                              {isSelected && (
                                <div className="absolute -top-2 -right-2 bg-brand text-white rounded-full p-0.5 shadow-md border-2 border-white">
                                  <svg className="w-3 h-3 fill-current" viewBox="0 0 20 20">
                                    <path d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" />
                                  </svg>
                                </div>
                              )}
                            </button>
                          );
                        })}
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Selection Recap Section */}
              <div className="mt-12 overflow-hidden rounded-2xl border border-slate-100 bg-slate-50/50">
                <div className="bg-slate-100/50 px-5 py-2 text-[10px] font-bold text-slate-400 uppercase tracking-widest border-b border-slate-100">
                  Selection Summary
                </div>
                <div className="p-5 flex flex-col md:flex-row md:items-center justify-between gap-6">
                  <div className="flex flex-wrap items-center gap-6">
                    {attributeGroups.map(g => (
                      <div key={g.name} className="flex flex-col gap-0.5">
                        <span className="text-[10px] text-slate-400 uppercase font-medium tracking-tighter">{g.name}</span>
                        <span className="text-sm font-bold text-slate-800">
                          {selectedAttributes[g.name] || <span className="text-slate-300 font-normal italic">Pending...</span>}
                        </span>
                      </div>
                    ))}
                  </div>
                  
                  {selectedVariant && (
                    <div className="flex flex-col items-end gap-1">
                      <div className="text-[10px] font-bold text-green-600 uppercase tracking-widest flex items-center gap-1">
                        <div className="w-1.5 h-1.5 rounded-full bg-green-500 animate-pulse"></div>
                        Ready to Ship
                      </div>
                      <div className="text-xs font-black text-slate-900">
                        {selectedVariant.stockQuantity} pieces available
                      </div>
                    </div>
                  )}
                </div>
              </div>

              <div className="mt-6 pt-6 flex flex-wrap items-center gap-6">
                <div className="flex items-center gap-4 bg-slate-100 p-1.5 rounded-xl">
                  <button 
                    onClick={() => setQuantity(q => Math.max(1, q - 1))}
                    className="w-10 h-10 flex items-center justify-center bg-white rounded-lg shadow-sm hover:text-brand transition-colors text-lg font-bold"
                  >
                    −
                  </button>
                  <input
                    type="number"
                    min={1}
                    max={selectedVariant?.stockQuantity ?? 99}
                    value={quantity}
                    onChange={(e) =>
                      setQuantity(Math.max(1, Math.min(selectedVariant?.stockQuantity ?? 99, Number(e.target.value))))
                    }
                    className="w-10 text-center bg-transparent focus:outline-none font-bold text-slate-700"
                  />
                  <button 
                    onClick={() => setQuantity(q => q + 1)}
                    className="w-10 h-10 flex items-center justify-center bg-white rounded-lg shadow-sm hover:text-brand transition-colors text-lg font-bold"
                  >
                    +
                  </button>
                </div>

                <div className="flex items-center gap-3 flex-1">
                  {session ? (
                    <button
                      disabled={busy || !isAllAttributesSelected || !selectedVariant || selectedVariant.stockQuantity === 0}
                      onClick={addToCart}
                      className="sb-btn-primary flex-1 py-4 rounded-xl text-sm font-bold shadow-lg shadow-brand/20 disabled:shadow-none disabled:opacity-50 disabled:grayscale transition-all hover:scale-[1.02] active:scale-95"
                    >
                      {busy
                        ? "Processing…"
                        : !isAllAttributesSelected
                          ? "SELECT ALL OPTIONS"
                          : selectedVariant?.stockQuantity === 0
                            ? "OUT OF STOCK"
                            : "ADD TO CART"}
                    </button>
                  ) : (
                    <button
                      onClick={() => signIn("keycloak")}
                      className="sb-btn-primary flex-1 py-4 rounded-xl text-sm font-bold shadow-lg shadow-brand/20"
                    >
                      SIGN IN TO PURCHASE
                    </button>
                  )}
                  <button
                    onClick={toggleWishlist}
                    disabled={wishlistBusy}
                    className={`p-4 rounded-xl border-2 transition-all active:scale-95 ${
                      isWishlisted 
                      ? 'border-brand/20 bg-brand/5 text-brand' 
                      : 'border-gray-100 bg-white text-gray-400 hover:text-brand hover:border-brand/20'
                    }`}
                    title={isWishlisted ? "Remove from Wishlist" : "Add to Wishlist"}
                  >
                    <svg className={`w-6 h-6 ${isWishlisted ? 'fill-current' : 'fill-none'}`} stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                       <path strokeLinecap="round" strokeLinejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                    </svg>
                  </button>
                </div>

              </div>
            </div>
          </div>
        </div>
      )}

      {boughtTogether.length > 0 && (
        <section className="mt-12 space-y-4">
          <h2 className="text-xl font-bold text-slate-900 flex items-center gap-2">
            Frequently Bought Together
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
            {boughtTogether.map(p => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}

      {similarProducts.length > 0 && (
        <section className="mt-12 space-y-4">
          <h2 className="text-xl font-bold text-slate-900">Similar Products</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
            {similarProducts.map(p => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
