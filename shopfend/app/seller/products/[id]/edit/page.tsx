"use client";

import { use } from "react";
import SellerProductEditor from "@/app/seller/components/SellerProductEditor";

export default function SellerEditProductPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  return <SellerProductEditor mode="edit" productId={id} />;
}
