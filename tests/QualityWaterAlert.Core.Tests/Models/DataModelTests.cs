namespace QualityWaterAlert.Core.Tests.Models;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using QualityWaterAlert.Core.Models;

/// <summary>
/// Comprehensive unit tests for all data models.
/// Tests model properties, computed values, and business logic methods.
/// </summary>
[TestFixture]
public class DataModelTests
{
    // ==================== WaterQualityParameter Tests ====================

    [TestFixture]
    public class WaterQualityParameterTests
    {
        [Test]
        public void Create_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var param = new WaterQualityParameter
            {
                ParameterCode = "BA001",
                MeasuredValue = "0.1",
                NumericValue = 0.1m,
                QualityLimit = "<0.5",
                Type = ParameterType.Numeric,
                QualityType = 'N'
            };

            // Assert
            Assert.That(param.ParameterCode, Is.EqualTo("BA001"));
            Assert.That(param.MeasuredValue, Is.EqualTo("0.1"));
            Assert.That(param.NumericValue, Is.EqualTo(0.1m));
            Assert.That(param.QualityLimit, Is.EqualTo("<0.5"));
            Assert.That(param.Type, Is.EqualTo(ParameterType.Numeric));
            Assert.That(param.QualityType, Is.EqualTo('N'));
        }

        [Test]
        public void IsQualitative_WithNumericType_ReturnsFalse()
        {
            // Arrange
            var param = new WaterQualityParameter { QualityType = 'N' };

            // Act & Assert
            Assert.That(param.IsQualitative, Is.False);
        }

        [Test]
        public void IsQualitative_WithQualitativeType_ReturnsTrue()
        {
            // Arrange
            var param = new WaterQualityParameter { QualityType = 'O' };

            // Act & Assert
            Assert.That(param.IsQualitative, Is.True);
        }

        [Test]
        public void IsConform_WithConformStatus_ReturnsTrue()
        {
            // Arrange
            var param = new WaterQualityParameter { ConformityStatus = 'C' };

            // Act & Assert
            Assert.That(param.IsConform, Is.True);
        }

        [Test]
        public void IsNonConform_WithNonConformStatus_ReturnsTrue()
        {
            // Arrange
            var param = new WaterQualityParameter { ConformityStatus = 'N' };

            // Act & Assert
            Assert.That(param.IsNonConform, Is.True);
        }

        [Test]
        public void DisplayName_WithWebName_ReturnsWebName()
        {
            // Arrange
            var param = new WaterQualityParameter 
            { 
                MinorName = "Minor",
                WebName = "Web"
            };

            // Act
            var result = param.DisplayName;

            // Assert
            Assert.That(result, Is.EqualTo("Web"));
        }

        [Test]
        public void DisplayName_WithoutWebName_ReturnsMinorName()
        {
            // Arrange
            var param = new WaterQualityParameter 
            { 
                MinorName = "Minor",
                WebName = null
            };

            // Act
            var result = param.DisplayName;

            // Assert
            Assert.That(result, Is.EqualTo("Minor"));
        }

        [Test]
        public void StatusBadge_WithConform_ReturnsConformBadge()
        {
            // Arrange
            var param = new WaterQualityParameter { ConformityStatus = 'C' };

            // Act
            var result = param.StatusBadge;

            // Assert
            Assert.That(result, Contains.Substring("Conforme"));
        }

        [Test]
        public void StatusBadge_WithNonConform_ReturnsNonConformBadge()
        {
            // Arrange
            var param = new WaterQualityParameter { ConformityStatus = 'N' };

            // Act
            var result = param.StatusBadge;

            // Assert
            Assert.That(result, Contains.Substring("Non-conforme"));
        }

        [Test]
        public void MeasurementDisplay_WithUnit_IncludesUnit()
        {
            // Arrange
            var param = new WaterQualityParameter 
            { 
                MeasuredValue = "0.5",
                Unit = "µg/L"
            };

            // Act
            var result = param.MeasurementDisplay;

            // Assert
            Assert.That(result, Contains.Substring("0.5"));
            Assert.That(result, Contains.Substring("µg/L"));
        }

        [Test]
        public void ParameterType_DefaultValue_IsNumeric()
        {
            // Arrange & Act
            var param = new WaterQualityParameter();

            // Assert
            Assert.That(param.Type, Is.EqualTo(ParameterType.Numeric));
        }
    }

    // ==================== Commune Tests ====================

    [TestFixture]
    public class CommuneTests
    {
        private Commune _commune = null!;

        [SetUp]
        public void SetUp()
        {
            _commune = new Commune
            {
                INSEECode = "75056",
                Name = "PARIS",
                DepartmentCode = "75"
            };
        }

        [Test]
        public void Create_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var commune = new Commune
            {
                INSEECode = "75056",
                Name = "PARIS",
                DepartmentCode = "75",
                FirstRecordDate = DateTime.Now,
                LastUpdateDate = DateTime.Now
            };

            // Assert
            Assert.That(commune.INSEECode, Is.EqualTo("75056"));
            Assert.That(commune.Name, Is.EqualTo("PARIS"));
            Assert.That(commune.DepartmentCode, Is.EqualTo("75"));
            Assert.That(commune.FirstRecordDate, Is.Not.Null);
            Assert.That(commune.LastUpdateDate, Is.Not.Null);
        }

        [Test]
        public void GetOverallConformity_WithNoNetworks_ReturnsUnknown()
        {
            // Arrange
            _commune.Networks = new List<WaterNetwork>();

            // Act
            var result = _commune.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('U'));
        }

        [Test]
        public void GetOverallConformity_WithAllConformNetworks_ReturnsConform()
        {
            // Arrange
            _commune.Networks = new List<WaterNetwork>
            {
                CreateConformNetwork("NET001"),
                CreateConformNetwork("NET002")
            };

            // Act
            var result = _commune.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('C'));
        }

        [Test]
        public void GetOverallConformity_WithAnyNonConformNetwork_ReturnsNonConform()
        {
            // Arrange
            _commune.Networks = new List<WaterNetwork>
            {
                CreateConformNetwork("NET001"),
                CreateNonConformNetwork("NET002")
            };

            // Act
            var result = _commune.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('N'));
        }

        [Test]
        public void GetNonConformNetworkCount_ReturnsCorrectCount()
        {
            // Arrange
            _commune.Networks = new List<WaterNetwork>
            {
                CreateConformNetwork("NET001"),
                CreateNonConformNetwork("NET002"),
                CreateNonConformNetwork("NET003")
            };

            // Act
            var result = _commune.GetNonConformNetworkCount();

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void GetStatusSummary_WithNoNetworks_ReturnsNoNetworksMessage()
        {
            // Arrange
            _commune.Networks = new List<WaterNetwork>();

            // Act
            var result = _commune.GetStatusSummary();

            // Assert
            Assert.That(result, Contains.Substring("Aucun").IgnoreCase);
        }

        [Test]
        public void GetStatusSummary_WithNetworks_ReturnsSummary()
        {
            // Arrange
            _commune.Networks = new List<WaterNetwork>
            {
                CreateConformNetwork("NET001"),
                CreateConformNetwork("NET002"),
                CreateNonConformNetwork("NET003")
            };

            // Act
            var result = _commune.GetStatusSummary();

            // Assert
            Assert.That(result, Contains.Substring("2"));
            Assert.That(result, Contains.Substring("3"));
        }

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            _commune.Networks = new List<WaterNetwork>
            {
                CreateConformNetwork("NET001"),
                CreateConformNetwork("NET002")
            };

            // Act
            var result = _commune.ToString();

            // Assert
            Assert.That(result, Contains.Substring("PARIS"));
            Assert.That(result, Contains.Substring("75056"));
            Assert.That(result, Contains.Substring("2"));
        }

        [Test]
        public void Equals_WithSameINSEECode_ReturnsTrue()
        {
            // Arrange
            var other = new Commune { INSEECode = "75056", Name = "Different" };

            // Act
            var result = _commune.Equals(other);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Equals_WithDifferentINSEECode_ReturnsFalse()
        {
            // Arrange
            var other = new Commune { INSEECode = "92044", Name = "NANTERRE" };

            // Act
            var result = _commune.Equals(other);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetHashCode_WithSameINSEECode_ReturnsSameHash()
        {
            // Arrange
            var other = new Commune { INSEECode = "75056" };

            // Act
            var hash1 = _commune.GetHashCode();
            var hash2 = other.GetHashCode();

            // Assert
            Assert.That(hash1, Is.EqualTo(hash2));
        }
    }

    // ==================== WaterNetwork Tests ====================

    [TestFixture]
    public class WaterNetworkTests
    {
        private WaterNetwork _network = null!;

        [SetUp]
        public void SetUp()
        {
            _network = new WaterNetwork
            {
                Code = "001000556",
                Name = "NETWORK 001",
                PrincipalCommuneCode = "01001"
            };
        }

        [Test]
        public void Create_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var network = new WaterNetwork
            {
                Code = "001000556",
                Name = "NETWORK 001",
                PrincipalCommuneCode = "01001",
                UpstreamNetworkCode = "001000123",
                UpstreamFlowPercentage = 50m
            };

            // Assert
            Assert.That(network.Code, Is.EqualTo("001000556"));
            Assert.That(network.Name, Is.EqualTo("NETWORK 001"));
            Assert.That(network.UpstreamNetworkCode, Is.EqualTo("001000123"));
            Assert.That(network.UpstreamFlowPercentage, Is.EqualTo(50m));
        }

        [Test]
        public void GetLatestSampling_WithNoSamplings_ReturnsNull()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>();

            // Act
            var result = _network.GetLatestSampling();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetLatestSampling_WithMultipleSamplings_ReturnsNewest()
        {
            // Arrange
            var oldDate = DateTime.Now.AddDays(-10);
            var newDate = DateTime.Now;

            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent { ReferenceId = "SAMP001", SamplingDate = oldDate },
                new SamplingEvent { ReferenceId = "SAMP002", SamplingDate = newDate }
            };

            // Act
            var result = _network.GetLatestSampling();

            // Assert
            Assert.That(result?.ReferenceId, Is.EqualTo("SAMP002"));
        }

        [Test]
        public void GetOverallConformity_WithNoSamplings_ReturnsUnknown()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>();

            // Act
            var result = _network.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('U'));
        }

        [Test]
        public void GetOverallConformity_WithConformSampling_ReturnsConform()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _network.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('C'));
        }

        [Test]
        public void GetOverallConformity_WithNonConformBacterio_ReturnsNonConform()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'N',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _network.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('N'));
        }

        [Test]
        public void GetOverallConformity_WithNonConformChemical_ReturnsNonConform()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'N'
                }
            };

            // Act
            var result = _network.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('N'));
        }

        [Test]
        public void IsConform_WithConformSampling_ReturnsTrue()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _network.IsConform;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsConform_WithNonConformSampling_ReturnsFalse()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'N',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _network.IsConform;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsUpstreamSupplied_WithUpstreamCode_ReturnsTrue()
        {
            // Arrange
            _network.UpstreamNetworkCode = "001000123";

            // Act
            var result = _network.IsUpstreamSupplied;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsUpstreamSupplied_WithoutUpstreamCode_ReturnsFalse()
        {
            // Arrange
            _network.UpstreamNetworkCode = null;

            // Act
            var result = _network.IsUpstreamSupplied;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetConformityTrendLastMonths_ReturnsCorrectDictionary()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C'
                },
                new SamplingEvent
                {
                    ReferenceId = "SAMP002",
                    SamplingDate = DateTime.Now.AddDays(-30),
                    BacterioConformity = 'N',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _network.GetConformityTrendLastMonths(3);

            // Assert
            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(result, Is.TypeOf<Dictionary<int, int>>());
        }

        [Test]
        public void GetConformityStatistics_ReturnsStatistics()
        {
            // Arrange
            _network.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C'
                },
                new SamplingEvent
                {
                    ReferenceId = "SAMP002",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'N',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _network.GetConformityStatistics();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalSamplings, Is.EqualTo(2));
        }
    }

    // ==================== SamplingEvent Tests ====================

    [TestFixture]
    public class SamplingEventTests
    {
        private SamplingEvent _event = null!;

        [SetUp]
        public void SetUp()
        {
            _event = new SamplingEvent
            {
                ReferenceId = "00100143925",
                NetworkCode = "001000556",
                SamplingDate = DateTime.Now
            };
        }

        [Test]
        public void Create_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var samplingEvent = new SamplingEvent
            {
                ReferenceId = "00100143925",
                NetworkCode = "001000556",
                CommuneCode = "01001",
                CommuneName = "AMBRONAY",
                SamplingDate = DateTime.Now,
                SamplingTime = new TimeSpan(12, 35, 0),
                BacterioConformity = 'C',
                ChemicalConformity = 'N'
            };

            // Assert
            Assert.That(samplingEvent.ReferenceId, Is.EqualTo("00100143925"));
            Assert.That(samplingEvent.NetworkCode, Is.EqualTo("001000556"));
            Assert.That(samplingEvent.CommuneCode, Is.EqualTo("01001"));
            Assert.That(samplingEvent.SamplingDate.Year, Is.EqualTo(DateTime.Now.Year));
            Assert.That(samplingEvent.SamplingTime, Is.EqualTo(new TimeSpan(12, 35, 0)));
            Assert.That(samplingEvent.BacterioConformity, Is.EqualTo('C'));
            Assert.That(samplingEvent.ChemicalConformity, Is.EqualTo('N'));
        }

        [Test]
        public void GetOverallConformity_WithBothConform_ReturnsConform()
        {
            // Arrange
            _event.BacterioConformity = 'C';
            _event.ChemicalConformity = 'C';
            _event.Measurements = new List<WaterQualityParameter>();

            // Act
            var result = _event.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('C'));
        }

        [Test]
        public void GetOverallConformity_WithBacterioNonConform_ReturnsNonConform()
        {
            // Arrange
            _event.BacterioConformity = 'N';
            _event.ChemicalConformity = 'C';
            _event.Measurements = new List<WaterQualityParameter>();

            // Act
            var result = _event.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('N'));
        }

        [Test]
        public void GetOverallConformity_WithChemicalNonConform_ReturnsNonConform()
        {
            // Arrange
            _event.BacterioConformity = 'C';
            _event.ChemicalConformity = 'N';
            _event.Measurements = new List<WaterQualityParameter>();

            // Act
            var result = _event.GetOverallConformity();

            // Assert
            Assert.That(result, Is.EqualTo('N'));
        }

        [Test]
        public void GetNonConformMeasurementCount_ReturnsCorrectCount()
        {
            // Arrange
            _event.Measurements = new List<WaterQualityParameter>
            {
                new WaterQualityParameter { ParameterCode = "BA001", ConformityStatus = 'C' },
                new WaterQualityParameter { ParameterCode = "BA002", ConformityStatus = 'N' },
                new WaterQualityParameter { ParameterCode = "BA003", ConformityStatus = 'N' }
            };

            // Act
            var result = _event.GetNonConformMeasurementCount();

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void DefaultValues_AreConform()
        {
            // Arrange & Act
            var samplingEvent = new SamplingEvent();

            // Assert
            Assert.That(samplingEvent.BacterioConformity, Is.EqualTo('C'));
            Assert.That(samplingEvent.ChemicalConformity, Is.EqualTo('C'));
            Assert.That(samplingEvent.ReferenceBacterioConformity, Is.EqualTo('C'));
            Assert.That(samplingEvent.ReferenceChemicalConformity, Is.EqualTo('C'));
        }
    }

    // ==================== WaterQualityAnalysis Tests ====================

    [TestFixture]
    public class WaterQualityAnalysisTests
    {
        private WaterQualityAnalysis _analysis = null!;

        [SetUp]
        public void SetUp()
        {
            _analysis = new WaterQualityAnalysis
            {
                Commune = new Commune
                {
                    INSEECode = "75056",
                    Name = "PARIS",
                    DepartmentCode = "75"
                }
            };
        }

        [Test]
        public void Create_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var analysis = new WaterQualityAnalysis
            {
                Commune = new Commune { INSEECode = "75056", Name = "PARIS" },
                AnalysisPeriodStart = DateTime.Now.AddMonths(-1),
                AnalysisPeriodEnd = DateTime.Now
            };

            // Assert
            Assert.That(analysis.Commune, Is.Not.Null);
            Assert.That(analysis.AnalysisPeriodStart, Is.Not.Null);
            Assert.That(analysis.AnalysisPeriodEnd, Is.Not.Null);
        }

        [Test]
        public void OverallConformity_WithAllConformSamplings_ReturnsConform()
        {
            // Arrange
            var network = new WaterNetwork
            {
                Code = "NET001",
                SamplingEvents = new List<SamplingEvent>
                {
                    new SamplingEvent
                    {
                        ReferenceId = "SAMP001",
                        SamplingDate = DateTime.Now,
                        BacterioConformity = 'C',
                        ChemicalConformity = 'C'
                    }
                }
            };
            _analysis.Commune.Networks = new List<WaterNetwork> { network };
            _analysis.SamplingEvents = network.SamplingEvents;
            _analysis.OverallConformity = 'C';

            // Act
            var result = _analysis.OverallConformity;

            // Assert
            Assert.That(result, Is.EqualTo('C'));
        }

        [Test]
        public void OverallConformity_WithAnyNonConformSampling_ReturnsNonConform()
        {
            // Arrange
            var network = new WaterNetwork
            {
                Code = "NET001",
                SamplingEvents = new List<SamplingEvent>
                {
                    new SamplingEvent
                    {
                        ReferenceId = "SAMP001",
                        SamplingDate = DateTime.Now,
                        BacterioConformity = 'C',
                        ChemicalConformity = 'C'
                    },
                    new SamplingEvent
                    {
                        ReferenceId = "SAMP002",
                        SamplingDate = DateTime.Now,
                        BacterioConformity = 'N',
                        ChemicalConformity = 'C'
                    }
                }
            };
            _analysis.Commune.Networks = new List<WaterNetwork> { network };
            _analysis.SamplingEvents = network.SamplingEvents;
            _analysis.OverallConformity = 'N';

            // Act
            var result = _analysis.OverallConformity;

            // Assert
            Assert.That(result, Is.EqualTo('N'));
        }

        [Test]
        public void ConformityPercentage_WithSamplings_ReturnsCorrectPercentage()
        {
            // Arrange
            _analysis.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C'
                },
                new SamplingEvent
                {
                    ReferenceId = "SAMP002",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'N',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _analysis.ConformityPercentage;

            // Assert
            Assert.That(result, Is.GreaterThanOrEqualTo(0));
            Assert.That(result, Is.LessThanOrEqualTo(100));
        }

        [Test]
        public void AllMeasurements_PopulatedCorrectly()
        {
            // Arrange
            var measurements = new List<WaterQualityParameter>
            {
                new WaterQualityParameter { ParameterCode = "BA001" },
                new WaterQualityParameter { ParameterCode = "BA002" }
            };
            _analysis.AllMeasurements = measurements;

            // Act
            var result = _analysis.AllMeasurements;

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].ParameterCode, Is.EqualTo("BA001"));
        }

        [Test]
        public void GetProblematicParameters_ReturnsNonConformParameters()
        {
            // Arrange
            var measurements = new List<WaterQualityParameter>
            {
                new WaterQualityParameter 
                { 
                    ParameterCode = "BA001",
                    MeasuredValue = "0.1",
                    NumericValue = 0.1m,
                    QualityLimit = "0",
                    Type = ParameterType.Numeric,
                    QualityType = 'N',
                    ConformityStatus = 'N'
                },
                new WaterQualityParameter 
                { 
                    ParameterCode = "BA002",
                    MeasuredValue = "0",
                    NumericValue = 0m,
                    QualityLimit = "1",
                    Type = ParameterType.Numeric,
                    QualityType = 'N',
                    ConformityStatus = 'C'
                }
            };
            var samplingEvent = new SamplingEvent
            {
                ReferenceId = "SAMP001",
                SamplingDate = DateTime.Now,
                Measurements = measurements
            };
            _analysis.SamplingEvents = new List<SamplingEvent> { samplingEvent };

            // Act
            var result = _analysis.GetProblematicParameters();

            // Assert
            Assert.That(result, Is.TypeOf<List<(string, string, int)>>());
        }

        [Test]
        public void GetConformityTrendByMonth_ReturnsMonthlyData()
        {
            // Arrange
            var now = DateTime.Now;
            _analysis.SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = "SAMP001",
                    SamplingDate = now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C'
                },
                new SamplingEvent
                {
                    ReferenceId = "SAMP002",
                    SamplingDate = now.AddMonths(-1),
                    BacterioConformity = 'N',
                    ChemicalConformity = 'C'
                }
            };

            // Act
            var result = _analysis.GetConformityTrendByMonth();

            // Assert
            Assert.That(result, Is.TypeOf<Dictionary<DateTime, int>>());
        }
    }

    // ==================== Helper Methods ====================

    private static WaterNetwork CreateConformNetwork(string code)
    {
        return new WaterNetwork
        {
            Code = code,
            Name = $"Network {code}",
            SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = $"SAMP-{code}",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'C',
                    ChemicalConformity = 'C'
                }
            }
        };
    }

    private static WaterNetwork CreateNonConformNetwork(string code)
    {
        return new WaterNetwork
        {
            Code = code,
            Name = $"Network {code}",
            SamplingEvents = new List<SamplingEvent>
            {
                new SamplingEvent
                {
                    ReferenceId = $"SAMP-{code}",
                    SamplingDate = DateTime.Now,
                    BacterioConformity = 'N',
                    ChemicalConformity = 'C'
                }
            }
        };
    }
}
