"use client";

import { Store } from "lucide-react";
import { useSession } from "next-auth/react";

export default function SellerHeader() {
  const { data: session } = useSession();

  return (
    <div className="flex flex-col gap-3 rounded-[28px] border border-slate-200 bg-white/95 p-5 shadow-sm md:flex-row md:items-center md:justify-between">
      <div className="flex items-center gap-3">
        <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand/10 text-brand">
          <Store size={22} />
        </div>
        <div>
          <p className="text-sm font-bold text-slate-900">{session?.user?.name ?? "Seller"}</p>
          <p className="text-sm text-slate-500">{session?.user?.email ?? "Authenticated via Keycloak"}</p>
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        {(session?.user?.roles ?? []).map((role) => (
          <span key={role} className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black uppercase tracking-[0.2em] text-slate-700">{role}</span>
        ))}
      </div>
    </div>
  );
}
