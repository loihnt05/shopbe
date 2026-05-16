"use client";

import { useSession } from "next-auth/react";

export default function UserProfile() {
  const { data: session } = useSession();

  if (!session) {
    return <div>Please sign in to view your profile.</div>;
  }

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-2xl font-bold mb-4">User Profile</h1>
      <div className="bg-white shadow-md rounded-lg p-6">
        <div className="mb-4">
          <label className="block text-gray-700 font-bold mb-2">Name</label>
          <p className="text-gray-900">{session.user?.name}</p>
        </div>
        <div className="mb-4">
          <label className="block text-gray-700 font-bold mb-2">Email</label>
          <p className="text-gray-900">{session.user?.email}</p>
        </div>
      </div>
    </div>
  );
}

