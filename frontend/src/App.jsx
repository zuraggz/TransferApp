import { useAccounts } from "./hooks/useAccounts";
import { useTransactions } from "./hooks/useTransactions";
import { AccountsList } from "./components/AccountsList/AccountsList";
import { TransactionsList } from "./components/TransactionsList/TransactionsList";
import { TransferForm } from "./components/TransferForm/TransferForm";
import "./App.css";

function App() {
  const { accounts, loading: accountsLoading, error: accountsError, refetch: refetchAccounts } = useAccounts();
  const {
    transactions,
    loading: transactionsLoading,
    error: transactionsError,
    refetch: refetchTransactions,
  } = useTransactions();

  function handleTransferComplete() {
    refetchAccounts();
    refetchTransactions();
  }

  return (
    <div className="app">
      <h1>Money Transfer</h1>

      <TransferForm accounts={accounts} onTransferComplete={handleTransferComplete} />

      <AccountsList accounts={accounts} loading={accountsLoading} error={accountsError} />

      <TransactionsList
        transactions={transactions}
        accounts={accounts}
        loading={transactionsLoading}
        error={transactionsError}
      />
    </div>
  );
}

export default App;
