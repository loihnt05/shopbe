"use client";

import { MessageCircle, X } from "lucide-react";

interface ChatButtonProps {
  onClick: () => void;
  isOpen: boolean;
  unreadCount: number;
}

export default function ChatButton({ onClick, isOpen, unreadCount }: ChatButtonProps) {
  return (
    <button
      onClick={onClick}
      className={`
        relative group flex items-center justify-center w-14 h-14 
        rounded-full shadow-lg transition-all duration-300 active:scale-90
        ${isOpen 
          ? "bg-white text-gray-800 rotate-90" 
          : "bg-brand text-white hover:bg-brand-hover hover:-translate-y-1"
        }
      `}
    >
      {isOpen ? (
        <X size={28} className="animate-in fade-in zoom-in duration-300" />
      ) : (
        <MessageCircle size={28} className="animate-in fade-in zoom-in duration-300" />
      )}

      {!isOpen && unreadCount > 0 && (
        <span className="absolute -top-1 -right-1 flex h-6 w-6 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white shadow-sm ring-2 ring-white animate-bounce">
          {unreadCount > 9 ? "9+" : unreadCount}
        </span>
      )}
      
      {/* Tooltip */}
      {!isOpen && (
        <span className="absolute right-full mr-4 px-3 py-1 bg-gray-900 text-white text-xs rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap pointer-events-none">
          Need help? Chat with us!
        </span>
      )}
    </button>
  );
}
