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
      className="group bg-white rounded-sm overflow-hidden hover:-translate-y-[1px] hover:shadow-[0_1px_2.5px_0_rgba(0,0,0,0.15)] transition-all duration-100 border border-transparent hover:border-[#ee4d2d] flex flex-col h-full"
    >
      <div className="relative aspect-square w-full bg-gray-100 grid place-items-center shrink-0">
        {thumbnailSrc ? (
          <Image
            src={thumbnailSrc}
            alt={product.name}
            fill
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 20vw"
            className="object-cover"
            unoptimized
          />
        ) : (
          <div className="text-slate-400 text-xs">No image</div>
        )}
        {hasDiscount && (
          <div className="absolute top-0 right-0 bg-[#ffea5f] text-[#ee4d2d] text-[10px] font-bold px-1 py-1 flex flex-col items-center leading-none">
            <span>10%</span>
            <span className="font-normal text-[9px] uppercase text-white bg-[#ee4d2d] px-1 mt-0.5">Off</span>
          </div>
        )}
      </div>

      <div className="p-2 flex flex-col flex-1">
        <div className="line-clamp-2 text-xs text-[#000000cc] mb-1 min-h-[32px] leading-tight">
          {product.name}
        </div>

        <div className="mt-auto flex items-center justify-between">
          <div className="text-[#ee4d2d] text-base truncate flex items-baseline">
            <span className="text-[10px] mr-0.5">₫</span>
            <span>{product.price ? product.price.toLocaleString() : "0"}</span>
          </div>
          <div className="text-[10px] text-black/50 ml-2 truncate">
            1.2k sold
          </div>
        </div>
      </div>
    </Link>
  );
}