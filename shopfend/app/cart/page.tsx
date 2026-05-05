"use client";

import { useEffect, useMemo, useState } from "react";
import { useSession, signIn } from "next-auth/react";
import Link from "next/link";
import { isAbortError, shopbeApi, type CartDto } from "../../lib/shopbeApi";

export default function CartPage() {
  const { data: session, status } = useSession();
  const [cart, setCart] = useState<CartDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [busyItem, setBusyItem] = useState<string | null>(null);

  const abort = useMemo(() => new AbortController(), []);

  const refresh = async () => {
    if (!session?.accessToken) return;
    try {
      setLoading(true);
      setError(null);
      const data = await shopbeApi.cart.getMyCart(session.accessToken, abort.signal);
      setCart(data);
    } catch (e: any) {
      if (isAbortError(e)) return;
      setError(e?.message ?? "Failed to load cart");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (status === "authenticated") refresh();
    return () => abort.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status]);

  const setQty = async (productVariantId: string, quantity: number) => {
    if (!session?.accessToken) return;
    try {
      setBusyItem(productVariantId);
      const data = await shopbeApi.cart.setQuantity(
        session.accessToken,
        productVariantId,
        { quantity }
      );
      setCart(data);
    } catch (e: any) {
      setError(e?.message ?? "Failed to update quantity");
    } finally {
      setBusyItem(null);
    }
  };

  const remove = async (productVariantId: string) => {
    if (!session?.accessToken) return;
    try {
      setBusyItem(productVariantId);
      const data = await shopbeApi.cart.removeItem(session.accessToken, productVariantId);
      setCart(data);
    } catch (e: any) {
      setError(e?.message ?? "Failed to remove item");
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
          className="bg-blue-600 text-white px-4 py-2 rounded"
        >
          Sign in
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Cart</h1>
        <div className="flex items-center gap-3 text-sm">
          <button className="underline" onClick={refresh} disabled={loading}>
            {loading ? "Refreshing…" : "Refresh"}
          </button>
          <Link href="/products" className="underline">
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
        <p>{loading ? "Loading cart…" : "No cart loaded yet."}</p>
      ) : cart.items.length === 0 ? (
        <div className="space-y-2">
          <p className="opacity-80">Your cart is empty.</p>
          <Link href="/products" className="underline">
            Browse products
          </Link>
        </div>
      ) : (
        <div className="space-y-3">
          <div className="space-y-2">
            {cart.items.map((it) => (
              <div
                key={it.productVariantId}
                className="border rounded p-3 flex items-center justify-between gap-4"
              >
                <div className="min-w-0">
                  <div className="font-medium truncate">
                    {it.productName ?? "Item"}
                  </div>
                  <div className="text-xs opacity-70 font-mono break-all">
                    variant: {it.productVariantId}
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <input
                    type="number"
                    min={1}
                    value={it.quantity}
                    disabled={busyItem === it.productVariantId}
                    onChange={(e) =>
                      setQty(it.productVariantId, Math.max(1, Number(e.target.value)))
                    }
                    className="border rounded px-2 py-1 w-20"
                  />
                  <button
                    disabled={busyItem === it.productVariantId}
                    onClick={() => remove(it.productVariantId)}
                    className="border px-3 py-1 rounded"
                  >
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div className="border-t pt-3 flex items-center justify-between">
            <div>
              <div className="text-sm opacity-70">Total</div>
              <div className="font-semibold">
                {cart.totalAmount ?? "—"} {cart.currency ?? ""}
              </div>
            </div>
            <Link
              href="/checkout"
              className="bg-indigo-600 text-white px-4 py-2 rounded"
            >
              Checkout
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}


