"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { AdminProductDto, BackendPagedResult, shopbeApi } from "@/lib/shopbeApi";
import { AdminBadge, AdminButton, AdminCard, AdminCompactMoney, AdminEmptyState, AdminErrorState, AdminInput, AdminLoadingState, AdminPageIntro, AdminSelect, AdminToolbar, formatAdminDate } from "../components/AdminUi";

export default function AdminProductsPage() {
  const { data: session, status } = useSession();
  const [search, setSearch] = useState("");
  const [approvalStatus, setApprovalStatus] = useState("");
  const [result, setResult] = useState<BackendPagedResult<AdminProductDto> | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = async (accessToken: string, signal?: AbortSignal) => {
    return shopbeApi.admin.products(accessToken, {
      searchTerm: search || undefined,
      approvalStatus: approvalStatus || undefined,
    }, signal);
  };

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    shopbeApi.admin.products(session.accessToken, {
      searchTerm: search || undefined,
      approvalStatus: approvalStatus || undefined,
    }, controller.signal)
      .then((response) => {
        setResult(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load products."));
      });
    return () => controller.abort();
  }, [approvalStatus, search, session?.accessToken, status]);

  const refresh = async () => {
    if (!session?.accessToken) return;
    const response = await load(session.accessToken);
    setResult(response);
  };

  const approve = async (product: AdminProductDto, nextStatus: string, adminNotes?: string) => {
    if (!session?.accessToken) return;
    try {
      await shopbeApi.admin.updateProductApproval(session.accessToken, product.id, nextStatus, adminNotes);
      await refresh();
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to update product approval."));
    }
  };

  const setVisibility = async (product: AdminProductDto, isActive: boolean) => {
    if (!session?.accessToken) return;
    try {
      await shopbeApi.admin.updateProductVisibility(session.accessToken, product.id, isActive);
      await refresh();
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to update product visibility."));
    }
  };

  const remove = async (product: AdminProductDto) => {
    if (!session?.accessToken || !window.confirm(`Remove ${product.name}?`)) return;
    try {
      await shopbeApi.admin.deleteProduct(session.accessToken, product.id);
      await refresh();
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to remove product."));
    }
  };

  if (status === "loading" || (status === "authenticated" && !result && !error)) return <AdminLoadingState />;
  if (error && !result) return <AdminErrorState message={error} />;

  return (
    <div className="space-y-6">
      <AdminPageIntro title="Product moderation" description="Review listings, approve catalog changes, and hide inventory that should not be customer-facing." />

      <AdminCard>
        <AdminToolbar>
          <div className="grid flex-1 gap-3 md:grid-cols-2">
            <AdminInput placeholder="Search by product or seller" value={search} onChange={(event) => setSearch(event.target.value)} />
            <AdminSelect value={approvalStatus} onChange={(event) => setApprovalStatus(event.target.value)}>
              <option value="">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
              <option value="Hidden">Hidden</option>
            </AdminSelect>
          </div>
          <p className="text-sm text-slate-500">{result?.totalCount ?? 0} products</p>
        </AdminToolbar>

        {error ? <p className="mt-4 text-sm font-medium text-rose-600">{error}</p> : null}

        {!result?.items.length ? (
          <div className="mt-6"><AdminEmptyState title="No products found" message="There are no product records matching the current moderation filter." /></div>
        ) : (
          <div className="mt-6 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate-400">
                <tr>
                  <th className="pb-3 font-bold">Product</th>
                  <th className="pb-3 font-bold">Seller</th>
                  <th className="pb-3 font-bold">Category</th>
                  <th className="pb-3 font-bold">Price</th>
                  <th className="pb-3 font-bold">Status</th>
                  <th className="pb-3 font-bold">Actions</th>
                </tr>
              </thead>
              <tbody>
                {result.items.map((product) => (
                  <tr key={product.id} className="border-t border-slate-100 align-top">
                    <td className="py-4">
                      <p className="font-medium text-slate-900">{product.name}</p>
                      <p className="mt-1 text-xs text-slate-500">{formatAdminDate(product.createdAt)}</p>
                    </td>
                    <td className="py-4 text-slate-600">{product.shopName ?? product.sellerName}</td>
                    <td className="py-4 text-slate-600">{product.categoryName ?? "Uncategorized"}</td>
                    <td className="py-4 text-slate-600"><AdminCompactMoney amount={product.price} currency="USD" /></td>
                    <td className="py-4 space-y-2">
                      <AdminBadge value={product.approvalStatus} />
                      <div><AdminBadge value={product.isActive} /></div>
                    </td>
                    <td className="py-4">
                      <div className="flex flex-wrap gap-2">
                        <AdminButton variant="secondary" onClick={() => void approve(product, "Approved")}>Approve</AdminButton>
                        <AdminButton variant="secondary" onClick={() => {
                          const reason = window.prompt("Rejection note", product.adminNotes ?? "");
                          void approve(product, "Rejected", reason ?? undefined);
                        }}>Reject</AdminButton>
                        <AdminButton variant="secondary" onClick={() => void setVisibility(product, !product.isActive)}>
                          {product.isActive ? "Hide" : "Show"}
                        </AdminButton>
                        <AdminButton variant="danger" onClick={() => void remove(product)}>Remove</AdminButton>
                      </div>
                    </td>
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
