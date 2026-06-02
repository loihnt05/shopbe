interface NotificationItemProps {
  label: string;
  enabled: boolean;
  onToggle: () => void;
}

export function NotificationItem({ label, enabled, onToggle }: NotificationItemProps) {
  return (
    <div className="flex items-center justify-between rounded-xl border border-slate-200 px-3 py-2.5">
      <p className="text-sm text-slate-700">{label}</p>
      <button
        onClick={onToggle}
        className={`h-6 w-11 rounded-full p-0.5 transition ${enabled ? "bg-[#EE4D2D]" : "bg-slate-300"}`}
      >
        <span className={`block h-5 w-5 rounded-full bg-white transition ${enabled ? "translate-x-5" : "translate-x-0"}`} />
      </button>
    </div>
  );
}
