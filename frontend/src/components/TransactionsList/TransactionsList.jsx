import { formatCurrency, formatTimestamp } from "../../utils/formatters";

function resolveAccountName(accounts, accountId) {
  const account = accounts.find((a) => a.id === accountId);
  return account ? account.accountName : `Account #${accountId}`;
}

/** Displays all transactions (successful and failed) in a table, most recent first. */
export function TransactionsList({ transactions, accounts, loading, error }) {
  if (loading) {
    return <p>Loading transactions...</p>;
  }

  if (error) {
    return <p className="error-message">Failed to load transactions: {error}</p>;
  }

  if (transactions.length === 0) {
    return <p>No transactions yet.</p>;
  }

  return (
    <section className="panel">
      <h2>Transactions</h2>
      <table>
        <thead>
          <tr>
            <th>Sender</th>
            <th>Receiver</th>
            <th>Amount</th>
            <th>Timestamp</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map((transaction) => (
            <tr key={transaction.id}>
              <td>{resolveAccountName(accounts, transaction.fromAccountId)}</td>
              <td>{resolveAccountName(accounts, transaction.toAccountId)}</td>
              <td>{formatCurrency(transaction.amount)}</td>
              <td>{formatTimestamp(transaction.timestamp)}</td>
              <td>
                <span className={`status-badge status-${transaction.status.toLowerCase()}`}>
                  {transaction.status}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
