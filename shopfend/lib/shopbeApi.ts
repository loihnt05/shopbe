export type ApiEnvelope<T> = {
  data: T;
};

import { asRecord, errorMessage } from "./errors";

export type ProductImageDto = {
  id: string;
  imageUrl: string;
  isPrimary: boolean;
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

export type ProductVariantDto = {
  id: string;
  productId: string;
  sku?: string | null;
  price: number;
  currency: string;
  stockQuantity?: number | null;
  isActive: boolean;
  attributeValues?: string[];
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
  metadata?: any | null;
  createdAt: string;
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

  const pickString = (k: string): string | undefined => {
    const v = obj[k];
    return typeof v === "string" ? v : undefined;
  };

  const pickStringOrNull = (k: string): string | null | undefined => {
    const v = obj[k];
    if (typeof v === "string") return v;
    if (v === null) return null;
    return undefined;
  };

  const pickNumber = (k: string): number | undefined => {
    const v = obj[k];
    return typeof v === "number" ? v : undefined;
  };

  const pickBoolean = (k: string): boolean => {
    const v = obj[k];
    return typeof v === "boolean" ? v : false;
  };

  const mapImages = (imgs: unknown): ProductImageDto[] | undefined => {
    if (!Array.isArray(imgs)) return undefined;
    return imgs.map((img: unknown) => {
      const i = asRecord(img) ?? {};
      return {
        id: (i.id as string) ?? "",
        imageUrl: (i.imageUrl as string) ?? "",
        isPrimary: (i.isPrimary as boolean) ?? false,
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
        attributeValues: Array.isArray(r.attributeValues) ? (r.attributeValues as string[]) : [],
      };
    });
  };

  return {
    id: pickString("id") ?? "",
    name: pickString("name") ?? "",
    slug: pickString("slug") ?? "",
    description: pickStringOrNull("description"),
    price: pickNumber("price"),
    discountPrice: pickNumber("discountPrice"),
    currency: pickString("currency"),
    primaryImageUrl: pickStringOrNull("primaryImageUrl") ?? pickStringOrNull("imageUrl"),
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
    images: mapImages(obj.images),
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

export const shopbeApi = {
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
      body: { content: string; metadata?: any },
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
      return requestJson<unknown[]>(`/api/products/discover?limit=${limit}&excludeIds=${ids}`, { signal });
    },
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
    get: async () => {
      const res = await fetch(`${API_BASE_URL}/wishlist`, {
        headers: {
          Authorization: `Bearer ${await getAuthToken()}`,
        },
      });
      if (!res.ok) {
        throw new Error("Failed to fetch wishlist");
      }
      return res.json();
    },
  },
};

async function getAuthToken() {
  // This is a placeholder. You need to implement a way to get the
  // authentication token, for example, from next-auth session.
  return "your-auth-token";
}
