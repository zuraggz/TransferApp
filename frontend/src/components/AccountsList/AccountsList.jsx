import { formatCurrency } from "../../utils/formatters";

/** Displays all accounts and their current balances in a table. */
export function AccountsList({ accounts, loading, error }) {
  if (loading) {
    return <p>Loading accounts...</p>;
  }

  if (error) {
    return <p className="error-message">Failed to load accounts: {error}</p>;
  }

  if (accounts.length === 0) {
    return <p>No accounts found.</p>;
  }

  return (
    <section className="panel">
      <h2>Accounts</h2>
      <table>
        <thead>
          <tr>
            <th>Account Name</th>
            <th>Balance</th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((account) => (
            <tr key={account.id}>
              <td>{account.accountName}</td>
              <td>{formatCurrency(account.balance)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
