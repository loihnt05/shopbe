"use client";

import { motion } from "framer-motion";
import { Sparkles, Package, Lightbulb, ShoppingBag } from "lucide-react";

interface QuickActionsProps {
  onAction: (text: string) => void;
}

export default function QuickActions({ onAction }: QuickActionsProps) {
  const actions = [
    { label: "Recommended for you", icon: <Sparkles size={16} />, color: "text-amber-500", badge: "AI" },
    { label: "Order Status", icon: <Package size={16} />, color: "text-blue-500", badge: "Track" },
    { label: "Latest Deals", icon: <ShoppingBag size={16} />, color: "text-green-500", badge: "New" },
    { label: "How to return?", icon: <Lightbulb size={16} />, color: "text-purple-500", badge: "Help" },
  ];

  return (
    <div className="overflow-x-auto px-1 pb-2 sb-scrollbar">
      <div className="flex min-w-max gap-3 py-1">
      {actions.map((action, idx) => (
        <motion.button
          key={action.label}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: idx * 0.1 }}
          onClick={() => onAction(action.label)}
          className="group min-w-[158px] rounded-2xl border border-slate-200 bg-white px-4 py-3 text-left shadow-sm transition hover:-translate-y-0.5 hover:border-slate-300 hover:shadow-md active:scale-[0.98]"
        >
          <div className="flex items-center justify-between gap-3">
            <span className={action.color}>{action.icon}</span>
            <span className="rounded-full bg-[#FFF1ED] px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-[#EE4D2D]">
              {action.badge}
            </span>
          </div>
          <p className="mt-3 text-sm font-medium text-slate-700 whitespace-nowrap">
            {action.label}
          </p>
        </motion.button>
      ))}
      </div>
    </div>
  );
}
