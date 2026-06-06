"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { AdminCategoryDto, AdminCategoryUpsertDto, shopbeApi } from "@/lib/shopbeApi";
import { AdminBadge, AdminButton, AdminCard, AdminEmptyState, AdminErrorState, AdminInput, AdminLoadingState, AdminPageIntro, AdminSelect } from "../components/AdminUi";

const initialForm: AdminCategoryUpsertDto = {
  name: "",
  parentCategoryId: null,
  slug: "",
  sortOrder: 0,
  isActive: true,
};

export default function AdminCategoriesPage() {
  const { data: session, status } = useSession();
  const [items, setItems] = useState<AdminCategoryDto[]>([]);
  const [form, setForm] = useState<AdminCategoryUpsertDto>(initialForm);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loaded, setLoaded] = useState(false);

  const selected = useMemo(() => items.find((item) => item.id === selectedId) ?? null, [items, selectedId]);

  const load = async (accessToken: string, signal?: AbortSignal) => {
    return shopbeApi.admin.categories(accessToken, undefined, signal);
  };

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    load(session.accessToken, controller.signal)
      .then((response) => {
        setItems(response);
        setLoaded(true);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setLoaded(true);
        setError(errorMessage(err, "Failed to load categories."));
      });
    return () => controller.abort();
  }, [session?.accessToken, status]);

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!session?.accessToken) return;

    try {
      if (selectedId) {
        await shopbeApi.admin.updateCategory(session.accessToken, selectedId, form);
      } else {
        await shopbeApi.admin.createCategory(session.accessToken, form);
      }
      setForm(initialForm);
      setSelectedId(null);
      const response = await load(session.accessToken);
      setItems(response);
      setLoaded(true);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to save category."));
    }
  };

  const onEdit = (category: AdminCategoryDto) => {
    setSelectedId(category.id);
    setForm({
      name: category.name,
      parentCategoryId: category.parentCategoryId ?? null,
      slug: category.slug,
      sortOrder: category.sortOrder,
      isActive: category.isActive,
    });
  };

  const onDelete = async (category: AdminCategoryDto) => {
    if (!session?.accessToken || !window.confirm(`Delete ${category.name}?`)) return;
    try {
      await shopbeApi.admin.deleteCategory(session.accessToken, category.id);
      if (selectedId === category.id) {
        setSelectedId(null);
        setForm(initialForm);
      }
      const response = await load(session.accessToken);
      setItems(response);
      setLoaded(true);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to delete category."));
    }
  };

  if (status === "loading" || (status === "authenticated" && !loaded && !error)) return <AdminLoadingState />;
  if (error && !items.length) return <AdminErrorState message={error} />;

  return (
    <div className="space-y-6">
      <AdminPageIntro title="Category management" description="Maintain the navigation spine for the catalog and keep category order and visibility under control." />

      <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
        <AdminCard>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-black text-slate-950">Categories</h2>
            <p className="text-sm text-slate-500">{items.length} total</p>
          </div>
          {error ? <p className="mt-4 text-sm font-medium text-rose-600">{error}</p> : null}

          {!items.length ? (
            <div className="mt-6"><AdminEmptyState title="No categories yet" message="Create the first category to organize products." /></div>
          ) : (
            <div className="mt-5 space-y-3">
              {items.map((category) => (
                <div key={category.id} className="flex flex-col gap-3 rounded-2xl border border-slate-100 p-4 md:flex-row md:items-center md:justify-between">
                  <div>
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="font-bold text-slate-950">{category.name}</p>
                      <AdminBadge value={category.isActive} />
                    </div>
                    <p className="mt-1 text-sm text-slate-500">Slug: {category.slug || "(generated)"}</p>
                    <p className="mt-1 text-xs text-slate-400">Sort order: {category.sortOrder} | Parent: {category.parentCategoryId ?? "Root"}</p>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <AdminButton variant="secondary" onClick={() => onEdit(category)}>Edit</AdminButton>
                    <AdminButton variant="danger" onClick={() => void onDelete(category)}>Delete</AdminButton>
                  </div>
                </div>
              ))}
            </div>
          )}
        </AdminCard>

        <AdminCard>
          <h2 className="text-lg font-black text-slate-950">{selected ? `Edit ${selected.name}` : "Create category"}</h2>
          <form className="mt-5 space-y-4" onSubmit={onSubmit}>
            <div>
              <p className="sb-label">Name</p>
              <AdminInput value={form.name} onChange={(event) => setForm((value) => ({ ...value, name: event.target.value }))} required />
            </div>
            <div>
              <p className="sb-label">Slug</p>
              <AdminInput value={form.slug ?? ""} onChange={(event) => setForm((value) => ({ ...value, slug: event.target.value }))} placeholder="Optional" />
            </div>
            <div>
              <p className="sb-label">Parent category</p>
              <AdminSelect value={form.parentCategoryId ?? ""} onChange={(event) => setForm((value) => ({ ...value, parentCategoryId: event.target.value || null }))}>
                <option value="">Root category</option>
                {items.filter((item) => item.id !== selectedId).map((item) => (
                  <option key={item.id} value={item.id}>{item.name}</option>
                ))}
              </AdminSelect>
            </div>
            <div>
              <p className="sb-label">Sort order</p>
              <AdminInput type="number" value={form.sortOrder} onChange={(event) => setForm((value) => ({ ...value, sortOrder: Number(event.target.value) }))} />
            </div>
            <label className="flex items-center gap-3 rounded-2xl bg-slate-50 px-4 py-3 text-sm font-medium text-slate-700">
              <input type="checkbox" checked={form.isActive} onChange={(event) => setForm((value) => ({ ...value, isActive: event.target.checked }))} />
              Category is active
            </label>
            <div className="flex flex-wrap gap-3">
              <AdminButton type="submit">{selectedId ? "Update category" : "Create category"}</AdminButton>
              {selectedId ? <AdminButton type="button" variant="secondary" onClick={() => { setSelectedId(null); setForm(initialForm); }}>Cancel</AdminButton> : null}
            </div>
          </form>
        </AdminCard>
      </div>
    </div>
  );
}
