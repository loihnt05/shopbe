"use client";

import React, { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { useCart } from "./CartContext";
import { formatMoney } from "@/lib/format";
import { resolveApiUrl, shopbeApi, type CouponResponseDto } from "@/lib/shopbeApi";

export default function CartDrawer() {
  const { cart, isDrawerOpen, closeDrawer, updateQuantity, removeItem, applyCoupon, removeCoupon } = useCart();
  const [coupons, setCoupons] = useState<CouponResponseDto[]>([]);

  useEffect(() => {
    if (isDrawerOpen) {
      shopbeApi.coupons.list()
        .then(setCoupons)
        .catch(() => {});
    }
  }, [isDrawerOpen]);

  if (!isDrawerOpen) return null;

  const items = cart?.items ?? [];
  const totalCount = cart?.totalQuantity ?? 0;
  const subtotal = cart?.subtotal ?? 0;
  const discountAmount = cart?.discountAmount ?? 0;
  const totalAmount = cart?.total ?? subtotal;
  const currency = cart?.currency ?? "USD";
  const isEmpty = items.length === 0;

  const handleCouponChange = async (e: React.ChangeEvent<HTMLSelectElement>) => {
    const code = e.target.value;
    if (!code) {
      await removeCoupon();
    } else {
      try {
        await applyCoupon(code);
      } catch {
        alert("Could not apply coupon. Check conditions.");
      }
    }
  };

  return (
    <div
      style={{
        position: "fixed",
        inset: 0,
        zIndex: 100,
        display: "flex",
        justifyContent: "flex-end",
      }}
    >
      <div
        onClick={closeDrawer}
        style={{
          position: "absolute",
          inset: 0,
          background: "rgba(0,0,0,0.55)",
          backdropFilter: "blur(2px)",
        }}
      />
      <div
        style={{
          position: "relative",
          width: 380,
          maxWidth: "95vw",
          height: "100%",
          background: "#0e0e11",
          borderLeft: "1px solid #2a2a30",
          display: "flex",
          flexDirection: "column",
          fontFamily: "var(--font-dm-sans), sans-serif",
          animation: "slideIn 0.28s cubic-bezier(0.22,1,0.36,1)",
        }}
      >
        {/* Header */}
        <div
          style={{
            padding: "20px 24px 16px",
            borderBottom: "1px solid #1e1e24",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <div>
            <div style={{ fontSize: 11, letterSpacing: "0.12em", color: "#666", textTransform: "uppercase", marginBottom: 4 }}>
              Your Cart
            </div>
            <div style={{ fontSize: 22, fontWeight: 700, color: "#f0f0f0", letterSpacing: "-0.03em" }}>
              {totalCount} items
            </div>
          </div>
          <button
            onClick={closeDrawer}
            style={{
              width: 36,
              height: 36,
              borderRadius: 8,
              background: "#1a1a20",
              border: "1px solid #2a2a30",
              color: "#888",
              cursor: "pointer",
              fontSize: 18,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            ×
          </button>
        </div>

        {/* Items */}
        <div style={{ flex: 1, overflowY: "auto", padding: "16px 24px" }}>
          {isEmpty ? (
            <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", height: 200, color: "#444", gap: 12 }}>
              <div style={{ fontSize: 40 }}>🛒</div>
              <div style={{ fontSize: 14 }}>Your cart is empty</div>
            </div>
          ) : (
            items.map((item) => (
              <div
                key={item.productVariantId}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 14,
                  padding: "14px 0",
                  borderBottom: "1px solid #1a1a20",
                }}
              >
                <div
                  style={{
                    width: 44,
                    height: 44,
                    borderRadius: 10,
                    background: "#1a1a20",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontSize: 22,
                    flexShrink: 0,
                    overflow: "hidden",
                    position: "relative"
                  }}
                >
                  {item.imageUrl ? (
                    <Image
                      src={resolveApiUrl(item.imageUrl) || ""}
                      alt={item.productName ?? "Item"}
                      fill
                      className="object-cover"
                      unoptimized
                    />
                  ) : (
                    "📦"
                  )}
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontSize: 13, fontWeight: 600, color: "#e0e0e0", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                    {item.productName}
                  </div>
                  <div style={{ fontSize: 12, color: "#555", marginTop: 2, fontFamily: "var(--font-dm-mono), monospace" }}>
                    {formatMoney(item.unitPrice, currency)}
                  </div>
                </div>
                <div style={{ display: "flex", alignItems: "center", gap: 6 }}>
                  <button
                    onClick={() => updateQuantity(item.productVariantId, item.quantity - 1)}
                    style={{ width: 26, height: 26, borderRadius: 6, background: "#1a1a20", border: "1px solid #2a2a30", color: "#888", cursor: "pointer", fontSize: 14, display: "flex", alignItems: "center", justifyContent: "center" }}
                  >
                    −
                  </button>
                  <span style={{ fontSize: 13, fontWeight: 500, color: "#f0f0f0", minWidth: 18, textAlign: "center", fontFamily: "var(--font-dm-mono), monospace" }}>
                    {item.quantity}
                  </span>
                  <button
                    onClick={() => updateQuantity(item.productVariantId, item.quantity + 1)}
                    style={{ width: 26, height: 26, borderRadius: 6, background: "#1a1a20", border: "1px solid #2a2a30", color: "#888", cursor: "pointer", fontSize: 14, display: "flex", alignItems: "center", justifyContent: "center" }}
                  >
                    +
                  </button>
                </div>
                <button
                  onClick={() => removeItem(item.productVariantId)}
                  style={{ width: 26, height: 26, borderRadius: 6, background: "transparent", border: "none", color: "#444", cursor: "pointer", fontSize: 14, display: "flex", alignItems: "center", justifyContent: "center" }}
                >
                  ✕
                </button>
              </div>
            ))
          )}
        </div>

        {/* Coupons */}
        {!isEmpty && (
          <div style={{ padding: "12px 24px", borderTop: "1px solid #1e1e24", background: "#15151a" }}>
            <div style={{ fontSize: 11, color: "#666", textTransform: "uppercase", marginBottom: 8, letterSpacing: "0.05em" }}>
              Available Vouchers
            </div>
            <select
              value={cart?.couponCode ?? ""}
              onChange={handleCouponChange}
              style={{
                width: "100%",
                padding: "10px 12px",
                borderRadius: 8,
                background: "#1a1a20",
                border: "1px solid #2a2a30",
                color: "#e0e0e0",
                fontSize: 13,
                cursor: "pointer",
                outline: "none"
              }}
            >
              <option value="">Select a coupon...</option>
              {coupons.filter(c => c.isActive).map(c => {
                const isDisabled = subtotal < c.minOrderAmount || c.count <= 0;
                return (
                  <option 
                    key={c.id} 
                    value={c.code}
                    disabled={isDisabled}
                  >
                    {c.code} - {c.description ?? `${c.value}${c.discountType === 'Percentage' ? '%' : ''} off`}
                    {c.count <= 0 ? ' (Exhausted)' : ` (${c.count} left)`}
                    {subtotal < c.minOrderAmount ? ` (Min. ${formatMoney(c.minOrderAmount, currency)})` : ''}
                  </option>
                );
              })}
            </select>
          </div>
        )}

        {/* Footer */}
        {!isEmpty && (
          <div style={{ padding: "16px 24px 28px", borderTop: "1px solid #1e1e24" }}>
            <div style={{ display: "flex", flexDirection: "column", gap: 8, marginBottom: 16 }}>
              <div style={{ display: "flex", justifyContent: "space-between" }}>
                <span style={{ fontSize: 13, color: "#666" }}>Subtotal</span>
                <span style={{ fontSize: 13, color: "#888", fontFamily: "var(--font-dm-mono), monospace" }}>
                  {formatMoney(subtotal, currency)}
                </span>
              </div>
              
              {discountAmount > 0 && (
                <div style={{ display: "flex", justifyContent: "space-between", color: "#4ade80" }}>
                  <span style={{ fontSize: 13 }}>Discount</span>
                  <span style={{ fontSize: 13, fontFamily: "var(--font-dm-mono), monospace" }}>
                    -{formatMoney(discountAmount, currency)}
                  </span>
                </div>
              )}

              <div style={{ display: "flex", justifyContent: "space-between", marginTop: 4, paddingTop: 12, borderTop: "1px solid #1a1a20" }}>
                <span style={{ fontSize: 14, fontWeight: 600, color: "#f0f0f0" }}>Total</span>
                <span style={{ fontSize: 20, fontWeight: 500, color: "#f0f0f0", fontFamily: "var(--font-dm-mono), monospace", letterSpacing: "-0.03em" }}>
                  {formatMoney(totalAmount, currency)}
                </span>
              </div>
            </div>
            <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
              <Link
                href="/cart"
                onClick={closeDrawer}
                style={{
                  display: "block",
                  textAlign: "center",
                  width: "100%",
                  padding: "12px",
                  borderRadius: 10,
                  background: "transparent",
                  border: "1px solid #2a2a30",
                  color: "#e0e0e0",
                  fontSize: 13,
                  fontWeight: 600,
                  cursor: "pointer",
                  letterSpacing: "-0.01em",
                  fontFamily: "var(--font-dm-sans), sans-serif",
                  textDecoration: "none"
                }}
              >
                View Full Cart
              </Link>
              <Link
                href="/checkout"
                onClick={closeDrawer}
                style={{
                  display: "block",
                  textAlign: "center",
                  width: "100%",
                  padding: "14px",
                  borderRadius: 10,
                  background: "#e5e5e5",
                  border: "none",
                  color: "#0e0e11",
                  fontSize: 14,
                  fontWeight: 700,
                  cursor: "pointer",
                  letterSpacing: "-0.01em",
                  fontFamily: "var(--font-dm-sans), sans-serif",
                  textDecoration: "none"
                }}
              >
                Checkout →
              </Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
