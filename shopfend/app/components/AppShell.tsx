"use client";

import type { ReactNode } from "react";
import { Suspense } from "react";
import SiteHeader from "./SiteHeader";
import SiteFooter from "./SiteFooter";
import CartDrawer from "./CartDrawer";

export default function AppShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-dvh bg-background text-foreground">
      <Suspense
        fallback={
          <div className="sticky top-0 z-50 bg-surface border-b border-black/10">
            <div className="sb-container py-4">
              <div className="h-10 rounded-md bg-foreground/6" />
            </div>
          </div>
        }
      >
        <SiteHeader />
      </Suspense>
      <main className="sb-container py-6">{children}</main>
      <SiteFooter />
      <CartDrawer />
    </div>
  );
}
