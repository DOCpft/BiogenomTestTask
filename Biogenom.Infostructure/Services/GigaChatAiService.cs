using Biogenom.Application.DTOs;
using Biogenom.Application.Interfaces;
using Biogenom.Infrastructure.ServiceOptions;
using Biogenom.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GigaChatImageAnalyzer.Services
{

    /// <summary>
    /// Предназначен для взаимодействия с GigaChat, обеспечивая функциональность загрузки изображений, отправки запросов и получения ответов. Реализует интерфейс <see cref="IAiService"/> и использует <see cref="GigaChatFileUploader"/> для загрузки файлов, <see cref="GigaChatChatClient"/> для отправки сообщений и получения ответов, а также <see cref="IAiAuthorizationsService"/> для получения токенов доступа. Включает кэширование загруженных файлов на основе их хеша для оптимизации производительности и уменьшения количества загрузок. Логирует ключевые этапы процесса и обрабатывает возможные ошибки, обеспечивая надежность и удобство использования сервиса.
    /// </summary>
    public class GigaChatService : IAiService
    {
        

        // Кэш: хеш изображения -> fileRef (id/URL) от GigaChat
        private readonly ConcurrentDictionary<string, string> _uploadedFilesCache = new();
        private readonly GigaChatFileUploader _uploader;
        private readonly GigaChatChatClient _chatClient;
        private readonly IAiAuthorizationsService _authorizationsService;
        private readonly GigaChatPromptsOptions _prompts;
        private readonly GigaChatOptions _options;
        private readonly ILogger<GigaChatService> _logger;

        public GigaChatService(
            GigaChatFileUploader uploader,
            GigaChatChatClient chatClient,
            IAiAuthorizationsService auth,
            IOptions<GigaChatPromptsOptions> prompts,
            ILogger<GigaChatService> logger,
            IOptions<GigaChatOptions> options)
        {
            _uploader = uploader;
            _chatClient = chatClient;
            _authorizationsService = auth;
            _prompts = prompts.Value;
            _logger = logger;
            _options = options.Value;
        }


        // Вычисление SHA256 хеша байтов (в hex) — ключ в кэше
        private static string ComputeImageHash(byte[] bytes)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        // Обёртка: если в кэше есть fileRef для изображения — вернуть его, иначе загрузить и кэшировать
        private async Task<string> GetOrUploadFileAsync(byte[] imageBytes, string fileName, string token)
        {
            var key = ComputeImageHash(imageBytes);

            if (_uploadedFilesCache.TryGetValue(key, out var existingRef))
            {
                _logger.LogDebug("Using cached fileRef for image hash {hash}", key);
                return existingRef;
            }

            var fileRef = await _uploader.UploadAsync(imageBytes, fileName, token);
            if (!string.IsNullOrEmpty(fileRef))
            {
                // сохраняем в кэш
                _uploadedFilesCache.TryAdd(key, fileRef);
                _logger.LogDebug("Cached uploaded fileRef for image hash {hash}: {fileRef}", key, fileRef);
            }

            return fileRef;
        }

        public async Task<List<string>> PredictMainObjectsAsync(byte[] imageBytes)
        {
            var token = await _authorizationsService.GetAccessTokenAsync();

            var fileRef = await EnsureUploadedAsync(imageBytes, null);

            var prompt = _prompts.PredictMainObjects;
            var body = BuildChatBody(prompt, new[] { fileRef });

            var raw = await _chatClient.SendAsync(body, token);
            return ParseJsonArray(raw);
        }

        private object BuildChatBody(string prompt, string[] attachments)
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[] {
                    new
                    {
                        role = "user",
                        content = prompt,
                        attachments = attachments
                    }
                },
                temperature = 0.1,
                max_tokens = 300
            };
            return requestBody;
        }
        public async Task<string> EnsureUploadedAsync(byte[] imageBytes, string? existingFileRef = null)
        {
            if (!string.IsNullOrWhiteSpace(existingFileRef))
                return existingFileRef;

            var token = await _authorizationsService.GetAccessTokenAsync();
            var fileRef = await GetOrUploadFileAsync(imageBytes, "image.jpg", token);
            return fileRef;
        }
        public async Task<Dictionary<string, List<string>>> PredictMaterialsAsync(byte[] imageBytes, List<string> items, string? existingFileRef = null)
        {
            var token = await _authorizationsService.GetAccessTokenAsync();

            string fileRef = await EnsureUploadedAsync(imageBytes, existingFileRef);

            var prompt = _prompts.PredictMaterialsTemplate.Replace("{items}", string.Join(", ", items));
            var body = BuildChatBody(prompt, new[] { fileRef });

            var raw = await _chatClient.SendAsync(body, token);
            return ParseMaterialsResponse(raw);
        }

        private List<string> ParseJsonArray(string jsonContent)
        {
            // GigaChat sometimes wraps JSON in markdown ```json ... ```
            jsonContent = CleanResponse(jsonContent);
            try
            {
                return JsonSerializer.Deserialize<List<string>>(jsonContent) ?? new List<string>();
            }
            catch
            {
                _logger.LogWarning("Failed to parse AI response as array: {response}", jsonContent);
                return new List<string>();
            }
        }

        private Dictionary<string, List<string>> ParseMaterialsResponse(string jsonContent)
        {
            jsonContent = CleanResponse(jsonContent);
            try
            {
                var list = JsonSerializer.Deserialize<List<ItemMaterialDto>>(jsonContent);
                return list?.ToDictionary(x => x.ItemName, x => x.Materials) ?? new();
            }
            catch
            {
                _logger.LogWarning("Failed to parse materials response: {response}", jsonContent);
                return new();
            }
        }

        private string CleanResponse(string input)
        {
            input = input.Trim();
            if (input.StartsWith("```json"))
                input = input.Substring(7);
            if (input.StartsWith("```"))
                input = input.Substring(3);
            if (input.EndsWith("```"))
                input = input.Substring(0, input.Length - 3);
            return input.Trim();
        }
    }
}