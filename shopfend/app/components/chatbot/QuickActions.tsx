"use client";

import { Sparkles, Package, MessageSquareText } from "lucide-react";

interface QuickActionsProps {
  onAction: (text: string) => void;
}

export default function QuickActions({ onAction }: QuickActionsProps) {
  const actions = [
    { label: "Recommend products", icon: <Sparkles size={14} /> },
    { label: "Track order", icon: <Package size={14} /> },
    { label: "Contact support", icon: <MessageSquareText size={14} /> },
  ];

  return (
    <div className="flex flex-col gap-2 pt-2 pb-4 animate-in fade-in slide-in-from-bottom-2 duration-500 delay-200">
      <div className="flex items-center gap-2 pl-1 mb-1">
        <div className="w-8 h-[1px] bg-gray-200"></div>
        <p className="text-[9px] font-bold text-gray-400 uppercase tracking-[0.2em]">Suggested Actions</p>
      </div>
      <div className="flex flex-wrap gap-2">
        {actions.map((action) => (
          <button
            key={action.label}
            onClick={() => onAction(action.label)}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-white/60 backdrop-blur-sm border border-gray-100/50 rounded-full text-[11px] font-medium text-gray-600 hover:border-brand/40 hover:text-brand hover:bg-brand/5 hover:shadow-md transition-all active:scale-95"
          >
            <span className="text-brand/70">{action.icon}</span>
            {action.label}
          </button>
        ))}
      </div>
    </div>
  );
}
