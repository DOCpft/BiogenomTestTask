using Biogenom.Application.DTOs.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Commands
{
    /// <summary>
    /// Класс команды для анализа изображения. Содержит URL изображения, который будет использоваться для скачивания и анализа. Реализует интерфейс <see cref="IRequest{AnalyzeImageResult}"/>, что позволяет использовать его с MediatR для обработки запроса и получения результата в виде <see cref="AnalyzeImageResult"/>.
    /// </summary>
    public class AnalyzeImageCommand : IRequest<AnalyzeImageResult>
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
