"use client"

import { signIn, signOut, useSession } from "next-auth/react"

export default function Home() {
  const { data: session } = useSession();

  if (!session) {
    return (
      <button onClick={() => signIn("keycloak")}>
        Login
      </button>
    )
  }

  return (
    <>
      <p>Welcome {session.user?.name}</p>
      <button onClick={() => signOut()}>
        Logout
      </button>
    </>
  )
}