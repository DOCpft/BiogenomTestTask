using Biogenom.Infrastructure.ServiceOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Biogenom.Infrastructure.Services
{
    /// <summary>
    /// Отвечает за отправку сообщений в GigaChat и получение ответов. Использует HttpClientFactory для создания клиентов и поддерживает конфигурацию через GigaChatOptions. Логирует процесс отправки и обработки ответов, а также обрабатывает возможные ошибки, выбрасывая исключения при неудаче.
    /// </summary>
    public class GigaChatChatClient
    {
        private readonly HttpClient _httpClient;
        private readonly GigaChatOptions _options;
        private readonly ILogger<GigaChatChatClient> _logger;

        public GigaChatChatClient(IHttpClientFactory httpClientFactory, IOptions<GigaChatOptions> options, ILogger<GigaChatChatClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("giga");
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Отправляет запрос в GigaChat и возвращает строку из message.content первого элемента choices. Выбрасывает исключение при неудаче.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<string> SendAsync(object body, string token, CancellationToken ct = default)
        {
            var payload = JsonSerializer.Serialize(body);
            _logger.LogDebug("Chat request payload: {payload}", payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.ApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat request failed");
                throw;
            }

            var bodyStr = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Chat returned {status}: {body}", (int)response.StatusCode, bodyStr);
                throw new HttpRequestException($"Chat failed: {(int)response.StatusCode} - {bodyStr}");
            }

            // извлекаем message.content как строку 
            try
            {
                using var doc = JsonDocument.Parse(bodyStr);
                var root = doc.RootElement;
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                        return content.GetString()!;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Parse chat response failed");
                throw;
            }

            _logger.LogWarning("Unexpected chat response structure: {body}", bodyStr);
            throw new HttpRequestException("Unexpected chat response structure. See logs.");
        }
    }
}
