"use client";

import { useState } from "react";
import { Bell, ShoppingBag, Tag, Zap, Mail, Smartphone, MessageSquare } from "lucide-react";

export default function NotificationsTab() {
  const [prefs, setPrefs] = useState({
    orderUpdates: true,
    promotions: true,
    flashSales: false,
    wishlistDrops: true,
    email: true,
    sms: false,
    appPush: true,
  });

  const toggle = (key: keyof typeof prefs) => {
    setPrefs(prev => ({ ...prev, [key]: !prev[key] }));
  };

  const sections = [
    {
      title: "Activity Notifications",
      description: "Stay updated on your orders and shopping activity.",
      items: [
        { id: "orderUpdates", label: "Order Status Updates", description: "Get notified when your order is confirmed, shipped, or delivered.", icon: ShoppingBag },
        { id: "wishlistDrops", label: "Wishlist Price Drops", description: "Receive alerts when items in your wishlist go on sale.", icon: Tag },
      ]
    },
    {
      title: "Promotions & Offers",
      description: "Be the first to know about discounts and special events.",
      items: [
        { id: "promotions", label: "General Promotions", description: "Coupons, seasonal sales, and personalized offers.", icon: Bell },
        { id: "flashSales", label: "Flash Sales", description: "Real-time alerts for limited-time deals.", icon: Zap },
      ]
    },
    {
      title: "Delivery Channels",
      description: "Choose how you want to receive your notifications.",
      items: [
        { id: "email", label: "Email Notifications", description: "Sent to your registered email address.", icon: Mail },
        { id: "appPush", label: "Mobile Push Notifications", description: "Real-time alerts on your smartphone.", icon: Smartphone },
        { id: "sms", label: "SMS Notifications", description: "Important updates sent via text message.", icon: MessageSquare },
      ]
    }
  ];

  return (
    <div className="p-8 md:p-10 space-y-10">
      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight">Notification Preferences</h2>
        <p className="text-sm text-slate-500 font-medium mt-1">Control how and when you want to be notified.</p>
      </div>

      <div className="space-y-12">
        {sections.map((section) => (
          <div key={section.title} className="space-y-6">
            <div className="border-l-4 border-brand pl-4">
              <h3 className="text-lg font-bold text-slate-900">{section.title}</h3>
              <p className="text-xs text-slate-500 font-medium">{section.description}</p>
            </div>

            <div className="grid grid-cols-1 gap-4">
              {section.items.map((item) => (
                <div 
                  key={item.id}
                  onClick={() => toggle(item.id as keyof typeof prefs)}
                  className="group flex items-center justify-between p-5 bg-white border border-slate-100 rounded-3xl hover:border-brand/20 hover:shadow-xl hover:shadow-slate-200/50 transition-all cursor-pointer"
                >
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 bg-slate-50 text-slate-400 group-hover:bg-brand/10 group-hover:text-brand rounded-2xl flex items-center justify-center transition-colors">
                      <item.icon size={24} />
                    </div>
                    <div>
                      <div className="text-sm font-bold text-slate-900">{item.label}</div>
                      <div className="text-xs text-slate-500 font-medium mt-0.5">{item.description}</div>
                    </div>
                  </div>

                  <div className={`relative w-14 h-8 rounded-full transition-colors duration-300 ${prefs[item.id as keyof typeof prefs] ? 'bg-brand' : 'bg-slate-200'}`}>
                    <div className={`absolute top-1 left-1 w-6 h-6 bg-white rounded-full shadow-md transition-transform duration-300 ${prefs[item.id as keyof typeof prefs] ? 'translate-x-6' : 'translate-x-0'}`} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
