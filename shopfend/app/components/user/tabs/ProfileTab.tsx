"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { CheckCircle, Loader2, Save } from "lucide-react";
import { shopbeApi, UserRequestDto } from "@/lib/shopbeApi";
import { toast } from "@/lib/toast";

const DEFAULT_FORM_DATA: UserRequestDto = {
  fullName: "",
  email: "",
  phoneNumber: "",
  gender: "male",
  birthday: "",
  language: "English (US)",
  country: "Vietnam",
  avatarUrl: "",
};

export default function ProfileTab() {
  const { data: session } = useSession();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [formData, setFormData] = useState<UserRequestDto>({
    ...DEFAULT_FORM_DATA,
  });
  const [initialData, setInitialData] = useState<UserRequestDto>({
    ...DEFAULT_FORM_DATA,
  });

  useEffect(() => {
    const fetchProfile = async () => {
      if (!session?.accessToken) return;

      try {
        setFetching(true);
        const profile = await shopbeApi.users.getMe(session.accessToken);
        if (profile) {
          const normalizedData = {
            fullName: profile.fullName || session.user?.name || "",
            email: profile.email || session.user?.email || "",
            phoneNumber: profile.phoneNumber || "",
            gender: profile.gender || "male",
            birthday: profile.birthday ? new Date(profile.birthday).toISOString().split('T')[0] : "",
            language: profile.language || "English (US)",
            country: profile.country || "Vietnam",
            avatarUrl: profile.avatarUrl || session.user?.image || "",
          };
          setFormData(normalizedData);
          setInitialData(normalizedData);
        }
      } catch (error) {
        console.error("Failed to fetch profile:", error);
        const fallbackData = {
          ...DEFAULT_FORM_DATA,
          fullName: session.user?.name || "",
          email: session.user?.email || "",
          avatarUrl: session.user?.image || "",
        };
        setFormData(fallbackData);
        setInitialData(fallbackData);
      } finally {
        setFetching(false);
      }
    };

    fetchProfile();
  }, [session]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleGenderChange = (gender: "male" | "female" | "other") => {
    setFormData(prev => ({ ...prev, gender }));
  };

  const hasChanges = JSON.stringify(formData) !== JSON.stringify(initialData);
  const completionItems = [
    {key: 'Full Name', filled: !!formData.fullName},
    {key: 'Phone', filled: !!formData.phoneNumber},
    {key: 'Birthday', filled: !!formData.birthday},
    {key: 'Country', filled: !!formData.country},
  ];
  const completionRate = Math.round((completionItems.filter(item => item.filled).length / completionItems.length) * 100);
  const inputClass = "w-full rounded-xl border border-slate-300/70 bg-slate-50/50 px-4 py-3 text-sm font-semibold text-slate-800 outline-none transition-all placeholder:text-slate-400 focus:bg-white focus:border-brand focus:ring-4 focus:ring-brand/10";
  
  const handleReset = () => {
    setFormData(initialData);
    toast.info("Changes have been reset.");
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!session?.accessToken) {
      toast.error("You must be logged in to save changes.");
      return;
    }

    setLoading(true);
    try {
      // Convert empty strings to null for fields the backend expects as nullable
      // (especially DateTime? fields like birthday)
      const payload: UserRequestDto = {
        ...formData,
        birthday: formData.birthday || null,
        phoneNumber: formData.phoneNumber || null,
        avatarUrl: formData.avatarUrl || null,
      };
      await shopbeApi.users.sync(session.accessToken, payload);
      setInitialData(formData);
      toast.success("Profile updated successfully!");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to update profile. Please try again.";
      console.error("Failed to update profile:", message);
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  if (fetching) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[500px] gap-4">
        <Loader2 className="w-10 h-10 text-brand animate-spin" />
        <p className="text-slate-500 font-medium">Loading Your Profile...</p>
      </div>
    );
  }

  return (
    <div className="w-full box-border p-5 md:p-6 space-y-6">
      <section>
        <h2 className="text-xl font-extrabold text-slate-800 tracking-tight">My Profile</h2>
        <p className="text-sm text-slate-500 mt-1">Manage your account details and preferences.</p>
      </section>
      
      <hr className="border-slate-200/80" />

      <div className="grid w-full grid-cols-1 xl:grid-cols-[minmax(0,1fr)_320px] gap-8">
        <form onSubmit={handleSave} className="w-full box-border space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-x-5 gap-y-6">
            <label className="space-y-2">
              <span className="text-xs font-black uppercase tracking-widest text-slate-500">Full name</span>
              <div className="relative">
                <input
                  type="text"
                  name="fullName"
                  value={formData.fullName}
                  onChange={handleChange}
                  className={`${inputClass}`}
                  placeholder="Enter your full name"
                />
              </div>
            </label>

            <label className="space-y-2">
              <span className="text-xs font-black uppercase tracking-widest text-slate-500">Email address</span>
              <div className="relative">
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  readOnly
                  className={`${inputClass} bg-slate-100 text-slate-500 cursor-not-allowed border-slate-200`}
                />
              </div>
            </label>

            <label className="space-y-2">
              <span className="text-xs font-black uppercase tracking-widest text-slate-500">Phone number</span>
              <div className="relative">
                <input
                  type="text"
                  name="phoneNumber"
                  value={formData.phoneNumber || ""}
                  onChange={handleChange}
                  className={`${inputClass}`}
                  placeholder="Add your phone"
                />
              </div>
            </label>

            <label className="space-y-2">
              <span className="text-xs font-black uppercase tracking-widest text-slate-500">Date of birth</span>
              <div className="relative">
                <input
                  type="date"
                  name="birthday"
                  value={formData.birthday ?? ""}
                  onChange={handleChange}
                  className={`${inputClass}`}
                />
              </div>
            </label>
            
            <div className="space-y-2 md:col-span-2">
              <p className="text-xs font-black uppercase tracking-widest text-slate-500">Gender</p>
              <div className="flex gap-3">
                {[
                  { key: "male", label: "Male" },
                  { key: "female", label: "Female" },
                  { key: "other", label: "Other" },
                ].map(option => (
                  <button
                    key={option.key}
                    type="button"
                    onClick={() => handleGenderChange(option.key as "male" | "female" | "other")}
                    className={`flex-1 rounded-xl border px-4 py-3 text-sm font-extrabold transition-all ${formData.gender === option.key ? "border-brand bg-brand/10 text-brand" : "border-slate-300/70 bg-slate-50/50 text-slate-600 hover:bg-white hover:border-slate-400"}`}
                  >
                    {option.label}
                  </button>
                ))}
              </div>
            </div>
          </div>
          
          <hr className="border-slate-200/80" />

          <div className="flex justify-end gap-3">
            <button
              type="button"
              onClick={handleReset}
              disabled={!hasChanges || loading}
              className="px-6 py-2.5 rounded-xl text-sm font-bold text-slate-700 bg-slate-100 hover:bg-slate-200 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Reset
            </button>
            <button
              type="submit"
              disabled={loading || !hasChanges}
              className="flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl bg-brand text-white text-sm font-bold shadow-lg shadow-brand/20 hover:bg-brand/90 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? <Loader2 className="w-5 h-5 animate-spin" /> : <Save size={18} />}
              Save Changes
            </button>
          </div>
        </form>

        <aside className="w-full box-border space-y-5">
          <div className="w-full box-border rounded-xl border border-slate-200/80 bg-white p-5">
            <h3 className="text-xs font-black uppercase tracking-widest text-slate-500 mb-3">Profile Completion</h3>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-bold text-slate-600">{completionRate === 100 ? "Complete!" : "Incomplete"}</span>
              <span className="text-lg font-black text-brand">{completionRate}%</span>
            </div>
            <div className="h-2.5 w-full rounded-full bg-slate-100 overflow-hidden">
              <div className="h-full bg-gradient-to-r from-emerald-400 to-teal-500 transition-all duration-500" style={{ width: `${completionRate}%`, background: completionRate < 100 ? 'linear-gradient(to right, #fb923c, #f97316)' : 'linear-gradient(to right, #34d399, #14b8a6)' }} />
            </div>
            <div className="space-y-2 mt-4">
              {completionItems.map(item => (
                <div key={item.key} className="flex items-center gap-2 text-xs font-semibold">
                  <CheckCircle size={14} className={item.filled ? 'text-emerald-500' : 'text-slate-300'} />
                  <span className={item.filled ? 'text-slate-600' : 'text-slate-400'}>{item.key}</span>
                </div>
              ))}
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}
