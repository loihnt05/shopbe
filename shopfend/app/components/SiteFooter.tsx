import Link from "next/link";

export default function SiteFooter() {
  return (
    <footer className="mt-10 border-t border-black/10 bg-white">
      <div className="sb-container py-8 text-sm text-slate-600 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <div className="font-medium text-slate-800">Shopbee</div>
          <div className="opacity-80">
            Demo marketplace UI • Built with Next.js + ASP.NET Core
          </div>
        </div>
        <div className="flex gap-4">
          <Link className="hover:underline" href="/products">
            Products
          </Link>
          <Link className="hover:underline" href="/cart">
            Cart
          </Link>
          <Link className="hover:underline" href="/checkout">
            Checkout
          </Link>
        </div>
      </div>
    </footer>
  );
}


