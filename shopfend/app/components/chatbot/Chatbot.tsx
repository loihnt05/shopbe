"use client";

import { useState, useEffect } from "react";
import ChatButton from "./ChatButton";
import ChatWindow from "./ChatWindow";
import { useSession } from "next-auth/react";
import { shopbeApi, ChatMessageDto } from "../../../lib/shopbeApi";

export type Message = {
  id: string;
  text: string;
  sender: "user" | "bot";
  timestamp: Date;
};

export default function Chatbot() {
  const { data: session } = useSession();
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
  const [conversationId, setConversationId] = useState<string | null>(null);

  // Load conversation on mount if authenticated
  useEffect(() => {
    if (session?.accessToken) {
      initChat();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session?.accessToken]);

  const initChat = async () => {
    if (!session?.accessToken) return;
    try {
      const conversations = await shopbeApi.chat.getConversations(session.accessToken);
      let activeConv = conversations[0];

      if (!activeConv) {
        activeConv = await shopbeApi.chat.createConversation(session.accessToken, { title: "Overlay Chat" });
      }
      setConversationId(activeConv.id);
      
      const history = await shopbeApi.chat.getMessages(session.accessToken, activeConv.id, { take: 20 });
      setMessages(history.map(m => ({
        id: m.id,
        text: m.content,
        sender: m.sender === "user" ? "user" : "bot",
        timestamp: new Date(m.createdAt)
      })));
    } catch (e) {
      console.error("Failed to init overlay chat", e);
    }
  };

  const toggleChat = () => {
    setIsOpen(!isOpen);
    if (!isOpen) {
      setUnreadCount(0);
    }
  };

  const simulateStreaming = async (fullText: string) => {
    const botMsgId = (Date.now() + 1).toString();
    const botMessage: Message = {
      id: botMsgId,
      text: "",
      sender: "bot",
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, botMessage]);
    
    // Check if it's JSON recommendation
    if (fullText.trim().startsWith("{")) {
        try {
            const data = JSON.parse(fullText);
            if (data.products) {
                const summary = `${data.recommendation_reason}\n\nTop picks:\n${data.products.map((p: { name: string; price: string }) => `- ${p.name} (${p.price})`).join("\n")}\n\n[Open full chat for details](/chat)`;
                fullText = summary;
            }
        } catch(e) { 
            // Handle truncated JSON in overlay too
            const reasonMatch = fullText.match(/"recommendation_reason":\s*"([^"]+)"/);
            if (reasonMatch && reasonMatch[1]) {
                fullText = `${reasonMatch[1]}\n\n(Recommendations are being prepared, please check the full chat for the complete list.)\n\n[Open full chat](/chat)`;
            }
        }
    }

    const words = fullText.split(" ");
    let currentText = "";
    
    for (let i = 0; i < words.length; i++) {
      currentText += (i === 0 ? "" : " ") + words[i];
      setMessages((prev) => 
        prev.map(m => m.id === botMsgId ? { ...m, text: currentText } : m)
      );
      await new Promise(r => setTimeout(r, Math.random() * 20 + 5));
    }
  };

  const handleSendMessage = async (text: string) => {
    if (!session?.accessToken || !conversationId) {
        // Fallback or ask to login
        const loginMsg: Message = {
            id: Date.now().toString(),
            text: "Please sign in to chat with our AI assistant!",
            sender: "bot",
            timestamp: new Date(),
        };
        setMessages((prev) => [...prev, loginMsg]);
        return;
    }

    const userMessage: Message = {
      id: Date.now().toString(),
      text,
      sender: "user",
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setIsTyping(true);

    try {
      const results = await shopbeApi.chat.sendMessage(session.accessToken, conversationId, {
        content: text
      });

      const assistantMsg = results.find(m => m.sender === "assistant");
      setIsTyping(false);
      
      if (assistantMsg) {
        await simulateStreaming(assistantMsg.content);
      }

    } catch (error) {
      console.error("Chat error:", error);
      setIsTyping(false);
      const errorMessage: Message = {
        id: (Date.now() + 2).toString(),
        text: "I'm sorry, I'm having trouble connecting to the recommendation engine.",
        sender: "bot",
        timestamp: new Date(),
      };
      setMessages((prev) => [...prev, errorMessage]);
    } finally {
      setIsTyping(false);
      if (!isOpen) {
        setUnreadCount((prev) => prev + 1);
      }
    }
  };

  return (
    <div className="fixed bottom-6 right-6 z-[100] flex flex-col items-end">
      {isOpen && (
        <ChatWindow
          messages={messages}
          isTyping={isTyping}
          onClose={toggleChat}
          onSendMessage={handleSendMessage}
        />
      )}
      <ChatButton 
        onClick={toggleChat} 
        isOpen={isOpen} 
        unreadCount={unreadCount}
      />
    </div>
  );
}
