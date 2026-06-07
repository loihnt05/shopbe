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

const ROLE_OPTIONS = ["Admin", "Seller", "Customer", "Staff"];
const STATUS_OPTIONS = ["Active", "Inactive", "Banned", "PendingVerification"];

export default function AdminUsersPage() {
  const { data: session, status } = useSession();
  const [filters, setFilters] = useState<UserFilters>({ search: "", role: "", status: "" });
  const [result, setResult] = useState<BackendPagedResult<AdminUserDto> | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [selectedUser, setSelectedUser] = useState<AdminUserDto | null>(null);
  const [roleDrafts, setRoleDrafts] = useState<Record<string, string>>({});
  const [statusDrafts, setStatusDrafts] = useState<Record<string, string>>({});

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

  const openUserDetail = async (userId: string) => {
    if (!session?.accessToken) return;
    try {
      const user = await shopbeApi.admin.userById(session.accessToken, userId);
      setSelectedUser(user);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to load user details."));
    }
  };

  const draftRoleFor = (user: AdminUserDto) => roleDrafts[user.id] ?? user.role ?? "Customer";
  const draftStatusFor = (user: AdminUserDto) => statusDrafts[user.id] ?? user.status ?? "Active";

  if (status === "loading" || (status === "authenticated" && !result && !error)) return <AdminLoadingState />;
  if (error && !result) return <AdminErrorState message={error} />;

  return (
    <div className="space-y-6">
      <AdminPageIntro title="User management" description="Search the marketplace user base and correct role or lifecycle status without leaving the dashboard." />

      <div className="grid gap-4 md:grid-cols-4">
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Visible users</p><p className="mt-3 text-3xl font-black text-slate-950">{result?.items.length ?? 0}</p></AdminCard>
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">All matches</p><p className="mt-3 text-3xl font-black text-slate-950">{result?.totalCount ?? 0}</p></AdminCard>
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Admin tools</p><p className="mt-3 text-base font-bold text-slate-950">Role changes, status control, lifecycle cleanup</p></AdminCard>
        <AdminCard><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Scope</p><p className="mt-3 text-base font-bold text-slate-950">Platform-wide user administration</p></AdminCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.4fr_0.6fr]">
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
                    <td className="py-4">
                      <div className="space-y-2">
                        <AdminBadge value={user.role} />
                        <AdminSelect value={draftRoleFor(user)} onChange={(event) => setRoleDrafts((value) => ({ ...value, [user.id]: event.target.value }))}>
                          {ROLE_OPTIONS.map((role) => <option key={role} value={role}>{role}</option>)}
                        </AdminSelect>
                      </div>
                    </td>
                    <td className="py-4">
                      <div className="space-y-2">
                        <AdminBadge value={user.status} />
                        <AdminSelect value={draftStatusFor(user)} onChange={(event) => setStatusDrafts((value) => ({ ...value, [user.id]: event.target.value }))}>
                          {STATUS_OPTIONS.map((status) => <option key={status} value={status}>{status}</option>)}
                        </AdminSelect>
                      </div>
                    </td>
                    <td className="py-4 text-slate-600">{formatAdminDate(user.createdAt)}</td>
                    <td className="py-4">
                      <div className="flex flex-wrap gap-2">
                        <AdminButton variant="secondary" disabled={busyId === user.id} onClick={() => {
                          void openUserDetail(user.id);
                        }}>
                          View
                        </AdminButton>
                        <AdminButton variant="secondary" disabled={busyId === user.id || draftRoleFor(user) === (user.role ?? "Customer")} onClick={() => {
                          setBusyId(user.id);
                          void runAction(() => shopbeApi.admin.updateUserRole(session!.accessToken!, user.id, draftRoleFor(user)), "Failed to update user role.");
                        }}>
                          Save role
                        </AdminButton>
                        <AdminButton variant="secondary" disabled={busyId === user.id || draftStatusFor(user) === (user.status ?? "Active")} onClick={() => {
                          setBusyId(user.id);
                          void runAction(() => shopbeApi.admin.updateUserStatus(session!.accessToken!, user.id, draftStatusFor(user)), "Failed to update user status.");
                        }}>
                          Save status
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

      <AdminCard>
        <h2 className="text-lg font-black text-slate-950">User detail</h2>
        {selectedUser ? (
          <div className="mt-5 space-y-4 text-sm text-slate-600">
            <div>
              <p className="text-xl font-black text-slate-950">{selectedUser.fullName}</p>
              <p className="mt-1">{selectedUser.email}</p>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Role</p><p className="mt-2"><AdminBadge value={selectedUser.role} /></p></div>
              <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Status</p><p className="mt-2"><AdminBadge value={selectedUser.status} /></p></div>
              <div className="rounded-2xl bg-slate-50 p-4 sm:col-span-2"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Keycloak ID</p><p className="mt-2 break-all font-mono text-xs text-slate-700">{selectedUser.keycloakId}</p></div>
              <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Created</p><p className="mt-2 text-slate-800">{formatAdminDate(selectedUser.createdAt)}</p></div>
              <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Updated</p><p className="mt-2 text-slate-800">{formatAdminDate(selectedUser.updatedAt)}</p></div>
            </div>
          </div>
        ) : (
          <div className="mt-5">
            <AdminEmptyState title="Select a user" message="Use the View action in the table to inspect a user record before making platform changes." />
          </div>
        )}
      </AdminCard>
      </div>
    </div>
  );
}
