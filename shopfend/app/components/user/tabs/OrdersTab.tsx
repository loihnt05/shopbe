"use client";

import { useState, useEffect } from "react";
import { useSession } from "next-auth/react";
import { 
  Package, 
  Truck, 
  CheckCircle2, 
  XCircle, 
  RefreshCcw, 
  Clock,
  ExternalLink,
  Search
} from "lucide-react";
import { shopbeApi } from "@/lib/shopbeApi";
import { formatMoney } from "@/lib/format";

export default function OrdersTab() {
  const { data: session } = useSession();
  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (session?.accessToken) {
      shopbeApi.orders.getMyOrders(session.accessToken)
        .then(res => setOrders(res.items))
        .catch(err => console.error(err))
        .finally(() => setLoading(false));
    }
  }, [session?.accessToken]);

  const stats = [
    { label: "Pending", count: 2, icon: Clock, color: "text-amber-500", bg: "bg-amber-50" },
    { label: "Processing", count: 1, icon: RefreshCcw, color: "text-blue-500", bg: "bg-blue-50" },
    { label: "Shipping", count: 3, icon: Truck, color: "text-indigo-500", bg: "bg-indigo-50" },
    { label: "Delivered", count: 45, icon: CheckCircle2, color: "text-emerald-500", bg: "bg-emerald-50" },
    { label: "Cancelled", count: 0, icon: XCircle, color: "text-rose-500", bg: "bg-rose-50" },
  ];

  return (
    <div className="p-8 md:p-10 space-y-10">
      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight">Order History</h2>
        <p className="text-sm text-slate-500 font-medium mt-1">Track and manage your recent purchases.</p>
      </div>

      {/* Status Overview Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
        {stats.map((stat) => (
          <div 
            key={stat.label}
            className="p-5 rounded-[2rem] bg-slate-50 border border-slate-100 hover:bg-white hover:shadow-xl hover:shadow-slate-200/50 transition-all group cursor-pointer"
          >
            <div className={`w-12 h-12 ${stat.bg} ${stat.color} rounded-2xl flex items-center justify-center mb-4 group-hover:scale-110 transition-transform`}>
              <stat.icon size={24} />
            </div>
            <div className="text-2xl font-black text-slate-900 leading-none mb-1">{stat.count}</div>
            <div className="text-xs font-bold text-slate-400 uppercase tracking-wider">{stat.label}</div>
          </div>
        ))}
      </div>

      {/* Orders List */}
      <div className="space-y-4">
        <div className="flex items-center justify-between mb-2">
          <h3 className="text-lg font-bold text-slate-900">Recent Orders</h3>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
            <input 
              type="text" 
              placeholder="Search orders..." 
              className="pl-10 pr-4 py-2 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold outline-none focus:ring-2 focus:ring-brand/10 transition-all"
            />
          </div>
        </div>

        {loading ? (
          <div className="space-y-4">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-32 bg-slate-50 animate-pulse rounded-3xl" />
            ))}
          </div>
        ) : orders.length === 0 ? (
          <div className="py-20 text-center bg-slate-50/50 rounded-3xl border-2 border-dashed border-slate-100">
            <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
              <Package className="text-slate-200" size={32} />
            </div>
            <p className="text-slate-500 font-bold">No orders found</p>
            <button className="mt-4 text-brand font-black text-xs uppercase tracking-widest hover:underline">Start Shopping</button>
          </div>
        ) : (
          <div className="space-y-4">
            {orders.map((order) => (
              <div 
                key={order.id}
                className="group bg-white border border-slate-100 rounded-3xl p-6 hover:shadow-xl hover:shadow-slate-200/50 transition-all flex flex-col md:flex-row md:items-center justify-between gap-6"
              >
                <div className="flex items-center gap-4">
                  <div className="w-16 h-16 bg-slate-50 rounded-2xl flex items-center justify-center text-2xl group-hover:scale-110 transition-transform">
                    📦
                  </div>
                  <div>
                    <div className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">
                      Order #{order.id.slice(-8).toUpperCase()}
                    </div>
                    <div className="text-lg font-bold text-slate-900">
                      {formatMoney(order.totalAmount, "VND")}
                    </div>
                    <div className="text-xs text-slate-500 font-medium">
                      {new Date(order.createdAt).toLocaleDateString(undefined, { dateStyle: 'long' })}
                    </div>
                  </div>
                </div>

                <div className="flex flex-wrap items-center gap-3">
                  <span className={`px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest ${
                    order.status === 'Delivered' ? 'bg-emerald-100 text-emerald-700' :
                    order.status === 'Cancelled' ? 'bg-rose-100 text-rose-700' :
                    'bg-blue-100 text-blue-700'
                  }`}>
                    {order.status}
                  </span>
                  <button className="flex items-center gap-2 px-5 py-2.5 rounded-xl border border-slate-100 text-xs font-bold text-slate-600 hover:bg-slate-50 transition-all">
                    View Details
                    <ExternalLink size={14} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
