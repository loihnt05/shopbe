import type { LucideIcon } from "lucide-react";

interface OrderStatusCardProps {
  icon: LucideIcon;
  label: string;
  count: number;
  onClick?: () => void;
}

export function OrderStatusCard({ icon: Icon, label, count, onClick }: OrderStatusCardProps) {
  return (
    <button
      onClick={onClick}
      className="group min-w-[150px] rounded-2xl border border-slate-200 bg-white p-4 text-left shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="flex items-center justify-between">
        <Icon size={18} className="text-[#EE4D2D]" />
        <span className="rounded-full bg-[#FFF1ED] px-2 py-0.5 text-xs font-semibold text-[#EE4D2D]">{count}</span>
      </div>
      <p className="mt-3 text-sm font-medium text-slate-700">{label}</p>
    </button>
  );
}
