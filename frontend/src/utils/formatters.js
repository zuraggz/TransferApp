export function formatCurrency(amount) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(amount);
}

export function formatTimestamp(value) {
  return new Date(value).toLocaleString();
}
