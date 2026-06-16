import { useSession, signIn } from "next-auth/react";
import { useState, useEffect } from "react";
import Link from "next/link";
import { resolveApiUrl, shopbeApi, type ProductListItem, type WishlistItem } from "@/lib/shopbeApi";
import { formatMoney } from "@/lib/format";
import { StarIcon } from "./icons";

export default function ProductCard({ product }: { product: ProductListItem }) {
  const { data: session } = useSession();
  const [isWishlisted, setIsWishlisted] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (session?.accessToken) {
      // Small optimization: we could fetch the whole wishlist once in a context,
      // but for now let's keep it simple.
      shopbeApi.wishlist.get(session.accessToken).then((items: WishlistItem[]) => {
        setIsWishlisted(items.some((i) => i.productId === product.id));
      }).catch(() => {});
    }
  }, [session?.accessToken, product.id]);

  const toggleWishlist = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    if (!session) {
      signIn("keycloak");
      return;
    }

    setBusy(true);
    try {
      const token = session?.accessToken;
      if (!token) return;

      if (isWishlisted) {
        await shopbeApi.wishlist.remove(token, product.id);
        setIsWishlisted(false);
      } else {
        await shopbeApi.wishlist.add(token, product.id);
        setIsWishlisted(true);
      }
    } catch (error) {
      console.error("Failed to toggle wishlist", error);
    } finally {
      setBusy(false);
    }
  };

  const hasDiscount = product.discountPrice != null && product.price != null && product.discountPrice < product.price;
  const displayPrice = hasDiscount ? product.discountPrice : product.price;
  const originalPrice = hasDiscount ? product.price : null;
  
  const discountPercentage = hasDiscount 
    ? Math.round((1 - (product.discountPrice! / product.price!)) * 100) 
    : 0;

  const thumbnailSrc = resolveApiUrl(
    product.primaryImageUrl ?? product.images?.find((image) => image.isPrimary)?.imageUrl ?? product.images?.[0]?.imageUrl ?? ""
  );

  return (
    <Link
      href={`/products/${product.id}`}
      className="group bg-white rounded-xl overflow-hidden hover:scale-[1.02] hover:shadow-xl transition-all duration-300 border border-gray-100 flex flex-col h-full"
    >
      <div className="relative aspect-square w-full bg-slate-50 grid place-items-center shrink-0">
        {thumbnailSrc ? (
          // Using a plain img here avoids Next/Image rendering issues with backend-hosted URLs.
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={thumbnailSrc}
            alt={product.name}
            loading="lazy"
            className="absolute inset-0 h-full w-full object-cover transition-transform duration-500 group-hover:scale-110"
          />
        ) : (
          <div className="text-slate-400 text-xs">No image</div>
        )}
        
        {product.recommendationReason && (
          <div className="absolute top-2 left-2 bg-brand/90 text-white text-[8px] font-bold px-2 py-1 rounded-md shadow-sm z-10 uppercase tracking-tighter">
            {product.recommendationReason}
          </div>
        )}

        {hasDiscount && (
          <div className="absolute top-2 right-2 bg-yellow-300 text-brand text-[10px] font-bold px-2 py-1 rounded-full shadow-sm flex items-center gap-0.5">
            <span>{discountPercentage}%</span>
            <span className="uppercase text-[8px] opacity-80">Off</span>
          </div>
        )}

        <button
          onClick={toggleWishlist}
          disabled={busy}
          className={`absolute bottom-2 right-2 p-1.5 rounded-full shadow-sm transition-all active:scale-90 ${
            isWishlisted 
            ? 'bg-brand text-white' 
            : 'bg-white/80 backdrop-blur-sm text-gray-400 hover:text-brand'
          }`}
          title={isWishlisted ? "Remove from wishlist" : "Add to wishlist"}
        >
          <svg className={`w-4 h-4 ${isWishlisted ? 'fill-current' : 'fill-none'}`} stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
          </svg>
        </button>
      </div>

      <div className="p-3 flex flex-col flex-1">
        {product.categoryName && (
          <div className="text-[10px] text-brand/70 font-bold uppercase tracking-wider mb-1">
            {product.categoryName}
          </div>
        )}
        <div className="line-clamp-2 text-sm text-gray-800 mb-2 min-h-[40px] leading-snug group-hover:text-brand transition-colors">
          {product.name}
        </div>

        <div className="mt-auto flex flex-col gap-1">
          <div className="flex items-center gap-2">
            <div className="text-brand text-lg font-semibold">
              {formatMoney(displayPrice, product.currency)}
            </div>
            {originalPrice && (
              <div className="text-xs text-gray-400 line-through">
                {formatMoney(originalPrice, product.currency)}
              </div>
            )}
          </div>
          
          <div className="flex items-center justify-between mt-1">
            <div className="flex items-center gap-1">
              {product.averageRating && product.averageRating > 0 ? (
                <>
                  <div className="flex items-center gap-0.5 text-yellow-500">
                    <StarIcon className="w-3 h-3 fill-current" />
                    <span className="text-[11px] font-bold">{product.averageRating.toFixed(1)}</span>
                  </div>
                  <span className="text-[10px] text-gray-300">|</span>
                </>
              ) : null}
              <div className="text-[10px] text-gray-400 font-medium">
                {product.soldCount && product.soldCount > 0 ? (
                  <>
                    {product.soldCount >= 1000 
                      ? `${(product.soldCount / 1000).toFixed(1)}k` 
                      : product.soldCount} sold
                  </>
                ) : (
                  "0 sold"
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </Link>
  );
}
