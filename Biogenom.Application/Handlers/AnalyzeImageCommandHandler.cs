using Biogenom.Application.Commands;
using Biogenom.Application.DTOs.Results;
using Biogenom.Application.Interfaces;
using Biogenom.Domain.Entities;
using Biogenom.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Biogenom.Application.Handlers
{
    /// <summary>
    /// Класс обработчика команды AnalyzeImageCommand. Реализует интерфейс <see cref="IRequestHandler{AnalyzeImageCommand, AnalyzeImageResult}"/>, что позволяет использовать его с MediatR для обработки запроса на анализ изображения и получения результата в виде <see cref="AnalyzeImageResult"/>. В процессе обработки команды, этот класс скачивает изображение по указанному URL, передает его в AI-сервис для предсказания основных объектов, сохраняет результат анализа в базе данных и возвращает результат пользователю. Этот обработчик обеспечивает основную бизнес-логику для анализа изображений в приложении Biogenom.
    /// </summary>
    public class AnalyzeImageCommandHandler : IRequestHandler<AnalyzeImageCommand, AnalyzeImageResult>
    {
        private readonly IImageDownloader _imageDownloader;
        private readonly IAiService _aiService;
        private readonly IAnalysisRequestRepository _analysisRequestRepository;

        public AnalyzeImageCommandHandler(
            IImageDownloader imageDownloader,
            IAiService aiService,
            IAnalysisRequestRepository requestRepository)
        {
            _imageDownloader = imageDownloader;
            _aiService = aiService;
            _analysisRequestRepository = requestRepository;
        }
        public async Task<AnalyzeImageResult> Handle(AnalyzeImageCommand request, CancellationToken cancellationToken)
        {
            var imageBytes = await _imageDownloader.DownloadAsync(request.ImageUrl);
            var predictedItems = await _aiService.PredictMainObjectsAsync(imageBytes);

            var analysisRequest = new AnalysisRequest
            {
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                RawAiResponse = JsonSerializer.Serialize(predictedItems)
            };

            await _analysisRequestRepository.AddAsync(analysisRequest);
            await _analysisRequestRepository.SaveChangesAsync();

            return new AnalyzeImageResult
            {
                RequestId = analysisRequest.Id,
                PredictedItems = predictedItems
            };
        }
    }
}
