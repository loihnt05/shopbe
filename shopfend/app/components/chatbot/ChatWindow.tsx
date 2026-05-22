"use client";

import { useRef, useEffect } from "react";
import { X, Send, Bot, User, Minus } from "lucide-react";
import { Message } from "./Chatbot";
import ChatMessage from "./ChatMessage";
import TypingIndicator from "./TypingIndicator";
import QuickActions from "./QuickActions";

interface ChatWindowProps {
  messages: Message[];
  isTyping: boolean;
  onClose: () => void;
  onSendMessage: (text: string) => void;
}

export default function ChatWindow({ messages, isTyping, onClose, onSendMessage }: ChatWindowProps) {
  const scrollRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, isTyping]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const text = inputRef.current?.value.trim();
    if (text && inputRef.current) {
      onSendMessage(text);
      inputRef.current.value = "";
    }
  };

  return (
    <div className={`
      flex flex-col mb-4 overflow-hidden
      bg-white/70 backdrop-blur-2xl border border-white/30 shadow-[0_20px_50px_rgba(0,0,0,0.15)] rounded-[20px]
      transition-all duration-300 animate-in slide-in-from-bottom-10 fade-in
      w-[calc(100vw-2rem)] h-[70vh] sm:w-[380px] sm:h-[600px]
      relative
    `}>
      {/* Header */}
      <div className="flex items-center justify-between p-4 bg-brand/90 backdrop-blur-md text-white sticky top-0 z-10 shadow-sm">
        <div className="flex items-center gap-3">
          <div className="relative">
            <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center border border-white/40 shadow-inner">
              <Bot size={24} className="drop-shadow-sm" />
            </div>
            <span className="absolute bottom-0.5 right-0.5 w-2.5 h-2.5 bg-green-400 border-2 border-white rounded-full shadow-sm"></span>
          </div>
          <div>
            <h3 className="font-bold text-sm tracking-tight">Shopbee AI</h3>
            <div className="flex items-center gap-1.5">
              <span className="w-1.5 h-1.5 bg-green-400 rounded-full animate-pulse"></span>
              <p className="text-[10px] text-white/90 font-medium uppercase tracking-widest">Online</p>
            </div>
          </div>
        </div>
        <div className="flex items-center gap-1">
          <button 
            onClick={onClose} 
            className="p-2 hover:bg-white/20 rounded-full transition-all active:scale-90"
            title="Minimize"
          >
            <Minus size={18} />
          </button>
          <button 
            onClick={onClose} 
            className="p-2 hover:bg-white/20 rounded-full transition-all active:scale-90"
            title="Close"
          >
            <X size={18} />
          </button>
        </div>
      </div>

      {/* Messages */}
      <div 
        ref={scrollRef}
        className="flex-1 overflow-y-auto p-4 space-y-4 scroll-smooth scrollbar-hide"
      >
        <div className="flex flex-col gap-4 min-h-full justify-end">
          {messages.map((msg) => (
            <ChatMessage key={msg.id} message={msg} />
          ))}
          {isTyping && <TypingIndicator />}
          
          {/* Suggested Actions */}
          {messages.length > 0 && messages[messages.length - 1].sender === "bot" && !isTyping && (
            <QuickActions onAction={onSendMessage} />
          )}
        </div>
      </div>

      {/* Input */}
      <form onSubmit={handleSubmit} className="p-4 bg-white/40 backdrop-blur-md border-t border-white/20">
        <div className="flex items-center gap-2 bg-white/80 border border-gray-200/50 rounded-2xl px-4 py-2 shadow-sm focus-within:ring-2 focus-within:ring-brand/20 focus-within:border-brand/40 transition-all group">
          <input
            ref={inputRef}
            type="text"
            placeholder="Ask anything..."
            className="flex-1 bg-transparent border-none outline-none text-sm py-1.5 text-gray-800 placeholder:text-gray-400"
          />
          <button 
            type="submit"
            className="p-2 bg-brand text-white rounded-xl hover:bg-brand-hover hover:shadow-lg transition-all shadow-md active:scale-95 disabled:opacity-50 disabled:grayscale"
            disabled={isTyping}
          >
            <Send size={18} className={isTyping ? "" : "group-focus-within:translate-x-0.5 group-focus-within:-translate-y-0.5 transition-transform"} />
          </button>
        </div>
        <div className="flex items-center justify-center gap-1.5 mt-3 opacity-60">
          <div className="w-1 h-1 bg-gray-400 rounded-full"></div>
          <p className="text-[9px] font-bold text-gray-500 uppercase tracking-widest">
            Powered by Shopbee AI
          </p>
          <div className="w-1 h-1 bg-gray-400 rounded-full"></div>
        </div>
      </form>
    </div>
  );
}
