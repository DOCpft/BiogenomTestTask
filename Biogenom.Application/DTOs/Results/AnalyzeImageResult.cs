using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.DTOs.Results
{
    /// <summary>
    /// Класс результата анализа изображения. Содержит идентификатор запроса и список предсказанных предметов, которые были распознаны на изображении. Этот класс используется для передачи результатов анализа изображения от обработчика команды AnalyzeImageCommand к вызывающему коду, который может использовать эту информацию для отображения пользователю или для дальнейшей обработки.
    /// </summary>
    public class AnalyzeImageResult
    {
        public int RequestId {  get; set; }
        public List<string> PredictedItems { get; set; } = [];
    }
}
