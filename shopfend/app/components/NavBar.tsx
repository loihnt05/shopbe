"use client";

import Link from "next/link";
import { signIn, signOut, useSession } from "next-auth/react";

export default function NavBar() {
  const { data: session } = useSession();

  return (
    <header className="border-b">
      <div className="mx-auto max-w-5xl px-4 py-3 flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/" className="font-semibold">
            Shopbee
          </Link>
          <nav className="flex items-center gap-3 text-sm">
            <Link href="/products" className="hover:underline">
              Products
            </Link>
            <Link href="/cart" className="hover:underline">
              Cart
            </Link>
            <Link href="/checkout" className="hover:underline">
              Checkout
            </Link>
          </nav>
        </div>

        <div className="text-sm flex items-center gap-3">
          {session ? (
            <>
              <span className="opacity-80">{session.user?.email}</span>
              <button
                className="border px-3 py-1 rounded"
                onClick={() => signOut({ redirect: false })}
              >
                Sign out
              </button>
            </>
          ) : (
            <button
              className="border px-3 py-1 rounded"
              onClick={() => signIn("keycloak")}
            >
              Sign in
            </button>
          )}
        </div>
      </div>
    </header>
  );
}
