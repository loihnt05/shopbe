import { Heart, Star } from "lucide-react";

interface ProductPreviewCardProps {
  image: string;
  name: string;
  price: string;
  discount: string;
  rating: string;
}

export function ProductPreviewCard({ image, name, price, discount, rating }: ProductPreviewCardProps) {
  return (
    <article className="rounded-2xl border border-slate-200 bg-white p-3 shadow-sm">
      <div className="relative overflow-hidden rounded-xl">
        <img src={image} alt={name} className="h-36 w-full object-cover" />
        <span className="absolute left-2 top-2 rounded-md bg-[#EE4D2D] px-2 py-0.5 text-xs font-semibold text-white">{discount}</span>
        <button className="absolute right-2 top-2 rounded-full bg-white/90 p-1.5 text-rose-500"><Heart size={14} fill="currentColor" /></button>
      </div>
      <p className="mt-3 line-clamp-2 text-sm font-medium text-slate-700">{name}</p>
      <div className="mt-2 flex items-center justify-between">
        <span className="font-semibold text-[#EE4D2D]">{price}</span>
        <span className="flex items-center gap-1 text-xs text-amber-500"><Star size={12} fill="currentColor" />{rating}</span>
      </div>
      <button className="mt-3 w-full rounded-lg bg-slate-900 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800">Add to cart</button>
    </article>
  );
}
