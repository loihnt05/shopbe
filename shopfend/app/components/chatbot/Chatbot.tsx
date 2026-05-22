"use client";

import { useState, useEffect, useRef } from "react";
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
      // Initial greeting
      const initialMessage: Message = {
        id: "1",
        text: "Hi there! 👋 How can I help you today?",
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
      
      const botMessage: Message = {
        id: (Date.now() + 1).toString(),
        text: data.response,
        sender: "bot",
        timestamp: new Date(),
      };
      
      setMessages((prev) => [...prev, botMessage]);
    } catch (error) {
      console.error("Chat error:", error);
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
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
