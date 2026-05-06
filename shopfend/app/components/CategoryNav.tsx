"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";

type Category = {
  label: string;
  /** When provided, category navigates to /products?q=... */
  q?: string;
};

const DEFAULT_CATEGORIES: Category[] = [
  { label: "All", q: "" },
  { label: "Phones & Accessories", q: "phone" },
  { label: "Electronics", q: "electronics" },
  { label: "Fashion", q: "fashion" },
  { label: "Home & Living", q: "home" },
  { label: "Beauty", q: "beauty" },
  { label: "Sports", q: "sports" },
  { label: "Toys", q: "toy" },
];

export default function CategoryNav({ categories = DEFAULT_CATEGORIES }: { categories?: Category[] }) {
  const searchParams = useSearchParams();
  const currentQ = (searchParams.get("q") ?? "").trim().toLowerCase();

  return (
    <div className="sb-category-strip">
      <div className="sb-container">
        <div className="flex items-center gap-2 overflow-x-auto py-2 sb-scrollbar-hide">
          {categories.map((c) => {
            const q = (c.q ?? "").trim();
            const href = q ? `/products?q=${encodeURIComponent(q)}` : "/products";
            const active = q ? currentQ === q.toLowerCase() : !currentQ;
            return (
              <Link
                key={c.label}
                href={href}
                className={active ? "sb-pill sb-pill-active" : "sb-pill"}
              >
                {c.label}
              </Link>
            );
          })}
        </div>
      </div>
    </div>
  );
}
