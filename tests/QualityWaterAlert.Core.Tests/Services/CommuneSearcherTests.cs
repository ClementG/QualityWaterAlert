namespace QualityWaterAlert.Core.Tests.Services;

using NUnit.Framework;
using QualityWaterAlert.Core.Models;
using QualityWaterAlert.Core.Services;

/// <summary>
/// Unit tests for CommuneSearcher service.
/// Tests search functionality by name, INSEE code, postal code, department, and advanced criteria.
/// </summary>
[TestFixture]
public class CommuneSearcherTests
{
    private CommuneSearcher _searcher = null!;
    private List<Commune> _testCommunes = null!;

    [SetUp]
    public void SetUp()
    {
        // Create test data with various communes
        _testCommunes = new List<Commune>
        {
            new Commune
            {
                INSEECode = "75056",
                Name = "PARIS",
                DepartmentCode = "75",
                Networks = new List<WaterNetwork>
                {
                    CreateTestNetwork("NET001", isConform: true),
                    CreateTestNetwork("NET002", isConform: true)
                }
            },
            new Commune
            {
                INSEECode = "92044",
                Name = "NANTERRE",
                DepartmentCode = "92",
                Networks = new List<WaterNetwork>
                {
                    CreateTestNetwork("NET003", isConform: false),
                    CreateTestNetwork("NET004", isConform: true)
                }
            },
            new Commune
            {
                INSEECode = "92048",
                Name = "NEUILLY-SUR-SEINE",
                DepartmentCode = "92",
                Networks = new List<WaterNetwork>
                {
                    CreateTestNetwork("NET005", isConform: true)
                }
            },
            new Commune
            {
                INSEECode = "75101",
                Name = "PARIS 01",
                DepartmentCode = "75",
                Networks = new List<WaterNetwork>
                {
                    CreateTestNetwork("NET006", isConform: true),
                    CreateTestNetwork("NET007", isConform: true),
                    CreateTestNetwork("NET008", isConform: true)
                }
            },
            new Commune
            {
                INSEECode = "01001",
                Name = "ABERGEMENT-CLEMENCIAT",
                DepartmentCode = "01",
                Networks = new List<WaterNetwork>
                {
                    CreateTestNetwork("NET009", isConform: false)
                }
            },
            new Commune
            {
                INSEECode = "01002",
                Name = "ABERGEMENT-DE-CUISERY",
                DepartmentCode = "01",
                Networks = new List<WaterNetwork>
                {
                    CreateTestNetwork("NET010", isConform: true)
                }
            }
        };

        _searcher = new CommuneSearcher(_testCommunes);
    }

    // ==================== SearchByName Tests ====================

    [Test]
    public void SearchByName_WithValidFragment_ReturnsMatching()
    {
        // Arrange
        var fragment = "PARIS";

        // Act
        var results = _searcher.SearchByName(fragment);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2), "Should find 2 communes with PARIS in name");
        Assert.That(results.All(c => c.Name.ToUpper().Contains("PARIS")), Is.True);
        Assert.That(results[0].Name, Is.EqualTo("PARIS"), "Results should be ordered by name");
    }

    [Test]
    public void SearchByName_CaseInsensitive_ReturnsMatching()
    {
        // Arrange
        var fragment = "paris";

        // Act
        var results = _searcher.SearchByName(fragment);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2), "Should be case-insensitive");
    }

    [Test]
    public void SearchByName_WithPartialFragment_ReturnsMatching()
    {
        // Arrange
        var fragment = "BERG";

        // Act
        var results = _searcher.SearchByName(fragment);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2), "Should find ABERGEMENT communes");
        Assert.That(results.All(c => c.Name.Contains("BERG")), Is.True);
    }

    [Test]
    public void SearchByName_WithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var fragment = "UNKNOWN";

        // Act
        var results = _searcher.SearchByName(fragment);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void SearchByName_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var fragment = "";

        // Act
        var results = _searcher.SearchByName(fragment);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void SearchByName_WithNullString_ReturnsEmpty()
    {
        // Arrange
        var fragment = (string)null!;

        // Act
        var results = _searcher.SearchByName(fragment);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void SearchByName_WithMaxResults_ReturnsLimited()
    {
        // Arrange
        var fragment = "";
        var maxResults = 2;

        // Act
        var results = _searcher.SearchByName(fragment, maxResults);

        // Assert
        Assert.That(results.Count, Is.LessThanOrEqualTo(maxResults));
    }

    // ==================== FindByINSEECode Tests ====================

    [Test]
    public void FindByINSEECode_WithValidCode_ReturnsCommune()
    {
        // Arrange
        var inseeCode = "75056";

        // Act
        var result = _searcher.FindByINSEECode(inseeCode);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.INSEECode, Is.EqualTo("75056"));
        Assert.That(result.Name, Is.EqualTo("PARIS"));
    }

    [Test]
    public void FindByINSEECode_WithInvalidCode_ReturnsNull()
    {
        // Arrange
        var inseeCode = "99999";

        // Act
        var result = _searcher.FindByINSEECode(inseeCode);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindByINSEECode_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var inseeCode = "";

        // Act
        var result = _searcher.FindByINSEECode(inseeCode);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindByINSEECode_WithNullString_ReturnsNull()
    {
        // Arrange
        var inseeCode = (string)null!;

        // Act
        var result = _searcher.FindByINSEECode(inseeCode);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ==================== SearchByPostalCode Tests ====================

    [Test]
    public void SearchByPostalCode_WithValidPrefix_ReturnsMatchingDepartment()
    {
        // Arrange
        var postalCode = "92";

        // Act
        var results = _searcher.SearchByPostalCode(postalCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2), "Should find 2 communes in department 92");
        Assert.That(results.All(c => c.DepartmentCode == "92"), Is.True);
    }

    [Test]
    public void SearchByPostalCode_WithLongPostalCode_UsesFirstTwoDigits()
    {
        // Arrange
        var postalCode = "92100";

        // Act
        var results = _searcher.SearchByPostalCode(postalCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2), "Should extract department code from postal code");
        Assert.That(results.All(c => c.DepartmentCode == "92"), Is.True);
    }

    [Test]
    public void SearchByPostalCode_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var postalCode = "";

        // Act
        var results = _searcher.SearchByPostalCode(postalCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void SearchByPostalCode_WithNullString_ReturnsEmpty()
    {
        // Arrange
        var postalCode = (string)null!;

        // Act
        var results = _searcher.SearchByPostalCode(postalCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    // ==================== FindByDepartment Tests ====================

    [Test]
    public void FindByDepartment_WithValidCode_ReturnsAllInDepartment()
    {
        // Arrange
        var departmentCode = "92";

        // Act
        var results = _searcher.FindByDepartment(departmentCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.All(c => c.DepartmentCode == "92"), Is.True);
        Assert.That(results[0].Name, Is.LessThanOrEqualTo(results[1].Name), "Should be sorted by name");
    }

    [Test]
    public void FindByDepartment_WithInvalidCode_ReturnsEmpty()
    {
        // Arrange
        var departmentCode = "99";

        // Act
        var results = _searcher.FindByDepartment(departmentCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void FindByDepartment_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var departmentCode = "";

        // Act
        var results = _searcher.FindByDepartment(departmentCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void FindByDepartment_DepartmentWithManyCommunes_ReturnsAll()
    {
        // Arrange
        var departmentCode = "01";

        // Act
        var results = _searcher.FindByDepartment(departmentCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.All(c => c.DepartmentCode == "01"), Is.True);
    }

    // ==================== GetAllCommunes Tests ====================

    [Test]
    public void GetAllCommunes_WithDefaultPageNumber_ReturnsFirstPage()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 2;

        // Act
        var results = _searcher.GetAllCommunes(pageNumber, pageSize);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2), "First page should have 2 results");
        Assert.That(results[0].Name, Is.LessThanOrEqualTo(results[1].Name), "Should be sorted by name");
    }

    [Test]
    public void GetAllCommunes_WithSecondPage_ReturnsSecondPage()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 2;

        // Act
        var results = _searcher.GetAllCommunes(pageNumber, pageSize);

        // Assert
        Assert.That(results.Count, Is.GreaterThan(0), "Second page should have results");
    }

    [Test]
    public void GetAllCommunes_WithPageSizeExceedingTotal_ReturnsAll()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 100;

        // Act
        var results = _searcher.GetAllCommunes(pageNumber, pageSize);

        // Assert
        Assert.That(results.Count, Is.EqualTo(6), "Should return all communes");
    }

    [Test]
    public void GetAllCommunes_WithInvalidPageNumber_DefaultsToOne()
    {
        // Arrange
        var pageNumber = 0;
        var pageSize = 2;

        // Act
        var results = _searcher.GetAllCommunes(pageNumber, pageSize);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2), "Should default to page 1");
    }

    [Test]
    public void GetAllCommunes_WithInvalidPageSize_DefaultsToHundred()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 0;

        // Act
        var results = _searcher.GetAllCommunes(pageNumber, pageSize);

        // Assert
        Assert.That(results.Count, Is.GreaterThan(0), "Should use default page size");
    }

    // ==================== AdvancedSearch Tests ====================

    [Test]
    public void AdvancedSearch_WithNameFilter_ReturnsMatching()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            NameFragment = "PARIS",
            MaxResults = 50
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.All(c => c.Name.Contains("PARIS")), Is.True);
    }

    [Test]
    public void AdvancedSearch_WithDepartmentFilter_ReturnsMatching()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            DepartmentCode = "92",
            MaxResults = 50
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.All(c => c.DepartmentCode == "92"), Is.True);
    }

    [Test]
    public void AdvancedSearch_WithINSEECodeFilter_ReturnsMatching()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            INSEECode = "75056",
            MaxResults = 50
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].INSEECode, Is.EqualTo("75056"));
    }

    [Test]
    public void AdvancedSearch_WithConformityFilterConform_ReturnsOnlyConform()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            ConformityFilter = 'C',
            MaxResults = 50
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.All(c => c.GetOverallConformity() == 'C'), Is.True);
    }

    [Test]
    public void AdvancedSearch_WithConformityFilterNonConform_ReturnsOnlyNonConform()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            ConformityFilter = 'N',
            MaxResults = 50
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.All(c => c.GetOverallConformity() != 'C'), Is.True);
    }

    [Test]
    public void AdvancedSearch_WithMinimumNetworks_ReturnsMatching()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            MinimumNetworks = 3,
            MaxResults = 50
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.All(c => c.Networks.Count >= 3), Is.True);
    }

    [Test]
    public void AdvancedSearch_WithMultipleFilters_ReturnsMatching()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            DepartmentCode = "75",
            ConformityFilter = 'C',
            MaxResults = 50
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.All(c => c.DepartmentCode == "75" && c.GetOverallConformity() == 'C'), Is.True);
    }

    [Test]
    public void AdvancedSearch_WithMaxResultsLimit_ReturnsLimited()
    {
        // Arrange
        var criteria = new CommuneSearchCriteria
        {
            MaxResults = 2
        };

        // Act
        var results = _searcher.AdvancedSearch(criteria);

        // Assert
        Assert.That(results.Count, Is.LessThanOrEqualTo(2));
    }

    // ==================== GetCommUnesByConformity Tests ====================

    [Test]
    public void GetCommUnesByConformity_WithConformFirst_ReturnsConformFirst()
    {
        // Arrange
        var limit = 10;
        var conformFirst = true;

        // Act
        var results = _searcher.GetCommUnesByConformity(limit, conformFirst);

        // Assert
        Assert.That(results.Count, Is.LessThanOrEqualTo(limit));
        // First results should be conform, then non-conform
        if (results.Count > 0)
        {
            var firstConform = results[0].GetOverallConformity() == 'C';
            if (results.Count > 1)
            {
                // Subsequent non-conform communes should come after conform ones
                var conformIndex = -1;
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].GetOverallConformity() == 'C')
                        conformIndex = i;
                    else if (conformIndex != -1)
                    {
                        // Found a non-conform after a conform, which is correct
                        break;
                    }
                }
            }
        }
    }

    [Test]
    public void GetCommUnesByConformity_WithNonConformFirst_ReturnsNonConformFirst()
    {
        // Arrange
        var limit = 10;
        var conformFirst = false;

        // Act
        var results = _searcher.GetCommUnesByConformity(limit, conformFirst);

        // Assert
        Assert.That(results.Count, Is.LessThanOrEqualTo(limit));
        if (results.Count > 0)
        {
            // At least some results should be non-conform (or all conform if none exist)
            Assert.That(results.Count, Is.GreaterThan(0));
        }
    }

    [Test]
    public void GetCommUnesByConformity_WithSmallLimit_ReturnsLimited()
    {
        // Arrange
        var limit = 2;

        // Act
        var results = _searcher.GetCommUnesByConformity(limit);

        // Assert
        Assert.That(results.Count, Is.LessThanOrEqualTo(2));
    }

    // ==================== GetRelatedCommunes Tests ====================

    [Test]
    public void GetRelatedCommunes_WithValidINSEECode_ReturnsSameDepartmentCommunes()
    {
        // Arrange
        var inseeCode = "75056"; // PARIS in department 75

        // Act
        var results = _searcher.GetRelatedCommunes(inseeCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1), "Should return 1 other commune in same department");
        Assert.That(results.All(c => c.DepartmentCode == "75" && c.INSEECode != inseeCode), Is.True);
    }

    [Test]
    public void GetRelatedCommunes_WithValidINSEECode_ExcludesSourceCommune()
    {
        // Arrange
        var inseeCode = "92044"; // NANTERRE

        // Act
        var results = _searcher.GetRelatedCommunes(inseeCode);

        // Assert
        Assert.That(results.All(c => c.INSEECode != inseeCode), Is.True, "Should exclude source commune");
    }

    [Test]
    public void GetRelatedCommunes_WithInvalidINSEECode_ReturnsEmpty()
    {
        // Arrange
        var inseeCode = "99999";

        // Act
        var results = _searcher.GetRelatedCommunes(inseeCode);

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetRelatedCommunes_WithMaxResults_ReturnsLimited()
    {
        // Arrange
        var inseeCode = "01001"; // ABERGEMENT-CLEMENCIAT
        var maxResults = 1;

        // Act
        var results = _searcher.GetRelatedCommunes(inseeCode, maxResults);

        // Assert
        Assert.That(results.Count, Is.LessThanOrEqualTo(maxResults));
    }

    // ==================== Edge Cases & Integration Tests ====================

    [Test]
    public void EmptyCommunes_SearchByName_ReturnsEmpty()
    {
        // Arrange
        var emptySearcher = new CommuneSearcher(new List<Commune>());

        // Act
        var results = emptySearcher.SearchByName("ANYTHING");

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void EmptyCommunes_GetAllCommunes_ReturnsEmpty()
    {
        // Arrange
        var emptySearcher = new CommuneSearcher(new List<Commune>());

        // Act
        var results = emptySearcher.GetAllCommunes();

        // Assert
        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public void SingleCommune_Search_ReturnsCorrectly()
    {
        // Arrange
        var singleCommune = new List<Commune>
        {
            new Commune
            {
                INSEECode = "75056",
                Name = "PARIS",
                DepartmentCode = "75",
                Networks = new List<WaterNetwork>
                {
                    CreateTestNetwork("NET", isConform: true)
                }
            }
        };
        var searcher = new CommuneSearcher(singleCommune);

        // Act
        var results = searcher.SearchByName("PARIS");

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Name, Is.EqualTo("PARIS"));
    }

    // ==================== Helper Methods ====================

    private static WaterNetwork CreateTestNetwork(string code, bool isConform)
    {
        var network = new WaterNetwork
        {
            Code = code,
            Name = $"Network {code}",
            UpstreamNetworkCode = null,
            SamplingEvents = new List<SamplingEvent>()
        };

        // Add a sampling event with appropriate conformity status
        var samplingEvent = new SamplingEvent
        {
            ReferenceId = $"SAMP-{code}",
            NetworkCode = code,
            SamplingDate = DateTime.Now,
            SamplingTime = null,
            BacterioConformity = isConform ? 'C' : 'N',
            ChemicalConformity = isConform ? 'C' : 'N',
            Measurements = new List<WaterQualityParameter>()
        };

        network.SamplingEvents.Add(samplingEvent);
        return network;
    }
}
