public enum TransactionType
{
    Income,
    Expense
}

[Serializable]
public sealed class Wallet
{
    public string ID { get; }
    public string Name { get; }
    public string Currency { get; }
    public decimal InitialBalance { get; }
    public decimal Balance { get; private set; }

    private readonly List<Transaction> _transactions = new();

    public Wallet(string id, string name, string currency, decimal initialBalance)
    {
        ID = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Currency = string.IsNullOrWhiteSpace(currency) ? "RUB" : currency.Trim();
        InitialBalance = initialBalance;
        Balance = initialBalance;
    }

    public bool MakeTransaction(Transaction transaction)
    {
        var isExpense = transaction.Type == TransactionType.Expense;
        if (isExpense && transaction.Value > Balance) return false;
        _transactions.Add(transaction);
        Balance += isExpense ? -transaction.Value : transaction.Value;
        return true;
    }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    public IReadOnlyList<Transaction> GetTransactionsForMonth(YearMonth ym)
    {
        return _transactions
            .Where(t => t.DateTime.Year == ym.Year && t.DateTime.Month == ym.Month)
            .OrderBy(t => t.DateTime)
            .ToList();
    }

    public (decimal Income, decimal Expense) GetMonthIncomeExpense(YearMonth ym)
    {
        var monthTx = GetTransactionsForMonth(ym);
        var income = monthTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Value);
        var expense = monthTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Value);
        return (income, expense);
    }

    public List<Transaction> GetTopExpenses(YearMonth ym, int topN)
    {
        return _transactions
            .Where(t => t.Type == TransactionType.Expense && t.DateTime.Year == ym.Year && t.DateTime.Month == ym.Month)
            .OrderByDescending(t => t.Value)
            .ThenBy(t => t.DateTime)
            .Take(topN)
            .ToList();
    }
}
