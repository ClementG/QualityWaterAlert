namespace QualityWaterAlert.Core.Tests.Services;

using NUnit.Framework;
using QualityWaterAlert.Core.Models;
using QualityWaterAlert.Core.Services;

/// <summary>
/// Unit tests for the ComplianceChecker service.
/// Tests numeric and qualitative compliance checking, aggregation, and edge cases.
/// </summary>
[TestFixture]
public class ComplianceCheckerTests
{
    private IComplianceChecker _complianceChecker = null!;

    [SetUp]
    public void Setup()
    {
        _complianceChecker = new ComplianceChecker();
    }

    #region Numeric Compliance Tests

    [Test]
    [Description("Test less-than-or-equal operator: 0.5 µg/L (ARSENIC)")]
    public void CheckCompliance_NumericLeThreshold_ShouldBeCompliant()
    {
        // Arrange
        var parameter = new WaterQualityParameter
        {
            Id = "test-001",
            ParameterCode = "1379",
            MajorName = "ARSENIC",
            MinorName = "Arsenic",
            MeasuredValue = "0.3 µg/L",
            NumericValue = 0.3m,
            QualityLimit = "<=0.5 µg/L",
            Unit = "µg/L",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        // Act
        var result = _complianceChecker.CheckCompliance(parameter);

        // Assert
        Assert.That(result.IsConform, Is.True);
    }

    [Test]
    [Description("Test less-than-or-equal operator with boundary value")]
    public void CheckCompliance_NumericAtBoundary_ShouldBeCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-002",
            ParameterCode = "1379",
            MajorName = "ARSENIC",
            MinorName = "Arsenic",
            MeasuredValue = "0.5 µg/L",
            NumericValue = 0.5m,
            QualityLimit = "<=0.5 µg/L",
            Unit = "µg/L",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.True);
    }

    [Test]
    [Description("Test less-than-or-equal operator exceeding limit")]
    public void CheckCompliance_NumericExceedsLimit_ShouldBeNonCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-003",
            ParameterCode = "1379",
            MajorName = "ARSENIC",
            MinorName = "Arsenic",
            MeasuredValue = "0.6 µg/L",
            NumericValue = 0.6m,
            QualityLimit = "<=0.5 µg/L",
            Unit = "µg/L",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.False);
    }

    [Test]
    [Description("Test greater-than operator")]
    public void CheckCompliance_NumericGreaterThan_ShouldBeCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-004",
            ParameterCode = "1160",
            MajorName = "PH",
            MinorName = "PH",
            MeasuredValue = "7.5",
            NumericValue = 7.5m,
            QualityLimit = ">6.5",
            Unit = "",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.True);
    }

    [Test]
    [Description("Test range operator: 200-1100 µS/cm (CONDUCTIVITY)")]
    public void CheckCompliance_NumericInRange_ShouldBeCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-005",
            ParameterCode = "1177",
            MajorName = "CONDUCTIVITY",
            MinorName = "Conductivity",
            MeasuredValue = "500 µS/cm",
            NumericValue = 500m,
            QualityLimit = "200-1100 µS/cm",
            Unit = "µS/cm",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.True);
    }

    [Test]
    [Description("Test range operator below minimum")]
    public void CheckCompliance_NumericBelowRange_ShouldBeNonCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-006",
            ParameterCode = "1177",
            MajorName = "CONDUCTIVITY",
            MinorName = "Conductivity",
            MeasuredValue = "100 µS/cm",
            NumericValue = 100m,
            QualityLimit = "200-1100 µS/cm",
            Unit = "µS/cm",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.False);
    }

    [Test]
    [Description("Test range operator above maximum")]
    public void CheckCompliance_NumericAboveRange_ShouldBeNonCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-007",
            ParameterCode = "1177",
            MajorName = "CONDUCTIVITY",
            MinorName = "Conductivity",
            MeasuredValue = "1200 µS/cm",
            NumericValue = 1200m,
            QualityLimit = "200-1100 µS/cm",
            Unit = "µS/cm",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.False);
    }

    #endregion

    #region Qualitative Compliance Tests

    [Test]
    [Description("Test normal aspect (compliant qualitative)")]
    public void CheckCompliance_QualitativeNormal_ShouldBeCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-008",
            ParameterCode = "1247",
            MajorName = "ASPECT",
            MinorName = "Aspect",
            MeasuredValue = "normal",
            QualityLimit = "normal",
            Unit = "",
            Type = ParameterType.Qualitative,
            QualityType = 'O',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.True);
    }

    [Test]
    [Description("Test abnormal aspect (non-compliant qualitative)")]
    public void CheckCompliance_QualitativeAnormal_ShouldBeNonCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-009",
            ParameterCode = "1247",
            MajorName = "ASPECT",
            MinorName = "Aspect",
            MeasuredValue = "ANORMAL",
            QualityLimit = "normal",
            Unit = "",
            Type = ParameterType.Qualitative,
            QualityType = 'O',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.False);
    }

    [Test]
    [Description("Test present bacteria (non-compliant)")]
    public void CheckCompliance_QualitativeBacteriaPresent_ShouldBeNonCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-010",
            ParameterCode = "1087",
            MajorName = "E.COLI",
            MinorName = "E. Coli",
            MeasuredValue = "PRÉSENT",
            QualityLimit = "ABSENT",
            Unit = "",
            Type = ParameterType.Qualitative,
            QualityType = 'O',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.False);
    }

    #endregion

    #region Edge Cases Tests

    [Test]
    [Description("Test with missing measured value")]
    public void CheckCompliance_MissingMeasuredValue_ShouldHandleGracefully()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-011",
            ParameterCode = "1379",
            MajorName = "ARSENIC",
            MinorName = "Arsenic",
            MeasuredValue = "",
            QualityLimit = "<=0.5 µg/L",
            Unit = "µg/L",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    [Description("Test with zero value")]
    public void CheckCompliance_ZeroValue_ShouldBeCompliant()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-012",
            ParameterCode = "1379",
            MajorName = "ARSENIC",
            MinorName = "Arsenic",
            MeasuredValue = "0 µg/L",
            NumericValue = 0m,
            QualityLimit = "<=0.5 µg/L",
            Unit = "µg/L",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.True);
    }

    [Test]
    [Description("Test with decimal values")]
    public void CheckCompliance_DecimalValue_ShouldWorkCorrectly()
    {
        var parameter = new WaterQualityParameter
        {
            Id = "test-013",
            ParameterCode = "1379",
            MajorName = "ARSENIC",
            MinorName = "Arsenic",
            MeasuredValue = "0.167 µg/L",
            NumericValue = 0.167m,
            QualityLimit = "<=0.5 µg/L",
            Unit = "µg/L",
            Type = ParameterType.Numeric,
            QualityType = 'N',
            MeasurementDate = DateTime.Now
        };

        var result = _complianceChecker.CheckCompliance(parameter);

        Assert.That(result.IsConform, Is.True);
    }

    #endregion

    #region Multiple Parameters Tests

    [Test]
    [Description("Test checking multiple parameters")]
    public void CheckComplianceMultiple_SeveralParameters_ShouldReturnAllResults()
    {
        var parameters = new List<WaterQualityParameter>
        {
            new WaterQualityParameter
            {
                Id = "test-m01",
                ParameterCode = "1379",
                MajorName = "ARSENIC",
                MinorName = "Arsenic",
                MeasuredValue = "0.3 µg/L",
                NumericValue = 0.3m,
                QualityLimit = "<=0.5 µg/L",
                Unit = "µg/L",
                Type = ParameterType.Numeric,
                QualityType = 'N',
                MeasurementDate = DateTime.Now
            },
            new WaterQualityParameter
            {
                Id = "test-m02",
                ParameterCode = "1087",
                MajorName = "E.COLI",
                MinorName = "E. Coli",
                MeasuredValue = "ABSENT",
                QualityLimit = "ABSENT",
                Unit = "",
                Type = ParameterType.Qualitative,
                QualityType = 'O',
                MeasurementDate = DateTime.Now
            },
            new WaterQualityParameter
            {
                Id = "test-m03",
                ParameterCode = "1177",
                MajorName = "CONDUCTIVITY",
                MinorName = "Conductivity",
                MeasuredValue = "1200 µS/cm",
                NumericValue = 1200m,
                QualityLimit = "200-1100 µS/cm",
                Unit = "µS/cm",
                Type = ParameterType.Numeric,
                QualityType = 'N',
                MeasurementDate = DateTime.Now
            }
        };

        var results = _complianceChecker.CheckComplianceMultiple(parameters);

        Assert.That(results, Has.Count.EqualTo(3));
        Assert.That(results[0].IsConform, Is.True);
        Assert.That(results[1].IsConform, Is.True);
        Assert.That(results[2].IsConform, Is.False);
    }

    #endregion

    #region Sampling Event Compliance Tests

    [Test]
    [Description("Test compliant sampling event")]
    public void CheckSamplingEventCompliance_AllCompliantParameters_ShouldBeCompliant()
    {
        var now = DateTime.Now;
        var timeSpan = TimeSpan.FromHours(12);

        var samplingEvent = new SamplingEvent
        {
            ReferenceId = "2025-001",
            NetworkCode = "UDP-001",
            SamplingDate = now,
            SamplingTime = timeSpan,
            BacterioConformity = 'C',
            ChemicalConformity = 'C',
            Measurements = new List<WaterQualityParameter>
            {
                new WaterQualityParameter
                {
                    Id = "test-se01",
                    ParameterCode = "1379",
                    MajorName = "ARSENIC",
                    MinorName = "Arsenic",
                    MeasuredValue = "0.3 µg/L",
                    NumericValue = 0.3m,
                    QualityLimit = "<=0.5 µg/L",
                    Unit = "µg/L",
                    Type = ParameterType.Numeric,
                    QualityType = 'N',
                    MeasurementDate = DateTime.Now
                },
                new WaterQualityParameter
                {
                    Id = "test-se02",
                    ParameterCode = "1087",
                    MajorName = "E.COLI",
                    MinorName = "E. Coli",
                    MeasuredValue = "ABSENT",
                    QualityLimit = "ABSENT",
                    Unit = "",
                    Type = ParameterType.Qualitative,
                    QualityType = 'O',
                    MeasurementDate = DateTime.Now
                }
            }
        };

        var summary = _complianceChecker.CheckSamplingEventCompliance(samplingEvent);

        Assert.That(summary.IsOverallConform, Is.True);
        Assert.That(summary.TotalMeasurements, Is.EqualTo(2));
        Assert.That(summary.ConformMeasurements, Is.EqualTo(2));
        Assert.That(summary.NonConformMeasurements, Is.EqualTo(0));
    }

    [Test]
    [Description("Test non-compliant sampling event")]
    public void CheckSamplingEventCompliance_SomeNonCompliantParameters_ShouldBeNonCompliant()
    {
        var now = DateTime.Now;
        var timeSpan = TimeSpan.FromHours(12);

        var samplingEvent = new SamplingEvent
        {
            ReferenceId = "2025-002",
            NetworkCode = "UDP-002",
            SamplingDate = now,
            SamplingTime = timeSpan,
            BacterioConformity = 'C',
            ChemicalConformity = 'N',
            Measurements = new List<WaterQualityParameter>
            {
                new WaterQualityParameter
                {
                    Id = "test-nc01",
                    ParameterCode = "1379",
                    MajorName = "ARSENIC",
                    MinorName = "Arsenic",
                    MeasuredValue = "0.8 µg/L",
                    NumericValue = 0.8m,
                    QualityLimit = "<=0.5 µg/L",
                    Unit = "µg/L",
                    Type = ParameterType.Numeric,
                    QualityType = 'N',
                    MeasurementDate = DateTime.Now
                },
                new WaterQualityParameter
                {
                    Id = "test-nc02",
                    ParameterCode = "1087",
                    MajorName = "E.COLI",
                    MinorName = "E. Coli",
                    MeasuredValue = "ABSENT",
                    QualityLimit = "ABSENT",
                    Unit = "",
                    Type = ParameterType.Qualitative,
                    QualityType = 'O',
                    MeasurementDate = DateTime.Now
                }
            }
        };

        var summary = _complianceChecker.CheckSamplingEventCompliance(samplingEvent);

        Assert.That(summary.IsOverallConform, Is.False);
        Assert.That(summary.TotalMeasurements, Is.EqualTo(2));
        Assert.That(summary.NonConformMeasurements, Is.GreaterThan(0));
    }

    #endregion

    #region Analysis Compliance Tests

    [Test]
    [Description("Test fully compliant analysis")]
    public void CheckAnalysisCompliance_AllCompliant_ShouldHaveHighConformity()
    {
        var now = DateTime.Now;
        var timeSpan = TimeSpan.FromHours(12);

        var analysis = new WaterQualityAnalysis
        {
            Id = "ana-001",
            Commune = new Commune { INSEECode = "75056", Name = "Paris", DepartmentCode = "75" },
            AnalysisPeriodStart = now.AddMonths(-1),
            AnalysisPeriodEnd = now,
            SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "2025-sa01",
                    NetworkCode = "UDP-001",
                    SamplingDate = now,
                    SamplingTime = timeSpan,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C',
                    Measurements = new List<WaterQualityParameter>
                    {
                        new WaterQualityParameter
                        {
                            Id = "test-ana01",
                            ParameterCode = "1379",
                            MajorName = "ARSENIC",
                            MinorName = "Arsenic",
                            MeasuredValue = "0.3 µg/L",
                            NumericValue = 0.3m,
                            QualityLimit = "<=0.5 µg/L",
                            Unit = "µg/L",
                            Type = ParameterType.Numeric,
                            QualityType = 'N',
                            MeasurementDate = DateTime.Now
                        }
                    }
                }
            }
        };

        var summary = _complianceChecker.CheckAnalysisCompliance(analysis);

        Assert.That(summary.OverallCompliancePercentage, Is.EqualTo(100.0m));
        Assert.That(summary.IsOverallAnalysisConform, Is.True);
    }

    #endregion
}
