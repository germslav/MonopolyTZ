[Serializable]
public readonly struct Transaction
{
    public string ID { get; }
    public DateTime DateTime { get; }
    public decimal Value { get; }
    public TransactionType Type { get; }
    public string Description { get; }

    public Transaction(string id, DateTime dateTime, decimal value, TransactionType type, string description)
    {
        ID = id;
        DateTime = dateTime;
        Value = value < 0 ? -value : value;
        Type = type;
        Description = description ?? string.Empty;
    }
}

internal static class TransactionTypeParser
{
    public static TransactionType Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new FormatException("Empty transaction type");
        var v = value.Trim();
        if (v.Equals("Income", StringComparison.OrdinalIgnoreCase) || v.Equals("Доход", StringComparison.OrdinalIgnoreCase))
            return TransactionType.Income;
        if (v.Equals("Expense", StringComparison.OrdinalIgnoreCase) || v.Equals("Расход", StringComparison.OrdinalIgnoreCase))
            return TransactionType.Expense;
        throw new FormatException($"Unknown transaction type: {value}");
    }
}