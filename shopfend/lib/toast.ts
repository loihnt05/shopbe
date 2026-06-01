"use client";

type ToastType = "success" | "error";

function notify(type: ToastType, message: string) {
  if (typeof window === "undefined") return;

  const event = new CustomEvent("shopbe:toast", {
    detail: { type, message },
  });

  window.dispatchEvent(event);
}

export const toast = {
  success: (message: string) => notify("success", message),
  error: (message: string) => notify("error", message),
};
