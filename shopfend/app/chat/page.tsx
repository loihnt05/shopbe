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
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="sb-card p-6 sm:p-8">
        <h1 className="text-2xl font-bold tracking-tight text-[var(--foreground)] mb-2">Shopbee Support Chat</h1>
        <p className="text-[var(--muted)] mb-6 text-sm">Ask about our products, shipping, or recommendations.</p>

        <div className="bg-[var(--surface)] border border-black/10 rounded-md flex flex-col h-[500px]">
          <div className="flex-1 overflow-y-auto p-4 space-y-4">
            {messages.map((m, idx) => (
              <div
                key={idx}
                className={`flex ${m.role === "user" ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[80%] rounded-2xl px-4 py-2 text-sm ${
                    m.role === "user"
                      ? "bg-[var(--brand)] text-white rounded-br-none"
                      : "bg-[color:color-mix(in_srgb,var(--foreground)_5%,transparent)] text-[var(--foreground)] rounded-bl-none"
                  }`}
                >
                  {m.content}
                </div>
              </div>
            ))}
            {isLoading && (
              <div className="flex justify-start">
                <div className="bg-[color:color-mix(in_srgb,var(--foreground)_5%,transparent)] text-[var(--foreground)] max-w-[80%] rounded-2xl rounded-bl-none px-4 py-2 text-sm flex gap-1 items-center">
                  <div className="w-2 h-2 bg-current rounded-full animate-bounce"></div>
                  <div className="w-2 h-2 bg-current rounded-full animate-bounce" style={{ animationDelay: '0.1s' }}></div>
                  <div className="w-2 h-2 bg-current rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          <form onSubmit={handleSubmit} className="border-t border-black/10 p-4 bg-[var(--surface)] flex gap-2">
            <input
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Type your message..."
              className="flex-1 border border-black/15 focus:border-[var(--brand)] rounded-md px-3 py-2 text-sm outline-none"
              disabled={isLoading}
            />
            <button
              type="submit"
              disabled={isLoading || !input.trim()}
              className="sb-btn-primary px-6 disabled:opacity-50"
            >
              Send
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
