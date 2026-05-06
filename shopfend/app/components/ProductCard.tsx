import Link from "next/link";
import Image from "next/image";
import type { ProductListItem } from "../../lib/shopbeApi";
import { formatCompactMoney } from "../../lib/format";

export default function ProductCard({ product }: { product: ProductListItem }) {
  const price = product.discountPrice ?? product.price;
  const hasDiscount = product.discountPrice != null && product.price != null;

  return (
    <Link
      href={`/products/${product.id}`}
      className="group sb-card overflow-hidden hover:shadow-md transition-shadow"
    >
      <div className="relative aspect-[1/1] bg-gradient-to-br from-slate-50 to-slate-100 grid place-items-center">
        {product.thumbnailUrl ? (
          <Image
            src={product.thumbnailUrl}
            alt={product.name}
            fill
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 20vw"
            className="object-cover"
            unoptimized
          />
        ) : (
          <div className="text-slate-400 text-xs">No image</div>
        )}
      </div>

      <div className="p-3 space-y-2">
        <div className="line-clamp-2 text-sm text-slate-900 group-hover:text-[var(--brand)]">
          {product.name}
        </div>

        {product.description ? (
          <div className="line-clamp-2 text-xs text-slate-500">
            {product.description}
          </div>
        ) : null}

        <div className="flex items-end justify-between gap-2">
          <div>
            <div className="text-[var(--brand)] font-semibold">
              {formatCompactMoney(price, product.currency)}
            </div>
            {hasDiscount ? (
              <div className="text-xs text-slate-400 line-through">
                {formatCompactMoney(product.price, product.currency)}
              </div>
            ) : null}
          </div>

          {hasDiscount ? (
            <span className="sb-badge sb-badge-brand">
              Sale
            </span>
          ) : (
            <span className="sb-badge sb-badge-muted">New</span>
          )}
        </div>
      </div>
    </Link>
  );
}
