using QualityWaterAlert.Core.Models;
using QualityWaterAlert.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace QualityWaterAlert.Infrastructure.Providers
{
    /// <summary>
    /// Implementation of IDataProvider that fetches water quality data from data.gouv.fr API.
    /// Provides access to French water quality control results and commune information.
    /// </summary>
    public class DataGouvFrProvider : IDataProvider
    {
        private readonly HttpClient _httpClient;
        private const string DataGouvBaseUrl = "https://data.gouv.fr/api/v2/datasets";
        private const string DatasetId = "5e165a9a3b0a6d5cb1ae1797"; // French water quality dataset ID
        private const string ResourceBaseUrl = "https://www.data.gouv.fr/fr/datasets/";

        // Cache for communes to avoid repeated API calls
        private List<Commune>? _communesCache;
        private DateTime _communesCacheTime = DateTime.MinValue;
        private const int CacheExpirationMinutes = 60;

        public DataGouvFrProvider(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Asynchronously retrieves a comprehensive water quality analysis for a specific commune.
        /// </summary>
        public async Task<WaterQualityAnalysis> GetWaterQualityAnalysisAsync(string inseeCode, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(inseeCode))
                throw new ArgumentException("INSEE code cannot be null or empty", nameof(inseeCode));

            try
            {
                // Fetch all communes and find the matching one
                var communes = await GetAllCommunesAsync();
                var commune = communes.FirstOrDefault(c => c.INSEECode == inseeCode);

                if (commune == null)
                    throw new InvalidOperationException($"Commune with INSEE code {inseeCode} not found");

                // Create the analysis object
                var analysis = new WaterQualityAnalysis
                {
                    Commune = commune,
                    AnalysisPeriodStart = startDate,
                    AnalysisPeriodEnd = endDate ?? DateTime.Now,
                    GeneratedDate = DateTime.Now
                };

                // Fetch sampling events and measurements for this commune
                await PopulateSamplingEventsAndMeasurements(analysis);

                // Calculate overall conformity
                analysis.OverallConformity = analysis.SamplingEvents.Count == 0 ? 'U' : 
                    (analysis.SamplingEvents.All(se => se.BacterioConformity == 'C' && se.ChemicalConformity == 'C') ? 'C' : 'N');

                return analysis;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to fetch water quality data for commune {inseeCode}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse water quality API response", ex);
            }
        }

        /// <summary>
        /// Asynchronously retrieves all available communes from the data source.
        /// Uses caching to avoid repeated API calls within the cache expiration period.
        /// </summary>
        public async Task<List<Commune>> GetAllCommunesAsync()
        {
            // Return cached result if still valid
            if (_communesCache != null && DateTime.Now.Subtract(_communesCacheTime).TotalMinutes < CacheExpirationMinutes)
            {
                return _communesCache;
            }

            try
            {
                _communesCache = await FetchCommunesFromDataGouvAsync();
                _communesCacheTime = DateTime.Now;
                return _communesCache ?? new List<Commune>();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to fetch communes from data.gouv.fr", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse communes API response", ex);
            }
        }

        /// <summary>
        /// Fetches commune data from data.gouv.fr API.
        /// </summary>
        private async Task<List<Commune>> FetchCommunesFromDataGouvAsync()
        {
            var communes = new Dictionary<string, Commune>();

            // Attempt to fetch from data.gouv.fr API
            // This is a simplified version that creates communes from known data
            // In production, this would parse the actual API response

            try
            {
                // Try to get the dataset information
                var url = $"{DataGouvBaseUrl}/{DatasetId}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    var jsonDoc = await JsonDocument.ParseAsync(stream);
                    var communes_list = ParseCommunesFromDataset(jsonDoc);
                    return communes_list;
                }
            }
            catch
            {
                // If API call fails, return empty list or use fallback data
            }

            // Fallback: return empty list (would be populated by actual API in production)
            return new List<Commune>();
        }

        /// <summary>
        /// Populates sampling events and measurements for a water quality analysis.
        /// </summary>
        private async Task PopulateSamplingEventsAndMeasurements(WaterQualityAnalysis analysis)
        {
            // Fetch sampling events (prélèvements) for this commune
            var samplingEvents = await FetchSamplingEventsForCommuneAsync(analysis.Commune.INSEECode, 
                analysis.AnalysisPeriodStart, 
                analysis.AnalysisPeriodEnd);

            analysis.SamplingEvents = samplingEvents;

            // Fetch measurements (résultats) for all sampling events
            var allMeasurements = new List<WaterQualityParameter>();
            foreach (var samplingEvent in samplingEvents)
            {
                var measurements = await FetchMeasurementsForSamplingEventAsync(samplingEvent.ReferenceId);
                allMeasurements.AddRange(measurements);
            }

            analysis.AllMeasurements = allMeasurements;
        }

        /// <summary>
        /// Fetches sampling events for a specific commune within a date range.
        /// </summary>
        private async Task<List<SamplingEvent>> FetchSamplingEventsForCommuneAsync(string inseeCode, DateTime? startDate, DateTime? endDate)
        {
            var samplingEvents = new List<SamplingEvent>();

            try
            {
                // In a real implementation, this would query the data.gouv.fr API
                // or a local database with the water quality data
                // For now, return empty list as data would need to be loaded separately
                
                // Example of how the API call might look:
                // var query = BuildSamplingEventQuery(inseeCode, startDate, endDate);
                // var response = await _httpClient.GetAsync(query);
                // samplingEvents = ParseSamplingEventsFromResponse(response);

                return samplingEvents;
            }
            catch
            {
                return new List<SamplingEvent>();
            }
        }

        /// <summary>
        /// Fetches measurements (results) for a specific sampling event.
        /// </summary>
        private async Task<List<WaterQualityParameter>> FetchMeasurementsForSamplingEventAsync(string referenceId)
        {
            var measurements = new List<WaterQualityParameter>();

            try
            {
                // In a real implementation, this would query the data.gouv.fr API
                // or a local database with the measurement results
                
                return measurements;
            }
            catch
            {
                return new List<WaterQualityParameter>();
            }
        }

        /// <summary>
        /// Parses communes from the dataset JSON response.
        /// </summary>
        private List<Commune> ParseCommunesFromDataset(JsonDocument doc)
        {
            var communes = new List<Commune>();

            try
            {
                var root = doc.RootElement;
                
                // Check if the response contains resources array
                if (root.TryGetProperty("resources", out var resourcesElement) && 
                    resourcesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var resource in resourcesElement.EnumerateArray())
                    {
                        // Look for commune reference files
                        if (resource.TryGetProperty("title", out var titleElement))
                        {
                            var title = titleElement.GetString();
                            if (title?.Contains("commune", StringComparison.OrdinalIgnoreCase) == true ||
                                title?.Contains("COM_UDI", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                // In production, this would fetch and parse the actual file
                                // For now, we demonstrate the structure
                            }
                        }
                    }
                }
            }
            catch
            {
                // If parsing fails, return empty list
            }

            return communes;
        }

        /// <summary>
        /// Parses a date string from various formats used by data.gouv.fr.
        /// </summary>
        private DateTime? ParseDataGouvDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // Try various date formats
            string[] formats = new[]
            {
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "yyyy-MM-dd HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            // Fallback to general parsing
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, 
                DateTimeStyles.AllowWhiteSpaces, out var generalResult))
            {
                return generalResult;
            }

            return null;
        }

        /// <summary>
        /// Parses a time string in HH:mm or HH:mm:ss format.
        /// </summary>
        private TimeSpan? ParseDataGouvTime(string? timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return null;

            if (TimeSpan.TryParseExact(timeString, new[] { "hh\\:mm", "hh\\:mm\\:ss" }, 
                CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Determines parameter type from a parameter code or value.
        /// </summary>
        private ParameterType DetermineParameterType(string parameterCode, string measuredValue)
        {
            // Parameters with numeric limits
            var numericParameters = new[] 
            { 
                "CLVYL", "BDT", "CDT", "CDT25", "PEST", "NITRATE", "NITRITE",
                "COLIF", "E.COLI", "ENTEROCOQUE", "RADIACTIVITE"
            };

            if (numericParameters.Contains(parameterCode, StringComparer.OrdinalIgnoreCase))
                return ParameterType.Numeric;

            // Check if measured value looks numeric
            if (decimal.TryParse(measuredValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return ParameterType.Numeric;

            // Default to qualitative
            return ParameterType.Qualitative;
        }
    }
}
