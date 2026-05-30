"use client";

import { useState, useEffect } from "react";
import { useSession } from "next-auth/react";
import { Plus, MapPin, Phone, User, Edit2, Trash2, CheckCircle2, X, Loader2 } from "lucide-react";
import { shopbeApi, type UserAddressResponseDto, type UserAddressRequestDto } from "@/lib/shopbeApi";
import { locations } from "@/lib/locations";
import { toast } from "react-hot-toast";
import { motion, AnimatePresence } from "framer-motion";

export default function AddressesTab() {
  const { data: session } = useSession();
  const [addresses, setAddresses] = useState<UserAddressResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingAddress, setEditingAddress] = useState<UserAddressResponseDto | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [formData, setFormData] = useState<UserAddressRequestDto>({
    receiverName: "",
    phone: "",
    addressLine: "",
    city: "",
    district: "",
    ward: "",
    isDefault: false,
  });

  const fetchAddresses = async () => {
    if (!session?.accessToken) return;
    try {
      setLoading(true);
      const res = await shopbeApi.userAddresses.getMyAddresses(session.accessToken);
      setAddresses(res);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load addresses");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAddresses();
  }, [session?.accessToken]);

  const handleOpenModal = (address?: UserAddressResponseDto) => {
    if (address) {
      setEditingAddress(address);
      setFormData({
        receiverName: address.receiverName,
        phone: address.phone,
        addressLine: address.addressLine,
        city: address.city,
        district: address.district,
        ward: address.ward,
        isDefault: address.isDefault,
      });
    } else {
      setEditingAddress(null);
      setFormData({
        receiverName: "",
        phone: "",
        addressLine: "",
        city: "",
        district: "",
        ward: "",
        isDefault: addresses.length === 0,
      });
    }
    setModalOpen(true);
  };

  const handleCloseModal = () => {
    setModalOpen(false);
    setEditingAddress(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!session?.accessToken) return;

    setSubmitting(true);
    try {
      if (editingAddress) {
        await shopbeApi.userAddresses.update(session.accessToken, editingAddress.id, formData);
        toast.success("Address updated successfully");
      } else {
        await shopbeApi.userAddresses.create(session.accessToken, formData);
        toast.success("Address added successfully");
      }
      handleCloseModal();
      fetchAddresses();
    } catch (err) {
      console.error(err);
      toast.error(editingAddress ? "Failed to update address" : "Failed to add address");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!session?.accessToken || !confirm("Are you sure you want to delete this address?")) return;

    try {
      await shopbeApi.userAddresses.delete(session.accessToken, id);
      toast.success("Address deleted");
      fetchAddresses();
    } catch (err) {
      console.error(err);
      toast.error("Failed to delete address");
    }
  };

  const provinces = locations.provinces;
  const selectedProvince = provinces.find(p => p.name === formData.city);
  const districts = selectedProvince?.districts || [];
  const selectedDistrict = districts.find(d => d.name === formData.district);
  const wards = selectedDistrict?.wards || [];

  return (
    <div className="p-8 md:p-10 space-y-8">
      <div className="flex items-center justify-between border-b border-slate-50 pb-6">
        <div>
          <h2 className="text-2xl font-black text-slate-900 tracking-tight">Delivery Addresses</h2>
          <p className="text-sm text-slate-500 font-medium mt-1">Manage your shipping destinations for faster checkout.</p>
        </div>
        <button
          onClick={() => handleOpenModal()}
          className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-brand text-white text-sm font-bold shadow-lg shadow-brand/20 hover:scale-[1.02] active:scale-95 transition-all"
        >
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
          <button
            onClick={() => handleOpenModal()}
            className="mt-4 text-brand font-black text-xs uppercase tracking-widest hover:underline"
          >
            + Add your first address
          </button>
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
                  <button
                    onClick={() => handleOpenModal(addr)}
                    className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-50 transition-all"
                  >
                    <Edit2 size={14} />
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(addr.id)}
                    className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded-xl text-xs font-bold text-rose-500 hover:bg-rose-50 transition-all"
                  >
                    <Trash2 size={14} />
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Address Modal */}
      <AnimatePresence>
        {modalOpen && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={handleCloseModal}
              className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm"
            />
            <motion.div
              initial={{ opacity: 0, scale: 0.9, y: 20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.9, y: 20 }}
              className="relative w-full max-w-lg bg-white rounded-[2.5rem] shadow-2xl overflow-hidden"
            >
              <div className="p-8 space-y-6">
                <div className="flex items-center justify-between">
                  <h3 className="text-xl font-black text-slate-900 tracking-tight">
                    {editingAddress ? "Edit Address" : "Add New Address"}
                  </h3>
                  <button
                    onClick={handleCloseModal}
                    className="p-2 hover:bg-slate-50 rounded-xl text-slate-400 transition-all"
                  >
                    <X size={20} />
                  </button>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                      <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Receiver Name</label>
                      <input
                        required
                        type="text"
                        value={formData.receiverName}
                        onChange={(e) => setFormData({...formData, receiverName: e.target.value})}
                        placeholder="John Doe"
                        className="w-full px-4 py-3 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 transition-all text-sm font-bold"
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Phone Number</label>
                      <input
                        required
                        type="text"
                        value={formData.phone}
                        onChange={(e) => setFormData({...formData, phone: e.target.value})}
                        placeholder="0912345678"
                        className="w-full px-4 py-3 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 transition-all text-sm font-bold"
                      />
                    </div>
                  </div>

                  <div className="space-y-1.5">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Street Address</label>
                    <input
                      required
                      type="text"
                      value={formData.addressLine}
                      onChange={(e) => setFormData({...formData, addressLine: e.target.value})}
                      placeholder="123 Example St"
                      className="w-full px-4 py-3 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 transition-all text-sm font-bold"
                    />
                  </div>

                  <div className="grid grid-cols-3 gap-4">
                    <div className="space-y-1.5">
                      <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Province/City</label>
                      <select
                        required
                        value={formData.city}
                        onChange={(e) => setFormData({...formData, city: e.target.value, district: "", ward: ""})}
                        className="w-full px-3 py-3 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 transition-all text-xs font-bold appearance-none"
                      >
                        <option value="">Select</option>
                        {provinces.map(p => <option key={p.name} value={p.name}>{p.name}</option>)}
                      </select>
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">District</label>
                      <select
                        required
                        value={formData.district}
                        onChange={(e) => setFormData({...formData, district: e.target.value, ward: ""})}
                        disabled={!formData.city}
                        className="w-full px-3 py-3 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 transition-all text-xs font-bold appearance-none disabled:opacity-50"
                      >
                        <option value="">Select</option>
                        {districts.map(d => <option key={d.name} value={d.name}>{d.name}</option>)}
                      </select>
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Ward</label>
                      <select
                        required
                        value={formData.ward}
                        onChange={(e) => setFormData({...formData, ward: e.target.value})}
                        disabled={!formData.district}
                        className="w-full px-3 py-3 bg-slate-50 border-2 border-transparent rounded-2xl outline-none focus:bg-white focus:border-brand/20 transition-all text-xs font-bold appearance-none disabled:opacity-50"
                      >
                        <option value="">Select</option>
                        {wards.map(w => <option key={w} value={w}>{w}</option>)}
                      </select>
                    </div>
                  </div>

                  <div className="flex items-center gap-3 py-2">
                    <button
                      type="button"
                      onClick={() => setFormData({...formData, isDefault: !formData.isDefault})}
                      className={`relative w-10 h-6 rounded-full transition-colors ${formData.isDefault ? 'bg-brand' : 'bg-slate-200'}`}
                    >
                      <div className={`absolute top-1 left-1 w-4 h-4 bg-white rounded-full transition-transform ${formData.isDefault ? 'translate-x-4' : ''}`} />
                    </button>
                    <span className="text-xs font-bold text-slate-600">Set as default delivery address</span>
                  </div>

                  <div className="flex gap-4 pt-4">
                    <button
                      type="button"
                      onClick={handleCloseModal}
                      className="flex-1 py-3.5 rounded-2xl bg-slate-50 text-slate-600 text-sm font-bold active:scale-95 transition-all"
                    >
                      Cancel
                    </button>
                    <button
                      type="submit"
                      disabled={submitting}
                      className="flex-1 py-3.5 rounded-2xl bg-brand text-white text-sm font-bold shadow-lg shadow-brand/20 active:scale-95 transition-all disabled:opacity-50 flex items-center justify-center gap-2"
                    >
                      {submitting && <Loader2 size={16} className="animate-spin" />}
                      {editingAddress ? "Update" : "Save Address"}
                    </button>
                  </div>
                </form>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
}
