"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { errorMessage } from "@/lib/errors";
import { SellerProfileDto, SellerProfileUpsertDto, shopbeApi } from "@/lib/shopbeApi";
import { SellerBadge, SellerButton, SellerCard, SellerCompactMoney, SellerErrorState, SellerInput, SellerLoadingState, SellerPageIntro, SellerTextArea } from "../components/SellerUi";

const initialForm: SellerProfileUpsertDto = {
  shopName: "",
  shopDescription: "",
  shopLogoUrl: "",
  shopBannerUrl: "",
  contactPhone: "",
  contactEmail: "",
  address: "",
  city: "",
};

export default function SellerProfilePage() {
  const { data: session, status } = useSession();
  const [profile, setProfile] = useState<SellerProfileDto | null>(null);
  const [form, setForm] = useState<SellerProfileUpsertDto>(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (status !== "authenticated" || !session.accessToken) return;
    const controller = new AbortController();
    shopbeApi.seller.profile(session.accessToken, controller.signal)
      .then((response) => {
        setProfile(response);
        setForm({
          shopName: response.shopName,
          shopDescription: response.shopDescription ?? "",
          shopLogoUrl: response.shopLogoUrl ?? "",
          shopBannerUrl: response.shopBannerUrl ?? "",
          contactPhone: response.contactPhone ?? "",
          contactEmail: response.contactEmail ?? "",
          address: response.address ?? "",
          city: response.city ?? "",
        });
        setError(null);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(errorMessage(err, "Failed to load seller profile."));
      });
    return () => controller.abort();
  }, [session?.accessToken, status]);

  const save = async () => {
    if (!session?.accessToken) return;
    try {
      setSaving(true);
      const response = await shopbeApi.seller.updateProfile(session.accessToken, form);
      setProfile(response);
      setError(null);
    } catch (err) {
      setError(errorMessage(err, "Failed to update seller profile."));
    } finally {
      setSaving(false);
    }
  };

  if (status === "loading" || (status === "authenticated" && !profile && !error)) return <SellerLoadingState />;
  if (error && !profile) return <SellerErrorState message={error} />;

  return (
    <div className="space-y-6">
      <SellerPageIntro title="Shop profile" description="Control the storefront details buyers see and keep contact information up to date." />

      <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
        <SellerCard>
          <div className="grid gap-4 md:grid-cols-2">
            <div className="md:col-span-2">
              <p className="sb-label">Shop name</p>
              <SellerInput value={form.shopName} onChange={(event) => setForm((value) => ({ ...value, shopName: event.target.value }))} />
            </div>
            <div className="md:col-span-2">
              <p className="sb-label">Description</p>
              <SellerTextArea value={form.shopDescription ?? ""} onChange={(event) => setForm((value) => ({ ...value, shopDescription: event.target.value }))} />
            </div>
            <div>
              <p className="sb-label">Logo URL</p>
              <SellerInput value={form.shopLogoUrl ?? ""} onChange={(event) => setForm((value) => ({ ...value, shopLogoUrl: event.target.value }))} />
            </div>
            <div>
              <p className="sb-label">Banner URL</p>
              <SellerInput value={form.shopBannerUrl ?? ""} onChange={(event) => setForm((value) => ({ ...value, shopBannerUrl: event.target.value }))} />
            </div>
            <div>
              <p className="sb-label">Contact phone</p>
              <SellerInput value={form.contactPhone ?? ""} onChange={(event) => setForm((value) => ({ ...value, contactPhone: event.target.value }))} />
            </div>
            <div>
              <p className="sb-label">Contact email</p>
              <SellerInput value={form.contactEmail ?? ""} onChange={(event) => setForm((value) => ({ ...value, contactEmail: event.target.value }))} />
            </div>
            <div>
              <p className="sb-label">Address</p>
              <SellerInput value={form.address ?? ""} onChange={(event) => setForm((value) => ({ ...value, address: event.target.value }))} />
            </div>
            <div>
              <p className="sb-label">City</p>
              <SellerInput value={form.city ?? ""} onChange={(event) => setForm((value) => ({ ...value, city: event.target.value }))} />
            </div>
          </div>

          {error ? <p className="mt-6 text-sm font-medium text-rose-600">{error}</p> : null}

          <div className="mt-8 flex gap-3">
            <SellerButton onClick={() => void save()} disabled={saving || !form.shopName.trim()}>{saving ? "Saving..." : "Save profile"}</SellerButton>
          </div>
        </SellerCard>

        <SellerCard>
          <h2 className="text-lg font-black text-slate-950">Shop preview</h2>
          {profile ? (
            <div className="mt-5 space-y-4 text-sm text-slate-600">
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <p className="text-2xl font-black text-slate-950">{profile.shopName}</p>
                  <SellerBadge value={profile.status} />
                </div>
                <p className="mt-2">{profile.shopDescription || "No description yet."}</p>
              </div>
              <div className="grid gap-3 sm:grid-cols-2">
                <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Commission</p><p className="mt-2 text-2xl font-black text-slate-950">{profile.commissionRate}%</p></div>
                <div className="rounded-2xl bg-slate-50 p-4"><p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Sales</p><p className="mt-2 text-2xl font-black text-slate-950">{profile.totalSales}</p></div>
                <div className="rounded-2xl bg-emerald-50 p-4 text-emerald-900 sm:col-span-2"><p className="text-xs font-black uppercase tracking-[0.2em]">Revenue</p><p className="mt-2 text-3xl font-black"><SellerCompactMoney amount={profile.totalRevenue} currency="USD" /></p></div>
              </div>
            </div>
          ) : (
            <p className="mt-4 text-sm text-slate-500">Your seller profile will appear here after the first save.</p>
          )}
        </SellerCard>
      </div>
    </div>
  );
}
