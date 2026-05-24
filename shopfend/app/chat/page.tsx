"use client";

import { useState, useRef, useEffect } from "react";

type Message = {
  role: "user" | "assistant";
  content: string;
};

export default function ChatPage() {
  const [messages, setMessages] = useState<Message[]>([
    { role: "assistant", content: "Hello! I am your Shopbee Assistant. How can I help you today?" }
  ]);
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || isLoading) return;

    const userMessage: Message = { role: "user", content: input };
    setMessages((prev) => [...prev, userMessage]);
    setInput("");
    setIsLoading(true);

    try {
      const response = await fetch("/api/demo-chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ messages: [...messages, userMessage] }),
      });

      if (!response.ok) throw new Error("Failed to fetch response");

      const data = await response.json();
      setMessages((prev) => [...prev, data]);
    } catch (error) {
      console.error(error);
      setMessages((prev) => [
        ...prev,
        { role: "assistant", content: "Sorry, I am having trouble connecting right now." }
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="sb-card overflow-hidden border-none shadow-xl">
        {/* Chat Header */}
        <div className="bg-brand p-6 text-white">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-white/20 rounded-full flex items-center justify-center text-2xl">
              🐝
            </div>
            <div>
              <h1 className="text-xl font-bold">Shopbee Assistant</h1>
              <div className="flex items-center gap-2 text-xs text-white/80">
                <span className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></span>
                Online | AI Powered Support
              </div>
            </div>
          </div>
        </div>

        <div 
          className="flex flex-col h-[600px] relative bg-slate-50"
          style={{
            backgroundImage: "radial-gradient(circle, #e2e8f0 1px, transparent 1px)",
            backgroundSize: "24px 24px"
          }}
        >
          {/* Messages Area */}
          <div className="flex-1 overflow-y-auto p-6 space-y-6">
            {messages.map((m, idx) => (
              <div
                key={idx}
                className={`flex ${m.role === "user" ? "justify-end" : "justify-start"} animate-in`}
                style={{ animationName: 'slideInUp' }}
              >
                <div
                  className={`max-w-[85%] rounded-2xl px-5 py-3 text-sm shadow-sm transition-all hover:shadow-md ${
                    m.role === "user"
                      ? "bg-brand text-white rounded-br-none"
                      : "bg-white text-slate-800 rounded-bl-none border border-slate-100"
                  }`}
                >
                  <div className="whitespace-pre-wrap leading-relaxed">{m.content}</div>
                </div>
              </div>
            ))}
            {isLoading && (
              <div className="flex justify-start animate-in" style={{ animationName: 'fadeIn' }}>
                <div className="bg-white text-slate-800 max-w-[80%] rounded-2xl rounded-bl-none px-5 py-3 text-sm shadow-sm border border-slate-100 flex gap-1.5 items-center">
                  <div className="w-1.5 h-1.5 bg-brand rounded-full animate-bounce"></div>
                  <div className="w-1.5 h-1.5 bg-brand rounded-full animate-bounce" style={{ animationDelay: '0.1s' }}></div>
                  <div className="w-1.5 h-1.5 bg-brand rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input Area */}
          <div className="p-4 bg-white/80 backdrop-blur-md border-t border-slate-100">
            <form onSubmit={handleSubmit} className="flex gap-2 max-w-3xl mx-auto">
              <input
                value={input}
                onChange={(e) => setInput(e.target.value)}
                placeholder="How can we help you today?"
                className="flex-1 bg-slate-50 border border-slate-200 focus:border-brand focus:ring-4 focus:ring-brand/5 rounded-2xl px-5 py-3 text-sm outline-none transition-all"
                disabled={isLoading}
              />
              <button
                type="submit"
                disabled={isLoading || !input.trim()}
                className="bg-brand hover:bg-brand-hover text-white w-12 h-12 rounded-2xl flex items-center justify-center transition-all shadow-lg shadow-brand/20 active:scale-95 disabled:opacity-50 disabled:shadow-none"
              >
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <path d="m22 2-7 20-4-9-9-4Z"/><path d="M22 2 11 13"/>
                </svg>
              </button>
            </form>
            <p className="text-[10px] text-center text-slate-400 mt-2 uppercase tracking-widest font-bold">
              AI assistant may provide inaccurate info
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
