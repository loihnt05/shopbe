"use client";

import { 
  User, 
  Package, 
  Heart, 
  MapPin, 
  CreditCard, 
  Bell, 
  Ticket, 
  Clock, 
  Shield, 
  LogOut 
} from "lucide-react";
import { signOut } from "next-auth/react";
import type { TabType } from "../../user/page";

interface SidebarProps {
  activeTab: TabType;
  setActiveTab: (tab: TabType) => void;
}

export default function UserSidebar({ activeTab, setActiveTab }: SidebarProps) {
  const menuItems = [
    { id: "profile", label: "My Profile", icon: User },
    { id: "orders", label: "My Orders", icon: Package },
    { id: "wishlist", label: "Wishlist", icon: Heart },
    { id: "addresses", label: "My Addresses", icon: MapPin },
    { id: "payments", label: "Payment Methods", icon: CreditCard },
    { id: "coupons", label: "My Coupons", icon: Ticket },
    { id: "recent", label: "Recently Viewed", icon: Clock },
    { id: "notifications", label: "Notifications", icon: Bell },
    { id: "security", label: "Account Security", icon: Shield },
  ];

  return (
    <div className="w-full box-border bg-white rounded-[1.5rem] border border-slate-200/60 shadow-lg shadow-slate-200/40 p-3 space-y-1">
      <div className="md:block hidden pt-2 pb-3 px-3">
        <h2 className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Manage Account</h2>
      </div>

      <nav className="flex flex-row md:flex-col gap-1 overflow-x-auto md:overflow-visible no-scrollbar pb-2 md:pb-0">
        {menuItems.map((item) => {
          const isActive = activeTab === item.id;
          return (
            <button
              key={item.id}
              onClick={() => setActiveTab(item.id as TabType)}
              className={`
                group flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all duration-200 whitespace-nowrap
                ${isActive 
                  ? "bg-brand/10 text-brand" 
                  : "text-slate-700 hover:bg-slate-100 hover:text-slate-800"}
              `}
            >
              <item.icon 
                size={20} 
                className={`transition-all duration-200 shrink-0 ${isActive ? "text-brand" : "text-slate-400 group-hover:text-slate-600"}`} 
              />
              <span className="flex-1 text-left">{item.label}</span>
              {isActive && (
                <div className="w-1.5 h-6 rounded-full bg-brand md:block hidden" />
              )}
            </button>
          );
        })}

        <div className="w-px h-full bg-slate-200/80 mx-2 md:hidden" />

        <button
          onClick={() => signOut({ callbackUrl: "/" })}
          className="group flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold text-slate-700 hover:bg-rose-50 hover:text-rose-600 transition-all duration-200 whitespace-nowrap mt-2 border-t border-slate-200/60 md:mt-3"
        >
          <LogOut size={20} className="text-slate-400 group-hover:text-rose-500 transition-colors" />
          <span className="flex-1 text-left">Logout</span>
        </button>
      </nav>
    </div>
  );
}
