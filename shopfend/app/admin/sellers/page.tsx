"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { AdminSellerDto, AdminSellerStatsDto, BackendPagedResult, shopbeApi } from "@/lib/shopbeApi";
import { AdminBadge, AdminButton, AdminCard, AdminCompactMoney, AdminEmptyState, AdminErrorState, AdminInput, AdminLoadingState, AdminPageIntro, AdminSelect, AdminToolbar, formatAdminDate } from "../components/AdminUi";

export default function AdminSellersPage() {
  const { data: session, status } = useSession();
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [result, setResult] = useState<BackendPagedResult<AdminSellerDto> | null>(null);
  const [selectedStats, setSelectedStats] = useState<AdminSellerStatsDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = async (accessToken: string, signal?: AbortSignal) => {
    return shopbeApi.admin.sellers(accessToken, {
      searchTerm: search || undefined,
      status: statusFilter || undefined,
    }, signal);
  };

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;

    const controller = new AbortController();
    shopbeApi.admin.sellers(session.accessToken, {
      searchTerm: search || undefined,
      status: statusFilter || undefined,
    }, controller.signal)
      .then((response) => {
        setResult(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load sellers."));
      });
    return () => controller.abort();
  }, [search, session?.accessToken, status, statusFilter]);

  const updateStatus = async (seller: AdminSellerDto, nextStatus: string) => {
    if (!session?.accessToken) return;
    try {
      await shopbeApi.admin.updateSellerStatus(session.accessToken, seller.userId, nextStatus);
      const response = await load(session.accessToken);
      setResult(response);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to update seller status."));
    }
  };

  const loadStats = async (sellerId: string) => {
    if (!session?.accessToken) return;
    try {
      const stats = await shopbeApi.admin.sellerStats(session.accessToken, sellerId);
      setSelectedStats(stats);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to load seller stats."));
    }
  };

  if (status === "loading" || (status === "authenticated" && !result && !error)) return <AdminLoadingState />;
  if (error && !result) return <AdminErrorState message={error} />;

  return (
    <div className="space-y-6">
      <AdminPageIntro title="Seller management" description="Monitor shop performance, review status, and inspect a seller's operational footprint." />

      <div className="grid gap-6 xl:grid-cols-[1.5fr_1fr]">
        <AdminCard>
          <AdminToolbar>
            <div className="grid flex-1 gap-3 md:grid-cols-2">
              <AdminInput placeholder="Search by owner or shop" value={search} onChange={(event) => setSearch(event.target.value)} />
              <AdminSelect value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
                <option value="">All statuses</option>
                <option value="Pending">Pending</option>
                <option value="Active">Active</option>
                <option value="Suspended">Suspended</option>
              </AdminSelect>
            </div>
            <p className="text-sm text-slate-500">{result?.totalCount ?? 0} sellers</p>
          </AdminToolbar>

          {error ? <p className="mt-4 text-sm font-medium text-rose-600">{error}</p> : null}

          {!result?.items.length ? (
            <div className="mt-6">
              <AdminEmptyState title="No sellers found" message="No seller profile matches the current filters." />
            </div>
          ) : (
            <div className="mt-6 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead className="text-left text-slate-400">
                  <tr>
                    <th className="pb-3 font-bold">Shop</th>
                    <th className="pb-3 font-bold">Owner</th>
                    <th className="pb-3 font-bold">Status</th>
                    <th className="pb-3 font-bold">Revenue</th>
                    <th className="pb-3 font-bold">Joined</th>
                    <th className="pb-3 font-bold">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {result.items.map((seller) => (
                    <tr key={seller.userId} className="border-t border-slate-100 align-top">
                      <td className="py-4">
                        <p className="font-medium text-slate-900">{seller.shopName}</p>
                        <p className="mt-1 text-xs text-slate-500">Commission {seller.commissionRate}%</p>
                      </td>
                      <td className="py-4 text-slate-600">
                        <p>{seller.ownerName}</p>
                        <p className="mt-1 text-xs">{seller.ownerEmail}</p>
                      </td>
                      <td className="py-4"><AdminBadge value={seller.status} /></td>
                      <td className="py-4 text-slate-600"><AdminCompactMoney amount={seller.totalRevenue} currency="USD" /></td>
                      <td className="py-4 text-slate-600">{formatAdminDate(seller.createdAt)}</td>
                      <td className="py-4">
                        <div className="flex flex-wrap gap-2">
                          <AdminButton variant="secondary" onClick={() => void loadStats(seller.userId)}>View stats</AdminButton>
                          <AdminButton variant="secondary" onClick={() => void updateStatus(seller, seller.status === "Active" ? "Suspended" : "Active")}>
                            {seller.status === "Active" ? "Suspend" : "Approve"}
                          </AdminButton>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </AdminCard>

        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">Seller stats</h2>
          {selectedStats ? (
            <div className="mt-5 space-y-4 text-sm text-slate-600">
              <div>
                <p className="text-xl font-black text-slate-950">{selectedStats.shopName}</p>
                <p className="mt-1">Seller ID: {selectedStats.userId}</p>
              </div>
              <div className="grid gap-3 sm:grid-cols-2">
                <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Products</p><p className="mt-2 text-2xl font-black text-slate-950">{selectedStats.totalProducts}</p></div>
                <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Approved</p><p className="mt-2 text-2xl font-black text-slate-950">{selectedStats.approvedProducts}</p></div>
                <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Pending</p><p className="mt-2 text-2xl font-black text-slate-950">{selectedStats.pendingProducts}</p></div>
                <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Orders</p><p className="mt-2 text-2xl font-black text-slate-950">{selectedStats.totalOrders}</p></div>
              </div>
              <div className="rounded-2xl bg-emerald-50 p-4 text-emerald-900">
                <p className="text-xs font-black uppercase tracking-[0.2em]">Revenue</p>
                <p className="mt-2 text-3xl font-black"><AdminCompactMoney amount={selectedStats.totalRevenue} currency="USD" /></p>
              </div>
            </div>
          ) : (
            <div className="mt-6">
              <AdminEmptyState title="Select a seller" message="Choose any seller row to inspect product and order performance." />
            </div>
          )}
        </AdminCard>
      </div>
    </div>
  );
}
