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
    <div className="p-6 text-center space-y-6">
      {session ? (
        <>
          <h1 className="text-xl">Hello, {session.user?.name}</h1>
          <div className="text-sm opacity-80">
            <p>{session.user?.email}</p>
          </div>

          <div className="flex items-center justify-center gap-3">
            <Link
              href="/products"
              className="bg-emerald-600 text-white px-4 py-2 rounded"
            >
              Browse Products
            </Link>
            <Link
              href="/cart"
              className="bg-slate-800 text-white px-4 py-2 rounded"
            >
              Cart
            </Link>
            <Link
              href="/checkout"
              className="bg-indigo-600 text-white px-4 py-2 rounded"
            >
              Checkout
            </Link>
          </div>

          <button
            onClick={handleLogout}
            className="bg-red-500 text-white px-4 py-2 rounded"
          >
            Sign Out
          </button>
        </>
      ) : (
        <>
          <div className="space-y-2">
            <p className="opacity-80">
              Sign in to add items to cart and test checkout.
            </p>
            <button
              onClick={() => signIn("keycloak")}
              className="bg-blue-500 text-white px-4 py-2 rounded"
            >
              Sign In with Keycloak
            </button>
          </div>

          <div className="flex items-center justify-center gap-3">
            <Link
              href="/products"
              className="bg-emerald-600 text-white px-4 py-2 rounded"
            >
              Browse Products (anonymous)
            </Link>
          </div>
        </>
      )}
    </div>
  );
}