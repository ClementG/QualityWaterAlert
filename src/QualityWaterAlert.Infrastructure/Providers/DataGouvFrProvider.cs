using QualityWaterAlert.Core.Models;
using QualityWaterAlert.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QualityWaterAlert.Infrastructure.Providers
{
    /// <summary>
    /// Fetches water quality data from data.gouv.fr.
    ///
    /// Flow for GetAllCommunesAsync():
    ///   1. GET /api/1/datasets/{id}/ → discover the download URL for DIS_COM_UDI_*.txt
    ///   2. Stream-download the CSV file from that URL
    ///   3. Parse into Commune / WaterNetwork objects
    ///   4. Cache the result for CacheExpirationMinutes
    /// </summary>
    public class DataGouvFrProvider : IDataProvider
    {
        private readonly HttpClient _httpClient;

        // Dataset: "Résultats du contrôle sanitaire de l'eau du robinet" on data.gouv.fr
        private const string DatasetApiPath = "/api/1/datasets/5e165a9a3b0a6d5cb1ae1797/";

        private List<Commune>? _communesCache;
        private DateTime _communesCacheTime = DateTime.MinValue;
        private const int CacheExpirationMinutes = 60;

        public DataGouvFrProvider(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        // -------------------------------------------------------------------------
        // IDataProvider
        // -------------------------------------------------------------------------

        public async Task<WaterQualityAnalysis> GetWaterQualityAnalysisAsync(
            string inseeCode, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(inseeCode))
                throw new ArgumentException("INSEE code cannot be null or empty", nameof(inseeCode));

            var communes = await GetAllCommunesAsync();
            var commune = communes.FirstOrDefault(c => c.INSEECode == inseeCode)
                ?? throw new InvalidOperationException($"Commune with INSEE code {inseeCode} not found");

            var analysis = new WaterQualityAnalysis
            {
                Commune = commune,
                AnalysisPeriodStart = startDate,
                AnalysisPeriodEnd = endDate ?? DateTime.Now,
                GeneratedDate = DateTime.Now
            };

            // TODO: implement DIS_PLV + DIS_RESULT streaming (same pattern as communes)
            analysis.UpdateOverallConformity();
            return analysis;
        }

        public async Task<List<Commune>> GetAllCommunesAsync()
        {
            if (_communesCache is not null &&
                DateTime.Now.Subtract(_communesCacheTime).TotalMinutes < CacheExpirationMinutes)
                return _communesCache;

            var fileUrl = await FindResourceUrlAsync("COM_UDI")
                ?? throw new InvalidOperationException(
                    "DIS_COM_UDI file not found in dataset resources on data.gouv.fr");

            _communesCache = await DownloadAndParseCommuneFileAsync(fileUrl);
            _communesCacheTime = DateTime.Now;
            return _communesCache;
        }

        // -------------------------------------------------------------------------
        // API discovery
        // -------------------------------------------------------------------------

        /// <summary>
        /// Calls the data.gouv.fr dataset API and returns the download URL of the first
        /// resource whose title contains <paramref name="fileNameFragment"/>.
        /// Returns null if the API call fails or no matching resource is found.
        /// </summary>
        private async Task<string?> FindResourceUrlAsync(string fileNameFragment)
        {
            try
            {
                using var response = await _httpClient.GetAsync(DatasetApiPath);
                if (!response.IsSuccessStatusCode) return null;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (!doc.RootElement.TryGetProperty("resources", out var resources))
                    return null;

                foreach (var resource in resources.EnumerateArray())
                {
                    var title = resource.TryGetProperty("title", out var t) ? t.GetString() : null;
                    var url   = resource.TryGetProperty("url",   out var u) ? u.GetString() : null;

                    if (title is not null && url is not null &&
                        title.Contains(fileNameFragment, StringComparison.OrdinalIgnoreCase))
                        return url;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                // Let the caller decide how to handle — return null so it throws a clear message
            }

            return null;
        }

        // -------------------------------------------------------------------------
        // CSV download & parsing
        // -------------------------------------------------------------------------

        /// <summary>
        /// Streams the CSV file at <paramref name="fileUrl"/> and parses it into communes.
        /// </summary>
        private async Task<List<Commune>> DownloadAndParseCommuneFileAsync(string fileUrl)
        {
            using var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            return await ParseCommunesFromReaderAsync(reader);
        }

        /// <summary>
        /// Parses the DIS_COM_UDI CSV format — one row per network, grouped by INSEE code.
        /// Columns: inseecommune, nomcommune, quartier, cdreseau, nomreseau, debutalim (all quoted)
        /// </summary>
        private static async Task<List<Commune>> ParseCommunesFromReaderAsync(TextReader reader)
        {
            var communeDict = new Dictionary<string, Commune>(StringComparer.Ordinal);

            await reader.ReadLineAsync(); // skip header

            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = SplitQuotedCsvLine(line);
                if (parts.Length < 6) continue;

                var inseeCode   = parts[0];
                var communeName = parts[1];
                var quartier    = parts[2];
                var networkCode = parts[3];
                var networkName = parts[4];
                var supplyStart = parts[5];

                // Department = first 3 chars for DOM-TOM (97x), else first 2
                var deptCode = inseeCode.StartsWith("97", StringComparison.Ordinal) && inseeCode.Length >= 3
                    ? inseeCode[..3]
                    : inseeCode.Length >= 2 ? inseeCode[..2] : inseeCode;

                if (!communeDict.TryGetValue(inseeCode, out var commune))
                {
                    commune = new Commune
                    {
                        INSEECode      = inseeCode,
                        Name           = communeName,
                        DepartmentCode = deptCode
                    };
                    communeDict[inseeCode] = commune;
                }

                var network = new WaterNetwork
                {
                    Code                 = networkCode,
                    Name                 = networkName,
                    PrincipalCommuneCode = inseeCode,
                    Neighborhood         = quartier == "-" ? null : quartier,
                    DepartmentCode       = deptCode
                };

                if (DateTime.TryParse(supplyStart, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date))
                    network.SupplyStartDate = date;

                commune.Networks.Add(network);
            }

            return communeDict.Values.ToList();
        }

        /// <summary>
        /// Splits a fully-quoted CSV line: "v1","v2","v3" → ["v1","v2","v3"]
        /// </summary>
        private static string[] SplitQuotedCsvLine(string line) =>
            line.Split("\",\"").Select(p => p.Trim('"')).ToArray();
    }
}
