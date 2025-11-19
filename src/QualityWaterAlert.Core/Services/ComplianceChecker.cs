namespace QualityWaterAlert.Core.Services;

using QualityWaterAlert.Core.Models;
using System.Text.RegularExpressions;

/// <summary>
/// Service for checking water quality compliance against regulatory limits.
/// Handles both numeric and qualitative measurements.
/// </summary>
public interface IComplianceChecker
{
    /// <summary>
    /// Checks if a measured value complies with the quality limit.
    /// </summary>
    ComplianceResult CheckCompliance(WaterQualityParameter parameter);

    /// <summary>
    /// Checks compliance for a collection of parameters.
    /// </summary>
    List<ComplianceResult> CheckComplianceMultiple(IEnumerable<WaterQualityParameter> parameters);

    /// <summary>
    /// Checks compliance for all measurements in a sampling event.
    /// </summary>
    ComplianceCheckSummary CheckSamplingEventCompliance(SamplingEvent samplingEvent);

    /// <summary>
    /// Checks compliance for all sampling events in an analysis.
    /// </summary>
    AnalysisComplianceSummary CheckAnalysisCompliance(WaterQualityAnalysis analysis);
}

/// <summary>
/// Default implementation of the compliance checker service.
/// </summary>
public class ComplianceChecker : IComplianceChecker
{
    /// <summary>
    /// Checks if a measured value complies with the quality limit.
    /// </summary>
    public ComplianceResult CheckCompliance(WaterQualityParameter parameter)
    {
        var result = new ComplianceResult
        {
            ParameterId = parameter.Id,
            ParameterCode = parameter.ParameterCode,
            ParameterName = parameter.MajorName,
            MeasuredValue = parameter.MeasuredValue,
            Unit = parameter.Unit,
            QualityLimit = parameter.QualityLimit
        };

        // If it's qualitative, it's typically considered conform unless explicitly marked non-conform
        if (parameter.IsQualitative)
        {
            result.IsConform = !IsQualitativeNonConform(parameter.MeasuredValue, parameter.QualityLimit);
            result.CheckMethod = "Qualitative Assessment";
            return result;
        }

        // Get numeric value - prefer pre-parsed value if available, otherwise parse it
        decimal? numericValue = parameter.NumericValue ?? parameter.ParseNumericValue();
        if (numericValue == null)
        {
            result.IsConform = true; // Can't determine, assume conform
            result.CheckMethod = "Unable to parse value";
            return result;
        }

        result.NumericValue = numericValue;

        // Parse the quality limit to determine conformity
        result.IsConform = CheckNumericCompliance(numericValue.Value, parameter.QualityLimit);
        result.CheckMethod = "Numeric Comparison";

        return result;
    }

    /// <summary>
    /// Checks compliance for a collection of parameters.
    /// </summary>
    public List<ComplianceResult> CheckComplianceMultiple(IEnumerable<WaterQualityParameter> parameters)
    {
        return parameters.Select(CheckCompliance).ToList();
    }

    /// <summary>
    /// Checks compliance for all measurements in a sampling event.
    /// </summary>
    public ComplianceCheckSummary CheckSamplingEventCompliance(SamplingEvent samplingEvent)
    {
        var summary = new ComplianceCheckSummary
        {
            ReferenceId = samplingEvent.ReferenceId,
            SamplingDate = samplingEvent.SamplingDate,
            NetworkCode = samplingEvent.NetworkCode,
            CommuneName = samplingEvent.CommuneName
        };

        if (samplingEvent.Measurements.Count == 0)
        {
            summary.TotalMeasurements = 0;
            summary.ConformMeasurements = 0;
            summary.NonConformMeasurements = 0;
            summary.IsOverallConform = samplingEvent.IsConform;
            return summary;
        }

        var results = CheckComplianceMultiple(samplingEvent.Measurements);

        summary.TotalMeasurements = results.Count;
        summary.ConformMeasurements = results.Count(r => r.IsConform);
        summary.NonConformMeasurements = results.Count(r => !r.IsConform);
        summary.CompliancePercentage = summary.TotalMeasurements > 0
            ? Math.Round((summary.ConformMeasurements * 100m) / summary.TotalMeasurements, 2)
            : 0;

        summary.IsOverallConform = summary.NonConformMeasurements == 0;
        summary.NonConformParameters = results
            .Where(r => !r.IsConform)
            .Select(r => new ProblemParameter
            {
                ParameterCode = r.ParameterCode,
                ParameterName = r.ParameterName,
                MeasuredValue = r.MeasuredValue,
                QualityLimit = r.QualityLimit,
                Reason = r.GetNonConformReason()
            })
            .ToList();

        return summary;
    }

    /// <summary>
    /// Checks compliance for all sampling events in an analysis.
    /// </summary>
    public AnalysisComplianceSummary CheckAnalysisCompliance(WaterQualityAnalysis analysis)
    {
        var summary = new AnalysisComplianceSummary
        {
            AnalysisId = analysis.Id,
            CommuneName = analysis.Commune.Name,
            AnalysisPeriodStart = analysis.AnalysisPeriodStart,
            AnalysisPeriodEnd = analysis.AnalysisPeriodEnd
        };

        if (analysis.SamplingEvents.Count == 0)
        {
            summary.TotalSamplings = 0;
            summary.ConformSamplings = 0;
            summary.NonConformSamplings = 0;
            return summary;
        }

        var samplingResults = new List<ComplianceCheckSummary>();

        foreach (var samplingEvent in analysis.SamplingEvents)
        {
            var samplingResult = CheckSamplingEventCompliance(samplingEvent);
            samplingResults.Add(samplingResult);
        }

        summary.TotalSamplings = samplingResults.Count;
        summary.ConformSamplings = samplingResults.Count(r => r.IsOverallConform);
        summary.NonConformSamplings = samplingResults.Count(r => !r.IsOverallConform);
        summary.OverallCompliancePercentage = summary.TotalSamplings > 0
            ? Math.Round((summary.ConformSamplings * 100m) / summary.TotalSamplings, 2)
            : 0;

        summary.IsOverallAnalysisConform = summary.NonConformSamplings == 0;

        // Aggregate non-conform parameters
        var problemParams = new Dictionary<string, ProblemParameterStats>();

        foreach (var samplingResult in samplingResults.Where(r => !r.IsOverallConform))
        {
            foreach (var problem in samplingResult.NonConformParameters)
            {
                var key = problem.ParameterCode;
                if (!problemParams.ContainsKey(key))
                {
                    problemParams[key] = new ProblemParameterStats
                    {
                        ParameterCode = problem.ParameterCode,
                        ParameterName = problem.ParameterName,
                        NonConformCount = 0
                    };
                }
                problemParams[key].NonConformCount++;
            }
        }

        summary.ProblematicParameters = problemParams.Values
            .OrderByDescending(p => p.NonConformCount)
            .ToList();

        summary.SamplingDetails = samplingResults;

        return summary;
    }

    /// <summary>
    /// Checks if a numeric value complies with a quality limit string.
    /// </summary>
    private bool CheckNumericCompliance(decimal value, string qualityLimit)
    {
        if (string.IsNullOrWhiteSpace(qualityLimit) || qualityLimit == "X" || qualityLimit == "SANS OBJET")
            return true; // No limit means conform

        // Remove unit if present
        var limit = qualityLimit.Trim();
        if (limit.Contains(" "))
            limit = limit.Split(' ')[0]; // Take just the first part (the numeric limit)

        // Use InvariantCulture for parsing to handle "." consistently
        var culture = System.Globalization.CultureInfo.InvariantCulture;

        // Handle range limits (e.g., "200-1100")
        if (limit.Contains("-") && !limit.StartsWith("-"))
        {
            var parts = limit.Split('-');
            if (decimal.TryParse(parts[0], System.Globalization.NumberStyles.Any, culture, out var min) && 
                decimal.TryParse(parts[1], System.Globalization.NumberStyles.Any, culture, out var max))
            {
                return value >= min && value <= max;
            }
        }

        // Handle less-than or equal (e.g., "<=0.5")
        if (limit.StartsWith("<="))
        {
            var numPart = limit.Substring(2);
            if (decimal.TryParse(numPart, System.Globalization.NumberStyles.Any, culture, out var threshold))
                return value <= threshold;
        }

        // Handle less-than (e.g., "<0.5")
        if (limit.StartsWith("<"))
        {
            var numPart = limit.Substring(1);
            if (decimal.TryParse(numPart, System.Globalization.NumberStyles.Any, culture, out var threshold))
                return value < threshold;
        }

        // Handle greater-than or equal (e.g., ">=200")
        if (limit.StartsWith(">="))
        {
            var numPart = limit.Substring(2);
            if (decimal.TryParse(numPart, System.Globalization.NumberStyles.Any, culture, out var threshold))
                return value >= threshold;
        }

        // Handle greater-than (e.g., ">0")
        if (limit.StartsWith(">"))
        {
            var numPart = limit.Substring(1);
            if (decimal.TryParse(numPart, System.Globalization.NumberStyles.Any, culture, out var threshold))
                return value > threshold;
        }

        // Try parsing as plain number (assume must be less than or equal)
        if (decimal.TryParse(limit, System.Globalization.NumberStyles.Any, culture, out var plainLimit))
            return value <= plainLimit;

        // If we can't parse it, assume conform
        return true;
    }

    /// <summary>
    /// Determines if a qualitative measurement indicates non-conformity.
    /// </summary>
    private bool IsQualitativeNonConform(string measuredValue, string qualityLimit)
    {
        if (string.IsNullOrWhiteSpace(measuredValue))
            return false;

        var value = measuredValue.ToUpper().Trim();

        // Typical non-conform values
        if (value.Contains("ANORMAL") || value.Contains("PRÉSENT") || value.Contains("DETECTED") ||
            value.Contains("ODEUR") || value.Contains("GOÛT") || value.Contains("COULEUR"))
            return true;

        // Check if the measured value matches the limit (for "normal" cases)
        if (qualityLimit.ToUpper().Contains("NORMAL") && !value.Contains("NORMAL"))
            return true;

        return false;
    }
}

/// <summary>
/// Result of a single compliance check.
/// </summary>
public class ComplianceResult
{
    /// <summary>
    /// ID of the parameter checked.
    /// </summary>
    public string ParameterId { get; set; } = string.Empty;

    /// <summary>
    /// Code of the parameter.
    /// </summary>
    public string ParameterCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the parameter.
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// The measured value as a string.
    /// </summary>
    public string MeasuredValue { get; set; } = string.Empty;

    /// <summary>
    /// Numeric value if applicable.
    /// </summary>
    public decimal? NumericValue { get; set; }

    /// <summary>
    /// Unit of measurement.
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Quality limit/reference.
    /// </summary>
    public string QualityLimit { get; set; } = string.Empty;

    /// <summary>
    /// Whether this measurement is conform.
    /// </summary>
    public bool IsConform { get; set; }

    /// <summary>
    /// How the compliance was determined.
    /// </summary>
    public string CheckMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets the reason for non-conformity if applicable.
    /// </summary>
    public string GetNonConformReason()
    {
        if (IsConform)
            return string.Empty;

        if (NumericValue.HasValue)
            return $"{MeasuredValue} {Unit} exceeds limit {QualityLimit}";

        return "Qualitative measurement indicates non-conformity";
    }
}

/// <summary>
/// Summary of compliance checks for a sampling event.
/// </summary>
public class ComplianceCheckSummary
{
    /// <summary>
    /// Reference ID of the sampling event.
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// Date of sampling.
    /// </summary>
    public DateTime SamplingDate { get; set; }

    /// <summary>
    /// Network code.
    /// </summary>
    public string NetworkCode { get; set; } = string.Empty;

    /// <summary>
    /// Commune name.
    /// </summary>
    public string CommuneName { get; set; } = string.Empty;

    /// <summary>
    /// Total measurements checked.
    /// </summary>
    public int TotalMeasurements { get; set; }

    /// <summary>
    /// Number of conform measurements.
    /// </summary>
    public int ConformMeasurements { get; set; }

    /// <summary>
    /// Number of non-conform measurements.
    /// </summary>
    public int NonConformMeasurements { get; set; }

    /// <summary>
    /// Compliance percentage (0-100).
    /// </summary>
    public decimal CompliancePercentage { get; set; }

    /// <summary>
    /// Whether the overall sampling is conform.
    /// </summary>
    public bool IsOverallConform { get; set; }

    /// <summary>
    /// List of non-conform parameters.
    /// </summary>
    public List<ProblemParameter> NonConformParameters { get; set; } = new();

    /// <summary>
    /// Gets a status string.
    /// </summary>
    public string GetStatus() => IsOverallConform ? "✅ Conforme" : "❌ Non-conforme";
}

/// <summary>
/// A problem parameter found during compliance checks.
/// </summary>
public class ProblemParameter
{
    /// <summary>
    /// Parameter code.
    /// </summary>
    public string ParameterCode { get; set; } = string.Empty;

    /// <summary>
    /// Parameter name.
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// Measured value.
    /// </summary>
    public string MeasuredValue { get; set; } = string.Empty;

    /// <summary>
    /// Quality limit.
    /// </summary>
    public string QualityLimit { get; set; } = string.Empty;

    /// <summary>
    /// Reason for non-conformity.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Summary of compliance checks for an entire analysis.
/// </summary>
public class AnalysisComplianceSummary
{
    /// <summary>
    /// ID of the analysis.
    /// </summary>
    public string AnalysisId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the commune analyzed.
    /// </summary>
    public string CommuneName { get; set; } = string.Empty;

    /// <summary>
    /// Start of analysis period.
    /// </summary>
    public DateTime? AnalysisPeriodStart { get; set; }

    /// <summary>
    /// End of analysis period.
    /// </summary>
    public DateTime? AnalysisPeriodEnd { get; set; }

    /// <summary>
    /// Total sampling events checked.
    /// </summary>
    public int TotalSamplings { get; set; }

    /// <summary>
    /// Number of conform samplings.
    /// </summary>
    public int ConformSamplings { get; set; }

    /// <summary>
    /// Number of non-conform samplings.
    /// </summary>
    public int NonConformSamplings { get; set; }

    /// <summary>
    /// Overall compliance percentage (0-100).
    /// </summary>
    public decimal OverallCompliancePercentage { get; set; }

    /// <summary>
    /// Whether the analysis is overall conform.
    /// </summary>
    public bool IsOverallAnalysisConform { get; set; }

    /// <summary>
    /// Most problematic parameters (those with most non-conform occurrences).
    /// </summary>
    public List<ProblemParameterStats> ProblematicParameters { get; set; } = new();

    /// <summary>
    /// Details of each sampling check.
    /// </summary>
    public List<ComplianceCheckSummary> SamplingDetails { get; set; } = new();

    /// <summary>
    /// Gets an overall status string.
    /// </summary>
    public string GetOverallStatus() =>
        IsOverallAnalysisConform ? "✅ Analyse Conforme" : "❌ Analyse Non-conforme";
}

/// <summary>
/// Statistics about a problematic parameter across multiple samplings.
/// </summary>
public class ProblemParameterStats
{
    /// <summary>
    /// Parameter code.
    /// </summary>
    public string ParameterCode { get; set; } = string.Empty;

    /// <summary>
    /// Parameter name.
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// Number of non-conform occurrences.
    /// </summary>
    public int NonConformCount { get; set; }
}
