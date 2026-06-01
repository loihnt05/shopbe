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
