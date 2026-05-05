"use client";

import { useSession, signIn } from "next-auth/react";
import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import {
  isAbortError,
  shopbeApi,
  type CartDto,
  type CreateOrderResponse,
  type CreateStripePaymentIntentResponse,
} from "../../lib/shopbeApi";

export default function CheckoutPage() {
  const { data: session, status } = useSession();

  const [cart, setCart] = useState<CartDto | null>(null);
  const [loadingCart, setLoadingCart] = useState(false);
  const [creating, setCreating] = useState(false);
  const [order, setOrder] = useState<CreateOrderResponse | null>(null);
  const [paymentIntent, setPaymentIntent] =
    useState<CreateStripePaymentIntentResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  const abort = useMemo(() => new AbortController(), []);

  const loadCart = async () => {
    if (!session?.accessToken) return;
    try {
      setLoadingCart(true);
      setError(null);
      const data = await shopbeApi.cart.getMyCart(session.accessToken, abort.signal);
      setCart(data);
    } catch (e: any) {
      if (isAbortError(e)) return;
      setError(e?.message ?? "Failed to load cart");
    } finally {
      setLoadingCart(false);
    }
  };

  useEffect(() => {
    if (status === "authenticated") loadCart();
    return () => abort.abort();
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
      });
      setOrder(created);

      // Create Stripe PaymentIntent for that order.
      const pi = await shopbeApi.payments.createStripePaymentIntent(
        session.accessToken,
        { orderId: created.id }
      );

      setPaymentIntent(pi);
    } catch (e: any) {
      setError(e?.message ?? "Checkout failed");
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
        <h1 className="text-2xl font-semibold">Checkout (test flow)</h1>
        <Link href="/cart" className="underline text-sm">
          Back to cart
        </Link>
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 p-3 rounded text-sm">
          {error}
        </div>
      )}

      <div className="border rounded p-4 space-y-2">
        <div className="flex items-center justify-between">
          <div>
            <div className="font-medium">Cart snapshot</div>
            <div className="text-sm opacity-70">
              {loadingCart ? "Loading…" : `${cart?.items.length ?? 0} items`}
            </div>
          </div>
          <button className="underline text-sm" onClick={loadCart}>
            Refresh
          </button>
        </div>

        {cart?.items?.length ? (
          <ul className="text-sm list-disc pl-5">
            {cart.items.map((it) => (
              <li key={it.productVariantId}>
                {it.productName ?? "Item"} × {it.quantity}
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-sm opacity-70">Cart is empty.</p>
        )}
      </div>

      <button
        disabled={creating}
        onClick={doCheckout}
        className="bg-indigo-600 disabled:opacity-50 text-white px-4 py-2 rounded"
      >
        {creating ? "Creating order…" : "Create order + Stripe PaymentIntent"}
      </button>

      {order && (
        <div className="border rounded p-4 space-y-2">
          <div className="font-medium">Order created</div>
          <div className="text-sm">
            <div>
              <span className="opacity-70">OrderId:</span>{" "}
              <span className="font-mono break-all">{order.id}</span>
            </div>
            {order.totalAmount != null ? (
              <div>
                <span className="opacity-70">Total:</span>{" "}
                {order.totalAmount} {order.currency ?? ""}
              </div>
            ) : null}
          </div>
        </div>
      )}

      {paymentIntent && (
        <div className="border rounded p-4 space-y-2">
          <div className="font-medium">Stripe PaymentIntent created</div>
          <div className="text-sm space-y-1">
            <div>
              <span className="opacity-70">paymentIntentId:</span>{" "}
              <span className="font-mono break-all">
                {paymentIntent.paymentIntentId}
              </span>
            </div>
            <div>
              <span className="opacity-70">clientSecret:</span>{" "}
              <span className="font-mono break-all">
                {paymentIntent.clientSecret}
              </span>
            </div>
          </div>

          <div className="text-sm opacity-80">
            Next step (optional): integrate Stripe.js Elements to confirm card
            payment using the clientSecret.
          </div>
        </div>
      )}

      <div className="text-sm opacity-70">
        If you get an error about shipping address, create a saved address in the
        backend (user profile endpoints) or extend this page to submit shipping
        fields.
      </div>
    </div>
  );
}


