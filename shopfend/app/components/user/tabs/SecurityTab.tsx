"use client";

import { Shield, Key, Smartphone, Fingerprint, History, Monitor, LogOut } from "lucide-react";

export default function SecurityTab() {
  const devices = [
    { name: "iPhone 15 Pro", location: "Ho Chi Minh City, VN", status: "Active Now", icon: Smartphone },
    { name: "MacBook Pro M2", location: "Ho Chi Minh City, VN", status: "Last active: 2 hours ago", icon: Monitor },
    { name: "Chrome on Windows", location: "Hanoi, VN", status: "Last active: Dec 12, 2024", icon: Monitor },
  ];

  return (
    <div className="p-8 md:p-10 space-y-12">
      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight">Security Settings</h2>
        <p className="text-sm text-slate-500 font-medium mt-1">Manage your password, authentication, and login activity.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Password & 2FA Section */}
        <div className="space-y-6">
          <h3 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <Shield className="text-brand" size={20} />
            Authentication
          </h3>

          <div className="space-y-4">
            <div className="p-5 bg-white border border-slate-100 rounded-3xl hover:border-brand/20 transition-all group cursor-pointer">
              <div className="flex items-center justify-between mb-4">
                <div className="w-10 h-10 bg-slate-50 text-slate-400 group-hover:bg-brand/10 group-hover:text-brand rounded-xl flex items-center justify-center transition-colors">
                  <Key size={20} />
                </div>
                <button className="text-xs font-black text-brand uppercase tracking-widest hover:underline">Update</button>
              </div>
              <div className="text-sm font-bold text-slate-900">Change Password</div>
              <div className="text-xs text-slate-500 font-medium mt-1">Last changed: 3 months ago</div>
            </div>

            <div className="p-5 bg-white border border-slate-100 rounded-3xl hover:border-brand/20 transition-all group cursor-pointer">
              <div className="flex items-center justify-between mb-4">
                <div className="w-10 h-10 bg-emerald-50 text-emerald-500 rounded-xl flex items-center justify-center">
                  <Smartphone size={20} />
                </div>
                <div className="px-2 py-1 bg-emerald-100 text-emerald-700 text-[10px] font-black uppercase tracking-widest rounded-full">Enabled</div>
              </div>
              <div className="text-sm font-bold text-slate-900">Two-Factor Authentication</div>
              <div className="text-xs text-slate-500 font-medium mt-1">Add an extra layer of security to your account.</div>
            </div>

            <div className="p-5 bg-white border border-slate-100 rounded-3xl hover:border-brand/20 transition-all group cursor-pointer">
              <div className="flex items-center justify-between mb-4">
                <div className="w-10 h-10 bg-blue-50 text-blue-500 rounded-xl flex items-center justify-center">
                  <Fingerprint size={20} />
                </div>
                <button className="text-xs font-black text-brand uppercase tracking-widest hover:underline">Setup</button>
              </div>
              <div className="text-sm font-bold text-slate-900">Biometric Login</div>
              <div className="text-xs text-slate-500 font-medium mt-1">Use Face ID or Touch ID for faster access.</div>
            </div>
          </div>
        </div>

        {/* Device Activity Section */}
        <div className="space-y-6">
          <h3 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <History className="text-brand" size={20} />
            Recent Devices
          </h3>

          <div className="bg-slate-50 rounded-[2.5rem] p-6 space-y-6">
            {devices.map((device, i) => (
              <div key={i} className="flex items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 bg-white rounded-2xl shadow-sm flex items-center justify-center text-slate-400">
                    <device.icon size={24} />
                  </div>
                  <div>
                    <div className="text-sm font-bold text-slate-900">{device.name}</div>
                    <div className="text-[10px] text-slate-500 font-medium uppercase tracking-tight">{device.location}</div>
                    <div className={`text-[10px] font-black uppercase tracking-tighter mt-0.5 ${device.status === 'Active Now' ? 'text-emerald-600' : 'text-slate-400'}`}>
                      {device.status}
                    </div>
                  </div>
                </div>
                <button className="p-2 text-slate-400 hover:text-rose-500 transition-colors">
                  <LogOut size={18} />
                </button>
              </div>
            ))}
            <button className="w-full py-3 text-xs font-black text-slate-400 hover:text-brand uppercase tracking-[0.2em] border-t border-slate-200 mt-2">
              Sign out all devices
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
