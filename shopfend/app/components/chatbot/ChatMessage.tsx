"use client";

import { Bot, User } from "lucide-react";
import { Message } from "./Chatbot";

interface ChatMessageProps {
  message: Message;
}

export default function ChatMessage({ message }: ChatMessageProps) {
  const isBot = message.sender === "bot";

  return (
    <div className={`flex w-full ${isBot ? "justify-start" : "justify-end"} animate-in fade-in slide-in-from-bottom-2 duration-300`}>
      <div className={`flex gap-3 max-w-[85%] ${isBot ? "flex-row" : "flex-row-reverse"}`}>
        {/* Avatar */}
        <div className={`
          flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-white shadow-sm
          ${isBot ? "bg-slate-800" : "bg-brand"}
        `}>
          {isBot ? <Bot size={14} /> : <User size={14} />}
        </div>

        {/* Bubble */}
        <div className={`flex flex-col gap-1 ${isBot ? "items-start" : "items-end"}`}>
          <div className={`
            px-4 py-2.5 rounded-2xl text-sm shadow-[0_2px_10px_rgba(0,0,0,0.05)] leading-relaxed
            ${isBot 
              ? "bg-white text-gray-800 border border-gray-100 rounded-tl-none" 
              : "bg-brand text-white rounded-tr-none"
            }
          `}>
            {message.text}
          </div>
          <span className="text-[9px] font-medium text-gray-400 px-1">
            {message.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
          </span>
        </div>
      </div>
    </div>
  );
}
