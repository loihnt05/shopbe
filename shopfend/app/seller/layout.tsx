import type { ReactNode } from "react";
import SellerHeader from "./components/SellerHeader";
import SellerSidebar from "./components/SellerSidebar";

export default function SellerLayout({ children }: { children: ReactNode }) {
  return (
    <div className="space-y-6">
      <SellerHeader />
      <div className="grid gap-6 xl:grid-cols-[260px_minmax(0,1fr)]">
        <div className="xl:sticky xl:top-24 xl:self-start">
          <SellerSidebar />
        </div>
        <div className="min-w-0">{children}</div>
      </div>
    </div>
  );
}
