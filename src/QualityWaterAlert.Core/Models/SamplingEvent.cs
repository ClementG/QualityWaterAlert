namespace QualityWaterAlert.Core.Models;

/// <summary>
/// Represents a water quality sampling event (PLV - Prélèvement).
/// Maps to DIS_PLV_2025.txt data from data.gouv.fr
/// </summary>
public class SamplingEvent
{
    /// <summary>
    /// Unique reference ID for this sampling event.
    /// Example: "00100143925", "00100144152"
    /// This links to all measurements in DIS_RESULT.
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// Code of the water network where this sampling occurred.
    /// Example: "001000556", "001000003"
    /// </summary>
    public string NetworkCode { get; set; } = string.Empty;

    /// <summary>
    /// INSEE code of the principal commune served.
    /// Example: "01001", "01007"
    /// </summary>
    public string CommuneCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the principal commune.
    /// Example: "AMBRONAY", "ABERGEMENT-CLEMENCIAT"
    /// </summary>
    public string CommuneName { get; set; } = string.Empty;

    /// <summary>
    /// Date when the water sample was collected.
    /// Example: "2025-01-21"
    /// </summary>
    public DateTime SamplingDate { get; set; }

    /// <summary>
    /// Time when the water sample was collected.
    /// Example: "12:35", "10:35"
    /// </summary>
    public TimeSpan? SamplingTime { get; set; }

    /// <summary>
    /// Conclusion text about the sampling result (in French).
    /// Example: "Eau d'alimentation conforme aux exigences de qualité en vigueur..."
    /// </summary>
    public string ConclusionText { get; set; } = string.Empty;

    /// <summary>
    /// Bacteriological conformity status.
    /// 'C' = Conform (bacteria levels acceptable), 'N' = Non-conform (bacteria detected)
    /// </summary>
    public char BacterioConformity { get; set; } = 'C';

    /// <summary>
    /// Chemical conformity status.
    /// 'C' = Conform (chemical levels acceptable), 'N' = Non-conform (chemicals exceed limit)
    /// </summary>
    public char ChemicalConformity { get; set; } = 'C';

    /// <summary>
    /// Reference bacteriological conformity (used for standardized comparison).
    /// Similar to BacterioConformity, used for regulatory comparisons.
    /// </summary>
    public char ReferenceBacterioConformity { get; set; } = 'C';

    /// <summary>
    /// Reference chemical conformity (used for standardized comparison).
    /// Similar to ChemicalConformity, used for regulatory comparisons.
    /// </summary>
    public char ReferenceChemicalConformity { get; set; } = 'C';

    /// <summary>
    /// If this network's water comes from an upstream network, this is the upstream network code.
    /// Example: "001001304"
    /// Empty/null if this is a primary source.
    /// </summary>
    public string? UpstreamNetworkCode { get; set; }

    /// <summary>
    /// Name of the upstream network.
    /// </summary>
    public string? UpstreamNetworkName { get; set; }

    /// <summary>
    /// Percentage of water flow from the upstream network.
    /// Example: "100%", "60%"
    /// </summary>
    public string? UpstreamFlowPercentage { get; set; }

    /// <summary>
    /// Water management unit (UGE) responsible for this area.
    /// Example: "SYND. EAUX REGION D'AMBERIEU-EN-B"
    /// </summary>
    public string? ManagementUnit { get; set; }

    /// <summary>
    /// Distribution company/operator.
    /// Example: "SERA - SYNDICAT DES EAUX..."
    /// </summary>
    public string? DistributionCompany { get; set; }

    /// <summary>
    /// Master of work / Network owner.
    /// </summary>
    public string? NetworkOwner { get; set; }

    /// <summary>
    /// Department code (INSEE).
    /// Example: "001", "075"
    /// </summary>
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Collection of parameter measurements for this sampling.
    /// Contains all individual parameter test results.
    /// </summary>
    public List<WaterQualityParameter> Measurements { get; set; } = new();

    /// <summary>
    /// Gets the overall conformity status of this sampling.
    /// Returns 'C' if all parameters passed, 'N' if any failed.
    /// </summary>
    public char GetOverallConformity()
    {
        // If we have measurement data, check all parameters
        if (Measurements.Count > 0)
        {
            var allConform = Measurements.All(m => m.IsConform);
            return allConform ? 'C' : 'N';
        }

        // Otherwise, use the recorded bacterio and chemical conformity
        return (BacterioConformity == 'C' && ChemicalConformity == 'C') ? 'C' : 'N';
    }

    /// <summary>
    /// Determines if this sampling shows conform water.
    /// </summary>
    public bool IsConform => GetOverallConformity() == 'C';

    /// <summary>
    /// Determines if this sampling shows non-conform water (quality issue).
    /// </summary>
    public bool IsNonConform => GetOverallConformity() == 'N';

    /// <summary>
    /// Gets the count of non-conform measurements.
    /// </summary>
    public int GetNonConformMeasurementCount()
    {
        return Measurements.Count(m => m.IsNonConform);
    }

    /// <summary>
    /// Gets the list of problem parameters (non-conform measurements).
    /// </summary>
    public List<WaterQualityParameter> GetProblemParameters()
    {
        return Measurements.Where(m => m.IsNonConform).ToList();
    }

    /// <summary>
    /// Gets a summary string about this sampling.
    /// </summary>
    public string GetSummary()
    {
        var date = SamplingDate.ToString("yyyy-MM-dd");
        var time = SamplingTime.HasValue ? $" à {SamplingTime:hh\\:mm}" : "";
        var status = IsConform ? "✅ Conforme" : "❌ Non-conforme";

        return $"Prélèvement {date}{time} - {status}";
    }

    /// <summary>
    /// Returns a string representation of the sampling event.
    /// </summary>
    public override string ToString()
    {
        return GetSummary();
    }

    /// <summary>
    /// Determines equality based on reference ID.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is SamplingEvent evt && evt.ReferenceId == ReferenceId;
    }

    /// <summary>
    /// Gets hash code based on reference ID.
    /// </summary>
    public override int GetHashCode()
    {
        return ReferenceId.GetHashCode();
    }
}
