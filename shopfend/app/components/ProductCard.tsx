import Link from "next/link";
import Image from "next/image";
import type { ProductListItem } from "@/lib/shopbeApi";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/+$/, "") ||
  "http://localhost:5072";

function resolveImageSrc(value?: string | null): string | undefined {
  if (!value) return undefined;
  if (/^https?:\/\//i.test(value)) return value;

  try {
    return new URL(value, API_BASE_URL).toString();
  } catch {
    return value;
  }
}

export default function ProductCard({ product }: { product: ProductListItem }) {
  const hasDiscount = product.discountPrice != null && product.price != null;
  const thumbnailSrc = resolveImageSrc(product.primaryImageUrl ?? product.thumbnailUrl);

  return (
    <Link
      href={`/products/${product.id}`}
      className="group bg-white rounded-xl overflow-hidden hover:scale-[1.02] hover:shadow-xl transition-all duration-300 border border-gray-100 flex flex-col h-full"
    >
      <div className="relative aspect-square w-full bg-slate-50 grid place-items-center shrink-0">
        {thumbnailSrc ? (
          <Image
            src={thumbnailSrc}
            alt={product.name}
            fill
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 20vw"
            className="object-cover transition-transform duration-500 group-hover:scale-110"
            unoptimized
          />
        ) : (
          <div className="text-slate-400 text-xs">No image</div>
        )}
        {hasDiscount && (
          <div className="absolute top-2 right-2 bg-yellow-300 text-brand text-[10px] font-bold px-2 py-1 rounded-full shadow-sm flex items-center gap-0.5">
            <span>10%</span>
            <span className="uppercase text-[8px] opacity-80">Off</span>
          </div>
        )}
      </div>

      <div className="p-3 flex flex-col flex-1">
        <div className="line-clamp-2 text-sm text-gray-800 mb-2 min-h-[40px] leading-snug group-hover:text-brand transition-colors">
          {product.name}
        </div>

        <div className="mt-auto flex items-center justify-between">
          <div className="text-brand text-lg font-semibold flex items-baseline">
            <span className="text-xs mr-0.5 font-normal">₫</span>
            <span>{product.price ? product.price.toLocaleString() : "0"}</span>
          </div>
          <div className="text-[10px] text-gray-400 ml-2 font-medium">
            1.2k sold
          </div>
        </div>
      </div>
    </Link>
  );
}