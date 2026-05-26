"use client";

import { Bot } from "lucide-react";
import { motion } from "framer-motion";

export default function TypingIndicator() {
  return (
    <motion.div 
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex w-full justify-start"
    >
      <div className="flex gap-4 max-w-[85%]">
        <div className="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-white bg-slate-900 shadow-md mt-1">
          <Bot size={16} />
        </div>
        <div className="bg-white text-slate-800 border border-slate-100 px-5 py-3 rounded-[20px] rounded-tl-none shadow-sm flex gap-1.5 items-center">
          <motion.div 
            animate={{ y: [0, -5, 0] }}
            transition={{ duration: 0.6, repeat: Infinity, delay: 0 }}
            className="w-1.5 h-1.5 bg-slate-300 rounded-full"
          ></motion.div>
          <motion.div 
            animate={{ y: [0, -5, 0] }}
            transition={{ duration: 0.6, repeat: Infinity, delay: 0.2 }}
            className="w-1.5 h-1.5 bg-slate-300 rounded-full"
          ></motion.div>
          <motion.div 
            animate={{ y: [0, -5, 0] }}
            transition={{ duration: 0.6, repeat: Infinity, delay: 0.4 }}
            className="w-1.5 h-1.5 bg-slate-300 rounded-full"
          ></motion.div>
        </div>
      </div>
    </motion.div>
  );
}
