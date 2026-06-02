import type { LucideIcon } from "lucide-react";

export type SectionId =
  | "profile"
  | "bank-cards"
  | "addresses"
  | "change-password"
  | "privacy-settings"
  | "all-orders"
  | "to-pay"
  | "to-ship"
  | "to-receive"
  | "completed"
  | "cancelled"
  | "return-refund"
  | "wishlist"
  | "recently-viewed"
  | "my-reviews"
  | "followed-shops"
  | "vouchers"
  | "order-updates"
  | "promotions"
  | "system-messages"
  | "help-center"
  | "chat-support"
  | "report-problem";

export interface MenuItem {
  id: SectionId;
  label: string;
  icon: LucideIcon;
}

export interface MenuGroup {
  title: string;
  items: MenuItem[];
}
