namespace QualityWaterAlert.Core.Models;

/// <summary>
/// Represents a water quality parameter with its measured value, limits, and conformity status.
/// Based on data from data.gouv.fr water quality control results.
/// Maps to fields in DIS_RESULT_2025.txt and DIS_PLV_2025.txt
/// </summary>
public class WaterQualityParameter
{
    /// <summary>
    /// Unique identifier for this parameter measurement.
    /// Example: "00100143015"
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// SISE-eaux parameter code (French water quality system).
    /// Example: "CLVYL" (Chlorure de vinyl monomère), "ASP" (Aspect), "CDT25" (Conductivity)
    /// </summary>
    public string ParameterCode { get; set; } = string.Empty;

    /// <summary>
    /// Major/official parameter name in French.
    /// Example: "CHLORURE DE VINYL MONOMÈRE"
    /// </summary>
    public string MajorName { get; set; } = string.Empty;

    /// <summary>
    /// Minor/detailed parameter name (more user-friendly).
    /// Example: "Chlorure de vinyl monomère"
    /// </summary>
    public string MinorName { get; set; } = string.Empty;

    /// <summary>
    /// Web-friendly parameter name (simplified for UI display).
    /// Can be empty if same as MinorName.
    /// </summary>
    public string? WebName { get; set; }

    /// <summary>
    /// Parameter type: indicates if measurement is numeric or qualitative.
    /// </summary>
    public ParameterType Type { get; set; }

    /// <summary>
    /// The measured value as a string (can be numeric or descriptive).
    /// Examples: "0.167", "Normal", "<1", "SANS OBJET"
    /// </summary>
    public string MeasuredValue { get; set; } = string.Empty;

    /// <summary>
    /// Numeric representation of the measured value for calculations.
    /// For qualitative measurements (e.g., "Normal"), this will be null.
    /// Example: 0.167 (parsed from "0.167 µg/L")
    /// </summary>
    public decimal? NumericValue { get; set; }

    /// <summary>
    /// Unit of measurement.
    /// Examples: "µg/L" (micrograms per liter), "n/(100mL)", "µS/cm", "SANS OBJET"
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Quality limit/reference value as a string.
    /// Examples: "<=0.5 µg/L", "200-1100 µS/cm", "<=0 n/(100mL)", "X" (no limit)
    /// </summary>
    public string QualityLimit { get; set; } = string.Empty;

    /// <summary>
    /// Analysis site type.
    /// 'L' = Laboratory analysis, 'N' = Field/On-site analysis
    /// </summary>
    public char AnalysisSite { get; set; } = 'L';

    /// <summary>
    /// Whether this parameter is qualitative (descriptive) vs numeric.
    /// 'N' = Numeric, 'O' = Qualitative (Organoleptic, Text-based)
    /// </summary>
    public char QualityType { get; set; } = 'N';

    /// <summary>
    /// Conformity status of this measurement.
    /// 'C' = Conform (passes), 'N' = Non-conform (fails), null = Unknown/Not tested
    /// </summary>
    public char? ConformityStatus { get; set; }

    /// <summary>
    /// CAS (Chemical Abstracts Service) number for chemical substances.
    /// Example: "75-01-4" for Vinyl Chloride
    /// Only populated for chemical parameters.
    /// </summary>
    public string? CASNumber { get; set; }

    /// <summary>
    /// The date this parameter was measured.
    /// </summary>
    public DateTime MeasurementDate { get; set; }

    /// <summary>
    /// The time this parameter was measured.
    /// </summary>
    public TimeSpan? MeasurementTime { get; set; }

    /// <summary>
    /// Reference to the sampling event this measurement belongs to.
    /// Links to DIS_PLV_2025.txt referenceprel
    /// </summary>
    public string SamplingReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// Department code (INSEE).
    /// Example: "001" for Ain, "075" for Paris
    /// </summary>
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Determines if this measurement indicates a conformity issue (non-conform or threshold warning).
    /// </summary>
    public bool IsNonConform => ConformityStatus == 'N';

    /// <summary>
    /// Determines if this measurement is compliant with water quality standards.
    /// </summary>
    public bool IsConform => ConformityStatus == 'C';

    /// <summary>
    /// Determines if this is a qualitative (text-based) measurement.
    /// </summary>
    public bool IsQualitative => QualityType == 'O';

    /// <summary>
    /// Determines if this measurement was done in a laboratory (vs field).
    /// </summary>
    public bool IsLabAnalysis => AnalysisSite == 'L';

    /// <summary>
    /// Gets the display name for this parameter (prefers WebName, falls back to MinorName).
    /// </summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(WebName) ? WebName : MinorName;

    /// <summary>
    /// Gets a human-readable status string for UI display.
    /// Examples: "✅ Conforme", "❌ Non-conforme", "⚠️ Inconnu"
    /// </summary>
    public string StatusBadge => ConformityStatus switch
    {
        'C' => "✅ Conforme",
        'N' => "❌ Non-conforme",
        _ => "⚠️ Inconnu"
    };

    /// <summary>
    /// Gets the measurement with unit for display.
    /// </summary>
    public string MeasurementDisplay => string.IsNullOrWhiteSpace(Unit) || Unit == "SANS OBJET"
        ? MeasuredValue
        : $"{MeasuredValue} {Unit}";

    /// <summary>
    /// Creates a new instance of WaterQualityParameter.
    /// </summary>
    public WaterQualityParameter()
    {
    }

    /// <summary>
    /// Creates a new instance with essential parameters.
    /// </summary>
    public WaterQualityParameter(
        string id,
        string parameterCode,
        string majorName,
        string minorName,
        string measuredValue,
        string unit,
        string qualityLimit,
        DateTime measurementDate,
        ParameterType type = ParameterType.Numeric)
    {
        Id = id;
        ParameterCode = parameterCode;
        MajorName = majorName;
        MinorName = minorName;
        MeasuredValue = measuredValue;
        Unit = unit;
        QualityLimit = qualityLimit;
        MeasurementDate = measurementDate;
        Type = type;
    }

    /// <summary>
    /// Parses the numeric value from MeasuredValue string, handling special cases.
    /// Examples: "0.167" → 0.167, "<1" → 0, "Normal" → null
    /// </summary>
    public decimal? ParseNumericValue()
    {
        if (IsQualitative)
            return null;

        // Handle special cases
        if (string.IsNullOrWhiteSpace(MeasuredValue))
            return null;

        var value = MeasuredValue.Trim().ToUpper();

        // Handle less-than and greater-than operators
        if (value.StartsWith("<"))
            return decimal.TryParse(value.Substring(1), out var result) ? result : null;

        if (value.StartsWith(">"))
            return decimal.TryParse(value.Substring(1), out var result) ? result : null;

        // Try to parse as decimal
        return decimal.TryParse(value, out var numValue) ? numValue : null;
    }

    /// <summary>
    /// Returns a string representation of the parameter for logging.
    /// </summary>
    public override string ToString()
    {
        return $"{MajorName}: {MeasuredValue}{(string.IsNullOrWhiteSpace(Unit) ? "" : $" {Unit}")} " +
               $"(Limit: {QualityLimit}) - {StatusBadge}";
    }
}

/// <summary>
/// Represents the type of parameter measurement.
/// </summary>
public enum ParameterType
{
    /// <summary>
    /// Numeric measurement with a value and unit.
    /// Example: 0.167 µg/L, 332 µS/cm
    /// </summary>
    Numeric,

    /// <summary>
    /// Qualitative measurement with a descriptive value.
    /// Example: "Aspect normal", "E. Coli présentes", "<1/100mL"
    /// </summary>
    Qualitative,

    /// <summary>
    /// Reference parameter (special type for standards/comparisons).
    /// </summary>
    Reference,

    /// <summary>
    /// Unknown or unclassified parameter type.
    /// </summary>
    Unknown
}
