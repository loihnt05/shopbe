"use client";

import { useState, useEffect } from "react";
import { useSession } from "next-auth/react";
import { Plus, MapPin, Phone, User, Edit2, Trash2, CheckCircle2 } from "lucide-react";
import { shopbeApi, type UserAddressResponseDto } from "@/lib/shopbeApi";

export default function AddressesTab() {
  const { data: session } = useSession();
  const [addresses, setAddresses] = useState<UserAddressResponseDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (session?.accessToken) {
      shopbeApi.userAddresses.getMyAddresses(session.accessToken)
        .then(res => setAddresses(res))
        .catch(err => console.error(err))
        .finally(() => setLoading(false));
    }
  }, [session?.accessToken]);

  return (
    <div className="p-8 md:p-10 space-y-8">
      <div className="flex items-center justify-between border-b border-slate-50 pb-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">Delivery Addresses</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">Manage your shipping destinations for faster checkout.</p>
        </div>
        <button className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-brand text-white text-sm font-bold shadow-lg shadow-brand/20 hover:scale-[1.02] active:scale-95 transition-all">
          <Plus size={18} />
          Add New Address
        </button>
      </div>

      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {[1, 2].map(i => (
            <div key={i} className="h-48 bg-slate-50 animate-pulse rounded-3xl" />
          ))}
        </div>
      ) : addresses.length === 0 ? (
        <div className="py-20 text-center bg-slate-50/50 rounded-3xl border-2 border-dashed border-slate-100">
          <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
            <MapPin className="text-slate-200" size={32} />
          </div>
          <p className="text-slate-500 font-bold">No addresses found</p>
          <button className="mt-4 text-brand font-black text-xs uppercase tracking-widest hover:underline">+ Add your first address</button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {addresses.map((addr) => (
            <div 
              key={addr.id}
              className={`group relative p-6 rounded-[2.5rem] border-2 transition-all duration-300 ${
                addr.isDefault 
                ? 'border-brand bg-brand/5 shadow-xl shadow-brand/5' 
                : 'border-slate-100 bg-white hover:border-brand/20 hover:shadow-xl hover:shadow-slate-200/50'
              }`}
            >
              {addr.isDefault && (
                <div className="absolute top-6 right-6 flex items-center gap-1.5 px-3 py-1 bg-brand text-white text-[10px] font-black uppercase tracking-widest rounded-full shadow-md">
                  <CheckCircle2 size={12} />
                  Default
                </div>
              )}

              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 rounded-2xl flex items-center justify-center ${addr.isDefault ? 'bg-brand text-white' : 'bg-slate-50 text-slate-400 group-hover:bg-brand/10 group-hover:text-brand'} transition-colors`}>
                    <User size={18} />
                  </div>
                  <div>
                    <div className="text-lg font-bold text-slate-900 leading-tight">{addr.receiverName}</div>
                    <div className="flex items-center gap-1.5 text-xs text-slate-500 font-medium">
                      <Phone size={12} />
                      {addr.phone}
                    </div>
                  </div>
                </div>

                <div className="flex gap-3">
                  <div className={`w-10 h-10 rounded-2xl flex items-center justify-center shrink-0 ${addr.isDefault ? 'bg-brand/10 text-brand' : 'bg-slate-50 text-slate-400 group-hover:bg-brand/10 group-hover:text-brand'} transition-colors`}>
                    <MapPin size={18} />
                  </div>
                  <div className="text-sm text-slate-600 font-medium leading-relaxed">
                    {addr.addressLine}, {addr.ward}, {addr.district}, {addr.city}
                  </div>
                </div>

                <div className="pt-4 flex items-center gap-3 border-t border-slate-100/50 mt-2">
                  <button className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 transition-all">
                    <Edit2 size={14} />
                    Edit
                  </button>
                  <button className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded-xl text-xs font-bold text-rose-500 hover:bg-rose-50 transition-all">
                    <Trash2 size={14} />
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
