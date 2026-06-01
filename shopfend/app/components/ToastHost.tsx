"use client";

import { CheckCircle2, XCircle } from "lucide-react";
import { useEffect, useState } from "react";

type ToastState = {
  message: string;
  type: "success" | "error";
};

export default function ToastHost() {
  const [toast, setToast] = useState<ToastState | null>(null);

  useEffect(() => {
    let timeout: ReturnType<typeof setTimeout> | undefined;

    const handleToast = (event: Event) => {
      const { message, type } = (event as CustomEvent<ToastState>).detail;
      setToast({ message, type });

      if (timeout) clearTimeout(timeout);
      timeout = setTimeout(() => setToast(null), 3000);
    };

    window.addEventListener("shopbe:toast", handleToast);
    return () => {
      window.removeEventListener("shopbe:toast", handleToast);
      if (timeout) clearTimeout(timeout);
    };
  }, []);

  if (!toast) return null;

  const Icon = toast.type === "success" ? CheckCircle2 : XCircle;

  return (
    <div
      className={`fixed bottom-8 left-1/2 z-[200] flex -translate-x-1/2 items-center gap-3 rounded-2xl px-6 py-4 text-sm font-bold text-white shadow-2xl animate-in fade-in slide-in-from-bottom-10 ${
        toast.type === "success" ? "bg-slate-950" : "bg-rose-600"
      }`}
      role="status"
      aria-live="polite"
    >
      <Icon className="h-5 w-5" />
      <span>{toast.message}</span>
    </div>
  );
}
