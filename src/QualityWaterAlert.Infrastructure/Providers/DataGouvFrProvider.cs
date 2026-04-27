using QualityWaterAlert.Core.Models;
using QualityWaterAlert.Core.Services;
using QualityWaterAlert.Infrastructure.Interfaces;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QualityWaterAlert.Infrastructure.Providers;

/// <summary>
/// Fetches water quality data from data.gouv.fr using targeted HTTP Range requests.
///
/// Instead of downloading the full 274 MB ZIP, we:
///   1. Fetch the ZIP's Central Directory (~20 KB) to get file offsets
///   2. Range-fetch only the entries we need (COM_UDI ~3.8 MB once, then
///      PLV + RESULT per department ~2–10 MB on first query for that dept)
///
/// All fetched data is cached in memory (communes globally, dept data per-dept, 24 h TTL).
/// </summary>
public sealed class DataGouvFrProvider : IDataProvider
{
    // Correct dataset ID: "Résultats du contrôle sanitaire de l'eau distribuée commune par commune"
    private const string DatasetApiPath = "/api/1/datasets/5cf8d9ed8b4c4110294c841d/";
    private const int CacheHours = 24;

    private readonly HttpClient _httpClient;
    private readonly IComplianceChecker _complianceChecker = new ComplianceChecker();

    // ── ZIP URL (cached separately so the API isn't re-queried on CD retries) ─
    private string? _zipUrl;
    private int _zipYear;
    private DateTime _urlCacheTime = DateTime.MinValue;
    private readonly SemaphoreSlim _urlLock = new(1, 1);

    // ── Central Directory (cached after successful range-fetch of the CD) ────
    private CdEntry[]? _cdEntries;
    private DateTime _cdCacheTime = DateTime.MinValue;
    private readonly SemaphoreSlim _cdLock = new(1, 1);

    // ── Communes ─────────────────────────────────────────────────────────────
    private Dictionary<string, Commune>? _communesByInsee;
    private List<Commune>? _communesList;
    private readonly SemaphoreSlim _communesLock = new(1, 1);

    // ── Per-department water quality data ────────────────────────────────────
    private readonly ConcurrentDictionary<string, DeptCache> _deptCache = new();
    private readonly SemaphoreSlim _deptLock = new(1, 1);

    private sealed record DeptCache(
        IReadOnlyDictionary<string, List<SamplingEvent>> SamplingsByInsee,
        DateTime LoadTime);

    // Internal representation of a ZIP central directory entry
    private sealed record CdEntry(
        string Name,
        long LocalHeaderOffset,
        long CompressedSize,
        long UncompressedSize,
        ushort CompressionMethod);

    public DataGouvFrProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // =========================================================================
    // IDataProvider
    // =========================================================================

    public async Task<List<Commune>> GetAllCommunesAsync()
    {
        await EnsureMetaLoadedAsync();
        await EnsureCommunesLoadedAsync();
        return _communesList!;
    }

    public async Task<WaterQualityAnalysis> GetWaterQualityAnalysisAsync(
        string inseeCode, DateTime? startDate, DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(inseeCode))
            throw new ArgumentException("INSEE code cannot be null or empty", nameof(inseeCode));

        await EnsureMetaLoadedAsync();
        await EnsureCommunesLoadedAsync();

        if (!_communesByInsee!.TryGetValue(inseeCode, out var commune))
            throw new InvalidOperationException(
                $"Commune with INSEE code '{inseeCode}' not found in the dataset");

        var deptCode = GetDeptFileCode(inseeCode);
        var deptData = await GetOrLoadDeptAsync(deptCode);

        var samplings = deptData.SamplingsByInsee.TryGetValue(inseeCode, out var list)
            ? list.ToList()
            : new List<SamplingEvent>();

        if (startDate.HasValue)
            samplings = samplings.Where(s => s.SamplingDate >= startDate.Value).ToList();
        if (endDate.HasValue)
            samplings = samplings.Where(s => s.SamplingDate <= endDate.Value).ToList();

        var analysis = new WaterQualityAnalysis
        {
            Commune = commune,
            AnalysisPeriodStart = startDate,
            AnalysisPeriodEnd = endDate ?? DateTime.Now,
            GeneratedDate = DateTime.Now,
            SamplingEvents = samplings,
            AllMeasurements = samplings.SelectMany(s => s.Measurements).ToList()
        };
        analysis.UpdateOverallConformity();
        return analysis;
    }

    // =========================================================================
    // ZIP metadata: URL discovery + Central Directory (two separate cache layers)
    // =========================================================================

    private bool IsUrlCached =>
        _zipUrl is not null &&
        DateTime.Now.Subtract(_urlCacheTime).TotalHours < CacheHours;

    private bool IsCdCached =>
        _cdEntries is not null &&
        DateTime.Now.Subtract(_cdCacheTime).TotalHours < CacheHours;

    private async Task EnsureMetaLoadedAsync()
    {
        await EnsureUrlCachedAsync();
        await EnsureCdLoadedAsync();
    }

    /// <summary>
    /// Discovers the latest dis-YYYY-dept.zip URL via one API call and caches it.
    /// Cached independently so a CD-loading failure doesn't force another API round-trip.
    /// </summary>
    private async Task EnsureUrlCachedAsync()
    {
        if (IsUrlCached) return;

        await _urlLock.WaitAsync();
        try
        {
            if (IsUrlCached) return;

            var (url, year) = await FindLatestDeptZipAsync();
            if (url is null)
                throw new InvalidOperationException(
                    "Cannot locate the latest dis-dept.zip on data.gouv.fr. " +
                    "Check network connectivity.");

            _zipUrl = url;
            _zipYear = year;
            _urlCacheTime = DateTime.Now;
        }
        finally
        {
            _urlLock.Release();
        }
    }

    /// <summary>
    /// Fetches the ZIP's Central Directory (2 range requests, ~20 KB).
    /// On a new ZIP (different URL), invalidates commune and dept caches.
    /// </summary>
    private async Task EnsureCdLoadedAsync()
    {
        if (IsCdCached) return;

        await _cdLock.WaitAsync();
        try
        {
            if (IsCdCached) return;

            var entries = await LoadCentralDirectoryAsync(_zipUrl!);

            // New ZIP content → invalidate dependent caches
            _communesByInsee = null;
            _communesList = null;
            _deptCache.Clear();

            _cdEntries = entries;
            _cdCacheTime = DateTime.Now;
        }
        finally
        {
            _cdLock.Release();
        }
    }

    private async Task<(string? Url, int Year)> FindLatestDeptZipAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync(DatasetApiPath);
            if (!response.IsSuccessStatusCode) return (null, 0);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("resources", out var resources))
                return (null, 0);

            // Resources are ordered newest-first; take the first dis-YYYY-dept.zip
            foreach (var resource in resources.EnumerateArray())
            {
                var title = resource.TryGetProperty("title", out var t) ? t.GetString() : null;
                var url = resource.TryGetProperty("url", out var u) ? u.GetString() : null;

                if (title is null || url is null) continue;

                var m = Regex.Match(title,
                    @"^dis-(\d{4})-dept\.zip$", RegexOptions.IgnoreCase);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int yr))
                    return (url, yr);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or IOException) { }

        return (null, 0);
    }

    // =========================================================================
    // ZIP Central Directory via HTTP Range requests
    // =========================================================================

    /// <summary>
    /// Reads the ZIP End-of-Central-Directory record (last 22+ bytes) and then
    /// range-fetches the Central Directory to build an index of all entries.
    /// This costs only 2 HTTP requests and typically ~20 KB of data.
    /// </summary>
    private async Task<CdEntry[]> LoadCentralDirectoryAsync(string url)
    {
        // ── Step 1: Fetch last 65 535 bytes to locate EOCD ──────────────────
        // The EOCD is at least 22 bytes; max comment size is 65 535 bytes.
        const int maxEocdSearch = 65_535 + 22;
        var tail = await FetchRangeSuffixAsync(url, maxEocdSearch);

        // Locate EOCD signature (0x06054b50) scanning from the end
        int eocdPos = -1;
        for (int i = tail.Length - 22; i >= 0; i--)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(i)) == 0x06054b50)
            { eocdPos = i; break; }
        }
        if (eocdPos < 0)
            throw new InvalidDataException("ZIP EOCD signature not found — unsupported archive format");

        var cdSize = (long)BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(eocdPos + 12));
        var cdOffset = (long)BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(eocdPos + 16));

        // ── Step 2: Fetch Central Directory ─────────────────────────────────
        var cd = await FetchRangeAsync(url, cdOffset, cdOffset + cdSize - 1);

        return ParseCentralDirectory(cd);
    }

    private static CdEntry[] ParseCentralDirectory(byte[] cd)
    {
        var entries = new List<CdEntry>();
        int pos = 0;

        while (pos + 46 <= cd.Length)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(cd.AsSpan(pos)) != 0x02014b50)
                break; // Not a CD entry signature

            ushort compression = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(pos + 10));
            uint compSize = BinaryPrimitives.ReadUInt32LittleEndian(cd.AsSpan(pos + 20));
            uint uncompSize = BinaryPrimitives.ReadUInt32LittleEndian(cd.AsSpan(pos + 24));
            ushort fnLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(pos + 28));
            ushort extraLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(pos + 30));
            ushort commentLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(pos + 32));
            uint localOffset = BinaryPrimitives.ReadUInt32LittleEndian(cd.AsSpan(pos + 42));

            var name = Encoding.UTF8.GetString(cd, pos + 46, fnLen);

            entries.Add(new CdEntry(name, localOffset, compSize, uncompSize, compression));

            pos += 46 + fnLen + extraLen + commentLen;
        }

        return entries.ToArray();
    }

    // =========================================================================
    // Range-fetching a single ZIP entry
    // =========================================================================

    /// <summary>
    /// Fetches and decompresses a single ZIP entry using two HTTP Range requests:
    /// one for the local file header (to determine the data start offset),
    /// one for the compressed data itself.
    /// </summary>
    private async Task<Stream> FetchZipEntryAsync(CdEntry entry)
    {
        // Local file header: 30 bytes fixed + variable filename + extra
        var lfh = await FetchRangeAsync(_zipUrl!, entry.LocalHeaderOffset,
                                        entry.LocalHeaderOffset + 29);

        ushort fnLen = BinaryPrimitives.ReadUInt16LittleEndian(lfh.AsSpan(26));
        ushort extraLen = BinaryPrimitives.ReadUInt16LittleEndian(lfh.AsSpan(28));
        long dataStart = entry.LocalHeaderOffset + 30 + fnLen + extraLen;

        var compressed = await FetchRangeAsync(_zipUrl!, dataStart,
                                               dataStart + entry.CompressedSize - 1);

        if (entry.CompressionMethod == 8) // DEFLATE
        {
            var ms = new MemoryStream((int)entry.UncompressedSize);
            using var deflate = new DeflateStream(
                new MemoryStream(compressed), CompressionMode.Decompress);
            await deflate.CopyToAsync(ms);
            ms.Position = 0;
            return ms;
        }

        // Stored (no compression)
        return new MemoryStream(compressed);
    }

    // =========================================================================
    // HTTP Range helpers
    // =========================================================================

    private async Task<byte[]> FetchRangeAsync(string url, long from, long to)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range =
            new System.Net.Http.Headers.RangeHeaderValue(from, to);

        using var response = await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead);

        // 206 Partial Content expected; 200 means server ignored Range header
        if (response.StatusCode != System.Net.HttpStatusCode.PartialContent &&
            response.StatusCode != System.Net.HttpStatusCode.OK)
            response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>Returns the last <paramref name="bytes"/> bytes of the resource.</summary>
    private async Task<byte[]> FetchRangeSuffixAsync(string url, int bytes)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range =
            new System.Net.Http.Headers.RangeHeaderValue(null, bytes);

        using var response = await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }

    // =========================================================================
    // Communes
    // =========================================================================

    private async Task EnsureCommunesLoadedAsync()
    {
        if (_communesByInsee is not null) return;

        await _communesLock.WaitAsync();
        try
        {
            if (_communesByInsee is not null) return;

            var entry = FindCdEntry(e =>
                e.Name.StartsWith("DIS_COM_UDI_", StringComparison.OrdinalIgnoreCase) &&
                e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

            if (entry is null)
                throw new InvalidOperationException(
                    "DIS_COM_UDI file not found in the dataset ZIP");

            using var stream = await FetchZipEntryAsync(entry);
            using var reader = new StreamReader(stream, Encoding.Latin1);

            var dict = new Dictionary<string, Commune>(StringComparer.Ordinal);
            ParseCommunesFromReader(reader, dict);

            _communesByInsee = dict;
            _communesList = dict.Values.ToList();
        }
        finally
        {
            _communesLock.Release();
        }
    }

    // =========================================================================
    // Per-department data
    // =========================================================================

    private async Task<DeptCache> GetOrLoadDeptAsync(string deptCode)
    {
        if (_deptCache.TryGetValue(deptCode, out var cached) &&
            DateTime.Now.Subtract(cached.LoadTime).TotalHours < CacheHours)
            return cached;

        await _deptLock.WaitAsync();
        try
        {
            if (_deptCache.TryGetValue(deptCode, out cached) &&
                DateTime.Now.Subtract(cached.LoadTime).TotalHours < CacheHours)
                return cached;

            var samplingsByInsee =
                new Dictionary<string, List<SamplingEvent>>(StringComparer.Ordinal);
            var samplingsByRef =
                new Dictionary<string, SamplingEvent>(StringComparer.Ordinal);

            // PLV ──────────────────────────────────────────────────────────────
            var plvEntry = FindCdEntry($"DIS_PLV_{_zipYear}_{deptCode}.txt");
            if (plvEntry is not null)
            {
                using var stream = await FetchZipEntryAsync(plvEntry);
                using var reader = new StreamReader(stream, Encoding.Latin1);
                ParsePlvFromReader(reader, samplingsByInsee, samplingsByRef);
            }

            // RESULT ───────────────────────────────────────────────────────────
            var resultEntry = FindCdEntry($"DIS_RESULT_{_zipYear}_{deptCode}.txt");
            if (resultEntry is not null)
            {
                using var stream = await FetchZipEntryAsync(resultEntry);
                using var reader = new StreamReader(stream, Encoding.Latin1);
                ParseResultFromReader(reader, samplingsByRef);
            }

            var data = new DeptCache(samplingsByInsee, DateTime.Now);
            _deptCache[deptCode] = data;
            return data;
        }
        finally
        {
            _deptLock.Release();
        }
    }

    private CdEntry? FindCdEntry(string exactName) =>
        _cdEntries?.FirstOrDefault(e =>
            e.Name.Equals(exactName, StringComparison.OrdinalIgnoreCase));

    private CdEntry? FindCdEntry(Func<CdEntry, bool> predicate) =>
        _cdEntries?.FirstOrDefault(predicate);

    // =========================================================================
    // CSV parsers
    // =========================================================================

    /// <summary>
    /// DIS_COM_UDI columns (0-based):
    ///   0:inseecommune  1:nomcommune  2:quartier  3:cdreseau  4:nomreseau  5:debutalim
    /// </summary>
    private static void ParseCommunesFromReader(
        TextReader reader, Dictionary<string, Commune> communeDict)
    {
        reader.ReadLine(); // skip header

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = ParseCsvLine(line);
            if (p.Length < 6) continue;

            var inseeCode = p[0];
            var deptCode = DeriveDeptCode(inseeCode);

            if (!communeDict.TryGetValue(inseeCode, out var commune))
            {
                commune = new Commune
                {
                    INSEECode = inseeCode,
                    Name = p[1],
                    DepartmentCode = deptCode
                };
                communeDict[inseeCode] = commune;
            }

            var network = new WaterNetwork
            {
                Code = p[3],
                Name = p[4],
                PrincipalCommuneCode = inseeCode,
                Neighborhood = p[2] == "-" ? null : p[2],
                DepartmentCode = deptCode
            };

            if (DateTime.TryParse(p[5], CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var supplyDate))
                network.SupplyStartDate = supplyDate;

            commune.Networks.Add(network);
        }
    }

    /// <summary>
    /// DIS_PLV columns (0-based):
    ///   0:cddept  1:cdreseau  2:inseecommuneprinc  3:nomcommuneprinc
    ///   4:cdreseauamont  5:nomreseauamont  6:pourcentdebit
    ///   7:referenceprel  8:dateprel  9:heureprel  10:conclusionprel
    ///   11:ugelib  12:distrlib  13:moalib
    ///   14:plvconformitebacterio  15:plvconformitechimique
    ///   16:plvconformitereferencebact  17:plvconformitereferencechim
    /// </summary>
    private static void ParsePlvFromReader(
        TextReader reader,
        Dictionary<string, List<SamplingEvent>> byInsee,
        Dictionary<string, SamplingEvent> byRef)
    {
        reader.ReadLine(); // skip header

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = ParseCsvLine(line);
            if (p.Length < 17) continue;

            var evt = new SamplingEvent
            {
                DepartmentCode = p[0],
                NetworkCode = p[1],
                CommuneCode = p[2],
                CommuneName = p[3],
                UpstreamNetworkCode = NullIfEmpty(p[4]),
                UpstreamNetworkName = NullIfEmpty(p[5]),
                UpstreamFlowPercentage = NullIfEmpty(p[6]),
                ReferenceId = p[7],
                ConclusionText = p[10],
                ManagementUnit = NullIfEmpty(p[11]),
                DistributionCompany = NullIfEmpty(p[12]),
                NetworkOwner = NullIfEmpty(p[13]),
                BacterioConformity = FirstCharOrDefault(p[14], 'C'),
                ChemicalConformity = FirstCharOrDefault(p[15], 'C'),
                ReferenceBacterioConformity = p.Length > 16
                    ? FirstCharOrDefault(p[16], 'C') : 'C',
                ReferenceChemicalConformity = p.Length > 17
                    ? FirstCharOrDefault(p[17], 'C') : 'C'
            };

            if (DateTime.TryParse(p[8], CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                evt.SamplingDate = date;

            // "12h35" → "12:35" → TimeSpan
            if (TimeSpan.TryParse(p[9].Replace('h', ':'), out var time))
                evt.SamplingTime = time;

            if (!byInsee.TryGetValue(evt.CommuneCode, out var list))
                byInsee[evt.CommuneCode] = list = new();
            list.Add(evt);
            byRef[evt.ReferenceId] = evt;
        }
    }

    /// <summary>
    /// DIS_RESULT columns (0-based):
    ///   0:cddept  1:referenceprel  2:cdparametresiseeaux  3:cdparametre
    ///   4:libmajparametre  5:libminparametre  6:libwebparametre
    ///   7:qualitparam  8:insituana  9:rqana
    ///   10:cdunitereferencesiseeaux  11:cdunitereference
    ///   12:limitequal  13:refqual  14:valtraduite  15:casparam  16:referenceanl
    /// </summary>
    private void ParseResultFromReader(
        TextReader reader,
        Dictionary<string, SamplingEvent> samplingsByRef)
    {
        reader.ReadLine(); // skip header

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = ParseCsvLine(line);
            if (p.Length < 15) continue;

            if (!samplingsByRef.TryGetValue(p[1], out var evt)) continue;

            var qualitParam = FirstCharOrDefault(p[7], 'N'); // 'N' numeric, 'O' qualitative
            var rawValue = p[9].Replace(',', '.');            // "0,167" → "0.167"
            var limitEqual = p[12];

            decimal? numericValue = decimal.TryParse(
                p[14], NumberStyles.Any, CultureInfo.InvariantCulture, out var dv)
                ? dv : null;

            var param = new WaterQualityParameter
            {
                Id = p.Length > 16 ? p[16] : string.Empty,
                SamplingReferenceId = p[1],
                DepartmentCode = p[0],
                ParameterCode = p[2],
                MajorName = p[4],
                MinorName = p[5],
                WebName = NullIfEmpty(p[6]),
                QualityType = qualitParam,
                AnalysisSite = FirstCharOrDefault(p[8], 'L'),
                MeasuredValue = rawValue,
                NumericValue = numericValue,
                Unit = p[10],
                QualityLimit = limitEqual,
                CASNumber = p.Length > 15 ? NullIfEmpty(p[15]) : null,
                MeasurementDate = evt.SamplingDate,
                MeasurementTime = evt.SamplingTime,
                Type = qualitParam == 'O' ? ParameterType.Qualitative : ParameterType.Numeric
            };

            // No regulatory limit → conformity is indeterminate
            if (string.IsNullOrWhiteSpace(limitEqual) || limitEqual == "X")
                param.ConformityStatus = null;
            else
            {
                var check = _complianceChecker.CheckCompliance(param);
                param.ConformityStatus = check.IsConform ? 'C' : 'N';
            }

            evt.Measurements.Add(param);
        }
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    /// <summary>
    /// 3-char dept code used in ZIP entry file names.
    /// "01007" → "001", "75056" → "075", "2A004" → "02A", "97100" → "971"
    /// </summary>
    private static string GetDeptFileCode(string inseeCode)
    {
        if (inseeCode.StartsWith("2A", StringComparison.OrdinalIgnoreCase)) return "02A";
        if (inseeCode.StartsWith("2B", StringComparison.OrdinalIgnoreCase)) return "02B";
        if (inseeCode.Length >= 3 && inseeCode.StartsWith("97"))            return inseeCode[..3];
        return inseeCode[..2].PadLeft(3, '0');
    }

    /// <summary>Dept code stored in the Commune model (no padding).</summary>
    private static string DeriveDeptCode(string inseeCode)
    {
        if (inseeCode.StartsWith("2A", StringComparison.OrdinalIgnoreCase)) return "2A";
        if (inseeCode.StartsWith("2B", StringComparison.OrdinalIgnoreCase)) return "2B";
        if (inseeCode.Length >= 3 && inseeCode.StartsWith("97"))            return inseeCode[..3];
        return inseeCode[..2];
    }

    private static string? NullIfEmpty(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;

    private static char FirstCharOrDefault(string s, char fallback) =>
        string.IsNullOrEmpty(s) ? fallback : s[0];

    /// <summary>
    /// RFC-4180-compliant CSV parser. Handles: quoted fields, empty unquoted fields,
    /// and escaped double-quotes (""). Example: "a",,"b" → ["a", "", "b"]
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    { current.Append('"'); i++; }
                    else
                        inQuotes = false;
                }
                else
                    current.Append(c);
            }
            else
            {
                if (c == '"')       inQuotes = true;
                else if (c == ',')  { fields.Add(current.ToString()); current.Clear(); }
                else                current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
