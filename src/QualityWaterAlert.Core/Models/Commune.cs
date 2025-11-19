namespace QualityWaterAlert.Core.Models;

/// <summary>
/// Represents a French commune (municipality) with water distribution networks.
/// Maps to DIS_COM_UDI_2025.txt data from data.gouv.fr
/// </summary>
public class Commune
{
    /// <summary>
    /// INSEE code for the commune (5-digit French municipality identifier).
    /// Example: "01001", "75056" (Paris)
    /// This is the primary identifier.
    /// </summary>
    public string INSEECode { get; set; } = string.Empty;

    /// <summary>
    /// Official name of the commune in French.
    /// Example: "ABERGEMENT-CLEMENCIAT", "PARIS"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Department code (French administrative division).
    /// Example: "01" for Ain, "75" for Paris
    /// Derived from INSEE code or stored explicitly.
    /// </summary>
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Collection of water distribution networks (UDP - Unité De Distribution Publique) serving this commune.
    /// A commune typically has 1-10+ networks, sometimes by neighborhood (quartier).
    /// </summary>
    public List<WaterNetwork> Networks { get; set; } = new();

    /// <summary>
    /// Date when water supply to this commune started being tracked (first data record).
    /// </summary>
    public DateTime? FirstRecordDate { get; set; }

    /// <summary>
    /// Date when water supply to this commune was last updated in the system.
    /// </summary>
    public DateTime? LastUpdateDate { get; set; }

    /// <summary>
    /// Gets the overall conformity status for all networks in this commune.
    /// Returns 'C' if all networks are conform, 'N' if any network has non-conform results.
    /// </summary>
    public char GetOverallConformity()
    {
        if (Networks.Count == 0)
            return 'U'; // Unknown

        // Check if all networks are conform
        var allConform = Networks.All(n => n.IsConform);
        return allConform ? 'C' : 'N';
    }

    /// <summary>
    /// Gets the number of non-conform networks in this commune.
    /// </summary>
    public int GetNonConformNetworkCount()
    {
        return Networks.Count(n => !n.IsConform);
    }

    /// <summary>
    /// Gets a summary of the commune's water quality status.
    /// </summary>
    public string GetStatusSummary()
    {
        if (Networks.Count == 0)
            return "Aucun réseau d'eau enregistré";

        var conformCount = Networks.Count(n => n.IsConform);
        return $"{conformCount}/{Networks.Count} réseaux conformes";
    }

    /// <summary>
    /// Returns a string representation of the commune.
    /// </summary>
    public override string ToString()
    {
        return $"{Name} ({INSEECode}) - {Networks.Count} réseau(x)";
    }

    /// <summary>
    /// Determines equality based on INSEE code.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Commune commune && commune.INSEECode == INSEECode;
    }

    /// <summary>
    /// Gets hash code based on INSEE code.
    /// </summary>
    public override int GetHashCode()
    {
        return INSEECode.GetHashCode();
    }
}
