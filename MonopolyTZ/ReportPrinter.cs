internal static class ReportPrinter
{
    public static void PrintHeader(YearMonth ym)
    {
        Console.WriteLine($"Report for {ym}");
        Console.WriteLine(new string('=', 48));
    }

    public static void PrintPerWalletGroupedByType(YearMonth ym, IEnumerable<Wallet> wallets)
    {
        Console.WriteLine($"Grouped by type per wallet for {ym}");
        foreach (var w in wallets.OrderBy(w => w.Name))
        {
            var monthTx = w.GetTransactionsForMonth(ym).ToList();
            Console.WriteLine($"  Wallet: {w.Name} [{w.Currency}]  Balance: {w.Balance:F2}");
            if (monthTx.Count == 0)
            {
                Console.WriteLine("    No transactions in selected month");
                continue;
            }

            var groups = monthTx
                .GroupBy(t => t.Type)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Value), Items = g.OrderBy(t => t.DateTime).ToList() })
                .OrderByDescending(g => g.Total)
                .ToList();

            foreach (var g in groups)
            {
                Console.WriteLine($"    {g.Type} total: {g.Total:F2}");
                foreach (var t in g.Items)
                    Console.WriteLine($"      {t.DateTime:yyyy-MM-dd} | {t.Type} | {t.Value:F2} | {t.Description}");
            }
        }
        Console.WriteLine();
    }

    public static void PrintTopExpensesPerWallet(YearMonth ym, IEnumerable<Wallet> wallets, int topN)
    {
        Console.WriteLine($"Top {topN} expenses per wallet for {ym}");
        foreach (var w in wallets.OrderBy(w => w.Name))
        {
            var top = w.GetTopExpenses(ym, topN);
            Console.WriteLine($"  Wallet: {w.Name} [{w.Currency}]  Balance: {w.Balance:F2}");
            if (top.Count == 0)
            {
                Console.WriteLine("    No expenses");
                continue;
            }
            for (int i = 0; i < top.Count; i++)
            {
                var t = top[i];
                Console.WriteLine($"    {i + 1}. {t.DateTime:yyyy-MM-dd} | {t.Value:F2} | {t.Description}");
            }
        }
        Console.WriteLine();
    }

    public static void PrintRejected(IEnumerable<(Wallet Wallet, Transaction Tx, string Reason)> rejected)
    {
        var list = rejected.ToList();
        if (list.Count == 0) return;
        Console.WriteLine("Rejected transactions while applying CSV");
        foreach (var r in list)
            Console.WriteLine($"  Wallet {r.Wallet.Name} | {r.Tx.DateTime:yyyy-MM-dd} | {r.Tx.Type} {r.Tx.Value:F2} | {r.Reason} | {r.Tx.Description}");
        Console.WriteLine();
    }
}
