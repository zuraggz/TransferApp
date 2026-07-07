import { apiPost } from "./httpClient";

/** Requests a transfer between two accounts. Throws with a readable message on failure. */
export function transferFunds({ fromAccountId, toAccountId, amount }) {
  return apiPost("/transfer", { fromAccountId, toAccountId, amount });
}
