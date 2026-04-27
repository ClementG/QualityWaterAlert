using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using QualityWaterAlert.Infrastructure.Providers;

namespace QualityWaterAlert.Infrastructure.Tests.Providers
{
    [TestFixture]
    public class DataGouvFrProviderTests
    {
        // ── Helpers ──────────────────────────────────────────────────────────

        private static HttpClient MakeClient(HttpStatusCode statusCode, string content = "{}")
        {
            var handler = new StubHttpMessageHandler(statusCode, content);
            return new HttpClient(handler);
        }

        // ── Constructor ───────────────────────────────────────────────────────

        [Test]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DataGouvFrProvider(null!));
        }

        [Test]
        public void Constructor_ValidHttpClient_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new DataGouvFrProvider(MakeClient(HttpStatusCode.OK)));
        }

        // ── GetWaterQualityAnalysisAsync ──────────────────────────────────────

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void GetWaterQualityAnalysisAsync_InvalidInseeCode_ThrowsArgumentException(string? inseeCode)
        {
            var provider = new DataGouvFrProvider(MakeClient(HttpStatusCode.OK));
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await provider.GetWaterQualityAnalysisAsync(inseeCode!, null, null));
        }

        [Test]
        public void GetWaterQualityAnalysisAsync_CommuneNotFound_ThrowsInvalidOperationException()
        {
            // API returns empty/unknown JSON → commune list will be empty
            var provider = new DataGouvFrProvider(MakeClient(HttpStatusCode.OK, "{}"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await provider.GetWaterQualityAnalysisAsync("99999", null, null));
        }

        // ── GetAllCommunesAsync ───────────────────────────────────────────────

        [Test]
        public void GetAllCommunesAsync_ApiFailure_ThrowsInvalidOperationException()
        {
            // The provider now throws rather than silently returning an empty list,
            // so the UI can display a proper error message instead of "no results found".
            var provider = new DataGouvFrProvider(
                MakeClient(HttpStatusCode.InternalServerError));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await provider.GetAllCommunesAsync());
        }

        [Test]
        public void GetAllCommunesAsync_NoMatchingResource_ThrowsInvalidOperationException()
        {
            // The dataset API returns a valid JSON object but with no dis-YYYY-dept.zip resource.
            var provider = new DataGouvFrProvider(MakeClient(HttpStatusCode.OK, "{}"));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await provider.GetAllCommunesAsync());
        }

        [Test]
        public async Task GetAllCommunesAsync_CalledTwice_DatasetApiQueriedOnlyOnce()
        {
            // The ZIP URL is discovered via one API call and then cached for 24 h.
            // Even if the subsequent ZIP range requests fail (the mock returns invalid
            // ZIP bytes), the dataset API endpoint must not be queried again.
            var handler = new UrlAwareCountingHandler(
                apiUrl: "/api/1/datasets/",
                apiResponse: """{"resources":[{"title":"dis-2025-dept.zip","url":"https://example.com/dis-2025-dept.zip"}]}""");

            var provider = new DataGouvFrProvider(
                new HttpClient(handler) { BaseAddress = new Uri("https://www.data.gouv.fr/") });

            try { await provider.GetAllCommunesAsync(); } catch { }
            try { await provider.GetAllCommunesAsync(); } catch { }

            Assert.That(handler.ApiCallCount, Is.EqualTo(1),
                "The dataset API must be queried exactly once; the URL is cached for 24 h.");
        }
    }

    // ── Test doubles ─────────────────────────────────────────────────────────

    internal sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    internal sealed class CountingHttpMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Counts requests whose path contains <paramref name="apiUrl"/> separately from
    /// all other requests (e.g., ZIP range requests to an external CDN).
    /// </summary>
    internal sealed class UrlAwareCountingHandler(string apiUrl, string apiResponse) : HttpMessageHandler
    {
        public int ApiCallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isApiCall = request.RequestUri?.PathAndQuery.Contains(
                apiUrl, StringComparison.OrdinalIgnoreCase) == true;

            if (isApiCall)
            {
                ApiCallCount++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(apiResponse, Encoding.UTF8, "application/json")
                });
            }

            // For ZIP range requests: return 206 with empty body (will fail ZIP parsing —
            // that's fine; we only care that the API endpoint is not re-queried).
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            });
        }
    }
}
