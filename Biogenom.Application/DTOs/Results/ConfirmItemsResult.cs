using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.DTOs.Results
{
    /// <summary>
    /// Класс результата подтверждения предметов. Содержит список объектов типа <see cref="ItemMaterialDto"/>, которые были подтверждены пользователем после анализа изображения. Этот класс используется для передачи информации о подтвержденных предметах от обработчика команды ConfirmItemsCommand к вызывающему коду, который может использовать эту информацию для отображения пользователю или для дальнейшей обработки.
    /// </summary>
    public class ConfirmItemsResult
    {
        public List<ItemMaterialDto> Items { get; set; } = [];
    }
}
