import { useState } from "react";
import { transferFunds } from "../../api/transferApi";
import { formatCurrency } from "../../utils/formatters";

/** Form for transferring money between two accounts. Notifies the parent on success so it can refresh data. */
export function TransferForm({ accounts, onTransferComplete }) {
  const [fromAccountId, setFromAccountId] = useState("");
  const [toAccountId, setToAccountId] = useState("");
  const [amount, setAmount] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState(null);
  const [errorMessage, setErrorMessage] = useState(null);

  async function handleSubmit(event) {
    event.preventDefault();
    setSuccessMessage(null);
    setErrorMessage(null);

    if (!fromAccountId || !toAccountId || !amount) {
      setErrorMessage("Please select both accounts and enter an amount.");
      return;
    }

    setSubmitting(true);
    try {
      const result = await transferFunds({
        fromAccountId: Number(fromAccountId),
        toAccountId: Number(toAccountId),
        amount: Number(amount),
      });

      setSuccessMessage(
        `Transfer of ${formatCurrency(result.amount)} completed successfully (transaction #${result.transactionId}).`
      );
      setAmount("");
      onTransferComplete();
    } catch (err) {
      setErrorMessage(err.message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="panel">
      <h2>Transfer Money</h2>
      <form onSubmit={handleSubmit} className="transfer-form">
        <label>
          From Account
          <select value={fromAccountId} onChange={(event) => setFromAccountId(event.target.value)}>
            <option value="">Select account</option>
            {accounts.map((account) => (
              <option key={account.id} value={account.id}>
                {account.accountName} ({formatCurrency(account.balance)})
              </option>
            ))}
          </select>
        </label>

        <label>
          To Account
          <select value={toAccountId} onChange={(event) => setToAccountId(event.target.value)}>
            <option value="">Select account</option>
            {accounts.map((account) => (
              <option key={account.id} value={account.id}>
                {account.accountName}
              </option>
            ))}
          </select>
        </label>

        <label>
          Amount
          <input
            type="number"
            min="0.01"
            step="0.01"
            value={amount}
            onChange={(event) => setAmount(event.target.value)}
            placeholder="0.00"
          />
        </label>

        <button type="submit" disabled={submitting}>
          {submitting ? "Transferring..." : "Transfer"}
        </button>
      </form>

      {successMessage && <p className="success-message">{successMessage}</p>}
      {errorMessage && <p className="error-message">{errorMessage}</p>}
    </section>
  );
}
