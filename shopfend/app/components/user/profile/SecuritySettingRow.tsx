import type { LucideIcon } from "lucide-react";

interface SecuritySettingRowProps {
  icon: LucideIcon;
  label: string;
  status: string;
  action: string;
  onAction: () => void;
}

export function SecuritySettingRow({ icon: Icon, label, status, action, onAction }: SecuritySettingRowProps) {
  return (
    <div className="flex items-center justify-between rounded-xl border border-slate-200 p-3">
      <div className="flex items-center gap-3">
        <Icon size={18} className="text-[#EE4D2D]" />
        <div>
          <p className="text-sm font-medium text-slate-800">{label}</p>
          <p className="text-xs text-slate-500">{status}</p>
        </div>
      </div>
      <button onClick={onAction} className="rounded-lg border border-slate-200 px-3 py-1.5 text-sm text-slate-700 hover:bg-slate-50">{action}</button>
    </div>
  );
}
