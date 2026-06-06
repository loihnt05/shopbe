"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { ProductDetail, PublicCategoryDto, SellerProductUpsertDto, shopbeApi } from "@/lib/shopbeApi";
import { SellerButton, SellerCard, SellerErrorState, SellerInput, SellerLoadingState, SellerPageIntro, SellerSelect, SellerTextArea } from "./SellerUi";

type VariantDraft = {
  sku: string;
  price: string;
  stockQuantity: string;
  isActive: boolean;
};

type FormState = {
  name: string;
  description: string;
  basePrice: string;
  categoryId: string;
  brandId: string;
  isActive: boolean;
  imageUrl: string;
  variant: VariantDraft;
};

const initialForm: FormState = {
  name: "",
  description: "",
  basePrice: "0",
  categoryId: "",
  brandId: "",
  isActive: true,
  imageUrl: "",
  variant: {
    sku: "",
    price: "0",
    stockQuantity: "0",
    isActive: true,
  },
};

function mapProductToForm(product: ProductDetail): FormState {
  const primaryImage = product.images?.find((image) => image.isPrimary)?.imageUrl ?? product.images?.[0]?.imageUrl ?? product.primaryImageUrl ?? "";
  const variant = product.variants?.[0];

  return {
    name: product.name,
    description: product.description ?? "",
    basePrice: String(product.price ?? 0),
    categoryId: product.categoryId,
    brandId: product.brandId ?? "",
    isActive: product.isActive,
    imageUrl: primaryImage,
    variant: {
      sku: variant?.sku ?? "",
      price: String(variant?.price ?? product.price ?? 0),
      stockQuantity: String(variant?.stockQuantity ?? product.totalStockQuantity ?? 0),
      isActive: variant?.isActive ?? true,
    },
  };
}

function buildPayload(form: FormState): SellerProductUpsertDto {
  return {
    name: form.name.trim(),
    description: form.description.trim(),
    basePrice: Number(form.basePrice),
    categoryId: form.categoryId,
    brandId: form.brandId.trim() || null,
    isActive: form.isActive,
    images: form.imageUrl.trim()
      ? [{ imageUrl: form.imageUrl.trim(), isPrimary: true }]
      : [],
    variants: form.variant.sku.trim()
      ? [{
          sku: form.variant.sku.trim(),
          price: Number(form.variant.price),
          stockQuantity: Number(form.variant.stockQuantity),
          isActive: form.variant.isActive,
          attributeValueIds: null,
        }]
      : [],
  };
}

export default function SellerProductEditor({
  mode,
  productId,
}: {
  mode: "create" | "edit";
  productId?: string;
}) {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [categories, setCategories] = useState<PublicCategoryDto[]>([]);
  const [form, setForm] = useState<FormState>(initialForm);
  const [loading, setLoading] = useState(mode === "edit");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    shopbeApi.categories.list(controller.signal)
      .then((items) => setCategories(items.filter((item) => item.isActive)))
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load categories."));
      });
    return () => controller.abort();
  }, []);

  useEffect(() => {
    if (mode !== "edit" || !productId) return;
    const controller = new AbortController();
    setLoading(true);
    shopbeApi.products.getById(productId, session?.accessToken, controller.signal)
      .then((product) => {
        setForm(mapProductToForm(product));
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load product details."));
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      });
    return () => controller.abort();
  }, [mode, productId, session?.accessToken]);

  const pageTitle = useMemo(() => (mode === "create" ? "Add product" : "Edit product"), [mode]);

  if (status === "loading" || (loading && mode === "edit")) {
    return <SellerLoadingState />;
  }

  if (!session?.accessToken) {
    return <SellerErrorState message="Seller session is required to manage products." />;
  }

  const accessToken = session.accessToken;

  const save = async () => {
    try {
      setSaving(true);
      setError(null);
      const payload = buildPayload(form);
      if (mode === "create") {
        await shopbeApi.seller.createProduct(accessToken, payload);
      } else if (productId) {
        await shopbeApi.seller.updateProduct(accessToken, productId, payload);
      }
      router.push("/seller/products");
      router.refresh();
    } catch (err) {
      setError(errorMessage(err, `Failed to ${mode} product.`));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      <SellerPageIntro title={pageTitle} description="Manage the essential catalog fields needed to publish and maintain a seller listing." />

      <SellerCard>
        <div className="grid gap-4 lg:grid-cols-2">
          <div>
            <p className="sb-label">Product name</p>
            <SellerInput value={form.name} onChange={(event) => setForm((value) => ({ ...value, name: event.target.value }))} />
          </div>
          <div>
            <p className="sb-label">Category</p>
            <SellerSelect value={form.categoryId} onChange={(event) => setForm((value) => ({ ...value, categoryId: event.target.value }))}>
              <option value="">Select category</option>
              {categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
            </SellerSelect>
          </div>
          <div>
            <p className="sb-label">Base price</p>
            <SellerInput type="number" min="0" step="0.01" value={form.basePrice} onChange={(event) => setForm((value) => ({ ...value, basePrice: event.target.value }))} />
          </div>
          <div>
            <p className="sb-label">Brand ID</p>
            <SellerInput value={form.brandId} onChange={(event) => setForm((value) => ({ ...value, brandId: event.target.value }))} placeholder="Optional" />
          </div>
          <div className="lg:col-span-2">
            <p className="sb-label">Description</p>
            <SellerTextArea value={form.description} onChange={(event) => setForm((value) => ({ ...value, description: event.target.value }))} />
          </div>
          <div className="lg:col-span-2">
            <p className="sb-label">Primary image URL</p>
            <SellerInput value={form.imageUrl} onChange={(event) => setForm((value) => ({ ...value, imageUrl: event.target.value }))} placeholder="https://..." />
          </div>
        </div>

        <div className="mt-8 grid gap-4 lg:grid-cols-3">
          <div>
            <p className="sb-label">Variant SKU</p>
            <SellerInput value={form.variant.sku} onChange={(event) => setForm((value) => ({ ...value, variant: { ...value.variant, sku: event.target.value } }))} />
          </div>
          <div>
            <p className="sb-label">Variant price</p>
            <SellerInput type="number" min="0" step="0.01" value={form.variant.price} onChange={(event) => setForm((value) => ({ ...value, variant: { ...value.variant, price: event.target.value } }))} />
          </div>
          <div>
            <p className="sb-label">Stock quantity</p>
            <SellerInput type="number" min="0" step="1" value={form.variant.stockQuantity} onChange={(event) => setForm((value) => ({ ...value, variant: { ...value.variant, stockQuantity: event.target.value } }))} />
          </div>
        </div>

        <div className="mt-6 grid gap-3 md:grid-cols-2">
          <label className="flex items-center gap-3 rounded-2xl bg-slate-50 px-4 py-3 text-sm font-medium text-slate-700">
            <input type="checkbox" checked={form.isActive} onChange={(event) => setForm((value) => ({ ...value, isActive: event.target.checked }))} />
            Product is active
          </label>
          <label className="flex items-center gap-3 rounded-2xl bg-slate-50 px-4 py-3 text-sm font-medium text-slate-700">
            <input type="checkbox" checked={form.variant.isActive} onChange={(event) => setForm((value) => ({ ...value, variant: { ...value.variant, isActive: event.target.checked } }))} />
            Variant is active
          </label>
        </div>

        {error ? <p className="mt-6 text-sm font-medium text-rose-600">{error}</p> : null}

        <div className="mt-8 flex flex-wrap gap-3">
          <SellerButton onClick={() => void save()} disabled={saving || !form.name.trim() || !form.categoryId}>{saving ? "Saving..." : mode === "create" ? "Create product" : "Update product"}</SellerButton>
          <SellerButton variant="secondary" onClick={() => router.push("/seller/products")}>Cancel</SellerButton>
        </div>
      </SellerCard>
    </div>
  );
}
