"use client";

import { Camera, Trash2 } from "lucide-react";

interface AvatarUploaderProps {
  avatarUrl: string;
  onChangeAvatar: () => void;
  onRemoveAvatar: () => void;
}

export function AvatarUploader({ avatarUrl, onChangeAvatar, onRemoveAvatar }: AvatarUploaderProps) {
  return (
    <aside className="rounded-2xl border border-slate-200 bg-slate-50 p-5">
      <div className="mx-auto h-32 w-32 overflow-hidden rounded-full border-4 border-white shadow">
        <img src={avatarUrl} alt="Avatar preview" className="h-full w-full object-cover" />
      </div>
      <button
        type="button"
        onClick={onChangeAvatar}
        className="mt-4 flex w-full items-center justify-center gap-2 rounded-xl bg-[#EE4D2D] px-4 py-2.5 text-sm font-semibold text-white hover:bg-[#d94224]"
      >
        <Camera size={16} /> Upload Image
      </button>
      <p className="mt-2 text-center text-xs text-slate-500">File size maximum 1MB. Supported formats: JPG, PNG.</p>
      <button
        type="button"
        onClick={onRemoveAvatar}
        className="mt-3 flex w-full items-center justify-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-700 hover:bg-slate-100"
      >
        <Trash2 size={16} /> Remove Avatar
      </button>
    </aside>
  );
}
