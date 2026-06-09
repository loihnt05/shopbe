"use client";

import { FormEvent, useState } from "react";
import { useSession } from "next-auth/react";
import { Bell, Megaphone, PackageSearch } from "lucide-react";
import { errorMessage } from "@/lib/errors";
import { shopbeApi, type AdminLowStockNotificationResultDto, type AdminNotificationActionResultDto } from "@/lib/shopbeApi";
import { AdminButton, AdminCard, AdminErrorState, AdminInput, AdminPageIntro } from "../components/AdminUi";

export default function AdminNotificationsPage() {
  const { data: session, status } = useSession();
  const [threshold, setThreshold] = useState(10);
  const [title, setTitle] = useState("");
  const [emailSubject, setEmailSubject] = useState("");
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState<"low-stock" | "marketing" | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [lowStockResult, setLowStockResult] = useState<AdminLowStockNotificationResultDto | null>(null);
  const [marketingResult, setMarketingResult] = useState<AdminNotificationActionResultDto | null>(null);

  if (status === "loading") {
    return (
      <AdminCard className="flex min-h-60 items-center justify-center">
        <div className="h-10 w-10 animate-spin rounded-full border-4 border-brand border-t-transparent" />
      </AdminCard>
    );
  }

  if (!session?.accessToken) {
    return <AdminErrorState message="Admin session is required." />;
  }

  const createLowStockAlerts = async () => {
    setBusy("low-stock");
    setError(null);
    setLowStockResult(null);
    try {
      const result = await shopbeApi.admin.createLowStockAlerts(session.accessToken!, threshold);
      setLowStockResult(result);
    } catch (err) {
      setError(errorMessage(err, "Failed to create low-stock alerts."));
    } finally {
      setBusy(null);
    }
  };

  const sendMarketing = async (event: FormEvent) => {
    event.preventDefault();
    setBusy("marketing");
    setError(null);
    setMarketingResult(null);
    try {
      const result = await shopbeApi.admin.sendMarketingNotification(session.accessToken!, {
        title,
        message,
        emailSubject: emailSubject || undefined,
      });
      setMarketingResult(result);
      setTitle("");
      setEmailSubject("");
      setMessage("");
    } catch (err) {
      setError(errorMessage(err, "Failed to send marketing notification."));
    } finally {
      setBusy(null);
    }
  };

  return (
    <div className="space-y-6">
      <AdminPageIntro
        title="Notifications"
        description="Create operational admin alerts and send promotional messages to customers who have opted in."
      />

      {error ? <AdminErrorState message={error} /> : null}

      <div className="grid gap-6 xl:grid-cols-2">
        <AdminCard>
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-orange-50 text-orange-600">
              <PackageSearch className="h-6 w-6" />
            </div>
            <div>
              <h2 className="text-lg font-black text-slate-950">Low-stock alerts</h2>
              <p className="mt-1 text-sm text-slate-500">Create in-app admin notifications for active products at or below the stock threshold.</p>
            </div>
          </div>

          <div className="mt-6 flex flex-col gap-3 sm:flex-row">
            <AdminInput
              type="number"
              min={0}
              value={threshold}
              onChange={(event) => setThreshold(Number(event.target.value))}
              aria-label="Low stock threshold"
            />
            <AdminButton
              type="button"
              onClick={createLowStockAlerts}
              disabled={busy !== null}
              className="shrink-0"
            >
              Create alerts
            </AdminButton>
          </div>

          {lowStockResult ? (
            <div className="mt-5 rounded-2xl bg-slate-50 p-4 text-sm text-slate-700">
              <p className="font-bold text-slate-950">{lowStockResult.adminNotificationsCreated} admin notifications created</p>
              <p className="mt-1">{lowStockResult.productsMatched} products matched the threshold.</p>
            </div>
          ) : null}
        </AdminCard>

        <AdminCard>
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-sky-50 text-sky-600">
              <Megaphone className="h-6 w-6" />
            </div>
            <div>
              <h2 className="text-lg font-black text-slate-950">Marketing broadcast</h2>
              <p className="mt-1 text-sm text-slate-500">Queue email and in-app messages for customers whose preferences allow them.</p>
            </div>
          </div>

          <form onSubmit={sendMarketing} className="mt-6 space-y-4">
            <AdminInput
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              placeholder="Campaign title"
              required
            />
            <AdminInput
              value={emailSubject}
              onChange={(event) => setEmailSubject(event.target.value)}
              placeholder="Email subject"
            />
            <textarea
              value={message}
              onChange={(event) => setMessage(event.target.value)}
              placeholder="Message"
              required
              rows={5}
              className="sb-input w-full resize-none"
            />
            <AdminButton type="submit" disabled={busy !== null || !title.trim() || !message.trim()}>
              <span className="inline-flex items-center gap-2">
                <Bell className="h-4 w-4" />
                Send broadcast
              </span>
            </AdminButton>
          </form>

          {marketingResult ? (
            <div className="mt-5 rounded-2xl bg-slate-50 p-4 text-sm text-slate-700">
              <p className="font-bold text-slate-950">{marketingResult.inAppNotificationsCreated} in-app notifications created</p>
              <p className="mt-1">{marketingResult.emailsQueued} marketing emails queued.</p>
            </div>
          ) : null}
        </AdminCard>
      </div>
    </div>
  );
}
