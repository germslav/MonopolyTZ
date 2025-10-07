
namespace MonopolyTZ.Tests
{
    [TestFixture]
    public class Sorting
    {
        [Test]
        public void GetTransactionsForMonth_ReturnsAscendingByDate()
        {
            var w = new Wallet("w1", "Main", "RUB", 0m);
            w.MakeTransaction(new Transaction("t3", new DateTime(2025, 9, 10, 12, 0, 0), 10m, TransactionType.Expense, "C"));
            w.MakeTransaction(new Transaction("t1", new DateTime(2025, 9, 1, 9, 0, 0), 100m, TransactionType.Income, "A"));
            w.MakeTransaction(new Transaction("t2", new DateTime(2025, 9, 1, 18, 0, 0), 20m, TransactionType.Expense, "B"));
            w.MakeTransaction(new Transaction("t0", new DateTime(2025, 8, 31, 23, 0, 0), 5m, TransactionType.Expense, "Prev"));
            w.MakeTransaction(new Transaction("t4", new DateTime(2025, 10, 1, 0, 0, 0), 5m, TransactionType.Expense, "Next"));

            var month = new YearMonth(2025, 9);
            var ordered = w.GetTransactionsForMonth(month).Select(t => t.ID).ToArray();

            CollectionAssert.AreEqual(new[] { "t1", "t2" }, ordered);
        }

        [Test]
        public void GetTopExpenses_SortsByValueDesc_ThenDateAsc()
        {
            var w = new Wallet("w1", "Main", "RUB", 1000m);
            w.MakeTransaction(new Transaction("i1", new DateTime(2025, 9, 1), 500m, TransactionType.Income, "Income"));

            w.MakeTransaction(new Transaction("e120_late", new DateTime(2025, 9, 20), 120m, TransactionType.Expense, "x"));
            w.MakeTransaction(new Transaction("e200_early", new DateTime(2025, 9, 2), 200m, TransactionType.Expense, "x"));
            w.MakeTransaction(new Transaction("e200_late", new DateTime(2025, 9, 15), 200m, TransactionType.Expense, "x"));
            w.MakeTransaction(new Transaction("e050_mid", new DateTime(2025, 9, 10), 50m, TransactionType.Expense, "x"));

            var top = w.GetTopExpenses(new YearMonth(2025, 9), 3).Select(t => t.ID).ToArray();

            CollectionAssert.AreEqual(
                new[] { "e200_early", "e200_late", "e120_late" },
                top);
        }

        [Test]
        public void GetTopExpenses_RespectsTopN_AfterSorting()
        {
            var w = new Wallet("w1", "Main", "RUB", 1000m);
            w.MakeTransaction(new Transaction("i", new DateTime(2025, 9, 1), 500m, TransactionType.Income, "Income"));

            w.MakeTransaction(new Transaction("a", new DateTime(2025, 9, 5), 10m, TransactionType.Expense, "a"));
            w.MakeTransaction(new Transaction("b", new DateTime(2025, 9, 6), 30m, TransactionType.Expense, "b"));
            w.MakeTransaction(new Transaction("c", new DateTime(2025, 9, 7), 20m, TransactionType.Expense, "c"));
            w.MakeTransaction(new Transaction("d", new DateTime(2025, 9, 8), 40m, TransactionType.Expense, "d"));

            var top2 = w.GetTopExpenses(new YearMonth(2025, 9), 2).Select(t => t.ID).ToArray();

            CollectionAssert.AreEqual(new[] { "d", "b" }, top2);
        }
    }
}
