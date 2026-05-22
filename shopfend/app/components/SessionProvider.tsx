"use client";

import { SessionProvider as NextAuthSessionProvider } from "next-auth/react";
import { ReactNode } from "react";
import { CartProvider } from "./CartContext";
import { Session } from "next-auth";

interface Props {
  children: ReactNode;
  session?: Session | null;
}

export default function SessionProvider({ children, session }: Props) {
  return (
    <NextAuthSessionProvider session={session}>
      <CartProvider>
      {children}
    </CartProvider>
    </NextAuthSessionProvider>
  );
}