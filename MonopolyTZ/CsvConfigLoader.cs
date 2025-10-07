using System.Globalization;
using System.Text;

internal static class CsvConfigLoader
{
    public static IEnumerable<Wallet> LoadWallets(string path)
    {
        using var sr = new StreamReader(path, Encoding.UTF8);
        var header = sr.ReadLine() ?? string.Empty;
        var delim = DetectDelimiter(header);
        var idx = HeaderIndex(header, delim);
        Require(idx, "WalletID");
        Require(idx, "Name");
        Require(idx, "Currency");
        Require(idx, "InitialBalance");

        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cells = SplitCsv(line, delim);
            var id = Get(cells, idx, "WalletID");
            var name = Get(cells, idx, "Name");
            var currency = Get(cells, idx, "Currency");
            var balanceStr = Get(cells, idx, "InitialBalance");
            var initial = ParseDecimalFlexible(balanceStr);
            yield return new Wallet(id, name, currency, initial);
        }
    }

    public static IEnumerable<(string WalletID, Transaction Tx)> LoadTransactions(string path)
    {
        using var sr = new StreamReader(path, Encoding.UTF8);
        var header = sr.ReadLine() ?? string.Empty;
        var delim = DetectDelimiter(header);
        var idx = HeaderIndex(header, delim);
        Require(idx, "WalletID");
        Require(idx, "TransactionID");
        Require(idx, "Date");
        Require(idx, "Value");
        Require(idx, "Type");
        Require(idx, "Description");

        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cells = SplitCsv(line, delim);
            var walletID = Get(cells, idx, "WalletID");
            var id = Get(cells, idx, "TransactionID");
            var dateStr = Get(cells, idx, "Date");
            var valueStr = Get(cells, idx, "Value");
            var typeStr = Get(cells, idx, "Type");
            var desc = Get(cells, idx, "Description");

            var date = ParseDateFlexible(dateStr);
            var value = ParseDecimalFlexible(valueStr);
            var type = TransactionTypeParser.Parse(typeStr);

            yield return (walletID, new Transaction(id, date, value, type, desc));
        }
    }

    private static char DetectDelimiter(string header)
    {
        if (header.Contains('\t')) return '\t';
        if (header.Contains(';')) return ';';
        return ',';
    }

    private static string[] SplitCsv(string line, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == delimiter && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }
        result.Add(current.ToString());
        return result.Select(s => s.Trim().Trim('"')).ToArray();
    }

    private static Dictionary<string, int> HeaderIndex(string header, char delim)
    {
        var cols = SplitCsv(header, delim);
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < cols.Length; i++) dict[cols[i]] = i;
        return dict;
    }

    private static void Require(Dictionary<string, int> map, string name)
    {
        if (!map.ContainsKey(name)) throw new InvalidDataException($"Header column missing: {name}");
    }

    private static string Get(string[] cells, Dictionary<string, int> map, string name)
    {
        return map.TryGetValue(name, out var i) && i >= 0 && i < cells.Length ? cells[i] : string.Empty;
    }

    private static DateTime ParseDateFlexible(string value)
    {
        string[] fmts =
        {
            "yyyy-MM-dd", "dd.MM.yyyy", "yyyy/MM/dd", "dd/MM/yyyy",
            "yyyy-MM-dd HH:mm:ss", "dd.MM.yyyy HH:mm:ss"
        };
        if (DateTime.TryParseExact(value, fmts, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
            return dt;
        if (DateTime.TryParse(value, out dt)) return dt;
        throw new FormatException($"Invalid date: {value}");
    }

    private static decimal ParseDecimalFlexible(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)) return d;
        if (decimal.TryParse(value, NumberStyles.Number, new CultureInfo("ru-RU"), out d)) return d;
        if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out d)) return d;
        throw new FormatException($"Invalid decimal: {value}");
    }
}
