"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { Bell, Check, Mail, Megaphone, ShoppingBag, WalletCards } from "lucide-react";
import { shopbeApi, type NotificationPreferenceDto } from "@/lib/shopbeApi";

const defaultPrefs: NotificationPreferenceDto = {
  orderStatusEmailsEnabled: true,
  paymentEmailsEnabled: true,
  marketingEmailsEnabled: false,
  inAppNotificationsEnabled: true,
};

export default function NotificationsTab() {
  const { data: session } = useSession();
  const [prefs, setPrefs] = useState<NotificationPreferenceDto>(defaultPrefs);
  const [loading, setLoading] = useState(false);
  const [savingKey, setSavingKey] = useState<keyof NotificationPreferenceDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!session?.accessToken) return;

    const controller = new AbortController();
    setLoading(true);
    setError(null);
    shopbeApi.notifications.getPreferences(session.accessToken, controller.signal)
      .then(setPrefs)
      .catch((err) => {
        if ((err as { name?: string }).name !== "AbortError") {
          setError(err instanceof Error ? err.message : "Could not load notification preferences.");
        }
      })
      .finally(() => setLoading(false));

    return () => controller.abort();
  }, [session?.accessToken]);

  const toggle = async (key: keyof NotificationPreferenceDto) => {
    if (!session?.accessToken || savingKey) return;

    const next = { ...prefs, [key]: !prefs[key] };
    const previous = prefs;
    setPrefs(next);
    setSavingKey(key);
    setError(null);

    try {
      const saved = await shopbeApi.notifications.updatePreferences(session.accessToken, next);
      setPrefs(saved);
    } catch (err) {
      setPrefs(previous);
      setError(err instanceof Error ? err.message : "Could not save notification preferences.");
    } finally {
      setSavingKey(null);
    }
  };

  const sections = [
    {
      title: "Shopping Updates",
      description: "Control updates tied to orders, shipping, and payments.",
      items: [
        { id: "orderStatusEmailsEnabled", label: "Order Status Emails", description: "Receive email when an order is placed, shipped, or delivered.", icon: ShoppingBag },
        { id: "paymentEmailsEnabled", label: "Payment Emails", description: "Receive email when payment succeeds or fails.", icon: WalletCards },
      ]
    },
    {
      title: "Channels",
      description: "Choose where account updates appear.",
      items: [
        { id: "inAppNotificationsEnabled", label: "In-App Notifications", description: "Show order and payment updates in the notification center.", icon: Bell },
      ]
    },
    {
      title: "Marketing",
      description: "Promotional messages require your opt-in.",
      items: [
        { id: "marketingEmailsEnabled", label: "Marketing Emails", description: "Receive promotions, coupons, and campaign announcements.", icon: Megaphone },
      ]
    }
  ];

  return (
    <div className="p-8 md:p-10 space-y-10">
      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight">Notification Preferences</h2>
        <p className="text-sm text-slate-500 font-medium mt-1">Control how order, payment, and promotional updates reach you.</p>
      </div>

      {error ? (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700">{error}</div>
      ) : null}

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
                  onClick={() => toggle(item.id as keyof NotificationPreferenceDto)}
                  className="group flex cursor-pointer items-center justify-between rounded-lg border border-slate-100 bg-white p-5 transition-all hover:border-brand/20 hover:shadow-xl hover:shadow-slate-200/50"
                >
                  <div className="flex items-center gap-4">
                    <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-slate-50 text-slate-400 transition-colors group-hover:bg-brand/10 group-hover:text-brand">
                      <item.icon size={24} />
                    </div>
                    <div>
                      <div className="text-sm font-bold text-slate-900">{item.label}</div>
                      <div className="text-xs text-slate-500 font-medium mt-0.5">{item.description}</div>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    {savingKey === item.id ? (
                      <span className="text-xs font-bold text-slate-400">Saving</span>
                    ) : null}
                    {loading ? (
                      <span className="h-8 w-14 rounded-full bg-slate-100" />
                    ) : (
                      <div className={`relative h-8 w-14 rounded-full transition-colors duration-300 ${prefs[item.id as keyof NotificationPreferenceDto] ? 'bg-brand' : 'bg-slate-200'}`}>
                        <div className={`absolute left-1 top-1 flex h-6 w-6 items-center justify-center rounded-full bg-white shadow-md transition-transform duration-300 ${prefs[item.id as keyof NotificationPreferenceDto] ? 'translate-x-6' : 'translate-x-0'}`}>
                          {prefs[item.id as keyof NotificationPreferenceDto] ? <Check className="h-3.5 w-3.5 text-brand" /> : null}
                        </div>
                      </div>
                    )}
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
