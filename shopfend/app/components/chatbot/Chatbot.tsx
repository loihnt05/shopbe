"use client";

import { useState, useEffect } from "react";
import ChatButton from "./ChatButton";
import ChatWindow from "./ChatWindow";

export type Message = {
  id: string;
  text: string;
  sender: "user" | "bot";
  timestamp: Date;
};

export default function Chatbot() {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);

  // Load history from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem("sb_chat_history");
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        setMessages(
          parsed.map((m: any) => ({
            ...m,
            timestamp: new Date(m.timestamp),
          }))
        );
      } catch (e) {
        console.error("Failed to parse chat history", e);
      }
    } else {
      // Premium Initial Greeting
      const initialMessage: Message = {
        id: "1",
        text: "### Welcome to Shopbee AI Assistant! ✨\n\nI'm here to help you find the perfect products, track your orders, or answer any questions about our services. \n\n**What can I help you with today?**",
        sender: "bot",
        timestamp: new Date(),
      };
      setMessages([initialMessage]);
    }
  }, []);

  // Save history to localStorage
  useEffect(() => {
    if (messages.length > 0) {
      localStorage.setItem("sb_chat_history", JSON.stringify(messages));
    }
  }, [messages]);

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
    
    const words = fullText.split(" ");
    let currentText = "";
    
    for (let i = 0; i < words.length; i++) {
      currentText += (i === 0 ? "" : " ") + words[i];
      setMessages((prev) => 
        prev.map(m => m.id === botMsgId ? { ...m, text: currentText } : m)
      );
      // Faster for long text, slower for short text to feel natural
      await new Promise(r => setTimeout(r, Math.random() * 30 + 10));
    }
  };

  const handleSendMessage = async (text: string) => {
    const userMessage: Message = {
      id: Date.now().toString(),
      text,
      sender: "user",
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setIsTyping(true);

    try {
      const response = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ 
          message: text,
          history: messages.slice(-10).map(m => ({ role: m.sender, content: m.text }))
        }),
      });

      if (!response.ok) throw new Error("Failed to get response");

      const data = await response.json();
      setIsTyping(false);
      
      // Use streaming simulation for better feel
      await simulateStreaming(data.response);

    } catch (error) {
      console.error("Chat error:", error);
      setIsTyping(false);
      const errorMessage: Message = {
        id: (Date.now() + 2).toString(),
        text: "I'm sorry, I'm having trouble connecting right now. Please try again later.",
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
