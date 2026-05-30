"use client";

import { useState, useEffect } from "react";
import { Ticket, Search, Clock, Zap, Gift } from "lucide-react";
import { shopbeApi, type CouponResponseDto } from "@/lib/shopbeApi";
import { formatMoney } from "@/lib/format";

export default function CouponsTab() {
  const [coupons, setCoupons] = useState<CouponResponseDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    shopbeApi.coupons.list()
      .then(res => setCoupons(res))
      .catch(err => console.error(err))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="p-8 md:p-10 space-y-10">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 border-b border-slate-50 pb-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">My Vouchers & Rewards</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">Check your available coupons and exclusive member rewards.</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
            <input 
              type="text" 
              placeholder="Enter code..." 
              className="pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold outline-none focus:ring-2 focus:ring-brand/10 transition-all w-full md:w-40"
            />
          </div>
          <button className="px-6 py-2.5 rounded-xl bg-brand text-white text-xs font-bold shadow-lg shadow-brand/20 hover:scale-[1.02] active:scale-95 transition-all">
            Redeem
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {loading ? (
          [1, 2, 3, 4].map(i => <div key={i} className="h-32 bg-slate-50 animate-pulse rounded-3xl" />)
        ) : coupons.length === 0 ? (
          <div className="col-span-full py-20 text-center bg-slate-50/50 rounded-3xl border-2 border-dashed border-slate-100">
             <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
              <Gift className="text-slate-200" size={32} />
            </div>
            <p className="text-slate-500 font-bold">No coupons available</p>
          </div>
        ) : (
          coupons.map((coupon) => (
            <div 
              key={coupon.id}
              className="group relative flex bg-white border border-slate-100 rounded-3xl overflow-hidden hover:shadow-xl hover:shadow-slate-200/50 transition-all"
            >
              {/* Left Decoration */}
              <div className="w-24 bg-brand flex flex-col items-center justify-center text-white p-4 gap-2">
                <Ticket size={24} className="opacity-80 group-hover:scale-110 transition-transform" />
                <div className="text-[10px] font-black uppercase tracking-widest text-center leading-tight">Shopbee Exclusive</div>
              </div>

              {/* Dotted Divider */}
              <div className="absolute left-[92px] top-0 bottom-0 flex flex-col justify-between py-2">
                {[...Array(12)].map((_, i) => (
                  <div key={i} className="w-1 h-1 bg-slate-100 rounded-full" />
                ))}
              </div>

              {/* Content */}
              <div className="flex-1 p-5 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="text-xs font-black text-brand uppercase tracking-widest">
                    {coupon.discountType === 'Percentage' ? `${coupon.value}% OFF` : `${formatMoney(coupon.value, "VND")} OFF`}
                  </div>
                  {coupon.isActive && (
                    <div className="flex items-center gap-1 text-[8px] font-black text-emerald-600 bg-emerald-100 px-2 py-0.5 rounded-full uppercase tracking-tighter">
                      <Zap size={8} className="fill-current" />
                      Active
                    </div>
                  )}
                </div>
                
                <div>
                  <div className="text-sm font-bold text-slate-800 line-clamp-1">{coupon.description || `Special discount for you`}</div>
                  <div className="text-[10px] text-slate-400 font-medium flex items-center gap-1 mt-1">
                    <Clock size={10} />
                    Expires: {new Date(coupon.expiredAt).toLocaleDateString()}
                  </div>
                </div>

                <div className="pt-2 flex items-center justify-between border-t border-slate-50">
                  <div className="text-[10px] font-bold text-slate-500">Min. spend {formatMoney(coupon.minOrderAmount, "VND")}</div>
                  <button className="text-[10px] font-black text-brand uppercase tracking-widest hover:underline">Use Now</button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
