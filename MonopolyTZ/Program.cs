using System.Globalization;

internal static class Program
{
    private static void Main(string[] args)
    {
        var targetMonth = YearMonth.Parse(args.Length > 0 ? args[0] : null);
        var walletsPath = args.Length > 1 ? args[1] : "wallets.csv";
        var txPath = args.Length > 2 ? args[2] : "transactions.csv";

        if (!File.Exists(walletsPath))
        {
            Console.Error.WriteLine($"File not found: {walletsPath}");
            return;
        }
        if (!File.Exists(txPath))
        {
            Console.Error.WriteLine($"File not found: {txPath}");
            return;
        }

        var wallets = CsvConfigLoader.LoadWallets(walletsPath).ToDictionary(w => w.ID, w => w);
        var allTx = CsvConfigLoader.LoadTransactions(txPath).ToList();

        var rejected = new List<(Wallet Wallet, Transaction Tx, string Reason)>();
        foreach (var group in allTx.GroupBy(t => t.WalletID))
        {
            if (!wallets.TryGetValue(group.Key, out var wallet))
            {
                foreach (var tx in group)
                    rejected.Add((new Wallet(group.Key, "<unknown>", "RUB", 0m), tx.Tx, "Unknown wallet"));
                continue;
            }

            foreach (var tx in group.OrderBy(t => t.Tx.DateTime))
            {
                var ok = wallet.MakeTransaction(tx.Tx);
                if (!ok)
                    rejected.Add((wallet, tx.Tx, "Insufficient funds"));
            }
        }

        ReportPrinter.PrintHeader(targetMonth);
        ReportPrinter.PrintPerWalletGroupedByType(targetMonth, wallets.Values);
        ReportPrinter.PrintTopExpensesPerWallet(targetMonth, wallets.Values, 3);
        ReportPrinter.PrintRejected(rejected);
    }
}

public readonly struct YearMonth
{
    public int Year { get; }
    public int Month { get; }

    public YearMonth(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public static YearMonth Parse(string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            var now = DateTime.Today;
            return new YearMonth(now.Year, now.Month);
        }
        arg = arg.Trim();
        if (DateTime.TryParseExact(arg, new[] { "yyyy-MM", "yyyy/MM", "MM.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return new YearMonth(dt.Year, dt.Month);
        if (DateTime.TryParse(arg, out dt))
            return new YearMonth(dt.Year, dt.Month);
        throw new FormatException($"Invalid target month format: {arg}");
    }

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}