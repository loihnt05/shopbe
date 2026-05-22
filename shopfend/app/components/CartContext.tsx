"use client";

import React, { createContext, useContext, useEffect, useState, ReactNode, useRef } from "react";
import { useSession } from "next-auth/react";
import { shopbeApi, type CartDto, isAbortError } from "@/lib/shopbeApi";

interface CartContextType {
  cart: CartDto | null;
  totalQuantity: number;
  displayQuantity: string;
  isDrawerOpen: boolean;
  loading: boolean;
  refreshCart: () => Promise<void>;
  openDrawer: () => void;
  closeDrawer: () => void;
  updateQuantity: (productVariantId: string, quantity: number) => Promise<void>;
  removeItem: (productVariantId: string) => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export function CartProvider({ children }: { children: ReactNode }) {
  const { data: session, status } = useSession();
  const [cart, setCart] = useState<CartDto | null>(null);
  const [totalQuantity, setTotalQuantity] = useState(0);
  const [displayQuantity, setDisplayQuantity] = useState("");
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  const abortRef = useRef<AbortController | null>(null);

  const getSignal = () => {
    abortRef.current?.abort();
    abortRef.current = new AbortController();
    return abortRef.current.signal;
  };

  const refreshCart = async () => {
    if (!session?.accessToken) {
      setCart(null);
      setTotalQuantity(0);
      setDisplayQuantity("");
      return;
    }
    try {
      setLoading(true);
      const data = await shopbeApi.cart.getMyCart(session.accessToken, getSignal());
      setCart(data);
      setTotalQuantity(data.totalQuantity ?? 0);
      setDisplayQuantity(data.displayQuantity ?? "");
    } catch (e) {
      if (isAbortError(e)) return;
      console.error("Failed to refresh cart", e);
    } finally {
      setLoading(false);
    }
  };

  const updateQuantity = async (productVariantId: string, quantity: number) => {
    if (!session?.accessToken) return;
    try {
      const data = await shopbeApi.cart.setQuantity(
        session.accessToken,
        productVariantId,
        { quantity }
      );
      setCart(data);
      setTotalQuantity(data.totalQuantity ?? 0);
      setDisplayQuantity(data.displayQuantity ?? "");
    } catch (e) {
      console.error("Failed to update quantity", e);
    }
  };

  const removeItem = async (productVariantId: string) => {
    if (!session?.accessToken) return;
    try {
      const data = await shopbeApi.cart.removeItem(session.accessToken, productVariantId);
      setCart(data);
      setTotalQuantity(data.totalQuantity ?? 0);
      setDisplayQuantity(data.displayQuantity ?? "");
    } catch (e) {
      console.error("Failed to remove item", e);
    }
  };

  useEffect(() => {
    if (status === "authenticated") {
      refreshCart();
    } else if (status === "unauthenticated") {
      setCart(null);
      setTotalQuantity(0);
      setDisplayQuantity("");
    }
    return () => abortRef.current?.abort();
  }, [session?.accessToken, status]);

  const openDrawer = () => setIsDrawerOpen(true);
  const closeDrawer = () => setIsDrawerOpen(false);

  return (
    <CartContext.Provider
      value={{
        cart,
        totalQuantity,
        displayQuantity,
        isDrawerOpen,
        loading,
        refreshCart,
        openDrawer,
        closeDrawer,
        updateQuantity,
        removeItem,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error("useCart must be used within a CartProvider");
  }
  return context;
}
