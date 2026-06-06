import Link from "next/link";
import type { ButtonHTMLAttributes, InputHTMLAttributes, ReactNode, SelectHTMLAttributes } from "react";
import { formatCompactMoney, formatMoney } from "@/lib/format";

export function formatAdminDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function AdminPageIntro({
  title,
  description,
  action,
}: {
  title: string;
  description: string;
  action?: ReactNode;
}) {
  return (
    <div className="flex flex-col gap-4 rounded-[28px] border border-slate-200 bg-white/95 p-6 shadow-sm md:flex-row md:items-end md:justify-between">
      <div className="space-y-2">
        <p className="text-xs font-black uppercase tracking-[0.3em] text-slate-400">Admin Workspace</p>
        <h1 className="text-3xl font-black tracking-tight text-slate-950">{title}</h1>
        <p className="max-w-3xl text-sm text-slate-600">{description}</p>
      </div>
      {action ? <div className="shrink-0">{action}</div> : null}
    </div>
  );
}

export function AdminCard({ children, className = "" }: { children: ReactNode; className?: string }) {
  return <section className={`rounded-[24px] border border-slate-200 bg-white p-5 shadow-sm ${className}`.trim()}>{children}</section>;
}

export function AdminStatCard({ label, value, tone = "slate" }: { label: string; value: ReactNode; tone?: "slate" | "orange" | "emerald" | "sky"; }) {
  const toneClass = {
    slate: "from-slate-50 to-white text-slate-900",
    orange: "from-orange-50 to-white text-orange-950",
    emerald: "from-emerald-50 to-white text-emerald-950",
    sky: "from-sky-50 to-white text-sky-950",
  }[tone];

  return (
    <div className={`rounded-[24px] border border-slate-200 bg-gradient-to-br p-5 shadow-sm ${toneClass}`.trim()}>
      <p className="text-xs font-black uppercase tracking-[0.25em] text-slate-400">{label}</p>
      <p className="mt-3 text-3xl font-black tracking-tight">{value}</p>
    </div>
  );
}

export function AdminBadge({ value }: { value?: string | null | boolean }) {
  const normalized = String(value ?? "Unknown").toLowerCase();
  const color = normalized.includes("active") || normalized.includes("approved") || normalized.includes("delivered")
    ? "bg-emerald-100 text-emerald-700"
    : normalized.includes("pending") || normalized.includes("processing")
      ? "bg-amber-100 text-amber-700"
      : normalized.includes("reject") || normalized.includes("ban") || normalized.includes("suspend") || normalized === "false"
        ? "bg-rose-100 text-rose-700"
        : "bg-slate-100 text-slate-700";

  return <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-bold ${color}`.trim()}>{String(value ?? "Unknown")}</span>;
}

export function AdminLoadingState() {
  return (
    <AdminCard className="flex min-h-60 items-center justify-center">
      <div className="h-10 w-10 animate-spin rounded-full border-4 border-brand border-t-transparent" />
    </AdminCard>
  );
}

export function AdminErrorState({ message }: { message: string }) {
  return (
    <AdminCard className="border-rose-200 bg-rose-50 text-rose-800">
      <p className="text-sm font-semibold">{message}</p>
    </AdminCard>
  );
}

export function AdminEmptyState({ title, message }: { title: string; message: string }) {
  return (
    <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center">
      <p className="text-base font-bold text-slate-900">{title}</p>
      <p className="mt-2 text-sm text-slate-500">{message}</p>
    </div>
  );
}

export function AdminToolbar({ children }: { children: ReactNode }) {
  return <div className="flex flex-col gap-3 md:flex-row md:flex-wrap md:items-center md:justify-between">{children}</div>;
}

export function AdminInput(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={`sb-input w-full ${props.className ?? ""}`.trim()} />;
}

export function AdminSelect(props: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select {...props} className={`sb-input w-full ${props.className ?? ""}`.trim()} />;
}

export function AdminButton({
  children,
  variant = "primary",
  className = "",
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: "primary" | "secondary" | "danger" }) {
  const variantClass = {
    primary: "bg-slate-950 text-white hover:bg-slate-800",
    secondary: "bg-slate-100 text-slate-800 hover:bg-slate-200",
    danger: "bg-rose-600 text-white hover:bg-rose-500",
  }[variant];

  return (
    <button
      {...props}
      className={`rounded-xl px-4 py-2 text-sm font-bold transition disabled:cursor-not-allowed disabled:opacity-50 ${variantClass} ${className}`.trim()}
    >
      {children}
    </button>
  );
}

export function AdminLinkButton({ href, children }: { href: string; children: ReactNode }) {
  return (
    <Link href={href} className="inline-flex rounded-xl bg-slate-950 px-4 py-2 text-sm font-bold text-white transition hover:bg-slate-800">
      {children}
    </Link>
  );
}

export function AdminMoney({ amount, currency }: { amount: number; currency?: string | null }) {
  return <>{formatMoney(amount, currency)}</>;
}

export function AdminCompactMoney({ amount, currency }: { amount: number; currency?: string | null }) {
  return <>{formatCompactMoney(amount, currency)}</>;
}
