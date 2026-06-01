"use client";

import { useState } from "react";
import { useSession } from "next-auth/react";
import { motion, AnimatePresence } from "framer-motion";
import UserHeader from "../components/user/UserHeader";
import UserSidebar from "../components/user/UserSidebar";

// Tab Components (we'll create these next)
import ProfileTab from "../components/user/tabs/ProfileTab";
import OrdersTab from "../components/user/tabs/OrdersTab";
import WishlistTab from "../components/user/tabs/WishlistTab";
import AddressesTab from "../components/user/tabs/AddressesTab";
import PaymentsTab from "../components/user/tabs/PaymentsTab";
import SecurityTab from "../components/user/tabs/SecurityTab";
import NotificationsTab from "../components/user/tabs/NotificationsTab";
import CouponsTab from "../components/user/tabs/CouponsTab";
import RecentlyViewedTab from "../components/user/tabs/RecentlyViewedTab";

export type TabType = 
  | "profile" 
  | "orders" 
  | "wishlist" 
  | "addresses" 
  | "payments" 
  | "notifications" 
  | "coupons" 
  | "recent" 
  | "security";

export default function UserProfilePage() {
  const { data: session, status } = useSession();
  const [activeTab, setActiveTab] = useState<TabType>("profile");

  if (status === "loading") {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-12 h-12 border-4 border-brand border-t-transparent rounded-full animate-spin"></div>
      </div>
    );
  }

  if (!session) {
    return (
      <div className="min-h-[60vh] flex flex-col items-center justify-center p-6 text-center">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Access Denied</h2>
        <p className="text-gray-500 mb-8 max-w-sm">
          Please sign in to view your personalized profile dashboard.
        </p>
        <button 
          onClick={() => window.location.href = '/api/auth/signin'}
          className="bg-brand text-white px-10 py-3 rounded-full font-bold shadow-lg hover:bg-brand/90 transition-all active:scale-95"
        >
          Login Now
        </button>
      </div>
    );
  }

  const renderTabContent = () => {
    switch (activeTab) {
      case "profile": return <ProfileTab />;
      case "orders": return <OrdersTab />;
      case "wishlist": return <WishlistTab />;
      case "addresses": return <AddressesTab />;
      case "payments": return <PaymentsTab />;
      case "coupons": return <CouponsTab />;
      case "recent": return <RecentlyViewedTab />;
      case "security": return <SecurityTab />;
      case "notifications": return <NotificationsTab />;
      default: return <ProfileTab />;
    }
  };

  return (
    <div className="w-full px-4 py-8 sm:px-6 lg:px-8">
      <div className="mx-auto w-full max-w-[1200px] space-y-8">
        <UserHeader session={session} />

        <div className="grid grid-cols-1 items-start gap-6 lg:grid-cols-[240px_minmax(0,1fr)] lg:gap-8">
          <aside className="w-full min-w-0">
            <UserSidebar activeTab={activeTab} setActiveTab={setActiveTab} />
          </aside>

          <main className="w-full min-w-0 flex-1">
            <AnimatePresence mode="wait">
              <motion.div
                key={activeTab}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                transition={{ duration: 0.3, ease: "easeOut" }}
                className="w-full min-w-0 box-border rounded-3xl border border-gray-100 bg-white shadow-sm overflow-visible min-h-[600px]"
              >
                {renderTabContent()}
              </motion.div>
            </AnimatePresence>
          </main>
        </div>
      </div>
    </div>
  );
}
