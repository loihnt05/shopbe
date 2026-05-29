"use client";

import React, { createContext, useContext, useEffect, useState, ReactNode, useRef, useCallback } from "react";
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
  addItem: (productId: string, productVariantId: string, quantity: number) => Promise<void>;
  updateQuantity: (productVariantId: string, quantity: number) => Promise<void>;
  removeItem: (productVariantId: string) => Promise<void>;
  applyCoupon: (code: string) => Promise<void>;
  removeCoupon: () => Promise<void>;
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

  const refreshCart = useCallback(async () => {
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
  }, [session?.accessToken]);

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

  const applyCoupon = async (code: string) => {
    if (!session?.accessToken) return;
    try {
      setLoading(true);
      const data = await shopbeApi.cart.applyCoupon(session.accessToken, code);
      setCart(data);
      setTotalQuantity(data.totalQuantity ?? 0);
      setDisplayQuantity(data.displayQuantity ?? "");
    } catch (e) {
      console.error("Failed to apply coupon", e);
      throw e;
    } finally {
      setLoading(false);
    }
  };

  const removeCoupon = async () => {
    if (!session?.accessToken) return;
    try {
      setLoading(true);
      const data = await shopbeApi.cart.removeCoupon(session.accessToken);
      setCart(data);
      setTotalQuantity(data.totalQuantity ?? 0);
      setDisplayQuantity(data.displayQuantity ?? "");
    } catch (e) {
      console.error("Failed to remove coupon", e);
      throw e;
    } finally {
      setLoading(false);
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
  }, [session?.accessToken, status, refreshCart]);

  const openDrawer = () => {
    setIsDrawerOpen(true);
    refreshCart();
  };
  const closeDrawer = () => setIsDrawerOpen(false);

  const addItem = async (productId: string, productVariantId: string, quantity: number) => {
    if (!session?.accessToken) return;
    try {
      setLoading(true);
      const data = await shopbeApi.cart.addItem(session.accessToken, {
        productId,
        productVariantId,
        quantity,
      });
      setCart(data);
      setTotalQuantity(data.totalQuantity ?? 0);
      setDisplayQuantity(data.displayQuantity ?? "");
      openDrawer();
    } catch (e) {
      console.error("Failed to add item to cart", e);
      throw e;
    } finally {
      setLoading(false);
    }
  };

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
        addItem,
        updateQuantity,
        removeItem,
        applyCoupon,
        removeCoupon,
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
