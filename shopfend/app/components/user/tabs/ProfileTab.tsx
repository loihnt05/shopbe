"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { Save, X, Globe, Phone, Mail, User, Calendar, Languages, Loader2 } from "lucide-react";
import { shopbeApi, UserRequestDto } from "@/lib/shopbeApi";
import { toast } from "react-hot-toast";

export default function ProfileTab() {
  const { data: session } = useSession();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [formData, setFormData] = useState<UserRequestDto>({
    fullName: "",
    email: "",
    phoneNumber: "",
    gender: "male",
    birthday: "",
    language: "English (US)",
    country: "Vietnam",
    avatarUrl: "",
  });

  useEffect(() => {
    const fetchProfile = async () => {
      if (!session?.accessToken) return;

      try {
        setFetching(true);
        const profile = await shopbeApi.users.getMe(session.accessToken);
        if (profile) {
          setFormData({
            fullName: profile.fullName || session.user?.name || "",
            email: profile.email || session.user?.email || "",
            phoneNumber: profile.phoneNumber || "",
            gender: profile.gender || "male",
            birthday: profile.birthday ? new Date(profile.birthday).toISOString().split('T')[0] : "",
            language: profile.language || "English (US)",
            country: profile.country || "Vietnam",
            avatarUrl: profile.avatarUrl || session.user?.image || "",
          });
        }
      } catch (error) {
        console.error("Failed to fetch profile:", error);
        // Fallback to session data if API fails
        setFormData(prev => ({
          ...prev,
          fullName: session.user?.name || "",
          email: session.user?.email || "",
          avatarUrl: session.user?.image || "",
        }));
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

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!session?.accessToken) {
      toast.error("You must be logged in to save changes.");
      return;
    }

    setLoading(true);
    try {
      await shopbeApi.users.sync(session.accessToken, formData);
      toast.success("Profile updated successfully!");
    } catch (error) {
      console.error("Failed to update profile:", error);
      toast.error("Failed to update profile. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  if (fetching) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px] gap-4">
        <Loader2 className="w-10 h-10 text-brand animate-spin" />
        <p className="text-slate-500 font-medium">Loading your profile...</p>
      </div>
    );
  }

  return (
    <div className="p-8 md:p-10 space-y-10">
      <div className="flex items-center justify-between border-b border-slate-50 pb-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">Personal Information</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">Manage your basic details and account preferences.</p>
        </div>
        <div className="hidden md:flex items-center gap-3">
          <button className="flex items-center gap-2 px-6 py-2.5 rounded-xl text-sm font-bold text-slate-600 hover:bg-slate-50 transition-all">
            <X size={18} />
            Cancel
          </button>
          <button
            onClick={handleSave}
            disabled={loading}
            className="flex items-center gap-2 px-8 py-2.5 rounded-xl bg-brand text-white text-sm font-bold shadow-lg shadow-brand/20 hover:scale-[1.02] active:scale-95 transition-all disabled:opacity-50"
          >
            {loading ? <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" /> : <Save size={18} />}
            Save Changes
          </button>
        </div>
      </div>

      <form onSubmit={handleSave} className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-10">
        {/* Full Name */}
        <div className="relative group">
          <div className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-brand transition-colors">
            <User size={20} />
          </div>
          <input
            type="text"
            name="fullName"
            value={formData.fullName}
            onChange={handleChange}
            className="peer w-full pl-12 pr-4 py-4 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 focus:ring-4 focus:ring-brand/5 transition-all text-slate-900 font-bold placeholder-transparent"
            placeholder="Full Name"
          />
          <label className="absolute left-12 top-0 -translate-y-1/2 text-[10px] font-black text-slate-400 uppercase tracking-widest bg-white px-2 peer-placeholder-shown:top-1/2 peer-placeholder-shown:text-sm peer-placeholder-shown:font-bold peer-placeholder-shown:tracking-normal peer-focus:top-0 peer-focus:text-[10px] peer-focus:font-black peer-focus:tracking-widest transition-all pointer-events-none">
            Full Name
          </label>
        </div>

        {/* Email */}
        <div className="relative group">
          <div className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-brand transition-colors">
            <Mail size={20} />
          </div>
          <input
            type="email"
            name="email"
            value={formData.email}
            readOnly
            className="w-full pl-12 pr-4 py-4 bg-slate-100/50 border-2 border-transparent rounded-2xl text-slate-500 font-bold cursor-not-allowed"
            placeholder="Email Address"
          />
          <label className="absolute left-12 top-0 -translate-y-1/2 text-[10px] font-black text-slate-400 uppercase tracking-widest bg-white px-2 transition-all">
            Email Address
          </label>
        </div>

        {/* Phone */}
        <div className="relative group">
          <div className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-brand transition-colors">
            <Phone size={20} />
          </div>
          <input
            type="text"
            name="phoneNumber"
            value={formData.phoneNumber || ""}
            onChange={handleChange}
            className="peer w-full pl-12 pr-4 py-4 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 focus:ring-4 focus:ring-brand/5 transition-all text-slate-900 font-bold placeholder-transparent"
            placeholder="Phone Number"
          />
          <label className="absolute left-12 top-0 -translate-y-1/2 text-[10px] font-black text-slate-400 uppercase tracking-widest bg-white px-2 peer-placeholder-shown:top-1/2 peer-placeholder-shown:text-sm peer-placeholder-shown:font-bold peer-placeholder-shown:tracking-normal peer-focus:top-0 peer-focus:text-[10px] peer-focus:font-black peer-focus:tracking-widest transition-all pointer-events-none">
            Phone Number
          </label>
        </div>

        {/* Birthday */}
        <div className="relative group">
          <div className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-brand transition-colors">
            <Calendar size={20} />
          </div>
          <input
            type="date"
            name="birthday"
            value={formData.birthday}
            onChange={handleChange}
            className="peer w-full pl-12 pr-4 py-4 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 focus:ring-4 focus:ring-brand/5 transition-all text-slate-900 font-bold"
          />
          <label className="absolute left-12 top-0 -translate-y-1/2 text-[10px] font-black text-slate-400 uppercase tracking-widest bg-white px-2 transition-all">
            Date of Birth
          </label>
        </div>

        {/* Language */}
        <div className="relative group">
          <div className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-brand transition-colors pointer-events-none">
            <Languages size={20} />
          </div>
          <select
            name="language"
            value={formData.language}
            onChange={handleChange}
            className="peer w-full pl-12 pr-4 py-4 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 focus:ring-4 focus:ring-brand/5 transition-all text-slate-900 font-bold appearance-none"
          >
            <option>English (US)</option>
            <option>Vietnamese</option>
            <option>Japanese</option>
            <option>French</option>
          </select>
          <label className="absolute left-12 top-0 -translate-y-1/2 text-[10px] font-black text-slate-400 uppercase tracking-widest bg-white px-2 transition-all">
            Preferred Language
          </label>
          <div className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>

        {/* Country */}
        <div className="relative group">
          <div className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-brand transition-colors pointer-events-none">
            <Globe size={20} />
          </div>
          <select
            name="country"
            value={formData.country}
            onChange={handleChange}
            className="peer w-full pl-12 pr-4 py-4 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 focus:ring-4 focus:ring-brand/5 transition-all text-slate-900 font-bold appearance-none"
          >
            <option>Vietnam</option>
            <option>United States</option>
            <option>Singapore</option>
            <option>Australia</option>
          </select>
          <label className="absolute left-12 top-0 -translate-y-1/2 text-[10px] font-black text-slate-400 uppercase tracking-widest bg-white px-2 transition-all">
            Country / Region
          </label>
          <div className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>

        {/* Mobile Action Buttons */}
        <div className="md:hidden flex flex-col gap-3 mt-4">
          <button
            type="submit"
            className="w-full py-4 rounded-2xl bg-brand text-white font-bold shadow-lg shadow-brand/20 active:scale-95 transition-all"
          >
            Save Changes
          </button>
          <button
            type="button"
            className="w-full py-4 rounded-2xl bg-slate-50 text-slate-600 font-bold active:scale-95 transition-all"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
