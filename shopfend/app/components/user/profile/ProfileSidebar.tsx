"use client";

import { Crown, Edit3, LogOut } from "lucide-react";
import { signOut } from "next-auth/react";
import type { Session } from "next-auth";
import type { MenuGroup, SectionId } from "./types";

interface ProfileSidebarProps {
  session: Session;
  activeSection: SectionId;
  groups: MenuGroup[];
  onSelect: (section: SectionId) => void;
}

export function ProfileSidebar({ session, activeSection, groups, onSelect }: ProfileSidebarProps) {
  return (
    <aside className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm lg:sticky lg:top-20 lg:max-h-[calc(100vh-100px)] lg:overflow-y-auto">
      <div className="rounded-2xl bg-[#FFF4F1] p-4">
        <div className="flex items-center gap-3">
          <img
            src={session.user?.image || "https://ui-avatars.com/api/?name=User&background=EE4D2D&color=fff"}
            alt="User avatar"
            className="h-14 w-14 rounded-full border-2 border-white object-cover"
          />
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm font-semibold text-slate-900">{session.user?.name || "User"}</p>
            <p className="truncate text-xs text-slate-500">{session.user?.email || "No email"}</p>
          </div>
          <button className="rounded-full border border-[#EE4D2D]/20 bg-white p-2 text-[#EE4D2D] hover:bg-[#EE4D2D]/5">
            <Edit3 size={14} />
          </button>
        </div>
        <div className="mt-3 inline-flex items-center gap-1 rounded-full bg-white px-3 py-1 text-xs font-semibold text-amber-700">
          <Crown size={14} /> Gold Member
        </div>
      </div>

      <nav className="mt-4 space-y-4">
        {groups.map((group) => (
          <div key={group.title}>
            <p className="mb-2 px-2 text-xs font-bold uppercase tracking-wide text-slate-400">{group.title}</p>
            <div className="space-y-1">
              {group.items.map((item) => {
                const active = item.id === activeSection;
                return (
                  <button
                    key={item.id}
                    onClick={() => onSelect(item.id)}
                    className={`flex w-full items-center gap-3 rounded-xl border-l-4 px-3 py-2.5 text-left text-sm transition ${
                      active
                        ? "border-l-[#EE4D2D] bg-[#FFF1ED] text-[#EE4D2D]"
                        : "border-l-transparent text-slate-600 hover:bg-slate-50"
                    }`}
                  >
                    <item.icon size={16} />
                    <span>{item.label}</span>
                  </button>
                );
              })}
            </div>
          </div>
        ))}
      </nav>

      <button
        onClick={() => signOut({ callbackUrl: "/" })}
        className="mt-5 flex w-full items-center justify-center gap-2 rounded-xl border border-slate-200 py-2.5 text-sm font-medium text-slate-600 hover:border-rose-200 hover:bg-rose-50 hover:text-rose-600"
      >
        <LogOut size={16} /> Logout
      </button>
    </aside>
  );
}
