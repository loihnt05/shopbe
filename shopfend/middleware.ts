import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getToken } from "next-auth/jwt";
import type { JWT } from "next-auth/jwt";

const ADMIN_PREFIX = "/admin";
const SELLER_PREFIX = "/seller";

function hasRole(token: string | JWT | null, role: string) {
  const roles = typeof token === "object" && token && Array.isArray(token.roles)
    ? token.roles
    : [];
  return roles.some((value) => value.toLowerCase() === role.toLowerCase());
}

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const needsAdmin = pathname.startsWith(ADMIN_PREFIX);
  const needsSeller = pathname.startsWith(SELLER_PREFIX);

  if (!needsAdmin && !needsSeller) {
    return NextResponse.next();
  }

  const token = await getToken({ req: request, secret: process.env.NEXTAUTH_SECRET });
  if (!token) {
    const signInUrl = new URL("/api/auth/signin", request.url);
    signInUrl.searchParams.set("callbackUrl", request.nextUrl.pathname + request.nextUrl.search);
    return NextResponse.redirect(signInUrl);
  }

  if (needsAdmin && !hasRole(token, "Admin")) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  if (needsSeller && !hasRole(token, "Seller")) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*", "/seller/:path*"],
};
