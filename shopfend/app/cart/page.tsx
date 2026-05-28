"use client";

import Image from "next/image";

import { useState, useEffect } from "react";
import { useSession, signIn } from "next-auth/react";
import Link from "next/link";
import { resolveApiUrl, shopbeApi, type CouponResponseDto } from "@/lib/shopbeApi";
import { useCart } from "../components/CartContext";
import { formatMoney } from "@/lib/format";
import { errorMessage } from "@/lib/errors";

export default function CartPage() {
  const { data: session, status } = useSession();
  const { cart, loading, updateQuantity, removeItem, refreshCart, applyCoupon, removeCoupon } = useCart();
  const [error, setError] = useState<string | null>(null);
  const [busyItem, setBusyItem] = useState<string | null>(null);
  const [coupons, setCoupons] = useState<CouponResponseDto[]>([]);
  const [applyingCoupon, setApplyingCoupon] = useState(false);

  useEffect(() => {
    shopbeApi.coupons.list()
      .then(setCoupons)
      .catch(() => {});
  }, []);

  const handleApplyCoupon = async (code: string) => {
    if (!code) return;
    try {
      setError(null);
      setApplyingCoupon(true);
      await applyCoupon(code);
    } catch (e: unknown) {
      setError(errorMessage(e, "Failed to apply coupon"));
    } finally {
      setApplyingCoupon(false);
    }
  };

  const handleRemoveCoupon = async () => {
    try {
      setError(null);
      await removeCoupon();
    } catch (e: unknown) {
      setError(errorMessage(e, "Failed to remove coupon"));
    }
  };

  const setQty = async (productVariantId: string, quantity: number) => {
    try {
      setBusyItem(productVariantId);
      await updateQuantity(productVariantId, quantity);
    } catch (e: unknown) {
      setError(errorMessage(e, "Failed to update quantity"));
    } finally {
      setBusyItem(null);
    }
  };

  const remove = async (productVariantId: string) => {
    try {
      setBusyItem(productVariantId);
      await removeItem(productVariantId);
    } catch (e: unknown) {
      setError(errorMessage(e, "Failed to remove item"));
    } finally {
      setBusyItem(null);
    }
  };

  if (status === "loading") return <p>Loading session…</p>;

  if (!session) {
    return (
      <div className="space-y-3">
        <h1 className="text-2xl font-semibold">Cart</h1>
        <p className="opacity-80">Sign in to view your cart.</p>
        <button
          onClick={() => signIn("keycloak")}
          className="sb-btn-primary"
        >
          Sign in
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold">Shopping cart</h1>
          <div className="text-sm text-slate-600">
            Review items before checkout.
          </div>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <button
            className="sb-btn-outline"
            onClick={refreshCart}
            disabled={loading}
          >
            {loading ? "Refreshing…" : "Refresh"}
          </button>
          <Link href="/products" className="sb-btn-outline">
            Continue shopping
          </Link>
        </div>
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 p-3 rounded text-sm">
          {error}
        </div>
      )}

      {!cart ? (
        <div className="sb-card p-6">
          {loading ? "Loading cart…" : "No cart loaded yet."}
        </div>
      ) : cart.items.length === 0 ? (
        <div className="sb-card p-6 space-y-2">
          <div className="font-semibold">Your cart is empty</div>
          <div className="text-sm text-slate-600">
            Add some products to see them here.
          </div>
          <Link href="/products" className="sb-btn-primary">
            Browse products
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
          <div className="lg:col-span-8 space-y-3">
            {cart.items.map((it) => (
              <div key={it.productVariantId} className="sb-card p-4">
                <div className="flex gap-4">
                  <div className="h-20 w-20 rounded-sm bg-gradient-to-br from-slate-50 to-slate-100 grid place-items-center text-slate-400 text-xs overflow-hidden shrink-0">
                    {it.imageUrl ? (
                      <Image
                        src={resolveApiUrl(it.imageUrl) || ""}
                        alt={it.productName ?? "Item"}
                        width={80}
                        height={80}
                        className="h-full w-full object-cover"
                        unoptimized
                      />
                    ) : (
                      "Img"
                    )}
                  </div>

                  <div className="min-w-0 flex-1">
                    <div className="font-medium text-slate-900 truncate">
                      {it.productName ?? "Item"}
                    </div>
                    <div className="text-xs text-slate-500 font-mono break-all mt-1">
                      variant: {it.productVariantId}
                    </div>
                    <div className="text-sm text-slate-700 mt-2">
                      {it.unitPrice != null ? (
                        <>
                          <span className="opacity-70">Price:</span>{" "}
                          <span className="font-semibold text-[var(--brand)]">
                            {formatMoney(it.unitPrice, cart.currency)}
                          </span>
                        </>
                      ) : null}
                    </div>
                  </div>

                  <div className="flex flex-col items-end gap-2">
                    <input
                      type="number"
                      min={1}
                      value={it.quantity}
                      disabled={busyItem === it.productVariantId}
                      onChange={(e) =>
                        setQty(
                          it.productVariantId,
                          Math.max(1, Number(e.target.value))
                        )
                      }
                      className="sb-input w-24"
                    />
                    <button
                      disabled={busyItem === it.productVariantId}
                      onClick={() => remove(it.productVariantId)}
                      className="sb-btn-outline"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>

          <aside className="lg:col-span-4">
            <div className="sb-card p-5 lg:sticky lg:top-28 space-y-4">
              <div className="font-semibold">Order summary</div>
              
              <div className="space-y-2 text-sm">
                <div className="flex items-center justify-between">
                  <span className="text-slate-600">Subtotal</span>
                  <span className="font-medium">
                    {formatMoney(cart.subtotal, cart.currency)}
                  </span>
                </div>

                {cart.discountAmount > 0 && (
                  <div className="flex items-center justify-between text-green-600 font-medium">
                    <span>Discount</span>
                    <span>-{formatMoney(cart.discountAmount, cart.currency)}</span>
                  </div>
                )}

                <div className="pt-2 border-t flex items-center justify-between text-base font-bold text-slate-900">
                  <span>Total</span>
                  <span>{formatMoney(cart.total, cart.currency)}</span>
                </div>
              </div>

              {!cart.couponCode ? (
                <div className="space-y-2">
                  <div className="text-sm font-medium text-slate-700">Available Coupons</div>
                  <select
                    className="sb-input w-full"
                    onChange={(e) => handleApplyCoupon(e.target.value)}
                    disabled={applyingCoupon || loading}
                    defaultValue=""
                  >
                    <option value="" disabled>Choose a coupon...</option>
                    {coupons.filter(c => c.isActive).map(c => {
                      const isDisabled = cart.subtotal < c.minOrderAmount || c.count <= 0;
                      return (
                        <option 
                          key={c.id} 
                          value={c.code} 
                          disabled={isDisabled}
                        >
                          {c.code} - {c.description || `${c.value}${c.discountType === 'Percentage' ? '%' : ''} off`}
                          {c.count <= 0 ? ' (Exhausted)' : ` (${c.count} left)`}
                          {cart.subtotal < c.minOrderAmount ? ` (Min. ${formatMoney(c.minOrderAmount, cart.currency)} required)` : ''}
                        </option>
                      );
                    })}
                  </select>
                  <div className="text-[10px] text-slate-400">
                    Coupons are automatically validated against your subtotal.
                  </div>
                </div>
              ) : (
                <div className="bg-green-50 border border-green-200 rounded p-3 space-y-2">
                  <div className="text-xs font-semibold text-green-700 uppercase tracking-wider">
                    Coupon Applied
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="font-mono font-bold text-green-800">{cart.couponCode}</span>
                    <button
                      onClick={handleRemoveCoupon}
                      className="text-xs text-green-700 hover:text-green-900 underline"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              )}

              <Link href="/checkout" className="sb-btn-primary w-full mt-4 block text-center">
                Checkout
              </Link>
              <div className="text-xs text-slate-500 mt-2">
                Taxes/shipping are demo-only in this UI.
              </div>
            </div>
          </aside>
        </div>
      )}
    </div>
  );
}


