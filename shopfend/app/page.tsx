"use client";

import Link from "next/link";
import { useSession, signIn, signOut } from "next-auth/react";

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
      <section className="sb-card overflow-hidden">
        <div className="p-6 sm:p-10 bg-gradient-to-r from-[var(--brand)] to-[var(--brand-2)] text-white">
          <div className="max-w-2xl space-y-3">
            <div className="sb-badge bg-white/15">Demo storefront</div>
            <h1 className="text-3xl sm:text-4xl font-bold leading-tight">
              A clean, fast marketplace UI for your Shopbee backend
            </h1>
            <p className="text-white/90">
              Browse products, add to cart, and run a test checkout flow. Styled in a
              modern “online marketplace” look.
            </p>

            <div className="flex flex-wrap gap-3 pt-2">
              <Link href="/products" className="sb-btn bg-white text-slate-900 hover:bg-white/90">
                Browse products
              </Link>
              <Link href="/cart" className="sb-btn bg-white/10 text-white hover:bg-white/15">
                View cart
              </Link>
              {session ? (
                <button onClick={handleLogout} className="sb-btn bg-black/20 hover:bg-black/30">
                  Sign out
                </button>
              ) : (
                <button onClick={() => signIn("keycloak")} className="sb-btn bg-black/20 hover:bg-black/30">
                  Sign in
                </button>
              )}
            </div>

            {session ? (
              <div className="pt-3 text-sm text-white/90">
                Signed in as <span className="font-medium">{session.user?.email ?? session.user?.name}</span>
              </div>
            ) : (
              <div className="pt-3 text-sm text-white/80">
                Sign in to access cart + checkout.
              </div>
            )}
          </div>
        </div>
      </section>

      <section className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="sb-card p-5">
          <div className="font-semibold">Flash deals</div>
          <p className="text-sm text-slate-600 mt-1">
            Highlight discounted items with clear pricing and quick navigation.
          </p>
          <Link href="/products" className="inline-block mt-3 text-sm font-medium text-[var(--brand)] hover:underline">
            Explore deals →
          </Link>
        </div>
        <div className="sb-card p-5">
          <div className="font-semibold">Secure sign-in</div>
          <p className="text-sm text-slate-600 mt-1">
            Keycloak + NextAuth session handling built-in.
          </p>
          <Link href="/checkout" className="inline-block mt-3 text-sm font-medium text-[var(--brand)] hover:underline">
            Go to checkout →
          </Link>
        </div>
        <div className="sb-card p-5">
          <div className="font-semibold">Fast UI</div>
          <p className="text-sm text-slate-600 mt-1">
            Card-based layout, responsive grid, and a sticky header with search.
          </p>
          <Link href="/products" className="inline-block mt-3 text-sm font-medium text-[var(--brand)] hover:underline">
            Start shopping →
          </Link>
        </div>
      </section>
    </div>
  );
}