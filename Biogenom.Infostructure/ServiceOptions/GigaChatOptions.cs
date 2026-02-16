using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Infrastructure.ServiceOptions
{
    /// <summary>
    /// Предназначен для хранения конфигурационных параметров, необходимых для взаимодействия с GigaChat. Содержит свойства для URL-адресов авторизации и API, а также для идентификации клиента и его секретного ключа. Эти параметры используются для настройки HttpClient при отправке запросов к GigaChat, обеспечивая правильную аутентификацию и авторизацию. Также включает параметр для указания модели, которая будет использоваться при взаимодействии с GigaChat, что позволяет гибко настраивать поведение модели в зависимости от конкретных задач и требований приложения. Параметр FilesUploadPurpose определяет цель загрузки файлов в GigaChat, что может быть полезно для оптимизации обработки файлов на стороне сервера.
    /// </summary>
    public class GigaChatOptions
    {
        public string AuthUrl { get; set; }
        public string ApiUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string Model { get; set; }

        public string FilesUploadPurpose { get; set; } = "general";
    }
}
