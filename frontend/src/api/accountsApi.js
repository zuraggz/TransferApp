import { apiGet } from "./httpClient";

/** Fetches all accounts with their current balances. */
export function getAccounts() {
  return apiGet("/accounts");
}
