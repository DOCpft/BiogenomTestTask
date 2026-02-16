using Biogenom.Application.Commands;
using Biogenom.Application.DTOs;
using Biogenom.Application.DTOs.Results;
using Biogenom.Application.Interfaces;
using Biogenom.Domain.Entities;
using Biogenom.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Handlers
{
    /// <summary>
    /// Класс обработчика команды ConfirmItemsCommand. Реализует интерфейс <see cref="IRequestHandler{ConfirmItemsCommand, ConfirmItemsResult}"/>, что позволяет использовать его с MediatR для обработки запроса на подтверждение предметов, предсказанных на изображении, и получения результата в виде <see cref="ConfirmItemsResult"/>. В процессе обработки команды, этот класс загружает запрос на анализ по идентификатору, скачивает изображение, обеспечивает загрузку изображения в AI-сервис (если это не было сделано ранее), вызывает метод предсказания материалов для подтвержденных предметов, сохраняет результаты в базе данных и возвращает результат пользователю. Этот обработчик обеспечивает основную бизнес-логику для подтверждения предметов и получения связанных с ними материалов в приложении Biogenom.
    /// </summary>
    public class ConfirmItemsCommandHandler : IRequestHandler<ConfirmItemsCommand, ConfirmItemsResult>
    {
        private readonly IImageDownloader _imageDownloader;
        private readonly IAiService _aiService;
        private readonly IAnalysisRequestRepository _analysisRequestRepository;
        private readonly IMaterialRepository _materialRepository;

        public ConfirmItemsCommandHandler(
            IImageDownloader downloader,
            IAiService aiService,
            IAnalysisRequestRepository requestRepository,
            IMaterialRepository materialRepository)
        {
            _imageDownloader = downloader;
            _aiService = aiService;
            _analysisRequestRepository = requestRepository;
            _materialRepository = materialRepository;
        }
        public async Task<ConfirmItemsResult> Handle(ConfirmItemsCommand request, CancellationToken cancellationToken)
        {
            var analysisRequest = await _analysisRequestRepository.GetByIdAsync(request.RequestId);
            if (analysisRequest == null)
                throw new InvalidOperationException($"Analysis request with ID {request.RequestId} not found.");

            var imageBytes = await _imageDownloader.DownloadAsync(analysisRequest.ImageUrl);
            string fileRef = analysisRequest.UploadedFileRef!;
            if (string.IsNullOrWhiteSpace(fileRef))
            {
                // загрузим и получим fileRef, сервис вернёт либо новый либо existing
                fileRef = await _aiService.EnsureUploadedAsync(imageBytes, null);
                analysisRequest.UploadedFileRef = fileRef;
                await _analysisRequestRepository.SaveChangesAsync(); // сохранить ref в БД
            }

            // затем вызываем PredictMaterialsAsync с существующим ref — сервис не станет загружать заново
            var materialsPerItem = await _aiService.PredictMaterialsAsync(imageBytes, request.Items, fileRef);

            var resultItems = new List<ItemMaterialDto>();
            foreach (var itemName in request.Items)
            {
                var item = new Item
                {
                    Name = itemName,
                    AnalysisRequestId = analysisRequest.Id,
                    ItemMaterials = new List<ItemMaterial>()
                };

                if(materialsPerItem.TryGetValue(itemName, out var materialNames))
                {
                    foreach(var materialName in materialNames.Distinct())
                    {
                        var material = await _materialRepository.GetOrCreateAsync(materialName);
                        item.ItemMaterials.Add(new ItemMaterial { MaterialId = material.Id });
                    }
                }

                analysisRequest.Items.Add(item);

                resultItems.Add(new ItemMaterialDto
                {
                    ItemName = itemName,
                    Materials = materialNames ?? new List<string>()
                });

            }

            await _analysisRequestRepository.SaveChangesAsync();

            return new ConfirmItemsResult
            {
                Items = resultItems
            };
        }
    }
}
