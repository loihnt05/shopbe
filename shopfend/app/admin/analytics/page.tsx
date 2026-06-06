"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { AdminRevenuePointDto, AdminSalesBreakdownDto, AdminTopProductDto, AdminTopSellerDto, shopbeApi } from "@/lib/shopbeApi";
import { AdminBadge, AdminCard, AdminCompactMoney, AdminErrorState, AdminLoadingState, AdminPageIntro, AdminSelect } from "../components/AdminUi";

const PERIODS = ["daily", "weekly", "monthly", "yearly"];

export default function AdminAnalyticsPage() {
  const { data: session, status } = useSession();
  const [period, setPeriod] = useState("monthly");
  const [revenue, setRevenue] = useState<AdminRevenuePointDto[]>([]);
  const [sales, setSales] = useState<AdminSalesBreakdownDto | null>(null);
  const [topProducts, setTopProducts] = useState<AdminTopProductDto[]>([]);
  const [topSellers, setTopSellers] = useState<AdminTopSellerDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;

    const controller = new AbortController();
    Promise.all([
      shopbeApi.admin.revenue(session.accessToken, period, controller.signal),
      shopbeApi.admin.sales(session.accessToken, period, controller.signal),
      shopbeApi.admin.topProducts(session.accessToken, 8, controller.signal),
      shopbeApi.admin.topSellers(session.accessToken, 8, controller.signal),
    ])
      .then(([revenueData, salesData, productData, sellerData]) => {
        setRevenue(revenueData);
        setSales(salesData);
        setTopProducts(productData);
        setTopSellers(sellerData);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load analytics."));
      });

    return () => controller.abort();
  }, [period, session?.accessToken, status]);

  if (status === "loading" || (status === "authenticated" && !sales && !error)) return <AdminLoadingState />;
  if (error || !sales) return <AdminErrorState message={error ?? "Analytics are unavailable."} />;

  const totalRevenue = revenue.reduce((sum, point) => sum + point.revenue, 0);
  const totalOrders = revenue.reduce((sum, point) => sum + point.orders, 0);

  return (
    <div className="space-y-6">
      <AdminPageIntro
        title="Analytics"
        description="Break down revenue and order status patterns across the marketplace by time period."
        action={
          <AdminSelect value={period} onChange={(event) => setPeriod(event.target.value)}>
            {PERIODS.map((value) => <option key={value} value={value}>{value}</option>)}
          </AdminSelect>
        }
      />

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Revenue ({period})</p><p className="mt-3 text-3xl font-black text-slate-950"><AdminCompactMoney amount={totalRevenue} currency="USD" /></p></AdminCard>
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Orders ({period})</p><p className="mt-3 text-3xl font-black text-slate-950">{totalOrders}</p></AdminCard>
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Top status</p><p className="mt-3 text-2xl font-black text-slate-950">{sales.salesByStatus[0]?.key ?? "N/A"}</p></AdminCard>
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Data points</p><p className="mt-3 text-3xl font-black text-slate-950">{revenue.length}</p></AdminCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Revenue by period</h2>
          <div className="mt-5 space-y-3">
            {revenue.map((point) => {
              const width = totalRevenue > 0 ? Math.max(8, (point.revenue / totalRevenue) * 100) : 8;
              return (
                <div key={point.label}>
                  <div className="mb-1 flex items-center justify-between text-sm">
                    <span className="font-medium text-slate-700">{point.label}</span>
                    <span className="text-slate-500"><AdminCompactMoney amount={point.revenue} currency="USD" /> / {point.orders} orders</span>
                  </div>
                  <div className="h-3 rounded-full bg-slate-100">
                    <div className="h-3 rounded-full bg-brand" style={{ width: `${width}%` }} />
                  </div>
                </div>
              );
            })}
          </div>
        </AdminCard>

        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Sales by status</h2>
          <div className="mt-5 space-y-3">
            {sales.salesByStatus.map((entry) => (
              <div key={entry.key} className="flex items-center justify-between rounded-2xl bg-slate-50 px-4 py-3">
                <AdminBadge value={entry.key} />
                <span className="text-sm font-bold text-slate-700">{entry.value}</span>
              </div>
            ))}
          </div>
        </AdminCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Top products</h2>
          <div className="mt-4 space-y-3">
            {topProducts.map((product) => (
              <div key={product.productId} className="flex items-center justify-between rounded-2xl border border-slate-100 p-4">
                <div>
                  <p className="font-bold text-slate-950">{product.productName}</p>
                  <p className="mt-1 text-sm text-slate-500">{product.soldCount} sold</p>
                </div>
                <span className="text-sm font-bold text-slate-700"><AdminCompactMoney amount={product.revenue} currency="USD" /></span>
              </div>
            ))}
          </div>
        </AdminCard>

        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Top sellers</h2>
          <div className="mt-4 space-y-3">
            {topSellers.map((seller) => (
              <div key={seller.sellerId} className="flex items-center justify-between rounded-2xl border border-slate-100 p-4">
                <div>
                  <p className="font-bold text-slate-950">{seller.shopName ?? seller.sellerName}</p>
                  <p className="mt-1 text-sm text-slate-500">{seller.totalSales} sales</p>
                </div>
                <span className="text-sm font-bold text-slate-700"><AdminCompactMoney amount={seller.revenue} currency="USD" /></span>
              </div>
            ))}
          </div>
        </AdminCard>
      </div>
    </div>
  );
}
