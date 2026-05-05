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
      <div className="aspect-[1/1] bg-gradient-to-br from-slate-50 to-slate-100 grid place-items-center">
        {product.thumbnailUrl ? (
          <Image
            src={product.thumbnailUrl}
            alt={product.name}
            width={400}
            height={400}
            className="h-full w-full object-cover"
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
            <span className="sb-badge bg-[color:rgba(238,77,45,0.10)] text-[var(--brand)]">
              Sale
            </span>
          ) : (
            <span className="sb-badge bg-slate-100 text-slate-600">New</span>
          )}
        </div>
      </div>
    </Link>
  );
}


