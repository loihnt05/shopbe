"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { BackendPagedResult, SellerOrderListItemDto, shopbeApi } from "@/lib/shopbeApi";
import { SellerBadge, SellerButton, SellerCard, SellerCompactMoney, SellerEmptyState, SellerErrorState, SellerInput, SellerLoadingState, SellerPageIntro, SellerSelect, SellerToolbar, formatSellerDate } from "../components/SellerUi";

export default function SellerOrdersPage() {
  const { data: session, status } = useSession();
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [result, setResult] = useState<BackendPagedResult<SellerOrderListItemDto> | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = async (accessToken: string, signal?: AbortSignal) => {
    return shopbeApi.seller.orders(accessToken, {
      search: search || undefined,
      status: statusFilter || undefined,
    }, signal);
  };

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    shopbeApi.seller.orders(session.accessToken, {
      search: search || undefined,
      status: statusFilter || undefined,
    }, controller.signal)
      .then((response) => {
        setResult(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load seller orders."));
      });
    return () => controller.abort();
  }, [search, session?.accessToken, status, statusFilter]);

  const updateStatus = async (id: string, nextStatus: string) => {
    if (!session?.accessToken) return;
    try {
      await shopbeApi.seller.updateOrderStatus(session.accessToken, id, nextStatus);
      const response = await load(session.accessToken);
      setResult(response);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to update seller order status."));
    }
  };

  if (status === "loading" || (status === "authenticated" && !result && !error)) return <SellerLoadingState />;
  if (error && !result) return <SellerErrorState message={error} />;

  return (
    <div className="space-y-6">
      <SellerPageIntro title="Order management" description="Monitor customer orders that include your inventory and move them through fulfillment." />

      <SellerCard>
        <SellerToolbar>
          <div className="grid flex-1 gap-3 md:grid-cols-2">
            <SellerInput placeholder="Search orders or customers" value={search} onChange={(event) => setSearch(event.target.value)} />
            <SellerSelect value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
              <option value="">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Processing">Processing</option>
              <option value="Shipped">Shipped</option>
              <option value="Delivered">Delivered</option>
            </SellerSelect>
          </div>
          <p className="text-sm text-slate-500">{result?.totalCount ?? 0} orders</p>
        </SellerToolbar>

        {error ? <p className="mt-4 text-sm font-medium text-rose-600">{error}</p> : null}

        {!result?.items.length ? (
          <div className="mt-6"><SellerEmptyState title="No orders found" message="Orders that contain your products will appear here." /></div>
        ) : (
          <div className="mt-6 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate-400">
                <tr>
                  <th className="pb-3 font-bold">Order</th>
                  <th className="pb-3 font-bold">Customer</th>
                  <th className="pb-3 font-bold">Items</th>
                  <th className="pb-3 font-bold">Total</th>
                  <th className="pb-3 font-bold">Status</th>
                  <th className="pb-3 font-bold">Actions</th>
                </tr>
              </thead>
              <tbody>
                {result.items.map((order) => (
                  <tr key={order.id} className="border-t border-slate-100 align-top">
                    <td className="py-4">
                      <p className="font-medium text-slate-900">{order.id}</p>
                      <p className="mt-1 text-xs text-slate-500">{formatSellerDate(order.createdAt)}</p>
                    </td>
                    <td className="py-4 text-slate-600">
                      <p>{order.customerName}</p>
                      <p className="mt-1 text-xs">{order.customerEmail}</p>
                    </td>
                    <td className="py-4 text-slate-600">{order.sellerItemsCount}</td>
                    <td className="py-4 text-slate-600"><SellerCompactMoney amount={order.sellerItemsTotal} currency="USD" /></td>
                    <td className="py-4"><SellerBadge value={order.status} /></td>
                    <td className="py-4">
                      <div className="flex flex-wrap gap-2">
                        <SellerButton variant="secondary" onClick={() => void updateStatus(order.id, "Processing")}>Processing</SellerButton>
                        <SellerButton variant="secondary" onClick={() => void updateStatus(order.id, "Shipped")}>Shipped</SellerButton>
                        <SellerButton variant="secondary" onClick={() => void updateStatus(order.id, "Delivered")}>Delivered</SellerButton>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </SellerCard>
    </div>
  );
}
