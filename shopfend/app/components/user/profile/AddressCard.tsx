import { MapPin } from "lucide-react";

interface AddressCardProps {
  receiver: string;
  phone: string;
  address: string;
  isDefault?: boolean;
  onEdit: () => void;
  onAdd: () => void;
}

export function AddressCard({ receiver, phone, address, isDefault, onEdit, onAdd }: AddressCardProps) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-slate-900">Saved Addresses</h3>
        <button onClick={onAdd} className="rounded-xl bg-[#EE4D2D] px-3 py-2 text-sm font-medium text-white hover:bg-[#d94224]">Add New Address</button>
      </div>
      <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
        <div className="flex items-start justify-between gap-3">
          <div>
            <p className="text-sm font-semibold text-slate-800">{receiver} <span className="font-normal text-slate-500">({phone})</span></p>
            <p className="mt-1 flex items-start gap-2 text-sm text-slate-600"><MapPin size={16} className="mt-0.5" />{address}</p>
          </div>
          {isDefault && <span className="rounded-full bg-emerald-100 px-2 py-1 text-xs font-semibold text-emerald-700">Default</span>}
        </div>
        <button onClick={onEdit} className="mt-3 rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-700 hover:bg-slate-100">Edit</button>
      </div>
    </section>
  );
}
