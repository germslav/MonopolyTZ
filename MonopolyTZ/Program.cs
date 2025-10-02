
internal static class Program
{
    private static void Main(string[] args)
    {

    }
}

[Serializable]
public sealed class Wallet
{
    public string ID { get; }
    public string Name { get; }
    public string Currency { get; }
    public decimal InitialBalance { get; }
    public decimal Balance { get; private set; }

    private readonly List<Transaction> _transactions;

    public Wallet(string id, string name, string currency, IEnumerable<Transaction> transactions)
    {
        ID = id;
        Name = name;
        Currency = currency ?? "RUB";

        _transactions = transactions?.ToList() ?? new List<Transaction>();
        RecalculateBalance();
    }

    public bool MakeTransaction(Transaction transaction)
    {
        var isExpense = transaction.Type == TransactionType.Expense;

        if (isExpense && transaction.Value > Balance)
        {
            return false;
        }

        _transactions.Add(transaction);

        Balance += isExpense ? -transaction.Value : transaction.Value;
        return true;
    }

    public void RecalculateBalance()
    {
        var sum = _transactions.Sum(t => t.Type == TransactionType.Expense ? -t.Value : t.Value);
        Balance = InitialBalance + sum;
    }

    public IReadOnlyList<Transaction> GetTransactionsForMonth(DateTime date)
    {
        return _transactions
            .Where(t => t.DateTime.Year == date.Year && t.DateTime.Month == date.Month)
            .OrderBy(t => t.DateTime)
            .ToList();
    }

    public (decimal Income, decimal Expense) GetMonthIncomeExpense(DateTime date)
    {
        var monthTx = GetTransactionsForMonth(date);
        var income = monthTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Value);
        var expense = monthTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Value);
        return (income, expense);
    }

    public List<Transaction> GetTopExpenses(DateTime date, int topN)
    {
        return _transactions
            .Where(t => t.Type == TransactionType.Expense && t.DateTime.Year == date.Year && t.DateTime.Month == date.Month)
            .OrderByDescending(t => t.Value)
            .ThenBy(t => t.DateTime)
            .Take(topN)
            .ToList();
    }
}


public enum TransactionType
{
    Income,
    Expense
}

[Serializable]
public struct Transaction
{

    public string ID { get; private set; }
    public DateTime DateTime { get; private set; }
    public decimal Value { get; private set; }
    public TransactionType Type { get; private set; }
    public string Description { get; private set; }

    public Transaction(string iD, DateTime dateTime, decimal value, TransactionType type, string description)
    {
        ID = iD;
        DateTime = dateTime;
        Value = value;
        Type = type;
        Description = description;
    }
}