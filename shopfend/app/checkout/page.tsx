"use client";

import { useSession, signIn } from "next-auth/react";
import { type FormEvent, useEffect, useRef, useState, useCallback } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Elements, PaymentElement, useElements, useStripe } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import { useCart } from "../components/CartContext";
import { locations } from "@/lib/locations";
import {
  isAbortError,
  shopbeApi,
  type CartDto,
  type CreateOrderResponse,
  type CreateStripePaymentIntentResponse,
  type ShippingCalculationResponseDto,
  type UserAddressResponseDto,
} from "@/lib/shopbeApi";
import { formatMoney } from "@/lib/format";
import { errorMessage } from "@/lib/errors";

const STRIPE_PUBLISHABLE_KEY =
  process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY || "";

function isLikelyStripeClientSecret(value: string | null | undefined): boolean {
  return (
    typeof value === "string" &&
    value.length > 0 &&
    // Typical format: pi_..._secret_...
    value.includes("_secret_")
  );
}

function StripePaymentForm(props: {
  accessToken: string;
  orderId: string;
  paymentIntentId: string;
  onPaid: () => void;
}) {
  const stripe = useStripe();
  const elements = useElements();
  const [submitting, setSubmitting] = useState(false);
  const [paymentElementReady, setPaymentElementReady] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const pay = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!stripe || !elements) {
      setError("Stripe has not loaded yet.");
      return;
    }

    // `elements` can be non-null before the PaymentElement is fully mounted.
    // Calling confirmPayment too early causes:
    // "Invalid value for stripe.confirmPayment(): elements should have a mounted Payment Element..."
    if (!paymentElementReady) {
      setError("Payment form is still loading. Please wait a moment and try again.");
      return;
    }

    try {
      setSubmitting(true);

      // Trigger PaymentElement validation / wallet collection before confirming.
      const submitResult = await elements.submit();
      if (submitResult.error) {
        setError(submitResult.error.message ?? "Payment details are incomplete");
        return;
      }

      const returnUrl = `${window.location.origin}/purchases`;

      const result = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: returnUrl,
        },
        redirect: "if_required",
      });

      if (result.error) {
        setError(result.error.message ?? "Payment failed");
        return;
      }

      // If we didn't get an immediate PaymentIntent back, Stripe may have redirected.
      // Our return_url should land in /purchases; still try syncing if we can.
      if (result.paymentIntent) {
        await shopbeApi.payments.syncStripePaymentIntent(
          props.accessToken,
          result.paymentIntent.id
        );
      } else {
        // Fall back to syncing the known PI id.
        await shopbeApi.payments.syncStripePaymentIntent(
          props.accessToken,
          props.paymentIntentId
        );
      }

      props.onPaid();
    } catch (e: unknown) {
      setError(errorMessage(e, "Payment failed"));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={pay} className="space-y-3">
      <PaymentElement
        onReady={() => setPaymentElementReady(true)}
        onLoadError={(e) => {
          // react-stripe-js will also log this to console; show a readable message.
          setError(e.error?.message ?? "Failed to load Stripe payment form");
        }}
      />
      {error ? (
        <div className="border border-red-300 bg-red-50 p-2 rounded text-sm text-red-800">
          {error}
        </div>
      ) : null}
      <button
        type="submit"
        className="sb-btn-primary w-full disabled:opacity-60"
        disabled={submitting || !stripe || !elements || !paymentElementReady}
      >
        {submitting ? "Processing…" : "Pay now"}
      </button>
      <div className="text-xs text-slate-500">
        Use a Stripe test card (e.g. 4242 4242 4242 4242, any future expiry, any CVC)
        in test mode.
      </div>
    </form>
  );
}

export default function CheckoutPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const { refreshCart } = useCart();

  const [stripePromise, setStripePromise] = useState<ReturnType<typeof loadStripe> | null>(
    () => (STRIPE_PUBLISHABLE_KEY ? loadStripe(STRIPE_PUBLISHABLE_KEY) : null)
  );
  const [stripeKeyWarning, setStripeKeyWarning] = useState<string | null>(null);

  // Address State
  const [savedAddresses, setSavedAddresses] = useState<UserAddressResponseDto[]>([]);
  const [selectedAddressId, setSelectedAddressId] = useState<string>("new");
  const [showAddressModal, setShowAddressModal] = useState(false);
  const [saveAddress, setSaveAddress] = useState(false);
  const [receiverName, setReceiverName] = useState("");
  const [phone, setPhone] = useState("");
  const [province, setProvince] = useState("");
  const [district, setDistrict] = useState("");
  const [ward, setWard] = useState("");
  const [addressLine, setAddressLine] = useState("");

  const [shippingResult, setShippingResult] = useState<ShippingCalculationResponseDto | null>(null);
  const [calculatingShipping, setCalculatingShipping] = useState(false);
  const [loadingAddresses, setLoadingAddresses] = useState(false);
  const shipAbortRef = useRef<AbortController | null>(null);

  // Load saved addresses
  const loadAddresses = useCallback(async () => {
    if (!session?.accessToken) return;
    try {
      setLoadingAddresses(true);
      const saved = await shopbeApi.userAddresses.getMyAddresses(session.accessToken);
      setSavedAddresses(saved);

      if (saved.length > 0) {
        const defaultAddr = saved.find(a => a.isDefault) || saved[0];
        setSelectedAddressId(defaultAddr.id);
      }
    } catch (e) {
      console.error("Failed to load addresses", e);
    } finally {
      setLoadingAddresses(false);
    }
  }, [session?.accessToken]);

  useEffect(() => {
    if (status === "authenticated") loadAddresses();
  }, [status, loadAddresses]);

  // When selected address changes, update the form fields if it's a saved address
  useEffect(() => {
    if (selectedAddressId !== "new") {
      const addr = savedAddresses.find(a => a.id === selectedAddressId);
      if (addr) {
        setReceiverName(addr.receiverName || "");
        setPhone(addr.phone || "");
        setProvince(addr.city || "");
        setDistrict(addr.district || "");
        setWard(addr.ward || "");
        setAddressLine(addr.addressLine || "");
      }
    } else {
      // Clear fields for new address if they were from a saved address
      // but only if we weren't already on "new"
      setReceiverName(session?.user?.name || "");
      setPhone("");
      setProvince("");
      setDistrict("");
      setWard("");
      setAddressLine("");
      setSaveAddress(false);
    }
  }, [selectedAddressId, savedAddresses, session?.user?.name]);

  // Set default receiver name when session is available and on new address
  useEffect(() => {
    if (session?.user?.name && !receiverName && selectedAddressId === "new") {
      setReceiverName(session.user.name);
    }
  }, [session, receiverName, selectedAddressId]);

  // If the env publishable key isn't set, fetch it from backend config.
  // Also warn when env key differs from backend key (common cause of PaymentElement loaderror).
  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const cfg = await shopbeApi.payments.getStripeConfig();
        if (cancelled) return;

        const backendKey = cfg.publishableKey ?? "";
        if (!backendKey) return;

        if (!STRIPE_PUBLISHABLE_KEY) {
          setStripePromise(loadStripe(backendKey));
          return;
        }

        if (STRIPE_PUBLISHABLE_KEY !== backendKey) {
          setStripeKeyWarning(
            "Stripe publishable key mismatch: frontend NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY differs from backend Stripe:PublishableKey. This can prevent PaymentElement from loading."
          );
        }
      } catch {
        // ignore - frontend will show the existing missing-key message if Stripe isn't initialized.
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const [cart, setCart] = useState<CartDto | null>(null);
  const [loadingCart, setLoadingCart] = useState(false);
  const [creating, setCreating] = useState(false);
  const [order, setOrder] = useState<CreateOrderResponse | null>(null);
  const [paymentIntent, setPaymentIntent] =
    useState<CreateStripePaymentIntentResponse | null>(null);
  const [markingPaid, setMarkingPaid] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const abortRef = useRef<AbortController | null>(null);

  const getSignal = () => {
    abortRef.current?.abort();
    abortRef.current = new AbortController();
    return abortRef.current.signal;
  };

  const loadCart = useCallback(async () => {
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
  }, [session?.accessToken]);

  useEffect(() => {
    if (status === "authenticated") loadCart();
    return () => abortRef.current?.abort();
  }, [status, loadCart]);

  // Shipping Calculation Effect
  useEffect(() => {
    if (!province || !district || !cart) {
      setShippingResult(null);
      return;
    }

    const timer = setTimeout(async () => {
      try {
        setCalculatingShipping(true);
        shipAbortRef.current?.abort();
        shipAbortRef.current = new AbortController();

        const res = await shopbeApi.shipping.calculate({
          city: province,
          district: district,
          ward: ward || undefined,
          subtotal: cart.subtotal
        }, shipAbortRef.current.signal);

        setShippingResult(res);
      } catch (e: unknown) {
        if (isAbortError(e)) return;
        console.error("Shipping calculation failed", e);
      } finally {
        setCalculatingShipping(false);
      }
    }, 500);

    return () => {
      clearTimeout(timer);
      shipAbortRef.current?.abort();
    };
  }, [province, district, ward, cart]);

  const doCheckout = async () => {
    if (!session?.accessToken) return;
    if (!cart || cart.items.length === 0) {
      setError("Cart is empty.");
      return;
    }
    if (!province || !district || !ward || !addressLine || !receiverName || !phone) {
      setError("Please complete all shipping information.");
      return;
    }

    setError(null);
    setCreating(true);
    setOrder(null);
    setPaymentIntent(null);

    try {
      let finalUserAddressId = selectedAddressId !== "new" ? selectedAddressId : undefined;

      // If it's a new address and user wants to save it
      if (selectedAddressId === "new" && saveAddress) {
        try {
          const newAddr = await shopbeApi.userAddresses.create(session.accessToken, {
            receiverName,
            phone,
            addressLine,
            city: province,
            district,
            ward,
            isDefault: savedAddresses.length === 0
          });
          finalUserAddressId = newAddr.id;
        } catch (e) {
          console.error("Failed to save address, continuing with checkout", e);
        }
      }

      const created = await shopbeApi.orders.create(session.accessToken, {
        userAddressId: finalUserAddressId,
        useDefaultAddressIfAvailable: false,
        shippingReceiverName: receiverName,
        shippingPhone: phone,
        shippingAddressLine: addressLine,
        shippingCity: province,
        shippingDistrict: district,
        shippingWard: ward,
        couponCode: cart?.couponCode || undefined
      });
      setOrder(created);

      // Refresh the global cart state now that it's been cleared in the DB.
      await refreshCart();

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

  const availableDistricts = locations.provinces.find(p => p.name === province)?.districts || [];
  const availableWards = availableDistricts.find(d => d.name === district)?.wards || [];

  const provinceOptions = locations.provinces.map(p => (
    <option key={p.name} value={p.name}>{p.name}</option>
  ));

  const districtOptions = availableDistricts.map(d => (
    <option key={d.name} value={d.name}>{d.name}</option>
  ));

  const wardOptions = availableWards.map(w => (
    <option key={w} value={w}>{w}</option>
  ));

  const markPaidDev = async () => {
    if (!session?.accessToken) return;
    if (!order?.id) return;

    try {
      setError(null);
      setMarkingPaid(true);
      await shopbeApi.payments.markStripePaymentPaidDev(session.accessToken, {
        orderId: order.id,
        paymentIntentId: paymentIntent?.paymentIntentId,
      });
      router.push("/purchases");
    } catch (e: unknown) {
      setError(
        errorMessage(
          e,
          "Failed to mark payment as paid. If you're not running in Development, this endpoint returns 404."
        )
      );
    } finally {
      setMarkingPaid(false);
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
            Select your location and pay securely.
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

      {stripeKeyWarning ? (
        <div className="border border-amber-300 bg-amber-50 p-3 rounded text-sm text-amber-900">
          {stripeKeyWarning}
        </div>
      ) : null}

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
        <div className="lg:col-span-7 space-y-4">
          {/* Shipping Address */}
          <div className="sb-card p-5 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="font-semibold text-lg">Shipping Address</h2>
              {savedAddresses.length > 0 && (
                <button 
                  type="button"
                  onClick={() => setShowAddressModal(true)}
                  className="text-xs font-bold text-[var(--brand)] hover:text-[var(--brand-hover)] bg-[var(--brand)]/5 px-2 py-1 rounded"
                >
                  {selectedAddressId === 'new' ? 'CHOOSE SAVED' : 'CHANGE ADDRESS'}
                </button>
              )}
            </div>
            
            {selectedAddressId !== 'new' && (
              <div className="p-3 border border-[var(--brand)] rounded-md bg-[var(--brand)]/5">
                {(() => {
                  const addr = savedAddresses.find(a => a.id === selectedAddressId);
                  if (!addr) return null;
                  return (
                    <div className="text-sm">
                      <div className="font-medium">
                        {addr.receiverName} 
                        {addr.isDefault && <span className="text-[10px] bg-slate-200 px-1 rounded ml-1">DEFAULT</span>}
                        <span className="text-[10px] bg-blue-100 text-blue-700 px-1 rounded ml-1 uppercase">SAVED</span>
                      </div>
                      <div className="text-slate-600">{addr.phone}</div>
                      <div className="text-slate-500 text-xs">{addr.addressLine}, {addr.ward}, {addr.district}, {addr.city}</div>
                      <button 
                        type="button" 
                        onClick={() => setSelectedAddressId('new')}
                        className="mt-2 text-xs text-slate-500 hover:text-slate-800 underline font-medium"
                      >
                        Use a different (new) address
                      </button>
                    </div>
                  );
                })()}
              </div>
            )}

            <div className={`space-y-4 ${selectedAddressId !== 'new' ? 'hidden' : ''}`}>
              <div className="text-xs font-medium text-slate-500 uppercase tracking-wider mb-2">New Shipping Address</div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div className="space-y-1">
                  <label className="text-xs font-medium text-slate-500 uppercase tracking-wider">Receiver Name</label>
                  <input 
                    type="text" 
                    value={receiverName} 
                    onChange={e => setReceiverName(e.target.value)}
                    className="w-full p-2 border rounded-md focus:ring-1 focus:ring-[var(--brand)] outline-none"
                    placeholder="Full name"
                  />
                </div>
                <div className="space-y-1">
                  <label className="text-xs font-medium text-slate-500 uppercase tracking-wider">Phone Number</label>
                  <input 
                    type="text" 
                    value={phone} 
                    onChange={e => setPhone(e.target.value)}
                    className="w-full p-2 border rounded-md focus:ring-1 focus:ring-[var(--brand)] outline-none"
                    placeholder="0123 456 789"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <div className="space-y-1">
                  <label className="text-xs font-medium text-slate-500 uppercase tracking-wider">Province</label>
                  <select 
                    value={province} 
                    onChange={e => { setProvince(e.target.value); setDistrict(""); setWard(""); }}
                    className="w-full p-2 border rounded-md focus:ring-1 focus:ring-[var(--brand)] outline-none"
                  >
                    <option value="">Select Province</option>
                    {provinceOptions}
                  </select>
                </div>
                <div className="space-y-1">
                  <label className="text-xs font-medium text-slate-500 uppercase tracking-wider">District</label>
                  <select 
                    value={district} 
                    onChange={e => { setDistrict(e.target.value); setWard(""); }}
                    disabled={!province}
                    className="w-full p-2 border rounded-md focus:ring-1 focus:ring-[var(--brand)] outline-none disabled:bg-slate-50"
                  >
                    <option value="">Select District</option>
                    {districtOptions}
                  </select>
                </div>
                <div className="space-y-1">
                  <label className="text-xs font-medium text-slate-500 uppercase tracking-wider">Ward</label>
                  <select 
                    value={ward} 
                    onChange={e => setWard(e.target.value)}
                    disabled={!district}
                    className="w-full p-2 border rounded-md focus:ring-1 focus:ring-[var(--brand)] outline-none disabled:bg-slate-50"
                  >
                    <option value="">Select Ward</option>
                    {wardOptions}
                  </select>
                </div>
              </div>

              <div className="space-y-1">
                <label className="text-xs font-medium text-slate-500 uppercase tracking-wider">Detailed Address</label>
                <input 
                  type="text" 
                  value={addressLine} 
                  onChange={e => setAddressLine(e.target.value)}
                  className="w-full p-2 border rounded-md focus:ring-1 focus:ring-[var(--brand)] outline-none"
                  placeholder="Street name, Building, House number..."
                />
              </div>

              <label className="flex items-center text-sm cursor-pointer select-none">
                <input 
                  type="checkbox" 
                  checked={saveAddress}
                  onChange={e => setSaveAddress(e.target.checked)}
                  className="mr-2 accent-[var(--brand)]"
                />
                Save this address for future use
              </label>
            </div>
          </div>
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
                      {it.unitPrice != null
                        ? formatMoney(it.unitPrice * it.quantity, cart?.currency)
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
              Confirm your shipping details above before paying.
            </div>

            <button
              disabled={creating || !province || !district || !ward || !addressLine || !receiverName || !phone}
              onClick={doCheckout}
              className="sb-btn-primary w-full mt-4 disabled:opacity-60"
            >
              {creating ? "Creating Order…" : "Confirm Order & Pay"}
            </button>

            {paymentIntent ? (
              <div className="mt-5">
                {stripePromise ? (
                  <div className="rounded-sm border border-black/10 bg-white p-4">
                    <div className="text-sm font-medium mb-2">Pay with card</div>
                    {session.accessToken ? (
                      isLikelyStripeClientSecret(paymentIntent.clientSecret) ? (
                        <Elements
                          // IMPORTANT: Elements does not reliably support changing clientSecret after mount.
                          // Key forces a remount when a new PaymentIntent is created.
                          key={paymentIntent.clientSecret}
                          stripe={stripePromise}
                          options={{ clientSecret: paymentIntent.clientSecret }}
                        >
                          <StripePaymentForm
                            accessToken={session.accessToken}
                            orderId={order?.id ?? ""}
                            paymentIntentId={paymentIntent.paymentIntentId}
                            onPaid={() => router.push("/purchases")}
                          />
                        </Elements>
                      ) : (
                        <div className="rounded-sm border border-red-300 bg-red-50 p-3 text-sm text-red-800">
                          Stripe did not return a valid <code>clientSecret</code> for the PaymentIntent.
                          <div className="mt-1 text-xs text-red-700">
                            clientSecret length: {paymentIntent.clientSecret?.length ?? 0}
                          </div>
                          <div className="mt-2 text-xs text-slate-600">
                            Check your backend Stripe keys (SecretKey) and frontend publishable key
                            (<code>NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY</code>) are from the same Stripe account
                            and same mode (test vs live).
                          </div>
                        </div>
                      )
                    ) : (
                      <div className="text-sm text-red-700">
                        Missing access token. Please sign out and sign in again.
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="rounded-sm border border-amber-300 bg-amber-50 p-3 text-sm text-amber-900">
                    Missing <code>NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY</code>.
                    You can still use the dev helper below to simulate payment.
                  </div>
                )}
              </div>
            ) : null}
          </div>
        </div>

        <aside className="lg:col-span-5">
          <div className="sb-card p-5 lg:sticky lg:top-28 space-y-4">
            <div className="font-semibold text-lg">Order Summary</div>
            
            <div className="space-y-2 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-slate-600">Subtotal</span>
                <span className="font-medium">
                  {formatMoney(cart?.subtotal ?? 0, cart?.currency)}
                </span>
              </div>

              {cart && cart.discountAmount > 0 && (
                <div className="flex items-center justify-between text-green-600 font-medium">
                  <div className="flex items-center gap-2">
                    <span>Discount</span>
                    <span className="text-[10px] bg-green-100 px-1 rounded uppercase tracking-tighter">{cart.couponCode}</span>
                  </div>
                  <span>-{formatMoney(cart.discountAmount, cart.currency)}</span>
                </div>
              )}

              <div className="flex items-center justify-between">
                <span className="text-slate-600">Shipping</span>
                <span className="font-medium">
                  {calculatingShipping ? (
                    <span className="text-xs animate-pulse opacity-50 italic">Calculating...</span>
                  ) : shippingResult ? (
                    shippingResult.shippingFee === 0 ? "FREE" : formatMoney(shippingResult.shippingFee, cart?.currency)
                  ) : (
                    <span className="text-xs opacity-50 italic">Select location</span>
                  )}
                </span>
              </div>

              {shippingResult?.estimatedDeliveryDate && (
                <div className="text-[11px] text-slate-500 italic text-right">
                  Estimated delivery by {new Date(shippingResult.estimatedDeliveryDate).toLocaleDateString()}
                </div>
              )}

              <div className="pt-2 border-t flex items-center justify-between text-base font-bold text-slate-900">
                <span>Total</span>
                <span>{formatMoney((cart?.subtotal ?? 0) - (cart?.discountAmount ?? 0) + (shippingResult?.shippingFee ?? 0), cart?.currency)}</span>
              </div>
            </div>

            {order ? (
              <div className="rounded-sm border border-black/10 p-3 bg-slate-50">
                <div className="text-sm font-medium text-green-700 flex items-center gap-1">
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
                  Order created
                </div>
                <div className="text-[10px] text-slate-500 mt-1 font-mono">
                  {order.id}
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

                {!stripePromise ? (
                  <button
                    className="sb-btn-primary w-full mt-3"
                    onClick={markPaidDev}
                    disabled={markingPaid}
                    title="Dev helper: simulates Stripe webhook and confirms the order"
                  >
                    {markingPaid
                      ? "Marking paid…"
                      : "Mark as paid (dev) → View purchases"}
                  </button>
                ) : null}
              </div>
            ) : null}

            <div className="text-xs text-slate-500">
              Purchases are shown after payment succeeds (Stripe webhook in prod, or
              sync call after Stripe.js confirmation in dev).
            </div>
          </div>
        </aside>
      </div>

      <div className="text-sm text-slate-600">
        If you get an error about shipping address, create a saved address in the
        backend (user profile endpoints) or extend this page to submit shipping
        fields.
      </div>

      {/* Address Selection Modal */}
      {showAddressModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-[2px] animate-in fade-in duration-200">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col max-h-[80vh] animate-in zoom-in-95 duration-200">
            <div className="p-4 border-b flex items-center justify-between bg-slate-50">
              <h3 className="font-bold text-lg text-slate-800">Select Saved Address</h3>
              <button 
                onClick={() => setShowAddressModal(false)} 
                className="p-2 hover:bg-slate-200 rounded-full transition-colors"
              >
                <svg className="w-5 h-5 text-slate-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            <div className="overflow-y-auto p-4 space-y-3 bg-white">
              {loadingAddresses ? (
                <div className="flex flex-col items-center justify-center py-12 space-y-3">
                  <div className="w-8 h-8 border-4 border-slate-200 border-t-[var(--brand)] rounded-full animate-spin"></div>
                  <div className="text-sm text-slate-500 font-medium">Loading your addresses...</div>
                </div>
              ) : savedAddresses.length > 0 ? (
                savedAddresses.map(addr => (
                  <div 
                    key={addr.id}
                    onClick={() => { setSelectedAddressId(addr.id); setShowAddressModal(false); }}
                    className={`p-4 border-2 rounded-xl cursor-pointer transition-all hover:border-[var(--brand)] hover:shadow-md ${
                      selectedAddressId === addr.id 
                        ? 'border-[var(--brand)] bg-[var(--brand)]/5 ring-1 ring-[var(--brand)]/20' 
                        : 'border-slate-100 bg-slate-50/50'
                    }`}
                  >
                    <div className="flex justify-between items-start mb-1">
                      <div className="font-bold text-slate-900">{addr.receiverName}</div>
                      {addr.isDefault && (
                        <span className="text-[10px] font-bold bg-slate-200 text-slate-600 px-2 py-0.5 rounded-full uppercase tracking-tight">Default</span>
                      )}
                    </div>
                    <div className="text-sm text-slate-600 font-medium mb-1">{addr.phone}</div>
                    <div className="text-xs text-slate-500 leading-relaxed">
                      {addr.addressLine}, {addr.ward}, {addr.district}, {addr.city}
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-center py-8 text-slate-500">
                  No saved addresses found.
                </div>
              )}
            </div>

            <div className="p-4 border-t bg-slate-50">
              <button 
                onClick={() => { setSelectedAddressId('new'); setShowAddressModal(false); }}
                className="w-full py-3 border-2 border-dashed border-slate-300 rounded-xl text-sm font-bold text-slate-500 hover:border-[var(--brand)] hover:text-[var(--brand)] hover:bg-white transition-all"
              >
                + ADD NEW ADDRESS
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}


