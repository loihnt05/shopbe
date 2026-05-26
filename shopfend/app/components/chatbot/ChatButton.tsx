"use client";

import { motion } from "framer-motion";
import { MessageCircle, X, Sparkles } from "lucide-react";

interface ChatButtonProps {
  onClick: () => void;
  isOpen: boolean;
  unreadCount: number;
}

export default function ChatButton({ onClick, isOpen, unreadCount }: ChatButtonProps) {
  return (
    <motion.button
      whileHover={{ scale: 1.05 }}
      whileTap={{ scale: 0.95 }}
      onClick={onClick}
      className={`
        relative group flex items-center justify-center w-16 h-16 
        rounded-full shadow-[0_8px_32px_rgba(0,0,0,0.15)] transition-all duration-300
        ${isOpen 
          ? "bg-white text-slate-900 border border-slate-200" 
          : "bg-slate-900 text-white hover:bg-slate-800"
        }
      `}
    >
      <div className="absolute inset-0 rounded-full bg-slate-900 opacity-0 group-hover:opacity-10 transition-opacity blur-xl"></div>
      
      {isOpen ? (
        <X size={28} />
      ) : (
        <div className="relative">
          <MessageCircle size={28} />
          <motion.div 
            animate={{ scale: [1, 1.2, 1], opacity: [0.5, 0, 0.5] }}
            transition={{ duration: 2, repeat: Infinity }}
            className="absolute -top-1 -right-1"
          >
            <Sparkles size={14} className="text-yellow-400 fill-yellow-400" />
          </motion.div>
        </div>
      )}

      {!isOpen && unreadCount > 0 && (
        <span className="absolute -top-1 -right-1 flex h-6 w-6 items-center justify-center rounded-full bg-brand text-[10px] font-bold text-white shadow-lg ring-2 ring-white">
          {unreadCount > 9 ? "9+" : unreadCount}
        </span>
      )}
      
      {/* Premium Tooltip */}
      {!isOpen && (
        <div className="absolute right-full mr-6 px-4 py-2 bg-slate-900 text-white text-[13px] font-medium rounded-2xl shadow-2xl opacity-0 group-hover:opacity-100 transition-all translate-x-4 group-hover:translate-x-0 pointer-events-none whitespace-nowrap">
          How can I help you today?
          <div className="absolute top-1/2 -right-1 -translate-y-1/2 border-[6px] border-transparent border-l-slate-900"></div>
        </div>
      )}
    </motion.button>
  );
}
