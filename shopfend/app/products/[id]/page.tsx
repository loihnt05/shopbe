"use client";

import { useSession, signIn } from "next-auth/react";
import { use, useEffect, useState } from "react";
import {
  isAbortError,
  shopbeApi,
  type ProductDetail,
} from "@/lib/shopbeApi";
import Link from "next/link";
import { formatMoney } from "@/lib/format";
import { errorMessage } from "@/lib/errors";
import Image from "next/image";

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

export default function ProductDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: session } = useSession();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

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
      } catch (e: unknown) {
        if (isAbortError(e)) return;
        setError(errorMessage(e, "Failed to load product"));
      }
    })();

    return () => abort.abort();
  }, [id, session?.accessToken]);

  const primaryVariantId = product?.variants?.[0]?.id;
  const hasDiscount = product?.discountPrice != null && product?.price != null && product.discountPrice < product.price;
  const displayPrice = hasDiscount ? product?.discountPrice : (product?.price ?? product?.variants?.[0]?.price);
  const originalPrice = hasDiscount ? product?.price : null;
  const displayCurrency = product?.currency ?? product?.variants?.[0]?.currency;
  const primaryImageSrc = resolveImageSrc(
    product?.primaryImageUrl ?? product?.images?.[0]?.imageUrl
  );

  const addToCart = async () => {
    setMessage(null);
    setError(null);

    if (!session?.accessToken) {
      setError("Please sign in to add items to cart.");
      return;
    }
    if (!primaryVariantId) {
      setError(
        "This product has no variants in the database, so it cannot be added to cart."
      );
      return;
    }

    try {
      setBusy(true);
      await shopbeApi.cart.addItem(
        session.accessToken,
        { productId: id, productVariantId: primaryVariantId, quantity },
        undefined
      );
      setMessage("Added to cart.");
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
        <div className="border  border-red-300 bg-white text-red-800 p-3 rounded text-sm">
          {error}
        </div>
      )}
      {message && (
        <div className="border text-green-600 border-emerald-300 bg-emerald-50 p-3 rounded text-sm">
          {message}
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
                    className={`shrink-0 w-20 h-20 rounded-lg border-2 overflow-hidden cursor-pointer transition-all ${
                      resolveImageSrc(img.imageUrl) === primaryImageSrc ? 'border-brand' : 'border-transparent opacity-70 hover:opacity-100'
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
                  </div>
                  {product.description ? (
                    <p className="text-sm text-slate-600 mt-2">
                      {product.description}
                    </p>
                  ) : null}
                </div>
                <div className="text-right">
                  <div className="text-xs text-slate-500">Price</div>
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

              <div className="mt-4 flex flex-wrap items-center gap-3">
                <div className="flex items-center gap-2">
                  <label className="text-sm text-slate-600">Qty</label>
                  <input
                    type="number"
                    min={1}
                    value={quantity}
                    onChange={(e) =>
                      setQuantity(Math.max(1, Number(e.target.value)))
                    }
                    className="sb-input w-24"
                  />
                </div>

                {session ? (
                  <button
                    disabled={busy}
                    onClick={addToCart}
                    className="sb-btn-primary disabled:opacity-60"
                  >
                    {busy ? "Adding…" : "Add to cart"}
                  </button>
                ) : (
                  <button
                    onClick={() => signIn("keycloak")}
                    className="sb-btn-primary"
                  >
                    Sign in to buy
                  </button>
                )}

                <Link href="/cart" className="sb-btn-outline">
                  Go to cart
                </Link>
              </div>
            </div>

            <div className="sb-card p-5">
              <div className="font-semibold text-sm mb-3">Available Variants</div>
              {product.variants && product.variants.length > 0 ? (
                <div className="flex flex-wrap gap-2">
                  {product.variants.map((v) => (
                    <div 
                      key={v.id} 
                      className={`px-3 py-2 rounded-lg border text-xs transition-all ${
                        v.id === primaryVariantId ? 'border-brand bg-orange-50 text-brand font-bold' : 'border-slate-200 text-slate-600 hover:border-slate-300'
                      }`}
                    >
                      <div className="flex flex-col gap-1">
                        <span>{v.sku}</span>
                        <span className="opacity-80 font-medium">
                          {formatMoney(v.price, v.currency)}
                        </span>
                        {v.attributeValues && v.attributeValues.length > 0 && (
                          <div className="flex gap-1 mt-1">
                            {v.attributeValues.map((attr, idx) => (
                              <span key={idx} className="bg-white/50 px-1 rounded border border-black/5 text-[8px] uppercase">{attr}</span>
                            ))}
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-sm text-red-600">No variants found</div>
              )}
              <div className="text-[10px] text-slate-400 mt-3 italic">
                * This demo currently uses the first variant for add-to-cart.
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}


