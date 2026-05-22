"use client";

import { Bot } from "lucide-react";

export default function TypingIndicator() {
  return (
    <div className="flex w-full justify-start animate-in fade-in slide-in-from-bottom-2 duration-300">
      <div className="flex gap-3 max-w-[85%]">
        <div className="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-white bg-slate-800 shadow-sm">
          <Bot size={14} />
        </div>
        <div className="bg-white/80 backdrop-blur-sm text-gray-800 border border-gray-100 px-4 py-3 rounded-2xl rounded-tl-none shadow-sm flex gap-1.5 items-center">
          <div className="w-1.5 h-1.5 bg-brand/40 rounded-full animate-bounce [animation-delay:-0.3s]"></div>
          <div className="w-1.5 h-1.5 bg-brand/40 rounded-full animate-bounce [animation-delay:-0.15s]"></div>
          <div className="w-1.5 h-1.5 bg-brand/40 rounded-full animate-bounce"></div>
        </div>
      </div>
    </div>
  );
}
