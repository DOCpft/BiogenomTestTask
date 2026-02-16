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
    /// Класс, отвечающий за загрузку файлов в GigaChat. Использует HttpClientFactory для создания клиентов и поддерживает конфигурацию через GigaChatOptions. Логирует процесс загрузки и обрабатывает возможные ошибки, выбрасывая исключения при неудаче. 
    /// </summary>
    public class GigaChatFileUploader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GigaChatOptions _options;
        private readonly ILogger<GigaChatFileUploader> _logger;

        public GigaChatFileUploader(IHttpClientFactory httpClientFactory, IOptions<GigaChatOptions> options, ILogger<GigaChatFileUploader> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Загружает файл в GigaChat и возвращает URL/id для использования в сообщениях. Выбрасывает исключение при неудаче.
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <param name="fileName"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <returns>URL/id - string</returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<string> UploadAsync(byte[] fileBytes, string fileName, string token, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient("giga");
            var filesUri = new Uri("https://gigachat.devices.sberbank.ru/api/v1/files");

            using var content = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(fileBytes);

            if (fileBytes.Length >= 3 && fileBytes[0] == 0xFF && fileBytes[1] == 0xD8)
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            else if (fileBytes.Length >= 8 && fileBytes[0] == 0x89 && fileBytes[1] == 0x50)
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            else
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var purpose = string.IsNullOrWhiteSpace(_options.FilesUploadPurpose) ? "chat_input" : _options.FilesUploadPurpose;
            content.Add(new StringContent(purpose), "purpose");
            content.Add(new StringContent(fileName), "filename");
            content.Add(byteContent, "file", fileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, filesUri) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response;
            try
            {
                _logger.LogDebug("Uploading file to {url} (purpose={purpose})", filesUri, purpose);
                response = await client.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed");
                throw;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("Upload response: {body}", body);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Upload failed {status}: {body}", (int)response.StatusCode, body);
                throw new HttpRequestException($"Upload failed: {(int)response.StatusCode} - {body}");
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("url", out var p) && p.ValueKind == JsonValueKind.String) return p.GetString()!;
                if (root.TryGetProperty("file_url", out p) && p.ValueKind == JsonValueKind.String) return p.GetString()!;
                if (root.TryGetProperty("id", out p) && p.ValueKind == JsonValueKind.String) return p.GetString()!;
                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                {
                    foreach (var name in new[] { "url", "file_url", "file", "id" })
                        if (data.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String) return v.GetString()!;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Parse upload response failed");
                throw;
            }

            throw new HttpRequestException("Unexpected upload response structure. See logs.");
        }
    }
}
