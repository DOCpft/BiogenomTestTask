using Biogenom.Application.Interfaces;
using Biogenom.Infrastructure.ServiceOptions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Biogenom.Infrastructure.Services
{
    /// <summary>
    /// Обеспечивает функциональность получения access_token для авторизации в GigaChat. Реализует интерфейс <see cref="IAiAuthorizationsService"/> и использует <see cref="IHttpClientFactory"/> для создания экземпляров <see cref="HttpClient"/>, что обеспечивает эффективное управление ресурсами и поддерживает масштабируемость приложения. Реализует кэширование токена с учетом его срока действия, извлекая информацию о времени жизни из ответа и обеспечивая безопасное обновление токена при необходимости. Выбрасывает исключение при неудаче получения токена или если ответ не содержит необходимой информации.
    /// </summary>
    public class GigaChatAuthorizationService : IAiAuthorizationsService
    {
        private readonly HttpClient _httpClient;
        private readonly GigaChatOptions _AiOptions;
        private string _cachedToken = string.Empty;
        private DateTime _tokenExpiry;
        public GigaChatAuthorizationService(
            IHttpClientFactory clientFactory,
            IOptions<GigaChatOptions> AiOptions)
        {
            _AiOptions = AiOptions.Value;
            _httpClient = clientFactory.CreateClient("giga");
        }

        /// <summary>
        /// Получение access_token для авторизации в GigaChat. Реализует кэширование токена с учетом его срока действия, извлекая информацию о времени жизни из ответа и обеспечивая безопасное обновление токена при необходимости. Выбрасывает исключение при неудаче получения токена или если ответ не содержит необходимой информации.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && _tokenExpiry > DateTime.UtcNow)
                return _cachedToken;

            var request = new HttpRequestMessage(HttpMethod.Post, _AiOptions.AuthUrl);
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_AiOptions.ClientId}:{_AiOptions.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Headers.Add("RqUID", Guid.NewGuid().ToString());
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("scope", _AiOptions.Scope)
            });

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            // access_token
            var accessToken = doc.RootElement.TryGetProperty("access_token", out var at) && at.ValueKind != JsonValueKind.Null
                ? at.GetString()
                : null;

            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Token response doesn't contain access_token.");

            // Попытаемся извлечь TTL (expires_in) или expires_at (unix or ttl)
            long? ttlSeconds = null;
            DateTime? absoluteExpiry = null;

            if (doc.RootElement.TryGetProperty("expires_in", out var expiresInProp))
            {
                if (expiresInProp.ValueKind == JsonValueKind.Number && expiresInProp.TryGetInt64(out var n))
                    ttlSeconds = n;
                else if (expiresInProp.ValueKind == JsonValueKind.String && long.TryParse(expiresInProp.GetString(), out var ns))
                    ttlSeconds = ns;
            }

            if (!ttlSeconds.HasValue && doc.RootElement.TryGetProperty("expires_at", out var expiresAtProp))
            {
                if (expiresAtProp.ValueKind == JsonValueKind.Number && expiresAtProp.TryGetInt64(out var n))
                {
                    // Если значение похоже на unix timestamp (больше ~1e9) — это абсолютное время
                    if (n > 1_000_000_000)
                    {
                        try
                        {
                            absoluteExpiry = DateTimeOffset.FromUnixTimeSeconds(n).UtcDateTime;
                        }
                        catch
                        {
                            // fallback: если не удалось — трактуем как ttl
                            ttlSeconds = n;
                        }
                    }
                    else
                    {
                        ttlSeconds = n;
                    }
                }
                else if (expiresAtProp.ValueKind == JsonValueKind.String)
                {
                    var s = expiresAtProp.GetString();
                    if (long.TryParse(s, out var ns))
                    {
                        if (ns > 1_000_000_000)
                        {
                            try { absoluteExpiry = DateTimeOffset.FromUnixTimeSeconds(ns).UtcDateTime; }
                            catch { ttlSeconds = ns; }
                        }
                        else
                        {
                            ttlSeconds = ns;
                        }
                    }
                    else if (DateTime.TryParse(s, out var dt))
                    {
                        absoluteExpiry = dt.ToUniversalTime();
                    }
                }
            }

            // Вычисляем _tokenExpiry безопасно
            if (absoluteExpiry.HasValue)
            {
                // safety margin
                _tokenExpiry = absoluteExpiry.Value.AddSeconds(-60);
            }
            else if (ttlSeconds.HasValue)
            {
                var seconds = ttlSeconds.Value - 60;
                if (seconds < 0) seconds = 0;
                try
                {
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(seconds);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // На случай невероятно большого значения — поставить разумный запас
                    _tokenExpiry = DateTime.UtcNow.AddHours(1);
                }
            }
            else
            {
                // Если ничего не пришло — ставим краткий запас
                _tokenExpiry = DateTime.UtcNow.AddMinutes(5);
            }

            _cachedToken = accessToken;
            return _cachedToken;
        
        }
    }
}
