"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { BackendPagedResult, SellerProductListItemDto, shopbeApi } from "@/lib/shopbeApi";
import { SellerBadge, SellerButton, SellerCard, SellerCompactMoney, SellerEmptyState, SellerErrorState, SellerInput, SellerLinkButton, SellerLoadingState, SellerPageIntro, SellerSelect, SellerToolbar, formatSellerDate } from "../components/SellerUi";

export default function SellerProductsPage() {
  const { data: session, status } = useSession();
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [result, setResult] = useState<BackendPagedResult<SellerProductListItemDto> | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = async (accessToken: string, signal?: AbortSignal) => {
    return shopbeApi.seller.products(accessToken, {
      search: search || undefined,
      status: statusFilter || undefined,
    }, signal);
  };

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    shopbeApi.seller.products(session.accessToken, {
      search: search || undefined,
      status: statusFilter || undefined,
    }, controller.signal)
      .then((response) => {
        setResult(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load seller products."));
      });
    return () => controller.abort();
  }, [search, session?.accessToken, status, statusFilter]);

  const removeProduct = async (id: string, name: string) => {
    if (!session?.accessToken || !window.confirm(`Delete ${name}?`)) return;
    try {
      await shopbeApi.seller.deleteProduct(session.accessToken, id);
      const response = await load(session.accessToken);
      setResult(response);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to delete product."));
    }
  };

  if (status === "loading" || (status === "authenticated" && !result && !error)) return <SellerLoadingState />;
  if (error && !result) return <SellerErrorState message={error} />;

  return (
    <div className="space-y-6">
      <SellerPageIntro title="Product management" description="Review the catalog you own, check moderation status, and jump into product maintenance fast." action={<SellerLinkButton href="/seller/products/new">Add product</SellerLinkButton>} />

      <SellerCard>
        <SellerToolbar>
          <div className="grid flex-1 gap-3 md:grid-cols-2">
            <SellerInput placeholder="Search your products" value={search} onChange={(event) => setSearch(event.target.value)} />
            <SellerSelect value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
              <option value="">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
              <option value="Hidden">Hidden</option>
            </SellerSelect>
          </div>
          <p className="text-sm text-slate-500">{result?.totalCount ?? 0} products</p>
        </SellerToolbar>

        {error ? <p className="mt-4 text-sm font-medium text-rose-600">{error}</p> : null}

        {!result?.items.length ? (
          <div className="mt-6"><SellerEmptyState title="No products yet" message="Create your first product to start selling through the marketplace." /></div>
        ) : (
          <div className="mt-6 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate-400">
                <tr>
                  <th className="pb-3 font-bold">Product</th>
                  <th className="pb-3 font-bold">Category</th>
                  <th className="pb-3 font-bold">Price</th>
                  <th className="pb-3 font-bold">Stock</th>
                  <th className="pb-3 font-bold">Status</th>
                  <th className="pb-3 font-bold">Actions</th>
                </tr>
              </thead>
              <tbody>
                {result.items.map((product) => (
                  <tr key={product.id} className="border-t border-slate-100 align-top">
                    <td className="py-4">
                      <p className="font-medium text-slate-900">{product.name}</p>
                      <p className="mt-1 text-xs text-slate-500">{formatSellerDate(product.createdAt)}</p>
                    </td>
                    <td className="py-4 text-slate-600">{product.categoryName ?? "Uncategorized"}</td>
                    <td className="py-4 text-slate-600"><SellerCompactMoney amount={product.price} currency="USD" /></td>
                    <td className="py-4 text-slate-600">{product.stock}</td>
                    <td className="py-4 space-y-2">
                      <SellerBadge value={product.approvalStatus} />
                      <div><SellerBadge value={product.isActive} /></div>
                    </td>
                    <td className="py-4">
                      <div className="flex flex-wrap gap-2">
                        <Link href={`/products/${product.id}`} className="inline-flex rounded-xl bg-slate-100 px-4 py-2 text-sm font-bold text-slate-800 transition hover:bg-slate-200">View</Link>
                        <Link href={`/seller/products/${product.id}/edit`} className="inline-flex rounded-xl bg-slate-100 px-4 py-2 text-sm font-bold text-slate-800 transition hover:bg-slate-200">Edit</Link>
                        <SellerButton variant="danger" onClick={() => void removeProduct(product.id, product.name)}>Delete</SellerButton>
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
