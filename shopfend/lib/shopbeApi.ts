export type ApiEnvelope<T> = {
  data: T;
};

import { asRecord, errorMessage } from "./errors";

export type ProductImageDto = {
  id: string;
  imageUrl: string;
  isPrimary: boolean;
  altText?: string | null;
};

export type ProductListItem = {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  price?: number | null;
  discountPrice?: number | null;
  currency?: string | null;
  primaryImageUrl?: string | null;
  totalStockQuantity?: number | null;
  soldCount?: number | null;
  averageRating?: number | null;
  ratingCount?: number | null;
  categoryId: string;
  categoryName?: string | null;
  brandId?: string | null;
  brandName?: string | null;
  isActive: boolean;
  recommendationReason?: string | null;
  images?: ProductImageDto[];
  variants?: ProductVariantDto[];
};

export type ProductVariantAttributeDto = {
  name: string;
  value: string;
};

export type ProductVariantDto = {
  id: string;
  productId: string;
  sku?: string | null;
  price: number;
  currency: string;
  stockQuantity?: number | null;
  isActive: boolean;
  attributes?: ProductVariantAttributeDto[];
};

export type ProductDetail = ProductListItem;

export type CartItem = {
  productVariantId: string;
  productName?: string;
  imageUrl?: string | null;
  quantity: number;
  unitPrice?: number;
  lineTotal?: number;
};
export type CartDto = {
  id: string;
  userId?: string;
  items: CartItem[];
  subtotal: number;
  discountAmount: number;
  total: number;
  couponCode?: string | null;
  totalQuantity: number;
  totalItems: number;
  displayQuantity: string;
  currency: string;
};

export type CouponResponseDto = {
  id: string;
  code: string;
  description?: string;
  discountType: string;
  value: number;
  minOrderAmount: number;
  maxDiscountAmount?: number;
  expiredAt: string;
  count: number;
  usageCount: number;
  isActive: boolean;
};

export type ShippingCalculationRequestDto = {
  city: string;
  district: string;
  ward?: string;
  subtotal: number;
  totalWeight?: number;
  distanceKm?: number;
};

export type ShippingCalculationResponseDto = {
  shippingFee: number;
  zoneName?: string;
  estimatedDeliveryDate: string;
  currency: string;
};

export type CreateOrderResponse = {
  id: string;
  status?: string;
  subtotal?: number;
  totalQuantity?: number;  totalItems?: number;  displayQuantity?: string;
  currency?: string;
};

export type CreateStripePaymentIntentResponse = {
  paymentId: string;
  paymentIntentId: string;
  clientSecret: string;
};

export type StripeConfigResponse = {
  publishableKey: string;
};

export type MarkStripePaymentPaidResponse = {
  ok: boolean;
  orderId: string;
  paymentId: string;
  status?: string;
};

export type SyncStripePaymentIntentResponse = {
  ok: boolean;
  orderId: string;
  paymentId: string;
  stripePaymentIntentStatus?: string;
  paymentStatus?: string;
  orderStatus?: string;
};

export enum BehaviorType {
  Unknown = 0,
  ProductView = 1,
  AddToCart = 2,
  RemoveFromCart = 3,
  AddToWishlist = 4,
  RemoveFromWishlist = 5,
  Purchase = 6,
  Review = 7,
  Search = 8,
}

export type TrackRequestDto = {
  behaviorType: BehaviorType;
  productId?: string;
  categoryId?: string;
  orderId?: string;
  quantity?: number;
  value?: number;
  currency?: string;
  metadata?: string;
};

export type PurchasedProductDto = {
  orderId: string;
  productId: string;
  productName: string;
  productImageUrl?: string | null;
  /** ISO datetime string */
  purchasedAt: string;
  isReviewed: boolean;
  reviewId?: string | null;
};

export type ConversationDto = {
  id: string;
  status: string;
  startedAt: string;
  endedAt?: string | null;
  lastMessageAt: string;
  lastMessagePreview?: string | null;
};

export type ChatMessageDto = {
  id: string;
  conversationId: string;
  sender: string;
  content: string;
  metadata?: Record<string, unknown> | null;
  createdAt: string;
};

export type WishlistItem = {
  productId: string;
};

export type OrderListItem = {
  id: string;
  createdAt: string;
  status: string;
  totalAmount: number;
  shippingReceiverName: string;
  shippingPhone: string;
  shippingAddressLine: string;
  shippingCity: string;
  shippingDistrict: string;
  shippingWard: string;
};

export type ReviewableProduct = {
  orderId: string;
  productId: string;
  productName: string;
  productImageUrl?: string | null;
  purchasedAt: string;
  isReviewed: boolean;
  reviewId?: string | null;
};

export type PagedResult<T> = {
  items: T[];
  totalItems: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
};

export type BackendPagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type NotificationDto = {
  id: string;
  channel: string;
  type: string;
  title: string;
  message: string;
  linkUrl?: string | null;
  isRead: boolean;
  readAt?: string | null;
  createdAt: string;
};

export type NotificationPageDto = BackendPagedResult<NotificationDto>;

export type UnreadCountDto = {
  count: number;
};

export type NotificationPreferenceDto = {
  orderStatusEmailsEnabled: boolean;
  paymentEmailsEnabled: boolean;
  marketingEmailsEnabled: boolean;
  inAppNotificationsEnabled: boolean;
};

export type AdminDashboardOverviewDto = {
  totalUsers: number;
  totalCustomers: number;
  totalSellers: number;
  totalProducts: number;
  pendingProducts: number;
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  totalRevenue: number;
  monthlyRevenue: number;
  topProducts: AdminTopProductDto[];
  topSellers: AdminTopSellerDto[];
  recentOrders: AdminOrderListItemDto[];
  recentUsers: AdminUserDto[];
};

export type AdminUserDto = {
  id: string;
  keycloakId: string;
  email: string;
  fullName: string;
  role?: string | null;
  status?: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string | null;
};

export type AdminSellerDto = {
  userId: string;
  sellerProfileId?: string | null;
  ownerName: string;
  ownerEmail: string;
  shopName: string;
  status: string;
  totalSales: number;
  totalRevenue: number;
  rating?: number | null;
  commissionRate: number;
  createdAt: string;
};

export type AdminSellerStatsDto = {
  userId: string;
  shopName: string;
  totalProducts: number;
  approvedProducts: number;
  pendingProducts: number;
  totalOrders: number;
  completedOrders: number;
  totalRevenue: number;
};

export type AdminProductDto = {
  id: string;
  name: string;
  slug: string;
  sellerName: string;
  shopName?: string | null;
  categoryName?: string | null;
  price: number;
  approvalStatus: string;
  isActive: boolean;
  adminNotes?: string | null;
  createdAt: string;
};

export type AdminOrderListItemDto = {
  id: string;
  userId: string;
  customerName: string;
  customerEmail: string;
  totalAmount: number;
  currency: string;
  status: string;
  itemsCount: number;
  createdAt: string;
};

export type AdminTopProductDto = {
  productId: string;
  productName: string;
  soldCount: number;
  revenue: number;
};

export type AdminTopSellerDto = {
  sellerId: string;
  sellerName: string;
  shopName?: string | null;
  totalSales: number;
  revenue: number;
};

export type AdminRevenuePointDto = {
  label: string;
  revenue: number;
  orders: number;
};

export type AdminSalesBreakdownDto = {
  revenueByPeriod: AdminRevenuePointDto[];
  salesByStatus: Array<{ key: string; value: number }>;
};

export type AdminNotificationActionResultDto = {
  inAppNotificationsCreated: number;
  emailsQueued: number;
};

export type AdminLowStockNotificationResultDto = {
  productsMatched: number;
  adminNotificationsCreated: number;
};

export type AdminCategoryDto = {
  id: string;
  name: string;
  parentCategoryId?: string | null;
  slug: string;
  sortOrder: number;
  isActive: boolean;
};

export type AdminCategoryUpsertDto = {
  name: string;
  parentCategoryId?: string | null;
  slug?: string | null;
  sortOrder: number;
  isActive: boolean;
};

export type SellerDashboardOverviewDto = {
  myProducts: number;
  pendingOrders: number;
  processingOrders: number;
  shippedOrders: number;
  deliveredOrders: number;
  totalRevenue: number;
  thisMonthRevenue: number;
  lowStockProducts: SellerLowStockProductDto[];
  recentOrders: SellerOrderListItemDto[];
  bestSellingProducts: SellerProductListItemDto[];
};

export type SellerProductListItemDto = {
  id: string;
  name: string;
  slug: string;
  categoryName?: string | null;
  price: number;
  stock: number;
  approvalStatus: string;
  isActive: boolean;
  createdAt: string;
};

export type SellerOrderListItemDto = {
  id: string;
  customerName: string;
  customerEmail: string;
  sellerItemsCount: number;
  sellerItemsTotal: number;
  status: string;
  createdAt: string;
};

export type SellerRevenuePointDto = {
  label: string;
  revenue: number;
  orders: number;
};

export type SellerLowStockProductDto = {
  productId: string;
  productName: string;
  stock: number;
};

export type SellerProfileDto = {
  id: string;
  userId: string;
  shopName: string;
  shopDescription?: string | null;
  shopLogoUrl?: string | null;
  shopBannerUrl?: string | null;
  contactPhone?: string | null;
  contactEmail?: string | null;
  address?: string | null;
  city?: string | null;
  status: string;
  commissionRate: number;
  rating?: number | null;
  totalSales: number;
  totalRevenue: number;
};

export type SellerProductUpsertDto = {
  name: string;
  description: string;
  basePrice: number;
  categoryId: string;
  brandId?: string | null;
  isActive: boolean;
  images?: Array<{ imageUrl: string; isPrimary: boolean }>;
  variants?: Array<{
    sku: string;
    price: number;
    stockQuantity: number;
    isActive: boolean;
    attributeValueIds?: string[] | null;
  }>;
};

export type SellerProfileUpsertDto = {
  shopName: string;
  shopDescription?: string | null;
  shopLogoUrl?: string | null;
  shopBannerUrl?: string | null;
  contactPhone?: string | null;
  contactEmail?: string | null;
  address?: string | null;
  city?: string | null;
};

export type ShopbeApiClientOptions = {
  /** Default timeout used when an endpoint doesn't provide one. */
  defaultTimeoutMs?: number;
};

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/+$/, "") ||
  // Shopbe.Web dev profile (see Shopbe.Web/Properties/launchSettings.json)
  "http://localhost:5072";

export { API_BASE_URL };

export function resolveApiUrl(value?: string | null): string | undefined {
  if (!value) return undefined;
  if (/^https?:\/\//i.test(value)) return value;

  try {
    return new URL(value, API_BASE_URL).toString();
  } catch {
    return value;
  }
}

const DEFAULT_TIMEOUT_MS = 15000;

export function isAbortError(err: unknown): boolean {
  const anyErr = asRecord(err);
  if (!anyErr) return false;
  // Browser fetch + Node/undici typically use name === 'AbortError'
  if (anyErr.name === "AbortError") return true;
  // Some environments surface abort as DOMException-ish
  if (anyErr.code === "ABORT_ERR") return true;
  const msg: unknown = anyErr.message;
  if (typeof msg === "string" && /aborted|abort/i.test(msg)) return true;
  return false;
}

function isLikelyNetworkError(err: unknown): boolean {
  const anyErr = asRecord(err);
  if (!anyErr) return false;
  const msg = typeof anyErr.message === "string" ? anyErr.message : "";
  // Browser: 'Failed to fetch'. Node (undici): 'fetch failed'.
  return (
    anyErr.name === "TypeError" &&
    (msg.includes("Failed to fetch") || msg.includes("fetch failed"))
  );
}

function getJwtSubject(accessToken: string): string | undefined {
  try {
    const payload = accessToken.split(".")[1];
    if (!payload) return undefined;

    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = normalized.padEnd(
      normalized.length + ((4 - (normalized.length % 4)) % 4),
      "="
    );
    const decoded =
      typeof window === "undefined"
        ? Buffer.from(padded, "base64").toString("utf8")
        : atob(padded);
    const claims = asRecord(JSON.parse(decoded));
    const sub = claims?.sub;

    return typeof sub === "string" && sub.trim() ? sub : undefined;
  } catch {
    return undefined;
  }
}

function withQuery(path: string, query: Record<string, string | undefined>) {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(query)) {
    if (value) params.set(key, value);
  }

  const queryString = params.toString();
  return queryString ? `${path}?${queryString}` : path;
}

async function fetchWithTimeout(
  url: string,
  init: RequestInit,
  timeoutMs: number
): Promise<Response> {
  if (!timeoutMs || timeoutMs <= 0) return fetch(url, init);

  const controller = new AbortController();
  const callerSignal = init.signal;

  // Propagate caller abort into our controller so either abort cancels the request.
  if (callerSignal) {
    if (callerSignal.aborted) {
      controller.abort();
    } else {
      callerSignal.addEventListener("abort", () => controller.abort(), {
        once: true,
      });
    }
  }

  const timer = setTimeout(() => {
    // AbortController.abort(reason) isn't supported everywhere; keep it simple.
    controller.abort();
  }, timeoutMs);

  try {
    return await fetch(url, {
      ...init,
      signal: controller.signal,
    });
  } finally {
    clearTimeout(timer);
  }
}

async function requestJson<T>(
  path: string,
  opts: {
    method?: string;
    accessToken?: string | null;
    body?: unknown;
    signal?: AbortSignal;
    timeoutMs?: number;
  } = {}
): Promise<T> {
  const url = `${API_BASE_URL}${path.startsWith("/") ? "" : "/"}${path}`;

  const headers: Record<string, string> = {
    Accept: "application/json",
  };
  if (opts.body !== undefined) headers["Content-Type"] = "application/json";
  if (opts.accessToken) headers["Authorization"] = `Bearer ${opts.accessToken}`;

  let res: Response;
  try {
    res = await fetchWithTimeout(
      url,
      {
        method: opts.method ?? (opts.body !== undefined ? "POST" : "GET"),
        headers,
        body: opts.body === undefined ? undefined : JSON.stringify(opts.body),
        signal: opts.signal,
        cache: "no-store",
      },
      opts.timeoutMs ?? DEFAULT_TIMEOUT_MS
    );
  } catch (e: unknown) {
    // Common when navigating away / component unmounts.
    if (isAbortError(e)) throw e;

    if (isLikelyNetworkError(e)) {
      throw new Error(
        `Cannot reach API at ${API_BASE_URL}. Is the backend running and accessible from the browser? (${errorMessage(
          e,
          "network error"
        )})`
      );
    }

    throw e;
  }

  // Many endpoints are wrapped by ApiResponseEnvelopeFilter.
  // If they are not, we fall back to reading plain JSON payload.
  const text = await res.text();
  let json: unknown = null;
  if (text) {
    try {
      json = JSON.parse(text) as unknown;
    } catch {
      // Non-JSON response (e.g. proxy error page). Keep raw text for error message.
      json = null;
    }
  }

  if (!res.ok) {
    const obj = asRecord(json);
    const pick = (key: string) => {
      const v = obj?.[key];
      return typeof v === "string" ? v : undefined;
    };
    const message =
      pick("message") ||
      pick("detail") ||
      pick("title") ||
      pick("error") ||
      (text && text.slice(0, 500)) ||
      res.statusText;
    throw new Error(`API ${res.status} ${res.statusText} (${path}): ${message}`);
  }

  const jsonObj = asRecord(json);
  if (jsonObj && "data" in jsonObj) {
    return (jsonObj as unknown as ApiEnvelope<T>).data;
  }

  return json as T;
}

export function productResponseToListItem(item: unknown): ProductListItem {
  const obj = asRecord(item) ?? {};

  const pickValue = (k: string): unknown => obj[k] ?? obj[k.charAt(0).toUpperCase() + k.slice(1)];

  const pickString = (k: string): string | undefined => {
    const v = pickValue(k);
    return typeof v === "string" && v.trim() ? v : undefined;
  };

  const pickStringOrNull = (k: string): string | null | undefined => {
    const v = pickValue(k);
    if (typeof v === "string") return v.trim() ? v : undefined;
    if (v === null) return null;
    return undefined;
  };

  const pickNumber = (k: string): number | undefined => {
    const v = pickValue(k);
    return typeof v === "number" ? v : undefined;
  };

  const pickBoolean = (k: string): boolean => {
    const v = pickValue(k);
    return typeof v === "boolean" ? v : false;
  };

  const mapImages = (imgs: unknown): ProductImageDto[] | undefined => {
    if (!Array.isArray(imgs)) return undefined;
    return imgs.map((img: unknown) => {
      const i = asRecord(img) ?? {};
      const imageUrl = i.imageUrl ?? i.ImageUrl;
      return {
        id: ((i.id ?? i.Id) as string) ?? "",
        imageUrl: typeof imageUrl === "string" ? imageUrl : "",
        isPrimary: ((i.isPrimary ?? i.IsPrimary) as boolean) ?? false,
        altText: ((i.altText ?? i.AltText) as string | null) ?? null,
      };
    });
  };

  const mapVariants = (vars: unknown): ProductVariantDto[] | undefined => {
    if (!Array.isArray(vars)) return undefined;
    return vars.map((v: unknown) => {
      const r = asRecord(v) ?? {};
      return {
        id: (r.id as string) ?? "",
        productId: (r.productId as string) ?? "",
        sku: (r.sku as string) ?? null,
        price: (r.price as number) ?? 0,
        currency: (r.currency as string) ?? "VND",
        stockQuantity: (r.stockQuantity as number) ?? 0,
        isActive: (r.isActive as boolean) ?? true,
        attributes: Array.isArray(r.attributes) 
          ? (r.attributes as unknown[]).map(a => ({
              name: (asRecord(a)?.name as string) ?? "Attribute",
              value: (asRecord(a)?.value as string) ?? ""
            }))
          : [],
      };
    });
  };

  const images = mapImages(pickValue("images"));
  const primaryImageUrl =
    pickStringOrNull("primaryImageUrl") ??
    pickStringOrNull("imageUrl") ??
    images?.find((image) => image.isPrimary)?.imageUrl ??
    images?.[0]?.imageUrl;

  return {
    id: pickString("productId") ?? pickString("id") ?? "",
    name: pickString("name") ?? "",
    slug: pickString("slug") ?? "",
    description: pickStringOrNull("description"),
    price: pickNumber("price"),
    discountPrice: pickNumber("discountPrice"),
    currency: pickString("currency"),
    primaryImageUrl,
    totalStockQuantity: pickNumber("totalStockQuantity"),
    soldCount: pickNumber("soldCount"),
    averageRating: pickNumber("averageRating"),
    ratingCount: pickNumber("ratingCount"),
    categoryId: pickString("categoryId") ?? "",
    categoryName: pickStringOrNull("categoryName"),
    brandId: pickStringOrNull("brandId"),
    brandName: pickStringOrNull("brandName"),
    isActive: pickBoolean("isActive"),
    recommendationReason: pickStringOrNull("recommendationReason"),
    images,
    variants: mapVariants(obj.variants),
  };
}

export type CategoryFacetDto = {
  id: string;
  name: string;
  slug: string;
  count: number;
};

export type BrandFacetDto = {
  id: string;
  name: string;
  slug: string;
  count: number;
};

export type PublicCategoryDto = {
  id: string;
  name: string;
  parentCategoryId?: string | null;
  slug: string;
  sortOrder: number;
  isActive: boolean;
};

export type ProductSearchResponse = {
  products: ProductListItem[];
  categoryFacets: CategoryFacetDto[];
  brandFacets: BrandFacetDto[];
  totalCount: number;
};

export type ProductQueryDto = {
  name?: string;
  categoryIds?: string[];
  categorySlugs?: string[];
  brandIds?: string[];
  minBasePrice?: number;
  maxBasePrice?: number;
  minRating?: number;
  sortBy?: string;
  pageNumber?: number;
  pageSize?: number;
};

export function couponResponseToDto(item: unknown): CouponResponseDto {
  const obj = asRecord(item) ?? {};

  const pickString = (k: string): string | undefined => {
    const v = obj[k] ?? obj[k.charAt(0).toUpperCase() + k.slice(1)];
    return typeof v === "string" ? v : undefined;
  };

  const pickNumber = (k: string): number | undefined => {
    const v = obj[k] ?? obj[k.charAt(0).toUpperCase() + k.slice(1)];
    return typeof v === "number" ? v : undefined;
  };

  const pickBoolean = (k: string): boolean => {
    const v = obj[k] ?? obj[k.charAt(0).toUpperCase() + k.slice(1)];
    return typeof v === "boolean" ? v : false;
  };

  return {
    id: pickString("id") ?? "",
    code: pickString("code") ?? "",
    description: pickString("description"),
    discountType: pickString("discountType") ?? "",
    value: pickNumber("value") ?? 0,
    minOrderAmount: pickNumber("minOrderAmount") ?? 0,
    maxDiscountAmount: pickNumber("maxDiscountAmount"),
    expiredAt: pickString("expiredAt") ?? "",
    count: pickNumber("count") ?? 0,
    usageCount: pickNumber("usageCount") ?? 0,
    isActive: pickBoolean("isActive"),
  };
}

export type UserAddressResponseDto = {
  id: string;
  userId: string;
  receiverName: string;
  phone: string;
  addressLine: string;
  city: string;
  district: string;
  ward: string;
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
};

export type UserAddressRequestDto = {
  receiverName: string;
  phone: string;
  addressLine: string;
  city: string;
  district: string;
  ward: string;
  isDefault: boolean;
};

export type UserResponseDto = {
  id: string;
  keycloakId: string;
  email: string;
  fullName: string;
  avatarUrl?: string | null;
  phoneNumber?: string | null;
  gender?: string | null;
  birthday?: string | null;
  language?: string | null;
  country?: string | null;
  role?: number;
  status?: number;
  createdAt: string;
  updatedAt: string;
};

export type UserRequestDto = {
  fullName: string;
  email: string;
  avatarUrl?: string | null;
  phoneNumber?: string | null;
  gender?: string | null;
  birthday?: string | null;
  language?: string | null;
  country?: string | null;
};

export const shopbeApi = {
  notifications: {
    list: (
      accessToken: string,
      params?: { unreadOnly?: boolean; page?: number; pageSize?: number },
      signal?: AbortSignal
    ) => {
      const query = new URLSearchParams();
      if (params?.unreadOnly) query.append("unreadOnly", "true");
      if (params?.page) query.append("page", params.page.toString());
      if (params?.pageSize) query.append("pageSize", params.pageSize.toString());
      const queryString = query.toString();
      return requestJson<NotificationPageDto>(`/api/notifications${queryString ? `?${queryString}` : ""}`, {
        accessToken,
        signal,
      });
    },
    unreadCount: (accessToken: string, signal?: AbortSignal) =>
      requestJson<UnreadCountDto>("/api/notifications/unread-count", {
        accessToken,
        signal,
      }),
    markRead: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<void>(`/api/notifications/${id}/read`, {
        accessToken,
        method: "PUT",
        signal,
      }),
    markAllRead: (accessToken: string, signal?: AbortSignal) =>
      requestJson<void>("/api/notifications/read-all", {
        accessToken,
        method: "PUT",
        signal,
      }),
    getPreferences: (accessToken: string, signal?: AbortSignal) =>
      requestJson<NotificationPreferenceDto>("/api/notifications/preferences", {
        accessToken,
        signal,
      }),
    updatePreferences: (accessToken: string, body: NotificationPreferenceDto, signal?: AbortSignal) =>
      requestJson<NotificationPreferenceDto>("/api/notifications/preferences", {
        accessToken,
        method: "PUT",
        body,
        signal,
      }),
  },
  admin: {
    dashboardOverview: (accessToken: string, signal?: AbortSignal) =>
      requestJson<AdminDashboardOverviewDto>("/api/admin/dashboard/overview", { accessToken, signal }),
    users: (accessToken: string, query?: Record<string, string | undefined>, signal?: AbortSignal) =>
      requestJson<BackendPagedResult<AdminUserDto>>(withQuery("/api/admin/users", query ?? {}), { accessToken, signal }),
    userById: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<AdminUserDto>(`/api/admin/users/${id}`, { accessToken, signal }),
    updateUserStatus: (accessToken: string, id: string, status: string, signal?: AbortSignal) =>
      requestJson<AdminUserDto>(`/api/admin/users/${id}/status`, { accessToken, method: "PUT", body: { status }, signal }),
    updateUserRole: (accessToken: string, id: string, role: string, signal?: AbortSignal) =>
      requestJson<AdminUserDto>(`/api/admin/users/${id}/role`, { accessToken, method: "PUT", body: { role }, signal }),
    deleteUser: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<void>(`/api/admin/users/${id}`, { accessToken, method: "DELETE", signal }),
    sellers: (accessToken: string, query?: Record<string, string | undefined>, signal?: AbortSignal) =>
      requestJson<BackendPagedResult<AdminSellerDto>>(withQuery("/api/admin/sellers", query ?? {}), { accessToken, signal }),
    sellerById: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<AdminSellerDto>(`/api/admin/sellers/${id}`, { accessToken, signal }),
    updateSellerStatus: (accessToken: string, id: string, status: string, signal?: AbortSignal) =>
      requestJson<AdminSellerDto>(`/api/admin/sellers/${id}/status`, { accessToken, method: "PUT", body: { status }, signal }),
    sellerStats: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<AdminSellerStatsDto>(`/api/admin/sellers/${id}/stats`, { accessToken, signal }),
    products: (accessToken: string, query?: Record<string, string | undefined>, signal?: AbortSignal) =>
      requestJson<BackendPagedResult<AdminProductDto>>(withQuery("/api/admin/products", query ?? {}), { accessToken, signal }),
    updateProductApproval: (accessToken: string, id: string, status: string, adminNotes?: string, signal?: AbortSignal) =>
      requestJson<AdminProductDto>(`/api/admin/products/${id}/approval`, { accessToken, method: "PUT", body: { status, adminNotes }, signal }),
    updateProductVisibility: (accessToken: string, id: string, isActive: boolean, signal?: AbortSignal) =>
      requestJson<AdminProductDto>(`/api/admin/products/${id}/visibility`, { accessToken, method: "PUT", body: { isActive }, signal }),
    deleteProduct: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<void>(`/api/admin/products/${id}`, { accessToken, method: "DELETE", signal }),
    orders: (accessToken: string, query?: Record<string, string | undefined>, signal?: AbortSignal) =>
      requestJson<BackendPagedResult<AdminOrderListItemDto>>(withQuery("/api/admin/orders", query ?? {}), { accessToken, signal }),
    orderById: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<unknown>(`/api/admin/orders/${id}`, { accessToken, signal }),
    revenue: (accessToken: string, period = "monthly", signal?: AbortSignal) =>
      requestJson<AdminRevenuePointDto[]>(`/api/admin/analytics/revenue?period=${encodeURIComponent(period)}`, { accessToken, signal }),
    sales: (accessToken: string, period = "monthly", signal?: AbortSignal) =>
      requestJson<AdminSalesBreakdownDto>(`/api/admin/analytics/sales?period=${encodeURIComponent(period)}`, { accessToken, signal }),
    topProducts: (accessToken: string, take = 10, signal?: AbortSignal) =>
      requestJson<AdminTopProductDto[]>(`/api/admin/analytics/top-products?take=${take}`, { accessToken, signal }),
    topSellers: (accessToken: string, take = 10, signal?: AbortSignal) =>
      requestJson<AdminTopSellerDto[]>(`/api/admin/analytics/top-sellers?take=${take}`, { accessToken, signal }),
    sendMarketingNotification: (
      accessToken: string,
      body: { title: string; message: string; emailSubject?: string },
      signal?: AbortSignal
    ) =>
      requestJson<AdminNotificationActionResultDto>("/api/admin/notifications/marketing", {
        accessToken,
        body,
        signal,
      }),
    createLowStockAlerts: (accessToken: string, threshold = 10, signal?: AbortSignal) =>
      requestJson<AdminLowStockNotificationResultDto>(`/api/admin/notifications/low-stock?threshold=${threshold}`, {
        accessToken,
        method: "POST",
        signal,
      }),
    categories: (accessToken: string, query?: Record<string, string | undefined>, signal?: AbortSignal) =>
      requestJson<AdminCategoryDto[]>(withQuery("/api/admin/categories", query ?? {}), { accessToken, signal }),
    createCategory: (accessToken: string, body: AdminCategoryUpsertDto, signal?: AbortSignal) =>
      requestJson<AdminCategoryDto>("/api/admin/categories", { accessToken, method: "POST", body, signal }),
    updateCategory: (accessToken: string, id: string, body: AdminCategoryUpsertDto, signal?: AbortSignal) =>
      requestJson<AdminCategoryDto>(`/api/admin/categories/${id}`, { accessToken, method: "PUT", body, signal }),
    deleteCategory: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<void>(`/api/admin/categories/${id}`, { accessToken, method: "DELETE", signal }),
  },
  seller: {
    dashboardOverview: (accessToken: string, signal?: AbortSignal) =>
      requestJson<SellerDashboardOverviewDto>("/api/seller/dashboard/overview", { accessToken, signal }),
    products: (accessToken: string, query?: Record<string, string | undefined>, signal?: AbortSignal) =>
      requestJson<BackendPagedResult<SellerProductListItemDto>>(withQuery("/api/seller/products", query ?? {}), { accessToken, signal }),
    createProduct: (accessToken: string, body: SellerProductUpsertDto, signal?: AbortSignal) =>
      requestJson<SellerProductListItemDto>("/api/seller/products", { accessToken, method: "POST", body, signal }),
    updateProduct: (accessToken: string, id: string, body: SellerProductUpsertDto, signal?: AbortSignal) =>
      requestJson<SellerProductListItemDto>(`/api/seller/products/${id}`, { accessToken, method: "PUT", body, signal }),
    deleteProduct: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<void>(`/api/seller/products/${id}`, { accessToken, method: "DELETE", signal }),
    orders: (accessToken: string, query?: Record<string, string | undefined>, signal?: AbortSignal) =>
      requestJson<BackendPagedResult<SellerOrderListItemDto>>(withQuery("/api/seller/orders", query ?? {}), { accessToken, signal }),
    orderById: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<unknown>(`/api/seller/orders/${id}`, { accessToken, signal }),
    updateOrderStatus: (accessToken: string, id: string, status: string, note?: string, signal?: AbortSignal) =>
      requestJson<unknown>(`/api/seller/orders/${id}/status`, { accessToken, method: "PUT", body: { status, note }, signal }),
    revenue: (accessToken: string, period = "monthly", signal?: AbortSignal) =>
      requestJson<SellerRevenuePointDto[]>(`/api/seller/analytics/revenue?period=${encodeURIComponent(period)}`, { accessToken, signal }),
    sales: (accessToken: string, period = "monthly", signal?: AbortSignal) =>
      requestJson<SellerRevenuePointDto[]>(`/api/seller/analytics/sales?period=${encodeURIComponent(period)}`, { accessToken, signal }),
    lowStock: (accessToken: string, threshold = 10, signal?: AbortSignal) =>
      requestJson<SellerLowStockProductDto[]>(`/api/seller/analytics/low-stock?threshold=${threshold}`, { accessToken, signal }),
    profile: (accessToken: string, signal?: AbortSignal) =>
      requestJson<SellerProfileDto>("/api/seller/profile", { accessToken, signal }),
    updateProfile: (accessToken: string, body: SellerProfileUpsertDto, signal?: AbortSignal) =>
      requestJson<SellerProfileDto>("/api/seller/profile", { accessToken, method: "PUT", body, signal }),
  },
  users: {
    getMe: (accessToken: string, signal?: AbortSignal) =>
      requestJson<UserResponseDto>(
        withQuery("/api/users/by-keycloak", {
          keycloakId: getJwtSubject(accessToken),
        }),
        { accessToken, signal }
      ),
    sync: (accessToken: string, body: UserRequestDto, signal?: AbortSignal) =>
      requestJson<UserResponseDto>("/api/users/sync", {
        accessToken,
        method: "POST",
        body,
        signal,
      }),
    uploadAvatar: async (accessToken: string, file: File) => {
      const formData = new FormData();
      formData.append("files", file);

      const res = await fetch(`${API_BASE_URL}/api/reviews/images`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${accessToken}`
        },
        body: formData
      });

      if (!res.ok) throw new Error("Upload failed");
      const data = await res.json();
      // data is wrapped in { data: [...] } if ApiResponseEnvelopeFilter is active
      const images = data.data || data;
      return images[0].url;
    }
  },
  recommendations: {
    topSelling: (limit: number, signal?: AbortSignal) =>
      requestJson<unknown[]>(`/api/recommendations/top-selling?limit=${limit}`, { signal }),
    me: (accessToken: string, limit: number, signal?: AbortSignal) =>
      requestJson<unknown[]>(`/api/recommendations/me?limit=${limit}`, { accessToken, signal }),
    similar: (productId: string, limit: number, signal?: AbortSignal) =>
      requestJson<unknown[]>(`/api/recommendations/products/${productId}/similar?limit=${limit}`, { signal }),
    frequentlyBoughtTogether: (productId: string, limit: number, signal?: AbortSignal) =>
      requestJson<unknown[]>(`/api/recommendations/products/${productId}/frequently-bought-together?limit=${limit}`, { signal }),
    recentlyViewed: (accessToken: string, limit: number, signal?: AbortSignal) =>
      requestJson<unknown[]>(`/api/recommendations/me/recently-viewed?limit=${limit}`, { accessToken, signal }),
  },
  chat: {
    getConversations: (accessToken: string, signal?: AbortSignal) =>
      requestJson<ConversationDto[]>("/api/chat/conversations", { accessToken, signal }),
    createConversation: (accessToken: string, body: { title?: string }, signal?: AbortSignal) =>
      requestJson<ConversationDto>("/api/chat/conversations", {
        accessToken,
        body,
        signal,
      }),
    getMessages: (
      accessToken: string,
      conversationId: string,
      params?: { after?: string; take?: number },
      signal?: AbortSignal
    ) => {
      const query = new URLSearchParams();
      if (params?.after) query.append("after", params.after);
      if (params?.take) query.append("take", params.take.toString());
      const queryString = query.toString();
      return requestJson<ChatMessageDto[]>(
        `/api/chat/conversations/${conversationId}/messages${queryString ? `?${queryString}` : ""}`,
        { accessToken, signal }
      );
    },
    sendMessage: (
      accessToken: string,
      conversationId: string,
      body: { content: string; metadata?: Record<string, unknown> },
      signal?: AbortSignal
    ) =>
      requestJson<ChatMessageDto[]>(`/api/chat/conversations/${conversationId}/messages`, {
        accessToken,
        body,
        signal,
      }),
  },
  simulation: {
    run: (accessToken: string, signal?: AbortSignal) =>
      requestJson<{
        message: string;
        profileCategories: string[];
        seedProduct: string;
        boughtWith: string;
      }>("/api/simulation/run", {
        accessToken,
        method: "POST",
        signal,
      }),
  },
  tracking: {
    track: (body: TrackRequestDto, accessToken?: string | null, signal?: AbortSignal) =>
      requestJson<void>("/api/tracking/track", {
        accessToken,
        body,
        signal,
      }),
  },
  products: {
    list: (filter?: ProductQueryDto, accessToken?: string, signal?: AbortSignal) => {
      const params = new URLSearchParams();
      if (filter?.name) params.append("name", filter.name);
      if (filter?.categoryIds) {
        filter.categoryIds.forEach((id) => params.append("categoryIds", id));
      }
      if (filter?.categorySlugs) {
        filter.categorySlugs.forEach((slug) => params.append("categorySlugs", slug));
      }
      if (filter?.brandIds) {
        filter.brandIds.forEach((id) => params.append("brandIds", id));
      }
      if (filter?.minBasePrice) params.append("minBasePrice", filter.minBasePrice.toString());
      if (filter?.maxBasePrice) params.append("maxBasePrice", filter.maxBasePrice.toString());
      if (filter?.minRating) params.append("minRating", filter.minRating.toString());
      if (filter?.sortBy) params.append("sortBy", filter.sortBy);
      if (filter?.pageNumber) params.append("pageNumber", filter.pageNumber.toString());
      if (filter?.pageSize) params.append("pageSize", filter.pageSize.toString());

      const queryString = params.toString();
      return requestJson<ProductSearchResponse>(`/api/products${queryString ? `?${queryString}` : ""}`, {
        accessToken,
        signal,
      });
    },
    getById: (id: string, accessToken?: string, signal?: AbortSignal) =>
      requestJson<ProductDetail>(`/api/products/${id}`, {
        accessToken,
        signal,
      }),
    discover: (limit: number, excludeIds?: string[], signal?: AbortSignal) => {
      const ids = excludeIds?.join(",") ?? "";
      return requestJson<ProductListItem[]>(`/api/products/discover?limit=${limit}&excludeIds=${ids}`, { signal });
    },
  },
  categories: {
    list: (signal?: AbortSignal) =>
      requestJson<PublicCategoryDto[]>("/api/categories", { signal }),
    getById: (id: string, signal?: AbortSignal) =>
      requestJson<PublicCategoryDto>(`/api/categories/${id}`, { signal }),
  },
  reviews: {
    getMyReviewableProducts: (accessToken: string, onlyNotReviewed = false, signal?: AbortSignal) =>
      requestJson<ReviewableProduct[]>(`/api/reviews/me/reviewable-products?onlyNotReviewed=${onlyNotReviewed}`, {
        accessToken,
        signal,
      }),
  },
  coupons: {
    list: async (signal?: AbortSignal) => {
      const data = await requestJson<unknown[]>("/api/coupons", { signal });
      return data.map(couponResponseToDto);
    },
  },
  shipping: {
    calculate: (body: ShippingCalculationRequestDto, signal?: AbortSignal) =>
      requestJson<ShippingCalculationResponseDto>("/api/shipping-zones/calculate", {
        method: "POST",
        body,
        signal,
      }),
  },
  userAddresses: {
    getMyAddresses: (accessToken: string, signal?: AbortSignal) =>
      requestJson<UserAddressResponseDto[]>("/api/user-addresses/me", {
        accessToken,
        signal,
      }),
    create: (accessToken: string, body: UserAddressRequestDto, signal?: AbortSignal) =>
      requestJson<UserAddressResponseDto>("/api/user-addresses", {
        accessToken,
        method: "POST",
        body,
        signal,
      }),
    update: (accessToken: string, id: string, body: UserAddressRequestDto, signal?: AbortSignal) =>
      requestJson<UserAddressResponseDto>(`/api/user-addresses/${id}`, {
        accessToken,
        method: "PUT",
        body,
        signal,
      }),
    delete: (accessToken: string, id: string, signal?: AbortSignal) =>
      requestJson<void>(`/api/user-addresses/${id}`, {
        accessToken,
        method: "DELETE",
        signal,
      }),
  },
  cart: {
    getMyCart: (accessToken: string, signal?: AbortSignal) =>
      requestJson<CartDto>("/api/cart", { accessToken, signal }),
    addItem: (
      accessToken: string,
      body: { productId: string; productVariantId: string; quantity: number },
      signal?: AbortSignal
    ) =>
      requestJson<CartDto>("/api/cart/items", {
        accessToken,
        body,
        signal,
      }),
    setQuantity: (
      accessToken: string,
      productVariantId: string,
      body: { quantity: number },
      signal?: AbortSignal
    ) =>
      requestJson<CartDto>(`/api/cart/items/${productVariantId}`, {
        accessToken,
        method: "PUT",
        body,
        signal,
      }),
    removeItem: (
      accessToken: string,
      productVariantId: string,
      signal?: AbortSignal
    ) =>
      requestJson<CartDto>(`/api/cart/items/${productVariantId}`, {
        accessToken,
        method: "DELETE",
        signal,
      }),
    clear: (accessToken: string, signal?: AbortSignal) =>
      requestJson<CartDto>("/api/cart", {
        accessToken,
        method: "DELETE",
        signal,
      }),
    applyCoupon: (accessToken: string, code: string, signal?: AbortSignal) =>
      requestJson<CartDto>(`/api/cart/coupon?code=${encodeURIComponent(code)}`, {
        accessToken,
        method: "POST",
        signal,
      }),
    removeCoupon: (accessToken: string, signal?: AbortSignal) =>
      requestJson<CartDto>("/api/cart/coupon", {
        accessToken,
        method: "DELETE",
        signal,
      }),
  },
  orders: {
    create: (
      accessToken: string,
      body: {
        userAddressId?: string;
        useDefaultAddressIfAvailable?: boolean;
        shippingReceiverName?: string;
        shippingPhone?: string;
        shippingAddressLine?: string;
        shippingCity?: string;
        shippingDistrict?: string;
        shippingWard?: string;
        note?: string;
        couponCode?: string;
        selectedItems?: Array<{ productVariantId: string; quantity: number }>;
      },
      signal?: AbortSignal
    ) =>
      requestJson<CreateOrderResponse>("/api/orders", {
        accessToken,
        body,
        signal,
      }),

    getMyOrders: (
      accessToken: string,
      params?: { page?: number; pageSize?: number },
      signal?: AbortSignal
    ) => {
      const query = new URLSearchParams();
      if (params?.page) query.append("page", params.page.toString());
      if (params?.pageSize) query.append("pageSize", params.pageSize.toString());
      const queryString = query.toString();
      return requestJson<PagedResult<OrderListItem>>(`/api/orders${queryString ? `?${queryString}` : ""}`, {
        accessToken,
        signal,
      });
    },

    purchasedProducts: (
      accessToken: string,
      opts?: { onlyNotReviewed?: boolean; signal?: AbortSignal }
    ) => {
      const onlyNotReviewed = opts?.onlyNotReviewed ? "true" : "false";
      return requestJson<PurchasedProductDto[]>(
        `/api/orders/me/purchased-products?onlyNotReviewed=${onlyNotReviewed}`,
        {
          accessToken,
          signal: opts?.signal,
        }
      );
    },
  },
  payments: {
    /** Public config used by frontend to initialize Stripe.js safely. */
    getStripeConfig: (signal?: AbortSignal) =>
      requestJson<StripeConfigResponse>("/api/payments/stripe/config", {
        signal,
      }),

    createStripePaymentIntent: (
      accessToken: string,
      body: { orderId: string },
      signal?: AbortSignal
    ) =>
      requestJson<CreateStripePaymentIntentResponse>(
        "/api/payments/stripe/payment-intents",
        {
          accessToken,
          body,
          signal,
        }
      ),

    /** Development-only helper. Backend returns 404 outside Development environment. */
    markStripePaymentPaidDev: (
      accessToken: string,
      body: { orderId: string; paymentIntentId?: string },
      signal?: AbortSignal
    ) =>
      requestJson<MarkStripePaymentPaidResponse>(
        "/api/payments/stripe/test/mark-paid",
        {
          accessToken,
          body,
          signal,
        }
      ),

    /** Sync a PaymentIntent status from Stripe and update local payment/order (useful if webhooks aren't configured). */
    syncStripePaymentIntent: (
      accessToken: string,
      paymentIntentId: string,
      signal?: AbortSignal
    ) =>
      requestJson<SyncStripePaymentIntentResponse>(
        `/api/payments/stripe/payment-intents/${encodeURIComponent(
          paymentIntentId
        )}/sync`,
        {
          accessToken,
          method: "POST",
          signal,
        }
      ),
  },
  wishlist: {
    get: async (accessToken: string, params?: { sortBy?: string; inStockOnly?: boolean; pageNumber?: number; pageSize?: number }, signal?: AbortSignal) => {
      const query = new URLSearchParams();
      if (params?.sortBy) query.append("sortBy", params.sortBy);
      if (params?.inStockOnly !== undefined) query.append("inStockOnly", params.inStockOnly.toString());
      if (params?.pageNumber) query.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) query.append("pageSize", params.pageSize.toString());
      
      const queryString = query.toString();
      return requestJson<WishlistItem[]>(`/api/wishlist${queryString ? `?${queryString}` : ""}`, {
        accessToken,
        signal,
      });
    },
    add: (accessToken: string, productId: string, signal?: AbortSignal) =>
      requestJson<unknown>("/api/wishlist/items", {
        accessToken,
        method: "POST",
        body: { productId },
        signal,
      }),
    remove: (accessToken: string, productId: string, signal?: AbortSignal) =>
      requestJson<boolean>(`/api/wishlist/items/${productId}`, {
        accessToken,
        method: "DELETE",
        signal,
      }),
    bulkRemove: (accessToken: string, productIds: string[], signal?: AbortSignal) =>
      requestJson<boolean>("/api/wishlist/items/bulk", {
        accessToken,
        method: "DELETE",
        body: productIds,
        signal,
      }),
  },
};
