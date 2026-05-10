"use client";

import { useSession, signIn } from "next-auth/react";
import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import {
  isAbortError,
  shopbeApi,
  type CartDto,
  type CreateOrderResponse,
  type CreateStripePaymentIntentResponse,
} from "@/lib/shopbeApi";
import { formatMoney } from "@/lib/format";
import { errorMessage } from "@/lib/errors";

export default function CheckoutPage() {
  const { data: session, status } = useSession();

  const [cart, setCart] = useState<CartDto | null>(null);
  const [loadingCart, setLoadingCart] = useState(false);
  const [creating, setCreating] = useState(false);
  const [order, setOrder] = useState<CreateOrderResponse | null>(null);
  const [paymentIntent, setPaymentIntent] =
    useState<CreateStripePaymentIntentResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  const abortRef = useRef<AbortController | null>(null);

  const getSignal = () => {
    abortRef.current?.abort();
    abortRef.current = new AbortController();
    return abortRef.current.signal;
  };

  const loadCart = async () => {
    if (!session?.accessToken) return;
    try {
      setLoadingCart(true);
      setError(null);
      const data = await shopbeApi.cart.getMyCart(
        session.accessToken,
        getSignal()
      );
      setCart(data);
    } catch (e: unknown) {
      if (isAbortError(e)) return;
      setError(errorMessage(e, "Failed to load cart"));
    } finally {
      setLoadingCart(false);
    }
  };

  useEffect(() => {
    if (status === "authenticated") loadCart();
    return () => abortRef.current?.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status]);

  const doCheckout = async () => {
    if (!session?.accessToken) return;
    if (!cart || cart.items.length === 0) {
      setError("Cart is empty.");
      return;
    }

    setError(null);
    setCreating(true);
    setOrder(null);
    setPaymentIntent(null);

    try {
      // Create order using default address if user has one.
      // If the user has no saved address, backend requires shipping fields.
      // For a basic test flow, start with default behavior.
      const created = await shopbeApi.orders.create(session.accessToken, {
        useDefaultAddressIfAvailable: true,
        shippingReceiverName: session.user?.name || "Demo User",
        shippingPhone: "0123456789",
        shippingAddressLine: "123 Demo Street",
        shippingCity: "Demo City",
        shippingDistrict: "Demo District",
        shippingWard: "Demo Ward"
      });
      setOrder(created);

      // Create Stripe PaymentIntent for that order.
      const pi = await shopbeApi.payments.createStripePaymentIntent(
        session.accessToken,
        { orderId: created.id }
      );

      setPaymentIntent(pi);
    } catch (e: unknown) {
      setError(errorMessage(e, "Checkout failed"));
    } finally {
      setCreating(false);
    }
  };

  if (status === "loading") return <p>Loading session…</p>;

  if (!session) {
    return (
      <div className="space-y-3">
        <h1 className="text-2xl font-semibold">Checkout</h1>
        <p className="opacity-80">Sign in to checkout.</p>
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
          <h1 className="text-2xl font-semibold">Checkout</h1>
          <div className="text-sm text-slate-600">
            Test flow: create order → create Stripe PaymentIntent
          </div>
        </div>
        <Link href="/cart" className="sb-btn-outline">
          ← Back to cart
        </Link>
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 p-3 rounded text-sm text-red-800">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
        <div className="lg:col-span-7 space-y-4">
          <div className="sb-card p-5">
            <div className="flex items-center justify-between">
              <div>
                <div className="font-semibold">Cart</div>
                <div className="text-sm text-slate-600">
                  {loadingCart ? "Loading…" : `${cart?.items.length ?? 0} items`}
                </div>
              </div>
              <button className="sb-btn-outline" onClick={loadCart}>
                Refresh
              </button>
            </div>

            <div className="mt-4 space-y-2">
              {cart?.items?.length ? (
                cart.items.map((it) => (
                  <div
                    key={it.productVariantId}
                    className="flex items-center justify-between gap-3 rounded-sm border border-black/10 bg-white p-3"
                  >
                    <div className="min-w-0">
                      <div className="font-medium truncate">
                        {it.productName ?? "Item"}
                      </div>
                      <div className="text-xs text-slate-500">
                        Qty: <span className="font-medium">{it.quantity}</span>
                      </div>
                    </div>
                    <div className="text-sm font-semibold text-[var(--brand)]">
                      {it.price != null
                        ? formatMoney(it.price * it.quantity, it.currency)
                        : "—"}
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-sm text-slate-600">Cart is empty.</div>
              )}
            </div>
          </div>

          <div className="sb-card p-5">
            <div className="font-semibold">Payment</div>
            <div className="text-sm text-slate-600 mt-1">
              Click below to create an order and a Stripe PaymentIntent.
            </div>

            <button
              disabled={creating}
              onClick={doCheckout}
              className="sb-btn-primary w-full mt-4 disabled:opacity-60"
            >
              {creating ? "Creating…" : "Create order + Stripe PaymentIntent"}
            </button>
          </div>
        </div>

        <aside className="lg:col-span-5">
          <div className="sb-card p-5 lg:sticky lg:top-28 space-y-3">
            <div className="font-semibold">Summary</div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-slate-600">Total</span>
              <span className="text-lg font-bold text-slate-900">
                {formatMoney(cart?.totalAmount ?? null, cart?.currency)}
              </span>
            </div>

            {order ? (
              <div className="rounded-sm border border-black/10 p-3 bg-slate-50">
                <div className="text-sm font-medium">Order created</div>
                <div className="text-xs text-slate-600 mt-1">
                  <span className="opacity-70">OrderId:</span>{" "}
                  <span className="font-mono break-all">{order.id}</span>
                </div>
              </div>
            ) : null}

            {paymentIntent ? (
              <div className="rounded-sm border border-black/10 p-3 bg-slate-50">
                <div className="text-sm font-medium">Stripe PaymentIntent</div>
                <div className="text-xs text-slate-600 mt-1">
                  <div className="opacity-70">paymentIntentId</div>
                  <div className="font-mono break-all">
                    {paymentIntent.paymentIntentId}
                  </div>
                </div>
              </div>
            ) : null}

            <div className="text-xs text-slate-500">
              Tip: integrate Stripe.js Elements to confirm payment using the
              clientSecret.
            </div>
          </div>
        </aside>
      </div>

      <div className="text-sm text-slate-600">
        If you get an error about shipping address, create a saved address in the
        backend (user profile endpoints) or extend this page to submit shipping
        fields.
      </div>
    </div>
  );
}


