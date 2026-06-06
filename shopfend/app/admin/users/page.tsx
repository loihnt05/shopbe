"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { AdminUserDto, BackendPagedResult, shopbeApi } from "@/lib/shopbeApi";
import { AdminBadge, AdminButton, AdminCard, AdminEmptyState, AdminErrorState, AdminInput, AdminLoadingState, AdminPageIntro, AdminSelect, AdminToolbar, formatAdminDate } from "../components/AdminUi";

type UserFilters = {
  search: string;
  role: string;
  status: string;
};

export default function AdminUsersPage() {
  const { data: session, status } = useSession();
  const [filters, setFilters] = useState<UserFilters>({ search: "", role: "", status: "" });
  const [result, setResult] = useState<BackendPagedResult<AdminUserDto> | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = async (accessToken: string, signal?: AbortSignal) => {
    const query: Record<string, string | undefined> = {
      searchTerm: filters.search || undefined,
      role: filters.role || undefined,
      status: filters.status || undefined,
    };
    return shopbeApi.admin.users(accessToken, query, signal);
  };

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;

    const controller = new AbortController();
    shopbeApi.admin.users(session.accessToken, {
      searchTerm: filters.search || undefined,
      role: filters.role || undefined,
      status: filters.status || undefined,
    }, controller.signal)
      .then((response) => {
        setResult(response);
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load users."));
      });
    return () => controller.abort();
  }, [filters.role, filters.search, filters.status, session?.accessToken, status]);

  const runAction = async (action: () => Promise<unknown>, fallback: string) => {
    if (!session?.accessToken) return;
    try {
      await action();
      const response = await load(session.accessToken);
      setResult(response);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, fallback));
    } finally {
      setBusyId(null);
    }
  };

  if (status === "loading" || (status === "authenticated" && !result && !error)) return <AdminLoadingState />;
  if (error && !result) return <AdminErrorState message={error} />;

  return (
    <div className="space-y-6">
      <AdminPageIntro title="User management" description="Search the marketplace user base and correct role or lifecycle status without leaving the dashboard." />

      <AdminCard>
        <AdminToolbar>
          <div className="grid flex-1 gap-3 md:grid-cols-3">
            <AdminInput placeholder="Search by name or email" value={filters.search} onChange={(event) => setFilters((value) => ({ ...value, search: event.target.value }))} />
            <AdminSelect value={filters.role} onChange={(event) => setFilters((value) => ({ ...value, role: event.target.value }))}>
              <option value="">All roles</option>
              <option value="Admin">Admin</option>
              <option value="Seller">Seller</option>
              <option value="Customer">Customer</option>
              <option value="Staff">Staff</option>
            </AdminSelect>
            <AdminSelect value={filters.status} onChange={(event) => setFilters((value) => ({ ...value, status: event.target.value }))}>
              <option value="">All statuses</option>
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
              <option value="Banned">Banned</option>
              <option value="PendingVerification">PendingVerification</option>
            </AdminSelect>
          </div>
          <p className="text-sm text-slate-500">{result?.totalCount ?? 0} users</p>
        </AdminToolbar>

        {error ? <p className="mt-4 text-sm font-medium text-rose-600">{error}</p> : null}

        {!result?.items.length ? (
          <div className="mt-6">
            <AdminEmptyState title="No users found" message="Adjust the filters or wait for more synced accounts to arrive." />
          </div>
        ) : (
          <div className="mt-6 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate-400">
                <tr>
                  <th className="pb-3 font-bold">Name</th>
                  <th className="pb-3 font-bold">Email</th>
                  <th className="pb-3 font-bold">Role</th>
                  <th className="pb-3 font-bold">Status</th>
                  <th className="pb-3 font-bold">Registered</th>
                  <th className="pb-3 font-bold">Actions</th>
                </tr>
              </thead>
              <tbody>
                {result.items.map((user) => (
                  <tr key={user.id} className="border-t border-slate-100 align-top">
                    <td className="py-4 font-medium text-slate-900">{user.fullName}</td>
                    <td className="py-4 text-slate-600">{user.email}</td>
                    <td className="py-4"><AdminBadge value={user.role} /></td>
                    <td className="py-4"><AdminBadge value={user.status} /></td>
                    <td className="py-4 text-slate-600">{formatAdminDate(user.createdAt)}</td>
                    <td className="py-4">
                      <div className="flex flex-wrap gap-2">
                        <AdminButton variant="secondary" disabled={busyId === user.id} onClick={() => {
                          setBusyId(user.id);
                          void runAction(() => shopbeApi.admin.updateUserRole(session!.accessToken!, user.id, user.role === "Seller" ? "Customer" : "Seller"), "Failed to update user role.");
                        }}>
                          {user.role === "Seller" ? "Make customer" : "Make seller"}
                        </AdminButton>
                        <AdminButton variant="secondary" disabled={busyId === user.id} onClick={() => {
                          setBusyId(user.id);
                          void runAction(() => shopbeApi.admin.updateUserStatus(session!.accessToken!, user.id, user.status === "Active" ? "Inactive" : "Active"), "Failed to update user status.");
                        }}>
                          {user.status === "Active" ? "Deactivate" : "Activate"}
                        </AdminButton>
                        <AdminButton variant="danger" disabled={busyId === user.id} onClick={() => {
                          if (!window.confirm(`Delete ${user.fullName}?`)) return;
                          setBusyId(user.id);
                          void runAction(() => shopbeApi.admin.deleteUser(session!.accessToken!, user.id), "Failed to delete user.");
                        }}>
                          Delete
                        </AdminButton>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </AdminCard>
    </div>
  );
}
