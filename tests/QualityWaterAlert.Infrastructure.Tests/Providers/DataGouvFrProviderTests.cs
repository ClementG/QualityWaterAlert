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
        public async Task GetAllCommunesAsync_ApiFailure_ReturnsEmptyList()
        {
            var provider = new DataGouvFrProvider(MakeClient(HttpStatusCode.InternalServerError));

            var result = await provider.GetAllCommunesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllCommunesAsync_CalledTwice_ReturnsCachedResult()
        {
            var handler = new CountingHttpMessageHandler(HttpStatusCode.OK, "{}");
            var provider = new DataGouvFrProvider(new HttpClient(handler));

            await provider.GetAllCommunesAsync();
            await provider.GetAllCommunesAsync();

            // Second call must hit the cache — only one real HTTP request should have been made
            Assert.That(handler.CallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllCommunesAsync_ValidResponse_ReturnsListOfCommunes()
        {
            var provider = new DataGouvFrProvider(MakeClient(HttpStatusCode.OK, "{}"));

            var result = await provider.GetAllCommunesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<QualityWaterAlert.Core.Models.Commune>>());
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
}
