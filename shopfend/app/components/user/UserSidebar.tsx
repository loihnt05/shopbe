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
    { id: "orders", label: "Orders", icon: Package },
    { id: "wishlist", label: "Wishlist", icon: Heart },
    { id: "addresses", label: "Addresses", icon: MapPin },
    { id: "payments", label: "Payments", icon: CreditCard },
    { id: "coupons", label: "Coupons", icon: Ticket },
    { id: "recent", label: "Recently Viewed", icon: Clock },
    { id: "notifications", label: "Notifications", icon: Bell },
    { id: "security", label: "Security", icon: Shield },
  ];

  return (
    <div className="bg-white rounded-[2rem] border border-gray-100 shadow-xl shadow-gray-200/50 p-4 space-y-2 overflow-hidden">
      <div className="md:block hidden pb-4 px-4">
        <h2 className="text-xs font-black text-slate-400 uppercase tracking-[0.2em]">Account Settings</h2>
      </div>

      <nav className="flex flex-row md:flex-col gap-1 overflow-x-auto md:overflow-visible no-scrollbar pb-2 md:pb-0">
        {menuItems.map((item) => {
          const isActive = activeTab === item.id;
          return (
            <button
              key={item.id}
              onClick={() => setActiveTab(item.id as TabType)}
              className={`
                flex items-center gap-3 px-5 py-3.5 rounded-2xl text-sm font-bold transition-all duration-300 whitespace-nowrap md:whitespace-normal
                ${isActive 
                  ? "bg-brand text-white shadow-lg shadow-brand/20 translate-x-1" 
                  : "text-slate-600 hover:bg-slate-50 hover:text-brand"}
              `}
            >
              <item.icon size={20} className={isActive ? "text-white" : "text-slate-400 group-hover:text-brand"} />
              <span className="flex-1 text-left">{item.label}</span>
              {isActive && (
                <div className="w-1.5 h-1.5 rounded-full bg-white md:block hidden animate-pulse" />
              )}
            </button>
          );
        })}

        <div className="w-px h-8 bg-slate-100 mx-2 md:hidden" />

        <button
          onClick={() => signOut({ callbackUrl: "/" })}
          className="flex items-center gap-3 px-5 py-3.5 rounded-2xl text-sm font-bold text-rose-500 hover:bg-rose-50 transition-all duration-300 whitespace-nowrap md:whitespace-normal mt-4 border-t border-slate-50 pt-6 md:mt-6"
        >
          <LogOut size={20} />
          <span className="flex-1 text-left">Logout</span>
        </button>
      </nav>
    </div>
  );
}
