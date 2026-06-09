"use client";

import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { useSession } from "next-auth/react";
import { Bell, CheckCheck } from "lucide-react";
import { shopbeApi, type NotificationDto } from "@/lib/shopbeApi";

function formatTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";

  const diffMs = Date.now() - date.getTime();
  const diffMinutes = Math.floor(diffMs / 60000);
  if (diffMinutes < 1) return "Just now";
  if (diffMinutes < 60) return `${diffMinutes}m ago`;

  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours}h ago`;

  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 7) return `${diffDays}d ago`;

  return date.toLocaleDateString();
}

export default function NotificationBell() {
  const { data: session, status } = useSession();
  const [open, setOpen] = useState(false);
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const panelRef = useRef<HTMLDivElement | null>(null);

  const accessToken = session?.accessToken;

  const load = async (signal?: AbortSignal) => {
    if (!accessToken) return;
    const [page, unread] = await Promise.all([
      shopbeApi.notifications.list(accessToken, { pageSize: 5 }, signal),
      shopbeApi.notifications.unreadCount(accessToken, signal),
    ]);
    setItems(page.items);
    setUnreadCount(unread.count);
  };

  useEffect(() => {
    if (status !== "authenticated" || !accessToken) {
      setItems([]);
      setUnreadCount(0);
      return;
    }

    const controller = new AbortController();
    void load(controller.signal).catch(() => {});
    const timer = window.setInterval(() => {
      void load().catch(() => {});
    }, 60000);

    return () => {
      controller.abort();
      window.clearInterval(timer);
    };
  }, [accessToken, status]);

  useEffect(() => {
    if (!open) return;

    const onPointerDown = (event: PointerEvent) => {
      if (panelRef.current && !panelRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    document.addEventListener("pointerdown", onPointerDown);
    return () => document.removeEventListener("pointerdown", onPointerDown);
  }, [open]);

  if (status !== "authenticated" || !accessToken) {
    return (
      <Link href="/api/auth/signin?callbackUrl=/notifications" className="hover:opacity-80 flex items-center gap-1">
        <Bell className="h-4 w-4" />
        Notifications
      </Link>
    );
  }

  const markAllRead = async () => {
    setLoading(true);
    try {
      await shopbeApi.notifications.markAllRead(accessToken);
      setItems((current) => current.map((item) => ({ ...item, isRead: true })));
      setUnreadCount(0);
    } finally {
      setLoading(false);
    }
  };

  const markRead = async (id: string) => {
    const current = items.find((item) => item.id === id);
    if (!current || current.isRead) return;

    setItems((list) => list.map((item) => item.id === id ? { ...item, isRead: true } : item));
    setUnreadCount((count) => Math.max(0, count - 1));

    try {
      await shopbeApi.notifications.markRead(accessToken, id);
    } catch {
      setItems((list) => list.map((item) => item.id === id ? { ...item, isRead: false } : item));
      setUnreadCount((count) => count + 1);
    }
  };

  return (
    <div ref={panelRef} className="relative">
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        className="relative flex items-center gap-1 hover:opacity-80"
        aria-label="Notifications"
        aria-expanded={open}
      >
        <Bell className="h-4 w-4" />
        <span>Notifications</span>
        {unreadCount > 0 ? (
          <span className="absolute -right-3 -top-2 min-w-4 rounded-full bg-white px-1 text-center text-[10px] font-bold leading-4 text-[#ee4d2d]">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        ) : null}
      </button>

      {open ? (
        <div className="absolute right-0 top-7 z-50 w-[360px] max-w-[calc(100vw-32px)] rounded-lg border border-black/10 bg-white text-slate-900 shadow-xl">
          <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
            <div className="text-sm font-bold">Notifications</div>
            <button
              type="button"
              onClick={markAllRead}
              disabled={loading || unreadCount === 0}
              className="inline-flex h-8 items-center gap-1 rounded-md px-2 text-xs font-semibold text-[#ee4d2d] hover:bg-orange-50 disabled:pointer-events-none disabled:opacity-40"
            >
              <CheckCheck className="h-4 w-4" />
              Mark all read
            </button>
          </div>

          <div className="max-h-[360px] overflow-y-auto">
            {items.length === 0 ? (
              <div className="px-4 py-8 text-center text-sm text-slate-500">No notifications</div>
            ) : (
              items.map((item) => (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => markRead(item.id)}
                  className="flex w-full gap-3 border-b border-slate-100 px-4 py-3 text-left hover:bg-slate-50"
                >
                  <span className={`mt-1 h-2 w-2 shrink-0 rounded-full ${item.isRead ? "bg-slate-200" : "bg-[#ee4d2d]"}`} />
                  <span className="min-w-0 flex-1">
                    <span className="block truncate text-sm font-semibold text-slate-900">{item.title}</span>
                    <span className="line-clamp-2 text-xs leading-5 text-slate-600">{item.message}</span>
                    <span className="mt-1 block text-[11px] font-medium text-slate-400">{formatTime(item.createdAt)}</span>
                  </span>
                </button>
              ))
            )}
          </div>

          <Link
            href="/notifications"
            onClick={() => setOpen(false)}
            className="block rounded-b-lg px-4 py-3 text-center text-sm font-bold text-[#ee4d2d] hover:bg-orange-50"
          >
            View all
          </Link>
        </div>
      ) : null}
    </div>
  );
}
