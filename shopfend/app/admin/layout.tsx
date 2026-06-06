import type { ReactNode } from "react";
import AdminHeader from "./components/AdminHeader";
import AdminSidebar from "./components/AdminSidebar";

export default function AdminLayout({ children }: { children: ReactNode }) {
  return (
    <div className="space-y-6">
      <AdminHeader />
      <div className="grid gap-6 xl:grid-cols-[260px_minmax(0,1fr)]">
        <div className="xl:sticky xl:top-24 xl:self-start">
          <AdminSidebar />
        </div>
        <div className="min-w-0">{children}</div>
      </div>
    </div>
  );
}
