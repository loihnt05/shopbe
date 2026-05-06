import Link from "next/link";

export default function PromoBanner() {
  return (
    <section className="sb-hero">
      <div className="sb-container py-6">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
          {/* Main banner */}
          <div className="lg:col-span-8 sb-card overflow-hidden relative">
            <div className="absolute inset-0 sb-hero-gradient" aria-hidden="true" />
            <div className="relative p-6 sm:p-8">
              <div className="inline-flex items-center gap-2 text-xs font-semibold text-white/95">
                <span className="sb-dot" />
                <span>Today’s picks</span>
                <span className="sb-badge bg-white/15 text-white">Limited time</span>
              </div>

              <h1 className="mt-3 text-3xl sm:text-4xl font-extrabold tracking-tight text-white">
                Big savings on everyday essentials
              </h1>
              <p className="mt-3 text-sm sm:text-base text-white/85 max-w-2xl">
                A clean marketplace UI with orange accents, fast search, and a simple cart
                flow.
              </p>

              <div className="mt-6 flex flex-wrap items-center gap-3">
                <Link href="/products" className="sb-btn-white">
                  Shop now
                </Link>
                <Link href="/cart" className="sb-btn-ghost-white">
                  View cart
                </Link>
              </div>

              <div className="mt-6 grid grid-cols-2 sm:grid-cols-3 gap-3">
                <div className="sb-mini-tile">
                  <div className="sb-mini-title">Fast search</div>
                  <div className="sb-mini-sub">Find products in seconds</div>
                </div>
                <div className="sb-mini-tile">
                  <div className="sb-mini-title">Secure checkout</div>
                  <div className="sb-mini-sub">Stripe test flow ready</div>
                </div>
                <div className="sb-mini-tile">
                  <div className="sb-mini-title">Orange deals</div>
                  <div className="sb-mini-sub">Discount badges + pricing</div>
                </div>
              </div>
            </div>
          </div>

          {/* Side promos */}
          <div className="lg:col-span-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-1 gap-4">
            <div className="sb-card p-5">
              <div className="text-xs font-semibold text-[var(--muted)]">Voucher</div>
              <div className="mt-1 font-semibold text-[var(--foreground)]">
                Extra 10% off (demo)
              </div>
              <div className="mt-2 text-sm text-[var(--muted)]">
                Apply coupons at checkout in a later iteration.
              </div>
              <Link href="/checkout" className="sb-btn-outline mt-4 w-full">
                Go to checkout
              </Link>
            </div>

            <div className="sb-card p-5">
              <div className="text-xs font-semibold text-[var(--muted)]">Flash</div>
              <div className="mt-1 font-semibold text-[var(--foreground)]">Daily picks</div>
              <div className="mt-2 text-sm text-[var(--muted)]">
                Browse trending items across categories.
              </div>
              <Link href="/products?q=sale" className="sb-btn-primary mt-4 w-full">
                Explore deals
              </Link>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
