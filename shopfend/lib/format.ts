export function formatMoney(amount: number | null | undefined, currency?: string | null) {
  if (amount == null) return "—";

  // If backend already returns formatted numbers, keep simple.
  // Prefer Intl if currency looks like ISO.
  try {
    if (currency && currency.length === 3) {
      return new Intl.NumberFormat(undefined, {
        style: "currency",
        currency,
        maximumFractionDigits: 2,
      }).format(amount);
    }
  } catch {
    // ignore
  }

  return `${amount}${currency ? ` ${currency}` : ""}`;
}

export function formatCompactMoney(amount: number | null | undefined, currency?: string | null) {
  if (amount == null) return "—";
  try {
    if (currency && currency.length === 3) {
      return new Intl.NumberFormat(undefined, {
        style: "currency",
        currency,
        notation: "compact",
        maximumFractionDigits: 2,
      }).format(amount);
    }
  } catch {
    // ignore
  }
  return `${amount}${currency ? ` ${currency}` : ""}`;
}

