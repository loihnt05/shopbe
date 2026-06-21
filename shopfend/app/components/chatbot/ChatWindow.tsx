"use client";

import { useRef, useEffect, useState } from "react";
import { X, Send, Bot, Maximize2, Settings, Paperclip, Mic, ChevronDown, Smile } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
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
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [inputText, setInputText] = useState("");
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showScrollButton, setShowScrollButton] = useState(false);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, isTyping]);

  const handleScroll = () => {
    if (scrollRef.current) {
      const { scrollTop, scrollHeight, clientHeight } = scrollRef.current;
      setShowScrollButton(scrollHeight - scrollTop - clientHeight > 200);
    }
  };

  const scrollToBottom = () => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: "smooth" });
  };

  const handleTextareaChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setInputText(e.target.value);
    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
      textareaRef.current.style.height = `${Math.min(textareaRef.current.scrollHeight, 150)}px`;
    }
  };

  const handleSubmit = (e?: React.FormEvent) => {
    e?.preventDefault();
    const text = inputText.trim();
    if (text) {
      onSendMessage(text);
      setInputText("");
      if (textareaRef.current) textareaRef.current.style.height = "auto";
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit();
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95, y: 20, transformOrigin: "bottom right" }}
      animate={{ opacity: 1, scale: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.95, y: 20 }}
      transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
      className={`
        flex min-h-0 flex-col mb-4 overflow-hidden
        bg-white/95 backdrop-blur-2xl border border-slate-200 shadow-[0_32px_128px_rgba(0,0,0,0.18)]
        transition-all duration-500 rounded-[24px]
        ${isFullscreen 
          ? "fixed inset-4 z-[200] h-[calc(100dvh-2rem)] w-auto sm:w-auto" 
          : "w-[calc(100vw-2rem)] h-[70vh] sm:w-[400px] sm:h-[680px]"
        }
        relative flex flex-col
      `}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 bg-white/40 backdrop-blur-md border-b border-slate-100 sticky top-0 z-10">
        <div className="flex items-center gap-3">
          <div className="relative group">
            <div className="w-10 h-10 rounded-full bg-slate-900 flex items-center justify-center text-white shadow-lg overflow-hidden ring-4 ring-slate-50 group-hover:scale-105 transition-transform">
              <Bot size={22} className="text-white" />
            </div>
            <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 border-2 border-white rounded-full"></span>
          </div>
          <div>
            <h3 className="font-bold text-sm text-slate-900 tracking-tight">Shopbee AI</h3>
            <div className="flex items-center gap-1.5">
              <span className="text-[10px] text-slate-400 font-semibold uppercase tracking-wider">Assistant</span>
            </div>
          </div>
        </div>
        <div className="flex items-center gap-1">
          <button 
            onClick={() => setIsFullscreen(!isFullscreen)}
            className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-xl transition-all active:scale-90"
            title="Toggle Fullscreen"
          >
            <Maximize2 size={16} />
          </button>
          <button 
            className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-xl transition-all active:scale-90"
            title="Settings"
          >
            <Settings size={16} />
          </button>
          <button 
            onClick={onClose} 
            className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-xl transition-all active:scale-90"
            title="Close"
          >
            <X size={18} />
          </button>
        </div>
      </div>

      {/* Messages */}
      <div 
        ref={scrollRef}
        onScroll={handleScroll}
        className="min-h-0 flex-1 overflow-y-auto px-4 py-4 sm:px-6 space-y-6 scroll-smooth bg-[#FAFAFA]/30 sb-scrollbar"
      >
        <div className="flex flex-col gap-4 min-h-full justify-end">
          <div className="text-center py-4">
            <span className="px-3 py-1 bg-white border border-slate-100 text-slate-400 text-[10px] font-bold rounded-full shadow-sm uppercase tracking-widest">
              Today
            </span>
          </div>
          
          <AnimatePresence mode="popLayout">
            {messages.map((msg) => (
              <ChatMessage key={msg.id} message={msg} />
            ))}
          </AnimatePresence>
          
          {isTyping && <TypingIndicator />}
          
          <div className="h-2" />
        </div>
      </div>

      {/* Floating Scroll Button */}
      <AnimatePresence>
        {showScrollButton && (
          <motion.button
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 10 }}
            onClick={scrollToBottom}
            className="absolute bottom-32 left-1/2 -translate-x-1/2 bg-white/90 backdrop-blur-md border border-slate-200 p-2 rounded-full shadow-xl text-slate-500 hover:text-brand transition-colors z-20"
          >
            <ChevronDown size={20} />
          </motion.button>
        )}
      </AnimatePresence>

      {/* Input Area */}
      <div className="shrink-0 p-4 bg-white/60 backdrop-blur-xl border-t border-slate-100">
        <div className="max-w-3xl mx-auto space-y-3">
          <QuickActions onAction={onSendMessage} />

          <div className="relative flex items-end gap-2 bg-white border border-slate-200/80 rounded-[20px] p-2 shadow-sm focus-within:shadow-md focus-within:border-slate-300 transition-all group">
            <div className="flex flex-col items-center pb-2 pl-1">
              <button className="p-2 text-slate-400 hover:text-slate-600 transition-colors">
                <Paperclip size={18} />
              </button>
            </div>
            
            <textarea
              ref={textareaRef}
              rows={1}
              value={inputText}
              onChange={handleTextareaChange}
              onKeyDown={handleKeyDown}
              placeholder="Message Shopbee AI..."
              className="flex-1 bg-transparent border-none outline-none text-[15px] py-3 text-slate-800 placeholder:text-slate-400 resize-none max-h-[150px]"
            />

            <div className="flex items-center gap-1 pb-1 pr-1">
              <button className="p-2 text-slate-400 hover:text-slate-600 transition-colors hidden sm:block">
                <Smile size={18} />
              </button>
              <button className="p-2 text-slate-400 hover:text-slate-600 transition-colors">
                <Mic size={18} />
              </button>
              <button 
                onClick={() => handleSubmit()}
                disabled={isTyping || !inputText.trim()}
                className="p-2.5 bg-slate-900 text-white rounded-xl hover:bg-slate-800 transition-all active:scale-95 disabled:opacity-20 disabled:bg-slate-400 shadow-md shadow-slate-200 disabled:shadow-none"
              >
                <Send size={18} />
              </button>
            </div>
          </div>
          
          <div className="flex items-center justify-between px-2">
            <div className="flex items-center gap-1.5">
              <div className="w-1.5 h-1.5 rounded-full bg-brand"></div>
              <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Memory Active</p>
            </div>
            <p className="text-[9px] text-slate-300 font-medium italic">⌘ + Enter to send</p>
          </div>
        </div>
      </div>
    </motion.div>
  );
}
