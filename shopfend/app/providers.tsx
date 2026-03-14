"use client";

import type { ReactNode } from "react";
import { SessionProvider } from "next-auth/react";
import { Session } from "inspector/promises";

type ProvidersProps = {
  children: ReactNode;
  session?: Session | null;
};

export default function Providers({ children, session }: ProvidersProps) {
  return <SessionProvider session={session}>{children}</SessionProvider>;
}
