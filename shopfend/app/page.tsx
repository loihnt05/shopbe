"use client";

import { useSession, signIn, signOut } from "next-auth/react";
import Link from "next/link";

export default function Home() {
  const { data: session } = useSession();

  const handleLogout = async () => {
    // 1) Clear NextAuth session
    await signOut({ redirect: false });

    // 2) Clear Keycloak SSO session (otherwise you will be auto-logged-in next time)
    const issuer = process.env.NEXT_PUBLIC_KEYCLOAK_ISSUER;
    if (!issuer) {
      // Fallback: at least return to home after NextAuth signout
      window.location.href = "/";
      return;
    }

    const postLogoutRedirectUri = window.location.origin;
    const url = new URL(`${issuer}/protocol/openid-connect/logout`);
    if (session?.idToken) {
      url.searchParams.set("id_token_hint", session.idToken);
    }
    url.searchParams.set("client_id", "shopfend");
    url.searchParams.set("post_logout_redirect_uri", postLogoutRedirectUri);
    window.location.href = url.toString();
  };

  return (
    <div className="space-y-6">
      <div className="sb-card p-6 sm:p-8 overflow-hidden relative">
        <div className="absolute inset-0 opacity-50 pointer-events-none" />

        <div className="relative">
          <div className="text-sm text-[var(--muted)]">Shopbee demo</div>
          <h1 className="mt-2 text-3xl sm:text-4xl font-bold tracking-tight text-[var(--foreground)]">
            Discover deals across a modern marketplace UI
          </h1>
          <p className="mt-3 text-sm sm:text-base text-[var(--muted)] max-w-2xl">
            Orange accents, clean cards, and a simple cart/checkout flow.
            Sign in to try the full experience.
          </p>

          <div className="mt-5 flex flex-wrap items-center gap-3">
            <Link href="/products" className="sb-btn-primary">
              Browse products
            </Link>
            <Link href="/cart" className="sb-btn-outline">
              View cart
            </Link>

            {session ? (
              <button onClick={handleLogout} className="sb-btn-outline">
                Sign out
              </button>
            ) : (
              <button onClick={() => signIn("keycloak")} className="sb-btn-primary">
                Sign in
              </button>
            )}
          </div>

          {session ? (
            <div className="mt-6 text-sm text-[var(--foreground)]">
              <div className="font-medium">Signed in as</div>
              <div className="mt-1">
                <span className="opacity-70">Name:</span> {session.user?.name ?? "—"}
              </div>
              <div>
                <span className="opacity-70">Email:</span> {session.user?.email ?? "—"}
              </div>
            </div>
          ) : null}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="sb-card p-5">
          <div className="font-semibold">Fast search</div>
          <div className="text-sm text-[var(--muted)] mt-1">
            Use the header search bar to filter products instantly.
          </div>
        </div>
        <div className="sb-card p-5">
          <div className="font-semibold">Cart flow</div>
          <div className="text-sm text-[var(--muted)] mt-1">
            Update quantities, remove items, and review a summary.
          </div>
        </div>
        <div className="sb-card p-5">
          <div className="font-semibold">Checkout (test)</div>
          <div className="text-sm text-[var(--muted)] mt-1">
            Create an order and a Stripe PaymentIntent via the API.
          </div>
        </div>
      </div>
    </div>
  );
}