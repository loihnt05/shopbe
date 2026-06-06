"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { SellerLowStockProductDto, SellerRevenuePointDto, shopbeApi } from "@/lib/shopbeApi";
import { SellerCard, SellerCompactMoney, SellerErrorState, SellerLoadingState, SellerPageIntro, SellerSelect } from "../components/SellerUi";

const PERIODS = ["daily", "weekly", "monthly", "yearly"];

export default function SellerAnalyticsPage() {
  const { data: session, status } = useSession();
  const [period, setPeriod] = useState("monthly");
  const [revenue, setRevenue] = useState<SellerRevenuePointDto[]>([]);
  const [sales, setSales] = useState<SellerRevenuePointDto[]>([]);
  const [lowStock, setLowStock] = useState<SellerLowStockProductDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    Promise.all([
      shopbeApi.seller.revenue(session.accessToken, period, controller.signal),
      shopbeApi.seller.sales(session.accessToken, period, controller.signal),
      shopbeApi.seller.lowStock(session.accessToken, 10, controller.signal),
    ])
      .then(([revenueData, salesData, lowStockData]) => {
        setRevenue(revenueData);
        setSales(salesData);
        setLowStock(lowStockData);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load seller analytics."));
      });
    return () => controller.abort();
  }, [period, session?.accessToken, status]);

  if (status === "loading" || (status === "authenticated" && !revenue.length && !sales.length && !error)) return <SellerLoadingState />;
  if (error) return <SellerErrorState message={error} />;

  const totalRevenue = revenue.reduce((sum, point) => sum + point.revenue, 0);
  const totalOrders = revenue.reduce((sum, point) => sum + point.orders, 0);

  return (
    <div className="space-y-6">
      <SellerPageIntro title="Revenue and analytics" description="Review how your shop performs over time and spot inventory that needs attention." action={<SellerSelect value={period} onChange={(event) => setPeriod(event.target.value)}>{PERIODS.map((value) => <option key={value} value={value}>{value}</option>)}</SellerSelect>} />

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <SellerCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Revenue ({period})</p><p className="mt-3 text-3xl font-black text-slate-950"><SellerCompactMoney amount={totalRevenue} currency="USD" /></p></SellerCard>
        <SellerCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Orders ({period})</p><p className="mt-3 text-3xl font-black text-slate-950">{totalOrders}</p></SellerCard>
        <SellerCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Low stock products</p><p className="mt-3 text-3xl font-black text-slate-950">{lowStock.length}</p></SellerCard>
        <SellerCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Data points</p><p className="mt-3 text-3xl font-black text-slate-950">{revenue.length}</p></SellerCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <SellerCard>
          <h2 className="text-lg font-black text-slate-950">Revenue trend</h2>
          <div className="mt-5 space-y-3">
            {revenue.map((point) => {
              const width = totalRevenue > 0 ? Math.max(8, (point.revenue / totalRevenue) * 100) : 8;
              return (
                <div key={point.label}>
                  <div className="mb-1 flex items-center justify-between text-sm">
                    <span className="font-medium text-slate-700">{point.label}</span>
                    <span className="text-slate-500"><SellerCompactMoney amount={point.revenue} currency="USD" /> / {point.orders} orders</span>
                  </div>
                  <div className="h-3 rounded-full bg-slate-100"><div className="h-3 rounded-full bg-brand" style={{ width: `${width}%` }} /></div>
                </div>
              );
            })}
          </div>
        </SellerCard>

        <SellerCard>
          <h2 className="text-lg font-black text-slate-950">Sales by product period</h2>
          <div className="mt-4 space-y-3">
            {sales.map((point) => (
              <div key={point.label} className="flex items-center justify-between rounded-2xl border border-slate-100 p-4">
                <div>
                  <p className="font-bold text-slate-950">{point.label}</p>
                  <p className="mt-1 text-sm text-slate-500">{point.orders} orders</p>
                </div>
                <span className="text-sm font-bold text-slate-700"><SellerCompactMoney amount={point.revenue} currency="USD" /></span>
              </div>
            ))}
          </div>
        </SellerCard>
      </div>

      <SellerCard>
        <h2 className="text-lg font-black text-slate-950">Low stock products</h2>
        <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {lowStock.length ? lowStock.map((product) => (
            <div key={product.productId} className="rounded-2xl border border-slate-100 p-4">
              <p className="font-bold text-slate-950">{product.productName}</p>
              <p className="mt-1 text-sm text-slate-500">Remaining stock: {product.stock}</p>
            </div>
          )) : <p className="text-sm text-slate-500">No low stock alerts right now.</p>}
        </div>
      </SellerCard>
    </div>
  );
}
