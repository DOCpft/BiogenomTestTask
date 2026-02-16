using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.DTOs.Requests
{
    /// <summary>
    /// DTO для передачи данных запроса на анализ изображения. Содержит URL изображения, который будет использоваться для скачивания и анализа. Этот класс служит для структурирования данных, которые пользователь отправляет при запросе на анализ изображения, и позволяет передавать эту информацию между слоями приложения, например, от контроллера к обработчику команды. Он обеспечивает удобный способ передачи URL изображения, который будет использоваться для дальнейшего анализа в приложении Biogenom.
    /// </summary>
    public class AnalyzeItemsRequest
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
