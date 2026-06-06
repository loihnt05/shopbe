"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { AdminOrderListItemDto, BackendPagedResult, shopbeApi } from "@/lib/shopbeApi";
import { AdminBadge, AdminCard, AdminCompactMoney, AdminEmptyState, AdminErrorState, AdminInput, AdminLoadingState, AdminPageIntro, AdminSelect, AdminToolbar, formatAdminDate } from "../components/AdminUi";

export default function AdminOrdersPage() {
  const { data: session, status } = useSession();
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [result, setResult] = useState<BackendPagedResult<AdminOrderListItemDto> | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    shopbeApi.admin.orders(session.accessToken, {
      searchTerm: search || undefined,
      status: statusFilter || undefined,
    }, controller.signal)
      .then((response) => {
        setResult(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load orders."));
      });
    return () => controller.abort();
  }, [search, session?.accessToken, status, statusFilter]);

  if (status === "loading" || (status === "authenticated" && !result && !error)) return <AdminLoadingState />;
  if (error && !result) return <AdminErrorState message={error} />;

  return (
    <div className="space-y-6">
      <AdminPageIntro title="Order monitoring" description="Track marketplace throughput, watch customer order status, and surface risky backlogs fast." />

      <AdminCard>
        <AdminToolbar>
          <div className="grid flex-1 gap-3 md:grid-cols-2">
            <AdminInput placeholder="Search by customer or order id" value={search} onChange={(event) => setSearch(event.target.value)} />
            <AdminSelect value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
              <option value="">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Confirmed">Confirmed</option>
              <option value="Processing">Processing</option>
              <option value="Shipped">Shipped</option>
              <option value="Delivered">Delivered</option>
              <option value="Cancelled">Cancelled</option>
              <option value="Refunded">Refunded</option>
            </AdminSelect>
          </div>
          <p className="text-sm text-slate-500">{result?.totalCount ?? 0} orders</p>
        </AdminToolbar>

        {error ? <p className="mt-4 text-sm font-medium text-rose-600">{error}</p> : null}

        {!result?.items.length ? (
          <div className="mt-6"><AdminEmptyState title="No orders found" message="There are no orders that match the current filter." /></div>
        ) : (
          <div className="mt-6 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate-400">
                <tr>
                  <th className="pb-3 font-bold">Order</th>
                  <th className="pb-3 font-bold">Customer</th>
                  <th className="pb-3 font-bold">Total</th>
                  <th className="pb-3 font-bold">Status</th>
                  <th className="pb-3 font-bold">Items</th>
                  <th className="pb-3 font-bold">Created</th>
                </tr>
              </thead>
              <tbody>
                {result.items.map((order) => (
                  <tr key={order.id} className="border-t border-slate-100">
                    <td className="py-4 font-medium text-slate-900">{order.id}</td>
                    <td className="py-4 text-slate-600">
                      <p>{order.customerName}</p>
                      <p className="mt-1 text-xs">{order.customerEmail}</p>
                    </td>
                    <td className="py-4 text-slate-600"><AdminCompactMoney amount={order.totalAmount} currency={order.currency} /></td>
                    <td className="py-4"><AdminBadge value={order.status} /></td>
                    <td className="py-4 text-slate-600">{order.itemsCount}</td>
                    <td className="py-4 text-slate-600">{formatAdminDate(order.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </AdminCard>
    </div>
  );
}
