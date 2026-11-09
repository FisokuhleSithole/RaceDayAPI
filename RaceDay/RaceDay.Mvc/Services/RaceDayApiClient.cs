using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Services
{
    /// <summary>
    /// Wraps HttpClient to call the Part 2 API.
    /// Automatically forwards the API session cookie stored in MVC session.
    /// </summary>
    public class RaceDayApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _factory = factory;
            _httpContextAccessor = accessor;
        }

        private HttpClient CreateClient() => _factory.CreateClient("RaceDayApi");

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            // Read the API session cookie we saved during login
            var apiCookie = _httpContextAccessor.HttpContext?
                .Session.GetString("ApiSessionCookie");

            // If we have the cookie, attach it to the request
            if (!string.IsNullOrWhiteSpace(apiCookie))
                request.Headers.TryAddWithoutValidation("Cookie", apiCookie);

            return request;
        }

        public async Task<HttpResponseMessage> SendAsync(
            HttpMethod method, string url, object? body = null)
        {
            var client = CreateClient();
            using var request = CreateRequest(method, url);

            if (body != null)
                request.Content = JsonContent.Create(body);

            return await client.SendAsync(request);
        }

        public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return default;

            string json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return default;

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        /// <summary>
        /// Extracts the API session cookie from the Set-Cookie header.
        /// This is what allows the API to identify the user on subsequent requests.
        /// </summary>
        public static string? ExtractSessionCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var values))
                return null;

            var sessionHeader = values.FirstOrDefault(v =>
                v.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase));

            return sessionHeader?.Split(';', 2)[0];
        }
    }
}