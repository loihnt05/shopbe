"use client";

import { motion } from "framer-motion";
import { Sparkles, Package, Lightbulb, ShoppingBag } from "lucide-react";

interface QuickActionsProps {
  onAction: (text: string) => void;
}

export default function QuickActions({ onAction }: QuickActionsProps) {
  const actions = [
    { label: "Recommended for me", icon: <Sparkles size={14} />, color: "text-amber-500" },
    { label: "Order Status", icon: <Package size={14} />, color: "text-blue-500" },
    { label: "Latest Deals", icon: <ShoppingBag size={14} />, color: "text-green-500" },
    { label: "How to return?", icon: <Lightbulb size={14} />, color: "text-purple-500" },
  ];

  return (
    <div className="flex gap-2 py-1 px-1">
      {actions.map((action, idx) => (
        <motion.button
          key={action.label}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: idx * 0.1 }}
          onClick={() => onAction(action.label)}
          className="flex items-center gap-2 px-4 py-2 bg-white border border-slate-200 rounded-full text-[13px] font-medium text-slate-600 hover:border-slate-400 hover:shadow-md transition-all whitespace-nowrap active:scale-95"
        >
          <span className={action.color}>{action.icon}</span>
          {action.label}
        </motion.button>
      ))}
    </div>
  );
}
