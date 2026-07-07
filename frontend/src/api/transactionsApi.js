import { apiGet } from "./httpClient";

/** Fetches all transactions (successful and failed), most recent first. */
export function getTransactions() {
  return apiGet("/transactions");
}
