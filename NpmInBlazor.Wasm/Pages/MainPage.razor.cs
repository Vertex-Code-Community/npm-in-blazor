using System.Globalization;
using System.Text.Json;

namespace NpmInBlazor.Wasm.Pages;

public partial class MainPage
{
    private string numberInput = "1, 2, 3, 10";
    private StatRow[]? numberStats;

    private List<PersonRow> columnRows = new()
    {
        new("Ava", 26.5, 88.2),
        new("Ben", 33.4, 72.0),
        new("Chloe", 29.1, 95.6),
        new("Diego", 31.9, 81.3)
    };

    private string selectedColumnKey = "age";
    private StatRow[]? columnStats;

    private List<PointRow> regressionPoints = new()
    {
        new(0, 1),
        new(1, 2.3),
        new(2, 3.8),
        new(3, 5.2),
        new(4, 6.1)
    };

    private StatRow[]? regressionStats;

    private async Task RunAnalyzeNumbers()
    {
        var parsed = ParseNumbers(numberInput);
        numberStats = await InvokeAndNormalize("simpleStats.analyzeNumbers", parsed);
    }

    private async Task RunAnalyzeColumn()
    {
        var rows = columnRows
            .Select(r => new Dictionary<string, object?>
            {
                ["name"] = r.Name,
                ["age"] = r.Age,
                ["score"] = r.Score
            })
            .ToArray();

        columnStats = await InvokeAndNormalize("simpleStats.analyzeColumn", rows, selectedColumnKey);
    }

    private async Task RunRegression()
    {
        var rows = regressionPoints
            .Select(p => new Dictionary<string, object?>
            {
                ["x"] = p.X,
                ["y"] = p.Y
            })
            .ToArray();

        regressionStats = await InvokeAndNormalize("simpleStats.linearRegressionTable", rows, "x", "y");
    }

    private void ResetNumbers() => numberInput = "1, 2, 3, 10";

    private void ResetRegression()
    {
        regressionPoints = new()
        {
            new(0, 1),
            new(1, 2.3),
            new(2, 3.8),
            new(3, 5.2),
            new(4, 6.1)
        };
        regressionStats = null;
    }

    private void ResetAll()
    {
        ResetNumbers();
        ResetRegression();
        columnRows = new()
        {
            new("Ava", 26.5, 88.2),
            new("Ben", 33.4, 72.0),
            new("Chloe", 29.1, 95.6),
            new("Diego", 31.9, 81.3)
        };
        numberStats = null;
        columnStats = null;
        regressionStats = null;
    }

    private void AddColumnRow() => columnRows.Add(new PersonRow($"Person {columnRows.Count + 1}", 30, 75));

    private void RemoveColumnRow(int index)
    {
        if (index >= 0 && index < columnRows.Count)
        {
            columnRows.RemoveAt(index);
        }
    }

    private void AddRegressionRow() => regressionPoints.Add(new PointRow(regressionPoints.Count, regressionPoints.Count + 1));

    private void RemoveRegressionRow(int index)
    {
        if (index >= 0 && index < regressionPoints.Count)
        {
            regressionPoints.RemoveAt(index);
        }
    }

    private static double[] ParseNumbers(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Array.Empty<double>();
        }

        var separators = new[] { ",", "\n", " ", "\t", ";", "|" };

        return input
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : double.NaN)
            .Where(double.IsFinite)
            .ToArray();
    }

    private async Task<StatRow[]> InvokeAndNormalize(string identifier, params object?[] args)
    {
        var raw = await JS.InvokeAsync<StatRow[]>(identifier, args);
        return raw.Select(NormalizeRow).ToArray();
    }

    private static StatRow NormalizeRow(StatRow row) => row.value switch
    {
        JsonElement json => row with { value = Extract(json) },
        _ => row
    };

    private static object Extract(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Number => element.TryGetInt64(out var i) ? i : element.GetDouble(),
        JsonValueKind.String => element.GetString() ?? string.Empty,
        _ => element.ToString()
    };

    private static object? TryGetMetric(IEnumerable<StatRow> stats, string key)
    {
        var match = stats.FirstOrDefault(s => string.Equals(s.metric, key, StringComparison.OrdinalIgnoreCase));
        return match?.value;
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "—",
            double d => d.ToString("0.###", CultureInfo.InvariantCulture),
            float f => f.ToString("0.###", CultureInfo.InvariantCulture),
            decimal m => m.ToString("0.###", CultureInfo.InvariantCulture),
            JsonElement json => Extract(json)?.ToString() ?? "—",
            _ => value.ToString() ?? "—"
        };
    }
    
    private class PersonRow
    {
        public PersonRow() {}

        public PersonRow(string name, double age, double score)
        {
            Name = name;
            Age = age;
            Score = score;
        }
        
        public string Name { get; set; }
        public double Age { get; set; }
        public double Score { get; set; }
    }

    private class PointRow
    {
        public PointRow() { }

        public PointRow(double x, double y)
        {
            X = x;
            Y = y;
        }
        
        public double X { get; set; }
        public double Y { get; set; }
    }
}