"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { signIn, useSession } from "next-auth/react";
import { Bell, CheckCheck, RefreshCw } from "lucide-react";
import { shopbeApi, type NotificationDto } from "@/lib/shopbeApi";

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

export default function NotificationsPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const accessToken = session?.accessToken;
  const unreadCount = useMemo(() => items.filter((item) => !item.isRead).length, [items]);

  const load = async (signal?: AbortSignal) => {
    if (!accessToken) return;

    setLoading(true);
    setError(null);
    try {
      const page = await shopbeApi.notifications.list(accessToken, { unreadOnly, pageSize: 50 }, signal);
      setItems(page.items);
      setTotalCount(page.totalCount);
    } catch (err) {
      if ((err as { name?: string }).name !== "AbortError") {
        setError(err instanceof Error ? err.message : "Could not load notifications.");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (status !== "authenticated" || !accessToken) return;
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [accessToken, status, unreadOnly]);

  const openNotification = async (item: NotificationDto) => {
    if (!accessToken) return;
    if (item.isRead) {
      if (item.linkUrl) {
        router.push(item.linkUrl);
      }
      return;
    }

    const previous = items;
    setItems((list) => list.map((candidate) => candidate.id === item.id ? { ...candidate, isRead: true, readAt: new Date().toISOString() } : candidate));

    try {
      await shopbeApi.notifications.markRead(accessToken, item.id);
      if (unreadOnly) {
        setItems((list) => list.filter((candidate) => candidate.id !== item.id));
      }
      if (item.linkUrl) {
        router.push(item.linkUrl);
      }
    } catch {
      setItems(previous);
    }
  };

  const markAllRead = async () => {
    if (!accessToken) return;
    const previous = items;
    const now = new Date().toISOString();
    setItems((list) => unreadOnly ? [] : list.map((item) => ({ ...item, isRead: true, readAt: item.readAt ?? now })));

    try {
      await shopbeApi.notifications.markAllRead(accessToken);
    } catch {
      setItems(previous);
    }
  };

  if (status === "loading") {
    return (
      <div className="min-h-[50vh] rounded-lg bg-white p-8">
        <div className="h-8 w-48 rounded-md bg-slate-100" />
        <div className="mt-6 space-y-3">
          {[0, 1, 2].map((item) => (
            <div key={item} className="h-20 rounded-lg bg-slate-100" />
          ))}
        </div>
      </div>
    );
  }

  if (!accessToken) {
    return (
      <div className="mx-auto flex min-h-[50vh] max-w-lg flex-col items-center justify-center rounded-lg bg-white p-8 text-center">
        <Bell className="h-10 w-10 text-[#ee4d2d]" />
        <h1 className="mt-4 text-2xl font-bold text-slate-900">Notifications</h1>
        <p className="mt-2 text-sm text-slate-500">Sign in to view your account updates.</p>
        <button
          type="button"
          onClick={() => signIn("keycloak", { callbackUrl: "/notifications" })}
          className="mt-6 rounded-md bg-[#ee4d2d] px-5 py-2 text-sm font-bold text-white hover:bg-[#fb5533]"
        >
          Sign in
        </button>
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-4xl space-y-4">
      <div className="flex flex-col gap-3 rounded-lg bg-white p-5 shadow-sm sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Notifications</h1>
          <p className="text-sm text-slate-500">{totalCount} total, {unreadCount} unread on this page</p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <div className="inline-flex rounded-md border border-slate-200 bg-slate-50 p-1">
            <button
              type="button"
              onClick={() => setUnreadOnly(false)}
              className={`rounded px-3 py-1.5 text-sm font-semibold ${!unreadOnly ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}
            >
              All
            </button>
            <button
              type="button"
              onClick={() => setUnreadOnly(true)}
              className={`rounded px-3 py-1.5 text-sm font-semibold ${unreadOnly ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}
            >
              Unread
            </button>
          </div>

          <button
            type="button"
            onClick={() => void load()}
            className="inline-flex h-9 items-center gap-2 rounded-md border border-slate-200 px-3 text-sm font-semibold text-slate-700 hover:bg-slate-50"
          >
            <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
            Refresh
          </button>

          <button
            type="button"
            onClick={markAllRead}
            disabled={items.every((item) => item.isRead)}
            className="inline-flex h-9 items-center gap-2 rounded-md bg-[#ee4d2d] px-3 text-sm font-bold text-white hover:bg-[#fb5533] disabled:pointer-events-none disabled:opacity-40"
          >
            <CheckCheck className="h-4 w-4" />
            Mark all read
          </button>
        </div>
      </div>

      {error ? (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>
      ) : null}

      <div className="overflow-hidden rounded-lg bg-white shadow-sm">
        {items.length === 0 ? (
          <div className="flex min-h-60 flex-col items-center justify-center px-6 py-12 text-center">
            <Bell className="h-10 w-10 text-slate-300" />
            <div className="mt-3 text-sm font-semibold text-slate-700">No notifications</div>
          </div>
        ) : (
          <div className="divide-y divide-slate-100">
            {items.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => openNotification(item)}
                className="flex w-full gap-4 px-5 py-4 text-left hover:bg-slate-50"
              >
                <span className={`mt-2 h-2.5 w-2.5 shrink-0 rounded-full ${item.isRead ? "bg-slate-200" : "bg-[#ee4d2d]"}`} />
                <span className="min-w-0 flex-1">
                  <span className="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
                    <span className="text-sm font-bold text-slate-900">{item.title}</span>
                    <span className="text-xs font-medium text-slate-400">{formatDate(item.createdAt)}</span>
                  </span>
                  <span className="mt-1 block text-sm leading-6 text-slate-600">{item.message}</span>
                  <span className="mt-2 inline-flex rounded-full bg-slate-100 px-2 py-1 text-[11px] font-bold text-slate-500">{item.type}</span>
                </span>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
