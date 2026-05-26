"use client";

import { useState, useRef, useEffect } from "react";
import { useSession, signIn } from "next-auth/react";
import { shopbeApi, ChatMessageDto } from "../../lib/shopbeApi";
import Link from "next/link";

type RecommendationResponse = {
  recommendation_reason: string;
  products: Array<{
    id: string;
    name: string;
    category: string;
    price: string;
    score: string;
  }>;
  data_source: string;
};

function RecommendationMessage({ content }: { content: string }) {
  try {
    const trimmed = content.trim();
    // Basic heuristic to check if it's JSON
    if (!trimmed.startsWith("{")) {
        return <div className="whitespace-pre-wrap leading-relaxed">{content}</div>;
    }

    let data: RecommendationResponse;
    try {
        data = JSON.parse(trimmed) as RecommendationResponse;
    } catch (parseError) {
        // If JSON is truncated, try to extract the reason at least
        const reasonMatch = trimmed.match(/"recommendation_reason":\s*"([^"]+)"/);
        if (reasonMatch && reasonMatch[1]) {
            return (
                <div className="space-y-2">
                    <p className="font-medium text-slate-800">{reasonMatch[1]}</p>
                    <p className="text-xs text-slate-400 italic">(Recommendations loading or incomplete...)</p>
                </div>
            );
        }
        throw parseError;
    }

    if (!data.products || !Array.isArray(data.products)) {
        return <div className="whitespace-pre-wrap leading-relaxed">{content}</div>;
    }

    return (
      <div className="space-y-4">
        <p className="font-medium text-slate-800">{data.recommendation_reason}</p>
        <div className="grid grid-cols-1 gap-3">
          {data.products.map((p) => (
            <Link 
              key={p.id} 
              href={`/products/${p.id}`}
              className="flex items-center gap-3 p-3 bg-slate-50 rounded-xl border border-slate-100 hover:border-brand/30 hover:bg-brand/5 transition-all group"
            >
              <div className="w-12 h-12 bg-white rounded-lg flex items-center justify-center text-xl shadow-sm group-hover:scale-110 transition-transform">
                🛍️
              </div>
              <div className="flex-1 min-w-0">
                <h4 className="font-bold text-slate-900 truncate">{p.name}</h4>
                <div className="flex items-center gap-2 text-xs text-slate-500">
                  <span className="bg-slate-200 px-1.5 py-0.5 rounded uppercase font-semibold text-[10px]">
                    {p.category}
                  </span>
                  <span className="text-brand font-bold">{p.price}</span>
                </div>
              </div>
              <div className="text-brand opacity-0 group-hover:opacity-100 transition-opacity">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"><path d="m9 18 6-6-6-6"/></svg>
              </div>
            </Link>
          ))}
        </div>
        <div className="flex items-center justify-between pt-2 border-t border-slate-100 text-[10px] text-slate-400 font-bold uppercase tracking-wider">
            <span>Powered by {data.data_source}</span>
        </div>
      </div>
    );
  } catch (error) {
    console.debug("Failed to parse message as JSON recommendation:", error);
    return <div className="whitespace-pre-wrap leading-relaxed">{content}</div>;
  }
}

export default function ChatPage() {
  const { data: session, status } = useSession();
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [conversationId, setConversationId] = useState<string | null>(null);
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isInitializing, setIsInitializing] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, isLoading]);

  useEffect(() => {
    if (status === "unauthenticated") {
        signIn();
        return;
    }

    if (status === "authenticated" && session?.accessToken) {
        initChat();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status, session?.accessToken]);

  const initChat = async () => {
    if (!session?.accessToken) return;
    setIsInitializing(true);
    try {
        const conversations = await shopbeApi.chat.getConversations(session.accessToken);
        let activeConv = conversations[0];

        if (!activeConv) {
            activeConv = await shopbeApi.chat.createConversation(session.accessToken, { title: "New Chat" });
        }

        setConversationId(activeConv.id);
        const history = await shopbeApi.chat.getMessages(session.accessToken, activeConv.id);
        setMessages(history);
    } catch (error) {
        console.error("Failed to initialize chat:", error);
    } finally {
        setIsInitializing(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || isLoading || !conversationId || !session?.accessToken) return;

    const userText = input.trim();
    setInput("");
    setIsLoading(true);

    try {
      const response = await shopbeApi.chat.sendMessage(session.accessToken, conversationId, {
        content: userText
      });

      // Backend returns the full pair (user + assistant) or just assistant depending on implementation.
      // Based on ChatController.SendMessage, it returns IReadOnlyList<ChatMessageDto> which contains both.
      setMessages(response);
    } catch (error) {
      console.error(error);
      // We don't have a stable way to add a local error message since ChatMessageDto needs backend IDs.
      // But we can show an alert or a temporary UI state.
    } finally {
      setIsLoading(false);
    }
  };

  if (status === "loading" || isInitializing) {
    return (
        <div className="flex flex-col items-center justify-center min-h-[400px] space-y-4">
            <div className="w-12 h-12 border-4 border-brand border-t-transparent rounded-full animate-spin"></div>
            <p className="text-slate-500 font-medium">Connecting to Shopbee AI...</p>
        </div>
    );
  }

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
              <h1 className="text-xl font-bold">Shopbee AI Assistant</h1>
              <div className="flex items-center gap-2 text-xs text-white/80">
                <span className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></span>
                Connected | Personalized Recommendations
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
            {messages.length === 0 && (
                <div className="text-center py-12">
                    <div className="text-4xl mb-4">👋</div>
                    <h3 className="text-lg font-bold text-slate-800">Welcome to Shopbee Chat!</h3>
                    <p className="text-slate-500 text-sm max-w-xs mx-auto">
                        Ask me for product recommendations, gift ideas, or help with your orders.
                    </p>
                </div>
            )}

            {messages.map((m) => (
              <div
                key={m.id}
                className={`flex ${m.sender === "user" ? "justify-end" : "justify-start"} animate-in`}
                style={{ animationName: 'slideInUp' }}
              >
                <div
                  className={`max-w-[85%] rounded-2xl px-5 py-3 text-sm shadow-sm transition-all hover:shadow-md ${
                    m.sender === "user"
                      ? "bg-brand text-white rounded-br-none"
                      : "bg-white text-slate-800 rounded-bl-none border border-slate-100"
                  }`}
                >
                  {m.sender === "assistant" ? (
                    <RecommendationMessage content={m.content} />
                  ) : (
                    <div className="whitespace-pre-wrap leading-relaxed">{m.content}</div>
                  )}
                  <div className={`text-[10px] mt-1 opacity-50 ${m.sender === "user" ? "text-right" : "text-left"}`}>
                    {new Date(m.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </div>
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
                placeholder="Ask for product recommendations..."
                className="flex-1 bg-slate-50 border border-slate-200 focus:border-brand focus:ring-4 focus:ring-brand/5 rounded-2xl px-5 py-3 text-sm outline-none transition-all"
                disabled={isLoading}
              />
              <button
                type="submit"
                disabled={isLoading || !input.trim() || !conversationId}
                className="bg-brand hover:bg-brand-hover text-white w-12 h-12 rounded-2xl flex items-center justify-center transition-all shadow-lg shadow-brand/20 active:scale-95 disabled:opacity-50 disabled:shadow-none"
              >
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <path d="m22 2-7 20-4-9-9-4Z"/><path d="M22 2 11 13"/>
                </svg>
              </button>
            </form>
            <p className="text-[10px] text-center text-slate-400 mt-2 uppercase tracking-widest font-bold">
              AI recommendations are based on your browsing history
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
