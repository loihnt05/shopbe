"use client";

import { useState } from "react";
import { Bot, User, Copy, ThumbsUp, ThumbsDown, RotateCcw } from "lucide-react";
import { motion } from "framer-motion";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { Message } from "./Chatbot";

interface ChatMessageProps {
  message: Message;
}

export default function ChatMessage({ message }: ChatMessageProps) {
  const [isHovered, setIsHovered] = useState(false);
  const isBot = message.sender === "bot";

  const copyToClipboard = () => {
    navigator.clipboard.writeText(message.text);
  };

  return (
    <motion.div 
      layout
      initial={{ opacity: 0, y: 10, scale: 0.95 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      className={`flex w-full ${isBot ? "justify-start" : "justify-end"} group relative`}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div className={`flex gap-4 max-w-[85%] ${isBot ? "flex-row" : "flex-row-reverse"}`}>
        {/* Avatar */}
        <div className={`
          flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-white shadow-md
          ${isBot ? "bg-slate-900" : "bg-brand"}
          mt-1
        `}>
          {isBot ? <Bot size={16} /> : <User size={16} />}
        </div>

        {/* Bubble & Actions */}
        <div className={`flex flex-col gap-2 ${isBot ? "items-start" : "items-end"}`}>
          <div className={`
            relative px-5 py-3 rounded-[20px] text-[15px] leading-relaxed shadow-sm transition-all
            ${isBot 
              ? "bg-white text-slate-800 border border-slate-100 rounded-tl-none" 
              : "bg-slate-900 text-white rounded-tr-none"
            }
          `}>
            <div className="prose prose-sm max-w-none prose-slate">
              <ReactMarkdown remarkPlugins={[remarkGfm]}>
                {message.text}
              </ReactMarkdown>
            </div>
          </div>

          {/* Message Footer (Timestamp & Actions) */}
          <div className={`flex items-center gap-3 transition-opacity duration-200 ${isHovered ? "opacity-100" : "opacity-0"}`}>
            <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">
              {message.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
            </span>
            
            {isBot && (
              <div className="flex items-center gap-1 bg-white border border-slate-100 rounded-full px-2 py-1 shadow-sm">
                <button 
                  onClick={copyToClipboard}
                  className="p-1 text-slate-400 hover:text-slate-600 transition-colors"
                  title="Copy to clipboard"
                >
                  <Copy size={12} />
                </button>
                <button className="p-1 text-slate-400 hover:text-slate-600 transition-colors" title="Like">
                  <ThumbsUp size={12} />
                </button>
                <button className="p-1 text-slate-400 hover:text-slate-600 transition-colors" title="Dislike">
                  <ThumbsDown size={12} />
                </button>
              </div>
            )}

            {!isBot && (
              <button className="p-1 text-slate-400 hover:text-slate-600 transition-colors" title="Regenerate">
                <RotateCcw size={12} />
              </button>
            )}
          </div>
        </div>
      </div>
    </motion.div>
  );
}
