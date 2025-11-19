namespace QualityWaterAlert.Core.Models;

/// <summary>
/// Represents a complete water quality analysis for a specific commune and time period.
/// Aggregates sampling data and measurements for analysis and reporting.
/// </summary>
public class WaterQualityAnalysis
{
    /// <summary>
    /// Unique identifier for this analysis.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The commune being analyzed.
    /// </summary>
    public Commune Commune { get; set; } = new();

    /// <summary>
    /// Start date of the analysis period.
    /// If null, includes all historical data.
    /// </summary>
    public DateTime? AnalysisPeriodStart { get; set; }

    /// <summary>
    /// End date of the analysis period.
    /// If null, up to the most recent sampling.
    /// </summary>
    public DateTime? AnalysisPeriodEnd { get; set; }

    /// <summary>
    /// When this analysis was generated.
    /// </summary>
    public DateTime GeneratedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// All sampling events included in this analysis.
    /// </summary>
    public List<SamplingEvent> SamplingEvents { get; set; } = new();

    /// <summary>
    /// All individual parameter measurements included in this analysis.
    /// </summary>
    public List<WaterQualityParameter> AllMeasurements { get; set; } = new();

    /// <summary>
    /// Overall conformity status for the entire analysis.
    /// 'C' = All samplings conform, 'N' = At least one non-conform
    /// </summary>
    public char OverallConformity { get; set; } = 'C';

    /// <summary>
    /// Total number of sampling events in this analysis.
    /// </summary>
    public int TotalSamplings => SamplingEvents.Count;

    /// <summary>
    /// Number of conform sampling events.
    /// </summary>
    public int ConformSamplings => SamplingEvents.Count(s => s.IsConform);

    /// <summary>
    /// Number of non-conform sampling events.
    /// </summary>
    public int NonConformSamplings => SamplingEvents.Count(s => !s.IsConform);

    /// <summary>
    /// Conformity percentage (0-100).
    /// </summary>
    public decimal ConformityPercentage =>
        TotalSamplings > 0
            ? Math.Round((ConformSamplings * 100m) / TotalSamplings, 2)
            : 0;

    /// <summary>
    /// Gets the most recent sampling in this analysis.
    /// </summary>
    public SamplingEvent? GetLatestSampling()
    {
        return SamplingEvents.OrderByDescending(s => s.SamplingDate).FirstOrDefault();
    }

    /// <summary>
    /// Gets the oldest sampling in this analysis.
    /// </summary>
    public SamplingEvent? GetOldestSampling()
    {
        return SamplingEvents.OrderBy(s => s.SamplingDate).FirstOrDefault();
    }

    /// <summary>
    /// Gets all problem parameters (non-conform measurements) from this analysis.
    /// </summary>
    public List<WaterQualityParameter> GetProblemParameters()
    {
        return AllMeasurements.Where(m => m.IsNonConform).ToList();
    }

    /// <summary>
    /// Gets the top N most frequently tested parameters.
    /// Useful for understanding what's being monitored.
    /// </summary>
    public List<(string Code, string Name, int TestCount)> GetMostTestedParameters(int topCount = 10)
    {
        return AllMeasurements
            .GroupBy(m => new { m.ParameterCode, m.MajorName })
            .Select(g => (g.Key.ParameterCode, g.Key.MajorName, g.Count()))
            .OrderByDescending(x => x.Item3)
            .Take(topCount)
            .ToList();
    }

    /// <summary>
    /// Gets parameters that have had non-conform results.
    /// Groups by parameter and counts non-conform occurrences.
    /// </summary>
    public List<(string Code, string Name, int NonConformCount)> GetProblematicParameters()
    {
        return GetProblemParameters()
            .GroupBy(m => new { m.ParameterCode, m.MajorName })
            .Select(g => (g.Key.ParameterCode, g.Key.MajorName, g.Count()))
            .OrderByDescending(x => x.Item3)
            .ToList();
    }

    /// <summary>
    /// Gets sampling events by conformity status.
    /// </summary>
    public List<SamplingEvent> GetSamplingsByConformity(bool conform)
    {
        return SamplingEvents.Where(s => s.IsConform == conform).ToList();
    }

    /// <summary>
    /// Gets the conformity trend over time.
    /// Returns a dictionary where key is month index and value is count of conform samplings.
    /// </summary>
    public Dictionary<DateTime, int> GetConformityTrendByMonth()
    {
        var result = new Dictionary<DateTime, int>();

        var groupedByMonth = SamplingEvents
            .GroupBy(s => new DateTime(s.SamplingDate.Year, s.SamplingDate.Month, 1))
            .OrderBy(g => g.Key);

        foreach (var group in groupedByMonth)
        {
            var conformCount = group.Count(s => s.IsConform);
            result[group.Key] = conformCount;
        }

        return result;
    }

    /// <summary>
    /// Gets average conformity by network.
    /// Useful for comparing network performance.
    /// </summary>
    public List<(string NetworkCode, string NetworkName, decimal ConformityPercent)> GetNetworkConformityComparison()
    {
        return SamplingEvents
            .GroupBy(s => new { s.NetworkCode, s.CommuneName })
            .Select(g =>
            {
                var total = g.Count();
                var conform = g.Count(s => s.IsConform);
                var percentage = total > 0 ? Math.Round((conform * 100m) / total, 2) : 0;
                return (g.Key.NetworkCode, g.Key.CommuneName, percentage);
            })
            .OrderByDescending(x => x.percentage)
            .ToList();
    }

    /// <summary>
    /// Gets sampling frequency (average samplings per month).
    /// </summary>
    public decimal GetSamplingFrequencyPerMonth()
    {
        if (TotalSamplings == 0)
            return 0;

        var oldest = GetOldestSampling();
        var latest = GetLatestSampling();

        if (oldest == null || latest == null)
            return 0;

        var monthsDiff = (latest.SamplingDate.Year - oldest.SamplingDate.Year) * 12 +
                         (latest.SamplingDate.Month - oldest.SamplingDate.Month) + 1;

        return monthsDiff > 0 ? Math.Round(TotalSamplings / (decimal)monthsDiff, 2) : TotalSamplings;
    }

    /// <summary>
    /// Gets a detailed summary of the analysis.
    /// </summary>
    public string GetDetailedSummary()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"Analyse de qualité d'eau - {Commune.Name}");
        sb.AppendLine($"Période: {(AnalysisPeriodStart?.ToString("yyyy-MM-dd") ?? "historique")} " +
                      $"au {(AnalysisPeriodEnd?.ToString("yyyy-MM-dd") ?? "récent")}");
        sb.AppendLine($"Générée: {GeneratedDate:yyyy-MM-dd HH:mm}");
        sb.AppendLine();

        sb.AppendLine($"Résultats généraux:");
        sb.AppendLine($"  Total prélèvements: {TotalSamplings}");
        sb.AppendLine($"  Conformes: {ConformSamplings} ({ConformityPercentage}%)");
        sb.AppendLine($"  Non-conformes: {NonConformSamplings}");
        sb.AppendLine($"  État global: {(OverallConformity == 'C' ? "✅ CONFORME" : "❌ NON-CONFORME")}");
        sb.AppendLine();

        var problematicParams = GetProblematicParameters();
        if (problematicParams.Count > 0)
        {
            sb.AppendLine($"Paramètres problématiques:");
            foreach (var param in problematicParams.Take(5))
            {
                sb.AppendLine($"  - {param.Name}: {param.NonConformCount} dépassements");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Updates the overall conformity status based on sampling events.
    /// </summary>
    public void UpdateOverallConformity()
    {
        OverallConformity = (NonConformSamplings == 0) ? 'C' : 'N';
    }

    /// <summary>
    /// Returns a string representation of the analysis.
    /// </summary>
    public override string ToString()
    {
        return $"Analyse {Commune.Name}: {ConformSamplings}/{TotalSamplings} conformes ({ConformityPercentage}%)";
    }
}
