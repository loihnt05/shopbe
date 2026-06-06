"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { SellerDashboardOverviewDto, shopbeApi } from "@/lib/shopbeApi";
import { SellerBadge, SellerCard, SellerCompactMoney, SellerErrorState, SellerLoadingState, SellerPageIntro, SellerStatCard, formatSellerDate } from "../components/SellerUi";

export default function SellerDashboardPage() {
  const { data: session, status } = useSession();
  const [data, setData] = useState<SellerDashboardOverviewDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    shopbeApi.seller.dashboardOverview(session.accessToken, controller.signal)
      .then((response) => {
        setData(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load seller dashboard."));
      });
    return () => controller.abort();
  }, [session?.accessToken, status]);

  if (status === "loading" || (status === "authenticated" && !data && !error)) return <SellerLoadingState />;
  if (error || !data) return <SellerErrorState message={error ?? "Seller dashboard is unavailable."} />;

  return (
    <div className="space-y-6">
      <SellerPageIntro title="Seller dashboard" description="Track fulfillment progress, revenue, and product performance from one workspace." />

      <div className="grid gap-4 md:grid-cols-2 2xl:grid-cols-4">
        <SellerStatCard label="My products" value={data.myProducts.toLocaleString()} />
        <SellerStatCard label="Pending orders" value={data.pendingOrders.toLocaleString()} tone="orange" />
        <SellerStatCard label="Delivered" value={data.deliveredOrders.toLocaleString()} tone="emerald" />
        <SellerStatCard label="This month" value={<SellerCompactMoney amount={data.thisMonthRevenue} currency="USD" />} tone="sky" />
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.5fr_1fr]">
        <SellerCard>
          <h2 className="text-lg font-black text-slate-950">Order pipeline</h2>
          <div className="mt-5 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Pending</p><p className="mt-2 text-2xl font-black text-slate-950">{data.pendingOrders}</p></div>
            <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Processing</p><p className="mt-2 text-2xl font-black text-slate-950">{data.processingOrders}</p></div>
            <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Shipped</p><p className="mt-2 text-2xl font-black text-slate-950">{data.shippedOrders}</p></div>
            <div className="rounded-2xl bg-emerald-50 p-4 text-emerald-900"><p className="text-xs font-black uppercase tracking-[0.2em]">Revenue</p><p className="mt-2 text-2xl font-black"><SellerCompactMoney amount={data.totalRevenue} currency="USD" /></p></div>
          </div>
        </SellerCard>

        <SellerCard>
          <h2 className="text-lg font-black text-slate-950">Low stock alerts</h2>
          <div className="mt-4 space-y-3">
            {data.lowStockProducts.length ? data.lowStockProducts.map((product) => (
              <div key={product.productId} className="rounded-2xl border border-slate-100 p-4">
                <p className="font-bold text-slate-950">{product.productName}</p>
                <p className="mt-1 text-sm text-slate-500">Only {product.stock} units remaining</p>
              </div>
            )) : <p className="text-sm text-slate-500">No low stock products right now.</p>}
          </div>
        </SellerCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <SellerCard>
          <h2 className="text-lg font-black text-slate-950">Recent orders</h2>
          <div className="mt-4 space-y-3">
            {data.recentOrders.map((order) => (
              <div key={order.id} className="rounded-2xl border border-slate-100 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="font-bold text-slate-950">{order.customerName}</p>
                  <SellerBadge value={order.status} />
                </div>
                <p className="mt-1 text-sm text-slate-500">{order.customerEmail}</p>
                <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-sm text-slate-600">
                  <span>{order.sellerItemsCount} items</span>
                  <span><SellerCompactMoney amount={order.sellerItemsTotal} currency="USD" /></span>
                  <span>{formatSellerDate(order.createdAt)}</span>
                </div>
              </div>
            ))}
          </div>
        </SellerCard>

        <SellerCard>
          <h2 className="text-lg font-black text-slate-950">Best-selling products</h2>
          <div className="mt-4 space-y-3">
            {data.bestSellingProducts.map((product) => (
              <div key={product.id} className="flex items-center justify-between rounded-2xl border border-slate-100 p-4">
                <div>
                  <p className="font-bold text-slate-950">{product.name}</p>
                  <p className="mt-1 text-sm text-slate-500">{product.categoryName ?? "Uncategorized"}</p>
                </div>
                <div className="text-right text-sm text-slate-600">
                  <p><SellerCompactMoney amount={product.price} currency="USD" /></p>
                  <p className="mt-1">Stock {product.stock}</p>
                </div>
              </div>
            ))}
          </div>
        </SellerCard>
      </div>
    </div>
  );
}
