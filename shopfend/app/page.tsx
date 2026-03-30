"use client";

import { useSession, signIn, signOut } from "next-auth/react";

export default function Home() {
  const { data: session } = useSession();

  const handleLogout = async () => {
    // 1) Clear NextAuth session
    await signOut({ redirect: false });

    // 2) Clear Keycloak SSO session (otherwise you will be auto-logged-in next time)
    const issuer = process.env.NEXT_PUBLIC_KEYCLOAK_ISSUER;
    if (!issuer) {
      // Fallback: at least return to home after NextAuth signout
      window.location.href = "/";
      return;
    }

    const postLogoutRedirectUri = window.location.origin;
    const url = new URL(`${issuer}/protocol/openid-connect/logout`);
    if (session?.idToken) {
      url.searchParams.set("id_token_hint", session.idToken);
    }
    url.searchParams.set("client_id", "shopfend");
    url.searchParams.set("post_logout_redirect_uri", postLogoutRedirectUri);
    window.location.href = url.toString();
  };

  return (
    <div className="p-6 text-center">
      {session ? (
        <>
          <h1 className="text-xl mb-4">Hello, {session.user?.name}</h1>
          <p>{session.user?.name}</p>
          <p>{session.user?.email}</p>
          <button
            onClick={handleLogout}
            className="bg-red-500 text-white px-4 py-2 rounded"
          >
            Sign Out
          </button>
        </>
      ) : (
        <button
          onClick={() => signIn("keycloak")}
          className="bg-blue-500 text-white px-4 py-2 rounded"
        >
          Sign In with Keycloak
        </button>
      )}
    </div>
  );
}