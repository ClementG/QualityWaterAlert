namespace QualityWaterAlert.Core.Models;

/// <summary>
/// Represents a water distribution network (UDP - Unité De Distribution Publique).
/// Maps to DIS_COM_UDI_2025.txt and DIS_PLV_2025.txt data from data.gouv.fr
/// </summary>
public class WaterNetwork
{
    /// <summary>
    /// Unique network code (UDP identifier).
    /// Example: "001000556", "001000003"
    /// This is the primary identifier for the network.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the water distribution network.
    /// Example: "BDS ST DIDIER/CHALARONNE", "SERA - SYNDICAT EAUX REGION D'AMBERIEU"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// INSEE code of the primary commune served by this network.
    /// Example: "01001", "01007"
    /// </summary>
    public string PrincipalCommuneCode { get; set; } = string.Empty;

    /// <summary>
    /// Optional neighborhood/district (quartier) served by this network.
    /// Example: "Dalivoy les granges" or "-" for entire commune
    /// </summary>
    public string? Neighborhood { get; set; }

    /// <summary>
    /// If this network is supplied by another network, this is the upstream network code.
    /// Example: "001001304" (upstream) supplies this network
    /// Empty/null if this is a primary source network.
    /// </summary>
    public string? UpstreamNetworkCode { get; set; }

    /// <summary>
    /// Name of the upstream network (if applicable).
    /// </summary>
    public string? UpstreamNetworkName { get; set; }

    /// <summary>
    /// Percentage of water flow supplied by the upstream network.
    /// Example: "100%" (entirely supplied), "60%" (partially supplied)
    /// Null if no upstream supplier.
    /// </summary>
    public decimal? UpstreamFlowPercentage { get; set; }

    /// <summary>
    /// Date when this network started supplying water (first record in system).
    /// Example: "2010-09-07", "2025-04-03"
    /// </summary>
    public DateTime SupplyStartDate { get; set; }

    /// <summary>
    /// Management unit (UGE - Unité de Gestion de l'Eau) responsible for this network.
    /// Example: "SYND. EAUX REGION D'AMBERIEU-EN-B"
    /// </summary>
    public string? ManagementUnit { get; set; }

    /// <summary>
    /// Distribution company/operator of this network.
    /// Example: "SERA - SYNDICAT DES EAUX DE LA REGION D'AMBERIEU-EN-BUGEY"
    /// </summary>
    public string? DistributionCompany { get; set; }

    /// <summary>
    /// Master of work / Owner of the network (MOA).
    /// Usually the same as DistributionCompany.
    /// </summary>
    public string? NetworkOwner { get; set; }

    /// <summary>
    /// Collection of sampling events for this network.
    /// Each entry represents a water quality test.
    /// </summary>
    public List<SamplingEvent> SamplingEvents { get; set; } = new();

    /// <summary>
    /// Department code (INSEE) derived from the network code.
    /// First 3 digits typically encode the department.
    /// </summary>
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets the most recent sampling event for this network.
    /// </summary>
    public SamplingEvent? GetLatestSampling() => 
        SamplingEvents.OrderByDescending(s => s.SamplingDate).FirstOrDefault();

    /// <summary>
    /// Gets the overall conformity status of this network based on latest/most recent samplings.
    /// Returns 'C' if conform, 'N' if non-conform, 'U' if unknown/no data.
    /// </summary>
    public char GetOverallConformity()
    {
        if (SamplingEvents.Count == 0)
            return 'U'; // Unknown

        var latestSampling = GetLatestSampling();
        if (latestSampling == null)
            return 'U';

        return (latestSampling.BacterioConformity == 'C' && latestSampling.ChemicalConformity == 'C')
            ? 'C'
            : 'N';
    }

    /// <summary>
    /// Determines if this network is currently conform based on latest data.
    /// </summary>
    public bool IsConform => GetOverallConformity() == 'C';

    /// <summary>
    /// Determines if this network is supplied by another network (not a primary source).
    /// </summary>
    public bool IsUpstreamSupplied => !string.IsNullOrWhiteSpace(UpstreamNetworkCode);

    /// <summary>
    /// Gets a conformity history summary for the past N months.
    /// </summary>
    public Dictionary<int, int> GetConformityTrendLastMonths(int months = 12)
    {
        var result = new Dictionary<int, int>();
        var now = DateTime.Now;

        for (int i = 0; i < months; i++)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var conformCount = SamplingEvents
                .Where(s => s.SamplingDate >= monthStart && s.SamplingDate <= monthEnd)
                .Count(s => s.BacterioConformity == 'C' && s.ChemicalConformity == 'C');

            result[i] = conformCount;
        }

        return result;
    }

    /// <summary>
    /// Gets statistics about the network's conformity.
    /// </summary>
    public NetworkConformityStatistics GetConformityStatistics()
    {
        var stats = new NetworkConformityStatistics
        {
            TotalSamplings = SamplingEvents.Count,
            ConformSamplings = SamplingEvents.Count(s => s.BacterioConformity == 'C' && s.ChemicalConformity == 'C'),
            NonConformSamplings = SamplingEvents.Count(s => s.BacterioConformity == 'N' || s.ChemicalConformity == 'N')
        };

        stats.ConformityPercentage = stats.TotalSamplings > 0
            ? Math.Round((stats.ConformSamplings * 100m) / stats.TotalSamplings, 2)
            : 0;

        return stats;
    }

    /// <summary>
    /// Returns a string representation of the network.
    /// </summary>
    public override string ToString()
    {
        var upstream = IsUpstreamSupplied ? $" (supplied by {UpstreamNetworkCode})" : "";
        return $"{Name} ({Code}){upstream}";
    }

    /// <summary>
    /// Determines equality based on network code.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is WaterNetwork network && network.Code == Code;
    }

    /// <summary>
    /// Gets hash code based on network code.
    /// </summary>
    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }
}

/// <summary>
/// Statistics about a water network's conformity.
/// </summary>
public class NetworkConformityStatistics
{
    /// <summary>
    /// Total number of sampling events recorded.
    /// </summary>
    public int TotalSamplings { get; set; }

    /// <summary>
    /// Number of samplings where water was conform (passed all tests).
    /// </summary>
    public int ConformSamplings { get; set; }

    /// <summary>
    /// Number of samplings where water was non-conform (failed one or more tests).
    /// </summary>
    public int NonConformSamplings { get; set; }

    /// <summary>
    /// Percentage of conform samplings (0-100).
    /// </summary>
    public decimal ConformityPercentage { get; set; }

    /// <summary>
    /// Gets a status description.
    /// </summary>
    public string GetStatusDescription() => ConformityPercentage switch
    {
        >= 99 => "Excellent",
        >= 95 => "Très bon",
        >= 90 => "Bon",
        >= 80 => "Acceptable",
        _ => "À surveiller"
    };
}
