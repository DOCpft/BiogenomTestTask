using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.DTOs
{
    /// <summary>
    /// DTO для передачи информации о предмете и связанных с ним материалах. Содержит имя предмета и список материалов, которые были предсказаны для этого предмета. Этот класс используется для передачи данных между слоями приложения, например, от обработчика команды к результату, который будет возвращен пользователю. Он служит для структурирования информации о том, какие материалы связаны с каждым подтвержденным предметом после анализа изображения.
    /// </summary>
    public class ItemMaterialDto
    {
        public string ItemName { get; set; } = string.Empty;
        public List<string> Materials { get; set; } = [];
    }
}
