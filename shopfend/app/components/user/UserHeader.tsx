"use client";

import type { Session } from "next-auth";
import Image from "next/image";
import { Camera, ShieldCheck, ShoppingBag, Heart, Star, Ticket } from "lucide-react";
import { motion } from "framer-motion";

export default function UserHeader({ session }: { session: Session }) {
  const stats = [
    { label: "Orders", value: "12", icon: ShoppingBag, color: "text-blue-500", bg: "bg-blue-50" },
    { label: "Wishlist", value: "24", icon: Heart, color: "text-rose-500", bg: "bg-rose-50" },
    { label: "Reviews", value: "8", icon: Star, color: "text-amber-500", bg: "bg-amber-50" },
    { label: "Coupons", value: "3", icon: Ticket, color: "text-emerald-500", bg: "bg-emerald-50" },
  ];

  return (
    <motion.div 
      initial={{ opacity: 0, y: -20 }}
      animate={{ opacity: 1, y: 0 }}
      className="relative overflow-hidden rounded-[2.5rem] bg-white border border-gray-100 shadow-xl shadow-gray-200/50"
    >
      {/* Premium Background Gradient */}
      <div className="absolute inset-0 bg-gradient-to-r from-brand/5 via-transparent to-brand/10 pointer-events-none" />
      <div className="absolute -top-24 -right-24 w-64 h-64 bg-brand/5 rounded-full blur-3xl" />
      <div className="absolute -bottom-24 -left-24 w-64 h-64 bg-blue-500/5 rounded-full blur-3xl" />

      <div className="relative p-8 md:p-10 flex flex-col md:flex-row items-center md:items-end justify-between gap-8">
        <div className="flex flex-col md:flex-row items-center md:items-center gap-6">
          {/* Avatar Section */}
          <div className="relative group">
            <div className="w-32 h-32 md:w-40 md:h-40 rounded-full border-4 border-white shadow-2xl overflow-hidden relative ring-4 ring-brand/10">
              <Image
                src={session.user?.image || `https://ui-avatars.com/api/?name=${encodeURIComponent(session.user?.name || "User")}&background=ee4d2d&color=fff&size=256`}
                alt={session.user?.name || "User"}
                fill
                className="object-cover transition-transform duration-500 group-hover:scale-110"
              />
            </div>
            <button className="absolute bottom-2 right-2 p-2.5 bg-brand text-white rounded-full shadow-lg hover:bg-brand-hover transition-all active:scale-90 ring-4 ring-white">
              <Camera size={20} />
            </button>
          </div>

          {/* User Info */}
          <div className="text-center md:text-left space-y-2">
            <div className="flex flex-wrap items-center justify-center md:justify-start gap-3">
              <h1 className="text-3xl md:text-4xl font-black text-slate-900 tracking-tight">
                {session.user?.name}
              </h1>
              <span className="flex items-center gap-1.5 px-3 py-1 bg-amber-100 text-amber-700 text-[10px] font-black uppercase tracking-widest rounded-full shadow-sm">
                <ShieldCheck size={12} className="fill-amber-700/20" />
                Platinum Member
              </span>
            </div>
            <p className="text-slate-500 font-medium">{session.user?.email}</p>
            <div className="flex items-center justify-center md:justify-start gap-4 mt-4">
              <div className="flex flex-col">
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Loyalty Points</span>
                <span className="text-xl font-black text-brand">2,450 <span className="text-xs font-bold text-slate-400">pts</span></span>
              </div>
              <div className="w-px h-8 bg-slate-100 mx-2" />
              <div className="flex flex-col">
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Member Since</span>
                <span className="text-sm font-bold text-slate-700">May 2024</span>
              </div>
            </div>
          </div>
        </div>

        {/* Quick Stats Grid */}
        <div className="grid grid-cols-2 sm:grid-cols-4 md:flex gap-4 w-full md:w-auto">
          {stats.map((stat, i) => (
            <motion.div 
              key={stat.label}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.1 * i }}
              className="flex-1 md:w-28 p-4 rounded-3xl bg-slate-50/50 border border-slate-100 hover:bg-white hover:shadow-xl hover:shadow-slate-200/50 transition-all group cursor-pointer"
            >
              <div className={`w-10 h-10 ${stat.bg} ${stat.color} rounded-2xl flex items-center justify-center mb-3 group-hover:scale-110 transition-transform`}>
                <stat.icon size={20} />
              </div>
              <div className="text-xl font-black text-slate-900 leading-none mb-1">{stat.value}</div>
              <div className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">{stat.label}</div>
            </motion.div>
          ))}
        </div>
      </div>
    </motion.div>
  );
}
