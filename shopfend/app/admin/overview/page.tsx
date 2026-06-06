"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { AdminDashboardOverviewDto, shopbeApi } from "@/lib/shopbeApi";
import { AdminBadge, AdminCard, AdminCompactMoney, AdminErrorState, AdminLoadingState, AdminPageIntro, AdminStatCard, formatAdminDate } from "../components/AdminUi";

export default function AdminOverviewPage() {
  const { data: session, status } = useSession();
  const [data, setData] = useState<AdminDashboardOverviewDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;

    const controller = new AbortController();
    shopbeApi.admin.dashboardOverview(session.accessToken, controller.signal)
      .then((response) => {
        setData(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load admin overview."));
      });

    return () => controller.abort();
  }, [session?.accessToken, status]);

  if (status === "loading" || (status === "authenticated" && !data && !error)) {
    return <AdminLoadingState />;
  }

  if (error || !data) {
    return <AdminErrorState message={error ?? "Admin overview is unavailable."} />;
  }

  return (
    <div className="space-y-6">
      <AdminPageIntro title="Platform overview" description="A quick pulse on users, orders, products, and the marketplace revenue stream." />

      <div className="grid gap-4 md:grid-cols-2 2xl:grid-cols-5">
        <AdminStatCard label="Total users" value={data.totalUsers.toLocaleString()} />
        <AdminStatCard label="Total sellers" value={data.totalSellers.toLocaleString()} tone="sky" />
        <AdminStatCard label="Pending products" value={data.pendingProducts.toLocaleString()} tone="orange" />
        <AdminStatCard label="Total orders" value={data.totalOrders.toLocaleString()} tone="emerald" />
        <AdminStatCard label="Monthly revenue" value={<AdminCompactMoney amount={data.monthlyRevenue} currency="USD" />} />
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.5fr_1fr]">
        <AdminCard>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-black text-slate-950">Revenue summary</h2>
            <span className="text-sm text-slate-500">Total: <AdminCompactMoney amount={data.totalRevenue} currency="USD" /></span>
          </div>
          <div className="mt-5 grid gap-4 md:grid-cols-2">
            <div className="rounded-2xl bg-slate-50 p-4">
              <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Customers</p>
              <p className="mt-2 text-2xl font-black text-slate-950">{data.totalCustomers.toLocaleString()}</p>
            </div>
            <div className="rounded-2xl bg-slate-50 p-4">
              <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Completed orders</p>
              <p className="mt-2 text-2xl font-black text-slate-950">{data.completedOrders.toLocaleString()}</p>
            </div>
            <div className="rounded-2xl bg-slate-50 p-4">
              <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Total products</p>
              <p className="mt-2 text-2xl font-black text-slate-950">{data.totalProducts.toLocaleString()}</p>
            </div>
            <div className="rounded-2xl bg-slate-50 p-4">
              <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Pending orders</p>
              <p className="mt-2 text-2xl font-black text-slate-950">{data.pendingOrders.toLocaleString()}</p>
            </div>
          </div>
        </AdminCard>

        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Top sellers</h2>
          <div className="mt-4 space-y-3">
            {data.topSellers.map((seller) => (
              <div key={seller.sellerId} className="rounded-2xl bg-slate-50 p-4">
                <p className="font-bold text-slate-950">{seller.shopName ?? seller.sellerName}</p>
                <p className="mt-1 text-sm text-slate-500">{seller.sellerName}</p>
                <div className="mt-3 flex items-center justify-between text-sm text-slate-600">
                  <span>{seller.totalSales} sales</span>
                  <span><AdminCompactMoney amount={seller.revenue} currency="USD" /></span>
                </div>
              </div>
            ))}
          </div>
        </AdminCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Top products</h2>
          <div className="mt-4 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate-400">
                <tr>
                  <th className="pb-3 font-bold">Product</th>
                  <th className="pb-3 font-bold">Sold</th>
                  <th className="pb-3 font-bold">Revenue</th>
                </tr>
              </thead>
              <tbody>
                {data.topProducts.map((product) => (
                  <tr key={product.productId} className="border-t border-slate-100">
                    <td className="py-3 font-medium text-slate-900">{product.productName}</td>
                    <td className="py-3 text-slate-600">{product.soldCount}</td>
                    <td className="py-3 text-slate-600"><AdminCompactMoney amount={product.revenue} currency="USD" /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </AdminCard>

        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Recent orders</h2>
          <div className="mt-4 space-y-3">
            {data.recentOrders.map((order) => (
              <div key={order.id} className="rounded-2xl border border-slate-100 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="font-bold text-slate-950">{order.customerName}</p>
                  <AdminBadge value={order.status} />
                </div>
                <p className="mt-1 text-sm text-slate-500">{order.customerEmail}</p>
                <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-sm text-slate-600">
                  <span>{order.itemsCount} items</span>
                  <span><AdminCompactMoney amount={order.totalAmount} currency={order.currency} /></span>
                  <span>{formatAdminDate(order.createdAt)}</span>
                </div>
              </div>
            ))}
          </div>
        </AdminCard>
      </div>

      <AdminCard>
        <h2 className="text-lg font-black text-slate-950">Recent users</h2>
        <div className="mt-4 overflow-x-auto">
          <table className="min-w-full text-sm">
            <thead className="text-left text-slate-400">
              <tr>
                <th className="pb-3 font-bold">Name</th>
                <th className="pb-3 font-bold">Email</th>
                <th className="pb-3 font-bold">Role</th>
                <th className="pb-3 font-bold">Joined</th>
              </tr>
            </thead>
            <tbody>
              {data.recentUsers.map((user) => (
                <tr key={user.id} className="border-t border-slate-100">
                  <td className="py-3 font-medium text-slate-900">{user.fullName}</td>
                  <td className="py-3 text-slate-600">{user.email}</td>
                  <td className="py-3"><AdminBadge value={user.role} /></td>
                  <td className="py-3 text-slate-600">{formatAdminDate(user.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </AdminCard>
    </div>
  );
}
