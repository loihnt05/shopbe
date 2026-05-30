"use client";

import { Shield, Key, Smartphone, Fingerprint, History, Monitor, LogOut, ExternalLink } from "lucide-react";
import { useSession } from "next-auth/react";

export default function SecurityTab() {
  const { data: session } = useSession();

  // Construct Keycloak Account Console URL
  // The issuer is usually something like .../realms/shopbee
  // Account console is at .../realms/shopbee/account
  const keycloakAccountUrl = session?.idToken
    ? "http://localhost:8080/realms/shopbee/account" // Fallback to local dev URL
    : "#";

  const devices = [
    { name: "iPhone 15 Pro", location: "Ho Chi Minh City, VN", status: "Active Now", icon: Smartphone },
    { name: "MacBook Pro M2", location: "Ho Chi Minh City, VN", status: "Last active: 2 hours ago", icon: Monitor },
  ];

  const handleExternalSecurity = () => {
    window.open(keycloakAccountUrl, "_blank");
  };

  return (
    <div className="p-8 md:p-10 space-y-12">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 border-b border-slate-50 pb-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">Security & Privacy</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">Security settings are managed securely via our authentication provider.</p>
        </div>
        <button
          onClick={handleExternalSecurity}
          className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-slate-900 text-white text-xs font-bold shadow-lg shadow-slate-900/20 hover:scale-[1.02] active:scale-95 transition-all"
        >
          Manage Account on Keycloak
          <ExternalLink size={14} />
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Password & 2FA Section */}
        <div className="space-y-6">
          <h3 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <Shield className="text-brand" size={20} />
            Authentication
          </h3>

          <div className="space-y-4">
            <div
              onClick={handleExternalSecurity}
              className="p-5 bg-white border border-slate-100 rounded-3xl hover:border-brand/20 transition-all group cursor-pointer"
            >
              <div className="flex items-center justify-between mb-4">
                <div className="w-10 h-10 bg-slate-50 text-slate-400 group-hover:bg-brand/10 group-hover:text-brand rounded-xl flex items-center justify-center transition-colors">
                  <Key size={20} />
                </div>
                <div className="text-xs font-black text-brand uppercase tracking-widest flex items-center gap-1">
                  Manage <ExternalLink size={10} />
                </div>
              </div>
              <div className="text-sm font-bold text-slate-900">Change Password</div>
              <div className="text-xs text-slate-500 font-medium mt-1">Update your password regularly to stay secure.</div>
            </div>

            <div
              onClick={handleExternalSecurity}
              className="p-5 bg-white border border-slate-100 rounded-3xl hover:border-brand/20 transition-all group cursor-pointer"
            >
              <div className="flex items-center justify-between mb-4">
                <div className="w-10 h-10 bg-emerald-50 text-emerald-500 rounded-xl flex items-center justify-center">
                  <Smartphone size={20} />
                </div>
                <div className="px-2 py-1 bg-emerald-100 text-emerald-700 text-[10px] font-black uppercase tracking-widest rounded-full">Secure</div>
              </div>
              <div className="text-sm font-bold text-slate-900">Two-Factor Authentication</div>
              <div className="text-xs text-slate-500 font-medium mt-1">Manage 2FA, authenticator apps, and recovery codes.</div>
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
