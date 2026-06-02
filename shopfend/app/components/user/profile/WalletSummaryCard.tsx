import type { LucideIcon } from "lucide-react";

interface WalletSummaryCardProps {
  icon: LucideIcon;
  label: string;
  value: string;
}

export function WalletSummaryCard({ icon: Icon, label, value }: WalletSummaryCardProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-center justify-between">
        <Icon size={17} className="text-[#EE4D2D]" />
        <span className="text-lg font-bold text-slate-800">{value}</span>
      </div>
      <p className="mt-2 text-sm text-slate-600">{label}</p>
    </div>
  );
}
