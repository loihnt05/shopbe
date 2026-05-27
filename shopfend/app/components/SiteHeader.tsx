"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { signIn, signOut, useSession } from "next-auth/react";
import { useEffect, useMemo, useState } from "react";
import { CartIcon, SearchIcon, UserIcon } from "./icons";
import { useCart } from "./CartContext";
import { Badge } from "./Badge";

export default function SiteHeader() {
  const { data: session, status } = useSession();
  const { totalQuantity, openDrawer } = useCart();
  const router = useRouter();
  const searchParams = useSearchParams();

  const initialQ = useMemo(() => searchParams.get("q") ?? "", [searchParams]);
  const [q, setQ] = useState(initialQ);

  useEffect(() => {
    // keep input in sync when navigating back/forward
    setQ(initialQ);
  }, [initialQ]);

  const onSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const params = new URLSearchParams(searchParams.toString());
    if (q.trim()) {
      params.set("q", q.trim());
    } else {
      params.delete("q");
    }
    // Optional: Reset page number when searching
    params.delete("pageNumber");
    
    router.push(`/products?${params.toString()}`);
  };

  return (
    <header className="sticky top-0 z-50 bg-[#ee4d2d] text-white">
      {/* top strip */}
      <div className="text-xs">
        <div className="sb-container h-8 flex items-center justify-between px-4">
          <div className="flex items-center gap-4 opacity-90">
            <Link href="#" className="hover:opacity-80">Seller Centre</Link>
            <span className="w-px h-3 bg-white/40"></span>
            <Link href="#" className="hover:opacity-80">Start Selling</Link>
            <span className="w-px h-3 bg-white/40"></span>
            <Link href="#" className="hover:opacity-80">Download</Link>
            <span className="w-px h-3 bg-white/40"></span>
            <span className="flex items-center gap-1">
              Follow us on
              {/* social icons placeholder */}
            </span>
          </div>
          <div className="flex items-center gap-4 opacity-90">
            <Link href="/notifications" className="hover:opacity-80 flex items-center gap-1">
              Notifications
            </Link>
            <Link href="/help" className="hover:opacity-80 flex items-center gap-1">
              Help
            </Link>
            {status !== "loading" ? (
              session ? (
                <div className="flex items-center gap-3">
                  <span className="flex items-center gap-1 font-medium hover:opacity-80 cursor-pointer">
                    <UserIcon className="h-4 w-4" />
                    {session.user?.name ?? session.user?.email ?? "User"}
                  </span>
                  <button
                    className="hover:opacity-80 font-medium"
                    onClick={() => signOut({ redirect: false })}
                  >
                    Logout
                  </button>
                </div>
              ) : (
                <div className="flex items-center gap-2 font-medium">
                  <button className="hover:opacity-80" onClick={() => signIn("keycloak")}>Sign Up</button>
                  <span className="w-px h-3 bg-white/40"></span>
                  <button className="hover:opacity-80" onClick={() => signIn("keycloak")}>Login</button>
                </div>
              )
            ) : null}
          </div>
        </div>
      </div>

      {/* main bar */}
      <div className="pb-4 pt-2">
        <div className="sb-container px-4 flex items-center gap-10">
          <Link href="/" className="flex items-center gap-2 shrink-0 group">
            <div className="text-4xl font-semibold tracking-tighter flex items-center">
              <span className="text-white">Shopbee</span>
            </div>
          </Link>

          <div className="flex-1 min-w-0 flex flex-col gap-1">
            <form onSubmit={onSearch} className="flex relative bg-white rounded-lg p-[2px] shadow-sm ring-1 ring-black/5 focus-within:ring-brand transition-all">
              <input
                value={q}
                onChange={(e) => setQ(e.target.value)}
                placeholder="Register and get 100% off your first order!"
                className="w-full bg-white px-4 py-2 text-[14px] text-black outline-none rounded-l-lg"
              />
              <button
                type="submit"
                className="px-6 py-2 text-sm font-medium text-white bg-[#ee4d2d] hover:bg-[#fb5533] rounded-md m-[2px] transition-colors"
              >
                <SearchIcon className="h-4 w-4" />
              </button>
            </form>
            <nav className="flex items-center gap-4 text-[12px] text-white/80 overflow-hidden whitespace-nowrap">
              <Link className="hover:text-white transition-colors" href="/products">All Products</Link>
              <Link className="hover:text-white transition-colors" href="/recommendations">Recommendations</Link>
              <Link className="hover:text-white transition-colors" href="/chat">Chat</Link>
              <Link className="hover:text-white transition-colors" href="/purchases">My Purchases</Link>
              <Link className="hover:text-white transition-colors" href="/cart">Cart</Link>
            </nav>
          </div>

          <div className="shrink-0 flex items-center justify-center w-[80px]">
            <button
              onClick={openDrawer}
              className="relative inline-flex items-center justify-center p-2 group"
              aria-label="Cart"
            >
              <CartIcon className="h-8 w-8 text-white" />
              <Badge count={totalQuantity} />
            </button>
          </div>
        </div>
      </div>
    </header>
  );
}