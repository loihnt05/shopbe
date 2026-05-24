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
      bg-white border border-slate-200 shadow-[0_20px_60px_rgba(0,0,0,0.18)] rounded-[20px]
      transition-all duration-500 animate-in slide-in-from-bottom-8 fade-in
      w-[calc(100vw-2rem)] h-[60vh] sm:w-[320px] sm:h-[500px]
      relative
    `}>
      {/* Header */}
      <div className="flex items-center justify-between p-4 bg-brand text-white sticky top-0 z-10 shadow-md">
        <div className="flex items-center gap-2.5">
          <div className="relative group">
            <div className="w-10 h-10 rounded-xl bg-white/20 flex items-center justify-center border border-white/30 shadow-inner">
              <Bot size={22} className="drop-shadow-sm" />
            </div>
            <span className="absolute -bottom-0.5 -right-0.5 w-3 h-3 bg-green-400 border-2 border-brand rounded-full"></span>
          </div>
          <div>
            <h3 className="font-bold text-sm tracking-tight leading-none mb-1">Shopbee Assistant</h3>
            <div className="flex items-center gap-1">
              <span className="w-1 h-1 bg-green-400 rounded-full animate-pulse"></span>
              <p className="text-[9px] text-white/80 font-bold uppercase tracking-wider">Online</p>
            </div>
          </div>
        </div>
        <div className="flex items-center gap-0.5">
          <button 
            onClick={onClose} 
            className="p-1.5 hover:bg-white/10 rounded-lg transition-all active:scale-90"
            title="Minimize"
          >
            <Minus size={18} />
          </button>
          <button 
            onClick={onClose} 
            className="p-1.5 hover:bg-white/10 rounded-lg transition-all active:scale-90"
            title="Close"
          >
            <X size={18} />
          </button>
        </div>
      </div>

      {/* Messages */}
      <div 
        ref={scrollRef}
        className="flex-1 overflow-y-auto p-4 space-y-4 scroll-smooth scrollbar-hide bg-slate-50/50"
        style={{
          backgroundImage: "radial-gradient(circle, #e2e8f0 1.2px, transparent 1.2px)",
          backgroundSize: "20px 20px"
        }}
      >
        <div className="flex flex-col gap-3 min-h-full justify-end">
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
      <form onSubmit={handleSubmit} className="p-3 bg-white border-t border-slate-100">
        <div className="flex items-center gap-2 bg-slate-50 border border-slate-200 rounded-xl px-3 py-1.5 focus-within:ring-4 focus-within:ring-brand/5 focus-within:border-brand/30 focus-within:bg-white transition-all group">
          <input
            ref={inputRef}
            type="text"
            placeholder="How can we help?"
            className="flex-1 bg-transparent border-none outline-none text-[13px] py-1 text-slate-700 placeholder:text-slate-400"
          />
          <button 
            type="submit"
            className="p-2 bg-brand text-white rounded-lg hover:bg-brand-hover transition-all active:scale-95 disabled:opacity-50"
            disabled={isTyping}
          >
            <Send size={16} />
          </button>
        </div>
        <p className="text-[8px] text-center text-slate-400 mt-2 font-bold uppercase tracking-widest">
          Shopbee AI Assistant
        </p>
      </form>
    </div>
  );
}
