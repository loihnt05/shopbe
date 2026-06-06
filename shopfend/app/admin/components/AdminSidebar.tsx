"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { BarChart3, Boxes, LayoutDashboard, LogOut, Package, ShoppingCart, Users, Store } from "lucide-react";
import { signOut } from "next-auth/react";

const items = [
  { href: "/admin/overview", label: "Overview", icon: LayoutDashboard },
  { href: "/admin/users", label: "Users", icon: Users },
  { href: "/admin/sellers", label: "Sellers", icon: Store },
  { href: "/admin/products", label: "Products", icon: Package },
  { href: "/admin/orders", label: "Orders", icon: ShoppingCart },
  { href: "/admin/analytics", label: "Analytics", icon: BarChart3 },
  { href: "/admin/categories", label: "Categories", icon: Boxes },
];

export default function AdminSidebar() {
  const pathname = usePathname();

  return (
    <aside className="rounded-[28px] border border-slate-200 bg-slate-950 p-4 text-white shadow-xl shadow-slate-950/15">
      <div className="px-3 py-4">
        <p className="text-xs font-black uppercase tracking-[0.3em] text-slate-400">Shopbee</p>
        <p className="mt-2 text-2xl font-black tracking-tight">Admin</p>
      </div>

      <nav className="mt-3 flex flex-row gap-2 overflow-x-auto pb-2 md:flex-col md:overflow-visible">
        {items.map((item) => {
          const active = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex min-w-fit items-center gap-3 rounded-2xl px-4 py-3 text-sm font-bold transition ${active ? "bg-white text-slate-950" : "text-slate-300 hover:bg-slate-900 hover:text-white"}`}
            >
              <item.icon size={18} />
              <span>{item.label}</span>
            </Link>
          );
        })}
      </nav>

      <button
        onClick={() => signOut({ callbackUrl: "/" })}
        className="mt-4 flex w-full items-center gap-3 rounded-2xl border border-slate-800 px-4 py-3 text-sm font-bold text-slate-300 transition hover:bg-slate-900 hover:text-white"
      >
        <LogOut size={18} />
        <span>Sign out</span>
      </button>
    </aside>
  );
}
