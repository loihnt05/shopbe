interface ProfileHeaderProps {
  title: string;
  subtitle: string;
}

export function ProfileHeader({ title, subtitle }: ProfileHeaderProps) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
      <p className="mt-1 text-sm text-slate-500">{subtitle}</p>
    </div>
  );
}
