"use client";

import type { Session } from "next-auth";
import Image from "next/image";
import { Camera, Crown, Heart, Loader2, ShoppingBag, Star, Ticket } from "lucide-react";
import { motion } from "framer-motion";
import { useCallback, useEffect, useState, useRef } from "react";
import { shopbeApi, User, PagedResult, Wishlist, Review } from "@/lib/shopbeApi";
import { toast } from "@/lib/toast";

interface OrdersResponse {
  totalItems?: number;
  items?: any[];
}

export default function UserHeader({ session }: { session: Session }) {
  const [statsData, setStatsData] = useState({
    orders: "0",
    wishlist: "0",
    reviews: "0",
    coupons: "0",
  });
  const [loadingStats, setLoadingStats] = useState(true);
  const [userProfile, setUserProfile] = useState<User | null>(null);
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const fetchData = useCallback(async () => {
    if (!session?.accessToken) return;

    try {
      setLoadingStats(true);
      const [profileRes, ordersRes, wishlistRes, reviewsRes, couponsRes] = await Promise.all([
        shopbeApi.users.getMe(session.accessToken),
        shopbeApi.orders.getMyOrders(session.accessToken, { pageSize: 1 }) as Promise<PagedResult<any>>,
        shopbeApi.wishlist.get(session.accessToken, { pageSize: 1 }) as Promise<Wishlist[]>,
        shopbeApi.reviews.getMyReviewableProducts(session.accessToken, false) as Promise<Review[]>,
        shopbeApi.coupons.list() as Promise<any[]>,
      ]);

      setUserProfile(profileRes);
      setStatsData({
        orders: ordersRes.totalItems?.toString() || "0",
        wishlist: wishlistRes.length?.toString() || "0",
        reviews: reviewsRes.length?.toString() || "0",
        coupons: couponsRes.length.toString(),
      });
    } catch (error) {
      console.error("Failed to fetch user stats:", error);
    } finally {
      setLoadingStats(false);
    }
  }, [session]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !session?.accessToken || !userProfile) return;

    try {
      setUploading(true);
      const avatarUrl = await shopbeApi.users.uploadAvatar(session.accessToken, file);

      const updatedProfile = await shopbeApi.users.sync(session.accessToken, {
        ...userProfile,
        avatarUrl: avatarUrl
      });

      setUserProfile(updatedProfile);
      toast.success("Avatar updated successfully!");
    } catch (error) {
      console.error("Failed to upload avatar:", error);
      toast.error("Failed to update avatar");
    } finally {
      setUploading(false);
    }
  };

  const stats = [
    { label: "Orders", value: statsData.orders, icon: ShoppingBag, color: "text-sky-600", bg: "bg-sky-100/80" },
    { label: "Wishlist", value: statsData.wishlist, icon: Heart, color: "text-rose-600", bg: "bg-rose-100/80" },
    { label: "Reviews", value: statsData.reviews, icon: Star, color: "text-amber-600", bg: "bg-amber-100/80" },
    { label: "Coupons", value: statsData.coupons, icon: Ticket, color: "text-emerald-600", bg: "bg-emerald-100/80" },
  ];

  return (
    <motion.div
      initial={{ opacity: 0, y: -20 }}
      animate={{ opacity: 1, y: 0 }}
      className="relative w-full overflow-hidden rounded-[2rem] border border-slate-200/60 bg-white shadow-lg shadow-slate-200/40"
    >
      <div className="absolute inset-0 bg-[url(/subtle-noise.svg)] opacity-30"></div>
      <div className="absolute inset-0 bg-gradient-to-r from-white via-white/80 to-slate-50/30"></div>
      
      <div className="relative p-6 md:p-8 flex flex-col md:flex-row items-center justify-between gap-6">
        <div className="flex flex-col md:flex-row items-center gap-5">
          <div className="relative group shrink-0">
            <div className="relative w-28 h-28 md:w-32 md:h-32 rounded-full shadow-xl shadow-slate-300/40">
              <Image
                src={userProfile?.avatarUrl || session.user?.image || `https://ui-avatars.com/api/?name=${encodeURIComponent(userProfile?.fullName || session.user?.name || "U")}&background=ee4d2d&color=fff&size=256`}
                alt={userProfile?.fullName || session.user?.name || "User"}
                fill
                className="object-cover rounded-full border-4 border-white"
              />
              {uploading && (
                <div className="absolute inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center rounded-full">
                  <Loader2 className="w-8 h-8 text-white animate-spin" />
                </div>
              )}
              <button
                onClick={handleAvatarClick}
                disabled={uploading}
                className="absolute bottom-1 right-1 flex items-center justify-center w-9 h-9 bg-brand text-white rounded-full shadow-md hover:bg-brand-hover transition-all active:scale-90 ring-2 ring-white disabled:opacity-50"
              >
                <Camera size={18} />
              </button>
            </div>
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleFileChange}
              className="hidden"
              accept="image/*"
            />
          </div>

          <div className="text-center md:text-left space-y-1.5">
            <div className="flex flex-wrap items-center justify-center md:justify-start gap-x-3 gap-y-1.5">
              <h1 className="text-2xl md:text-3xl font-black text-slate-800 tracking-tight">
                {userProfile?.fullName || session.user?.name || "Valued Customer"}
              </h1>
              <span className="flex items-center gap-1.5 px-3 py-1 bg-amber-100 text-amber-700 text-[10px] font-black uppercase tracking-widest rounded-full">
                <Crown size={12} className="fill-amber-400" />
                Platinum
              </span>
            </div>
            <p className="text-slate-500 font-medium break-all">{userProfile?.email || session.user?.email}</p>
            <div className="flex items-center justify-center md:justify-start gap-4 pt-2">
              <div>
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Points</span>
                <p className="text-lg font-black text-brand">2,450</p>
              </div>
              <div className="w-px h-6 bg-slate-200" />
              <div>
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Since</span>
                <p className="text-sm font-bold text-slate-700">
                  {userProfile?.createdAt ? new Date(userProfile.createdAt).toLocaleDateString(undefined, { month: 'short', year: 'numeric' }) : 'May 2024'}
                </p>
              </div>
            </div>
          </div>
        </div>

        <div className="w-full md:w-px md:h-24 bg-slate-200/80 mx-4 hidden lg:block" />

        <div className="grid grid-cols-2 sm:grid-cols-4 md:grid-cols-2 lg:grid-cols-4 gap-3 w-full md:w-auto md:max-w-md">
          {stats.map((stat, i) => (
            <motion.div
              key={stat.label}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.05 * i }}
              className="p-3 rounded-2xl bg-white/40 border border-slate-200/60 hover:bg-white transition-all cursor-pointer"
            >
              <div className="flex items-center gap-3">
                <div className={`w-8 h-8 ${stat.bg} ${stat.color} rounded-lg flex items-center justify-center`}>
                  <stat.icon size={16} />
                </div>
                <div>
                  <div className="text-base font-black text-slate-800 leading-none">
                    {loadingStats ? <Loader2 size={14} className="animate-spin text-slate-400" /> : stat.value}
                  </div>
                  <div className="text-[10px] font-bold text-slate-500 uppercase tracking-wider">{stat.label}</div>
                </div>
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    </motion.div>
  );
}
