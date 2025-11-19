namespace QualityWaterAlert.Core.Services;

using QualityWaterAlert.Core.Models;

/// <summary>
/// Service for searching and retrieving communes and their water networks.
/// Supports search by INSEE code, name, postal code, and other criteria.
/// </summary>
public interface ICommuneSearcher
{
    /// <summary>
    /// Searches communes by partial name (case-insensitive).
    /// </summary>
    List<Commune> SearchByName(string nameFragment, int maxResults = 50);

    /// <summary>
    /// Finds a commune by exact INSEE code.
    /// </summary>
    Commune? FindByINSEECode(string inseeCode);

    /// <summary>
    /// Searches communes by postal code prefix.
    /// </summary>
    List<Commune> SearchByPostalCode(string postalCodePrefix, int maxResults = 50);

    /// <summary>
    /// Finds communes in a specific department.
    /// </summary>
    List<Commune> FindByDepartment(string departmentCode);

    /// <summary>
    /// Gets all communes (paginated).
    /// </summary>
    List<Commune> GetAllCommunes(int pageNumber = 1, int pageSize = 100);

    /// <summary>
    /// Searches with multiple criteria.
    /// </summary>
    List<Commune> AdvancedSearch(CommuneSearchCriteria criteria);

    /// <summary>
    /// Gets communes ordered by conformity status (best first).
    /// </summary>
    List<Commune> GetCommUnesByConformity(int limit = 10, bool conformFirst = true);

    /// <summary>
    /// Gets related communes (nearby, same department, etc.).
    /// </summary>
    List<Commune> GetRelatedCommunes(string inseeCode, int maxResults = 10);
}

/// <summary>
/// Default implementation of the commune searcher service.
/// Note: In production, this would query a database. For now, it works with in-memory collections.
/// </summary>
public class CommuneSearcher : ICommuneSearcher
{
    private readonly List<Commune> _communes;

    /// <summary>
    /// Initializes a new instance of the CommuneSearcher.
    /// </summary>
    public CommuneSearcher(List<Commune> communes)
    {
        _communes = communes ?? new List<Commune>();
    }

    /// <summary>
    /// Searches communes by partial name (case-insensitive).
    /// </summary>
    public List<Commune> SearchByName(string nameFragment, int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(nameFragment))
            return new List<Commune>();

        var searchTerm = nameFragment.ToUpper();

        return _communes
            .Where(c => c.Name.ToUpper().Contains(searchTerm))
            .OrderBy(c => c.Name)
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// Finds a commune by exact INSEE code.
    /// </summary>
    public Commune? FindByINSEECode(string inseeCode)
    {
        if (string.IsNullOrWhiteSpace(inseeCode))
            return null;

        return _communes.FirstOrDefault(c => c.INSEECode == inseeCode);
    }

    /// <summary>
    /// Searches communes by postal code prefix.
    /// INSEE codes start with the department code which roughly corresponds to postal codes.
    /// </summary>
    public List<Commune> SearchByPostalCode(string postalCodePrefix, int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(postalCodePrefix))
            return new List<Commune>();

        // Postal codes are typically 5 digits. First 2 match department code.
        var deptCode = postalCodePrefix.Substring(0, Math.Min(2, postalCodePrefix.Length));

        return FindByDepartment(deptCode).Take(maxResults).ToList();
    }

    /// <summary>
    /// Finds communes in a specific department.
    /// </summary>
    public List<Commune> FindByDepartment(string departmentCode)
    {
        if (string.IsNullOrWhiteSpace(departmentCode))
            return new List<Commune>();

        return _communes
            .Where(c => c.DepartmentCode == departmentCode)
            .OrderBy(c => c.Name)
            .ToList();
    }

    /// <summary>
    /// Gets all communes (paginated).
    /// </summary>
    public List<Commune> GetAllCommunes(int pageNumber = 1, int pageSize = 100)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 100;

        var skip = (pageNumber - 1) * pageSize;

        return _communes
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(pageSize)
            .ToList();
    }

    /// <summary>
    /// Searches with multiple criteria.
    /// </summary>
    public List<Commune> AdvancedSearch(CommuneSearchCriteria criteria)
    {
        var results = _communes.AsEnumerable();

        // Filter by name
        if (!string.IsNullOrWhiteSpace(criteria.NameFragment))
        {
            var searchTerm = criteria.NameFragment.ToUpper();
            results = results.Where(c => c.Name.ToUpper().Contains(searchTerm));
        }

        // Filter by department
        if (!string.IsNullOrWhiteSpace(criteria.DepartmentCode))
        {
            results = results.Where(c => c.DepartmentCode == criteria.DepartmentCode);
        }

        // Filter by INSEE code
        if (!string.IsNullOrWhiteSpace(criteria.INSEECode))
        {
            results = results.Where(c => c.INSEECode == criteria.INSEECode);
        }

        // Filter by conformity
        if (criteria.ConformityFilter.HasValue)
        {
            results = results.Where(c =>
            {
                var conformity = c.GetOverallConformity();
                return criteria.ConformityFilter.Value == 'C'
                    ? conformity == 'C'
                    : conformity != 'C';
            });
        }

        // Filter by minimum number of networks
        if (criteria.MinimumNetworks > 0)
        {
            results = results.Where(c => c.Networks.Count >= criteria.MinimumNetworks);
        }

        // Sort results
        results = SortResults(results, criteria.SortBy);

        return results.Take(criteria.MaxResults).ToList();
    }

    /// <summary>
    /// Gets communes ordered by conformity status (best first).
    /// </summary>
    public List<Commune> GetCommUnesByConformity(int limit = 10, bool conformFirst = true)
    {
        var results = _communes
            .OrderBy(c =>
            {
                var conformity = c.GetOverallConformity();
                return conformFirst
                    ? (conformity == 'C' ? 0 : 1)
                    : (conformity == 'C' ? 1 : 0);
            })
            .Take(limit)
            .ToList();

        return results;
    }

    /// <summary>
    /// Gets related communes (nearby, same department, etc.).
    /// </summary>
    public List<Commune> GetRelatedCommunes(string inseeCode, int maxResults = 10)
    {
        var commune = FindByINSEECode(inseeCode);
        if (commune == null)
            return new List<Commune>();

        // Get communes from the same department
        var related = FindByDepartment(commune.DepartmentCode)
            .Where(c => c.INSEECode != inseeCode)
            .OrderBy(c => c.Name)
            .Take(maxResults)
            .ToList();

        return related;
    }

    /// <summary>
    /// Sorts results based on specified criteria.
    /// </summary>
    private IEnumerable<Commune> SortResults(IEnumerable<Commune> results, CommuneSortBy sortBy)
    {
        return sortBy switch
        {
            CommuneSortBy.NameAscending => results.OrderBy(c => c.Name),
            CommuneSortBy.NameDescending => results.OrderByDescending(c => c.Name),
            CommuneSortBy.ConformityFirst => results.OrderBy(c => c.GetOverallConformity() != 'C').ThenBy(c => c.Name),
            CommuneSortBy.NonConformityFirst => results.OrderBy(c => c.GetOverallConformity() == 'C').ThenBy(c => c.Name),
            CommuneSortBy.NetworkCount => results.OrderByDescending(c => c.Networks.Count).ThenBy(c => c.Name),
            _ => results.OrderBy(c => c.Name)
        };
    }
}

/// <summary>
/// Criteria for advanced commune search.
/// </summary>
public class CommuneSearchCriteria
{
    /// <summary>
    /// Fragment of commune name to search for.
    /// </summary>
    public string? NameFragment { get; set; }

    /// <summary>
    /// Department code to filter by.
    /// </summary>
    public string? DepartmentCode { get; set; }

    /// <summary>
    /// Exact INSEE code to search for.
    /// </summary>
    public string? INSEECode { get; set; }

    /// <summary>
    /// Conformity filter: 'C' for conform only, 'N' for non-conform only, null for all.
    /// </summary>
    public char? ConformityFilter { get; set; }

    /// <summary>
    /// Minimum number of water networks.
    /// </summary>
    public int MinimumNetworks { get; set; } = 0;

    /// <summary>
    /// How to sort results.
    /// </summary>
    public CommuneSortBy SortBy { get; set; } = CommuneSortBy.NameAscending;

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int MaxResults { get; set; } = 50;
}

/// <summary>
/// Sort options for commune search results.
/// </summary>
public enum CommuneSortBy
{
    /// <summary>
    /// Sort by name A-Z.
    /// </summary>
    NameAscending,

    /// <summary>
    /// Sort by name Z-A.
    /// </summary>
    NameDescending,

    /// <summary>
    /// Conform communes first.
    /// </summary>
    ConformityFirst,

    /// <summary>
    /// Non-conform communes first.
    /// </summary>
    NonConformityFirst,

    /// <summary>
    /// Most networks first.
    /// </summary>
    NetworkCount
}
