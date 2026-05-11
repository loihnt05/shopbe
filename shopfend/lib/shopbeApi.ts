export type ApiEnvelope<T> = {
  data: T;
};

import { asRecord, errorMessage } from "./errors";

export type ProductListItem = {
  id: string;
  name: string;
  description?: string | null;
  price?: number | null;
  discountPrice?: number | null;
  currency?: string | null;
  primaryImageUrl?: string | null;
  thumbnailUrl?: string | null;
};

export type ProductVariantDto = {
  id: string;
  sku?: string | null;
  price: number;
  currency: string;
  stockQuantity?: number | null;
};

export type ProductDetail = {
  id: string;
  name: string;
  description?: string | null;
  categoryId?: string | null;
  primaryImageUrl?: string | null;
  images?: Array<{ id: string; imageUrl: string }>;
  variants?: ProductVariantDto[];
  // backend may also expose aggregate price fields; keep optional
  price?: number | null;
  currency?: string | null;
};

export type CartItem = {
  productVariantId: string;
  quantity: number;
  productId?: string;
  productName?: string;
  price?: number;
  currency?: string;
  imageUrl?: string | null;
};

export type CartDto = {
  userId?: string;
  items: CartItem[];
  totalAmount?: number;
  currency?: string;
};

export type CreateOrderResponse = {
  id: string;
  status?: string;
  totalAmount?: number;
  currency?: string;
};

export type CreateStripePaymentIntentResponse = {
  paymentId: string;
  paymentIntentId: string;
  clientSecret: string;
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

export function productResponseToListItem(item: any): ProductListItem {
  return {
    id: item.id,
    name: item.name,
    description: item.description,
    price: item.price,
    discountPrice: item.discountPrice,
    currency: item.currency,
    primaryImageUrl: item.primaryImageUrl || item.imageUrl,
    thumbnailUrl: item.thumbnailUrl,
  };
}

export const shopbeApi = {
  recommendations: {
    topSelling: (limit: number, signal?: AbortSignal) =>
      requestJson<any[]>(`/api/recommendations/top-selling?limit=${limit}`, { signal }),
    me: (accessToken: string, limit: number, signal?: AbortSignal) =>
      requestJson<any[]>(`/api/recommendations/me?limit=${limit}`, { accessToken, signal }),
    similar: (productId: string, limit: number, signal?: AbortSignal) =>
      requestJson<any[]>(`/api/recommendations/products/${productId}/similar?limit=${limit}`, { signal }),
  },
  products: {
    list: (accessToken?: string, signal?: AbortSignal) =>
      requestJson<ProductListItem[]>("/api/products", {
        accessToken,
        signal,
      }),
    getById: (id: string, accessToken?: string, signal?: AbortSignal) =>
      requestJson<ProductDetail>(`/api/products/${id}`, {
        accessToken,
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
  },
  payments: {
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
  },
};


