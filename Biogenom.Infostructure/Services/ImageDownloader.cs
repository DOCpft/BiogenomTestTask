using Biogenom.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Infrastructure.Services
{
    /// <summary>
    /// Обеспечивает функциональность скачивания изображений по URL. Реализует интерфейс <see cref="IImageDownloader"/> и использует <see cref="IHttpClientFactory"/> для создания экземпляров <see cref="HttpClient"/>, что обеспечивает эффективное управление ресурсами и поддерживает масштабируемость приложения.
    /// </summary>
    public class ImageDownloader : IImageDownloader
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageDownloader(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Скачивает изображение по URL и возвращает его в виде массива байтов. Использует HttpClientFactory для создания клиента, что позволяет эффективно управлять ресурсами и поддерживать масштабируемость приложения.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>byte[]</returns>
        public async Task<byte[]> DownloadAsync(string url)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.GetByteArrayAsync(url);
        }
    }
}
