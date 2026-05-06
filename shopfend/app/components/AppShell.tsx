"use client";

import type { ReactNode } from "react";
import { Suspense } from "react";
import SiteHeader from "./SiteHeader";
import SiteFooter from "./SiteFooter";

export default function AppShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-dvh bg-[var(--background)] text-[var(--foreground)]">
      <Suspense
        fallback={
          <div className="sticky top-0 z-50 bg-[var(--surface)] border-b border-black/10">
            <div className="sb-container py-4">
              <div className="h-10 rounded-md bg-[color:color-mix(in_srgb,var(--foreground)_6%,transparent)]" />
            </div>
          </div>
        }
      >
        <SiteHeader />
      </Suspense>
      <main className="sb-container py-6">{children}</main>
      <SiteFooter />
    </div>
  );
}
