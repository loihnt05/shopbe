"use client";

import { Landmark, Plus, Trash2, CheckCircle2 } from "lucide-react";
import { motion } from "framer-motion";

export default function PaymentsTab() {
  const cards = [
    { id: 1, type: "Visa", number: "**** **** **** 4242", name: "JOHN DOE", expiry: "12/26", isDefault: true, color: "bg-slate-900" },
    { id: 2, type: "Mastercard", number: "**** **** **** 8888", name: "JOHN DOE", expiry: "08/25", isDefault: false, color: "bg-gradient-to-br from-indigo-600 to-violet-700" },
  ];

  const methods = [
    { label: "MoMo E-wallet", icon: "https://upload.wikimedia.org/wikipedia/vi/f/fe/MoMo_Logo.png", active: true },
    { label: "ShopeePay", icon: "https://shopee.vn/static/images/shopeepay.png", active: false },
    { label: "ZaloPay", icon: "https://upload.wikimedia.org/wikipedia/vi/d/d7/Logo_ZaloPay.png", active: false },
  ];

  return (
    <div className="p-8 md:p-10 space-y-10">
      <div className="flex items-center justify-between border-b border-slate-50 pb-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">Payment Methods</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">Manage your cards and linked accounts for easy payments.</p>
        </div>
        <button className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-brand text-white text-sm font-bold shadow-lg shadow-brand/20 hover:scale-[1.02] active:scale-95 transition-all">
          <Plus size={18} />
          Add Card
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
        {cards.map((card, i) => (
          <motion.div 
            key={card.id}
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: i * 0.1 }}
            className={`relative aspect-[1.6/1] rounded-[2.5rem] p-8 text-white flex flex-col justify-between shadow-2xl overflow-hidden group cursor-pointer ${card.color}`}
          >
            {/* Glossy Overlay */}
            <div className="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full blur-3xl -mr-16 -mt-16 group-hover:bg-white/20 transition-all" />
            
            <div className="flex items-center justify-between">
              <div className="w-12 h-10 bg-white/10 backdrop-blur-sm rounded-lg flex items-center justify-center font-bold text-xs italic">
                {card.type}
              </div>
              {card.isDefault && <CheckCircle2 size={24} className="text-emerald-400 fill-emerald-400/20" />}
            </div>

            <div className="space-y-4">
              <div className="text-xl font-bold tracking-[0.2em]">{card.number}</div>
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-[10px] font-black uppercase tracking-widest opacity-60">Card Holder</div>
                  <div className="text-sm font-bold uppercase">{card.name}</div>
                </div>
                <div className="text-right">
                  <div className="text-[10px] font-black uppercase tracking-widest opacity-60">Expires</div>
                  <div className="text-sm font-bold">{card.expiry}</div>
                </div>
              </div>
            </div>

            <button className="absolute bottom-4 right-4 p-2 bg-white/10 backdrop-blur-sm rounded-full opacity-0 group-hover:opacity-100 transition-all hover:bg-rose-500">
              <Trash2 size={16} />
            </button>
          </motion.div>
        ))}

        <button className="aspect-[1.6/1] rounded-[2.5rem] border-2 border-dashed border-slate-200 flex flex-col items-center justify-center gap-4 text-slate-400 hover:border-brand hover:text-brand hover:bg-brand/5 transition-all group">
          <div className="w-14 h-14 rounded-full bg-slate-50 flex items-center justify-center group-hover:bg-brand/10 transition-colors">
            <Plus size={32} />
          </div>
          <span className="font-bold">Add New Card</span>
        </button>
      </div>

      <div className="space-y-6">
        <h3 className="text-lg font-bold text-slate-900">E-Wallets & Bank Accounts</h3>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {methods.map((method) => (
            <div 
              key={method.label}
              className={`p-4 rounded-2xl border flex items-center justify-between group cursor-pointer transition-all ${
                method.active 
                ? 'border-emerald-200 bg-emerald-50/50' 
                : 'border-slate-100 bg-white hover:border-brand/20'
              }`}
            >
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 bg-white rounded-xl flex items-center justify-center overflow-hidden border border-slate-100 shadow-sm p-2">
                  <img src={method.icon} alt={method.label} className="w-full h-full object-contain" />
                </div>
                <div className="text-sm font-bold text-slate-800">{method.label}</div>
              </div>
              {method.active ? (
                <div className="text-[10px] font-black text-emerald-600 bg-emerald-100 px-2 py-1 rounded-full uppercase tracking-tighter">Active</div>
              ) : (
                <button className="text-[10px] font-black text-brand bg-brand/5 px-2 py-1 rounded-full uppercase tracking-tighter hover:bg-brand hover:text-white transition-all">Link</button>
              )}
            </div>
          ))}
          <div className="p-4 rounded-2xl border border-dashed border-slate-200 flex items-center justify-center gap-2 text-slate-400 hover:border-brand hover:text-brand hover:bg-brand/5 transition-all cursor-pointer">
            <Landmark size={18} />
            <span className="text-sm font-bold">Link Bank Account</span>
          </div>
        </div>
      </div>
    </div>
  );
}
