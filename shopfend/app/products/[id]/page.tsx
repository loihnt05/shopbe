"use client";

import { useSession, signIn } from "next-auth/react";
import { use, useMemo, useEffect, useState } from "react";
import { isAbortError, shopbeApi, type ProductDetail } from "../../../lib/shopbeApi";
import Link from "next/link";

export default function ProductDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: session } = useSession();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const abort = useMemo(() => new AbortController(), []);

  useEffect(() => {
    (async () => {
      try {
        setError(null);
        const data = await shopbeApi.products.getById(id, abort.signal);
        setProduct(data);
      } catch (e: any) {
        if (isAbortError(e)) return;
        setError(e?.message ?? "Failed to load product");
      }
    })();

    return () => abort.abort();
  }, [id, abort]);

  const primaryVariantId = product?.variants?.[0]?.id;

  const addToCart = async () => {
    setMessage(null);
    setError(null);

    if (!session?.accessToken) {
      setError("Please sign in to add items to cart.");
      return;
    }
    if (!primaryVariantId) {
      setError(
        "This product has no variants in the database, so it cannot be added to cart."
      );
      return;
    }

    try {
      setBusy(true);
      await shopbeApi.cart.addItem(
        session.accessToken,
        { productVariantId: primaryVariantId, quantity },
        undefined
      );
      setMessage("Added to cart.");
    } catch (e: any) {
      setError(e?.message ?? "Failed to add to cart");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Product</h1>
        <Link href="/products" className="text-sm underline">
          Back
        </Link>
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 p-3 rounded text-sm">
          {error}
        </div>
      )}
      {message && (
        <div className="border border-emerald-300 bg-emerald-50 p-3 rounded text-sm">
          {message}
        </div>
      )}

      {!product ? (
        <p>Loading…</p>
      ) : (
        <div className="border rounded p-4 space-y-3">
          <div>
            <div className="text-xl font-medium">{product.name}</div>
            {product.description ? (
              <p className="text-sm opacity-80 mt-1">{product.description}</p>
            ) : null}
          </div>

          <div className="text-sm">
            <div className="opacity-70">Variant used for test add-to-cart:</div>
            {primaryVariantId ? (
              <div className="font-mono break-all">{primaryVariantId}</div>
            ) : (
              <div className="text-red-600">No variants found</div>
            )}
          </div>

          <div className="flex items-center gap-3">
            <label className="text-sm">Qty</label>
            <input
              type="number"
              min={1}
              value={quantity}
              onChange={(e) => setQuantity(Math.max(1, Number(e.target.value)))}
              className="border rounded px-2 py-1 w-20"
            />

            {session ? (
              <button
                disabled={busy}
                onClick={addToCart}
                className="bg-slate-900 disabled:opacity-50 text-white px-4 py-2 rounded"
              >
                {busy ? "Adding…" : "Add to cart"}
              </button>
            ) : (
              <button
                onClick={() => signIn("keycloak")}
                className="bg-blue-600 text-white px-4 py-2 rounded"
              >
                Sign in to buy
              </button>
            )}

            <Link href="/cart" className="underline text-sm">
              Go to cart
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}


