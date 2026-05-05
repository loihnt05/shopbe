"use client";

import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { signIn, signOut, useSession } from "next-auth/react";
import { useEffect, useMemo, useState } from "react";
import { CartIcon, SearchIcon, UserIcon } from "./icons";

function cx(...parts: Array<string | false | null | undefined>) {
  return parts.filter(Boolean).join(" ");
}

export default function SiteHeader() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const initialQ = useMemo(() => searchParams.get("q") ?? "", [searchParams]);
  const [q, setQ] = useState(initialQ);

  useEffect(() => {
    // keep input in sync when navigating back/forward
    setQ(initialQ);
  }, [initialQ]);

  const onSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const url = q.trim() ? `/products?q=${encodeURIComponent(q.trim())}` : "/products";
    router.push(url);
  };

  return (
    <header className="sticky top-0 z-50">
      {/* top strip */}
      <div className="bg-[var(--brand)] text-white text-xs">
        <div className="sb-container h-9 flex items-center justify-between">
          <div className="flex items-center gap-3 opacity-95">
            <span className="hidden sm:inline">Free shipping demo • 0₫ returns</span>
            <span className="sb-badge bg-white/15">Hot deals</span>
          </div>
          <div className="flex items-center gap-3">
            {session ? (
              <span className="hidden sm:inline opacity-95">
                {session.user?.email ?? session.user?.name}
              </span>
            ) : null}
            {status !== "loading" ? (
              session ? (
                <button
                  className="hover:underline"
                  onClick={() => signOut({ redirect: false })}
                >
                  Sign out
                </button>
              ) : (
                <button className="hover:underline" onClick={() => signIn("keycloak")}>
                  Sign in
                </button>
              )
            ) : null}
          </div>
        </div>
      </div>

      {/* main bar */}
      <div className="bg-white border-b border-black/10">
        <div className="sb-container py-4 flex items-center gap-3">
          <Link href="/" className="flex items-center gap-2">
            <div className="h-9 w-9 rounded-sm bg-[var(--brand)] text-white grid place-items-center font-bold">
              S
            </div>
            <div className="leading-tight">
              <div className="font-semibold text-slate-900">Shopbee</div>
              <div className="text-[11px] text-slate-500 -mt-0.5">Marketplace</div>
            </div>
          </Link>

          <form onSubmit={onSearch} className="flex-1">
            <div className="flex items-stretch rounded-sm overflow-hidden border border-black/15 focus-within:border-[var(--brand)]">
              <div className="px-3 grid place-items-center text-slate-400">
                <SearchIcon className="h-4 w-4" />
              </div>
              <input
                value={q}
                onChange={(e) => setQ(e.target.value)}
                placeholder="Search products, brands, categories…"
                className="w-full bg-white px-2 py-2 text-sm outline-none"
              />
              <button
                type="submit"
                className="px-4 text-sm font-semibold text-white bg-[var(--brand)] hover:bg-[var(--brand-2)]"
              >
                Search
              </button>
            </div>
          </form>

          <nav className="hidden md:flex items-center gap-4 text-sm text-slate-600">
            <Link className={cx("hover:text-slate-900", pathname === "/products" && "text-slate-900 font-medium")}
                  href="/products">
              Products
            </Link>
            <Link className={cx("hover:text-slate-900", pathname === "/checkout" && "text-slate-900 font-medium")}
                  href="/checkout">
              Checkout
            </Link>
          </nav>

          <Link
            href="/cart"
            className="ml-1 inline-flex items-center gap-2 rounded-sm px-3 py-2 hover:bg-slate-50"
            aria-label="Cart"
          >
            <CartIcon className="h-5 w-5 text-slate-700" />
            <span className="hidden sm:inline text-sm font-medium">Cart</span>
          </Link>

          <div className="hidden sm:flex items-center gap-2 text-sm text-slate-600">
            <UserIcon className="h-5 w-5" />
            <span className="max-w-40 truncate">
              {session?.user?.name ?? (session ? "Account" : "Guest")}
            </span>
          </div>
        </div>
      </div>
    </header>
  );
}

