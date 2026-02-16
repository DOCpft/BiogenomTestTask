# Biogenom — Image analyzer with GigaChat
  
Сервис анализирует изображения — находит основные предметы и определяет материалы предметов, используя GigaChat. Запускать рекомендовано в Docker (Postgres + API).

## Быстрый старт

Требования:
- Docker, Docker Compose

Сборка и запуск:

- Остановить и удалить старые контейнеры (без удаления томов, если нужно сохранить БД)
	docker compose down
- Пересобрать образы и запустить в фоне
	docker compose up --build --force-recreate -d

Проверки:
- Статус контейнеров:
	docker compose ps
- Логи API:
	docker compose logs -f api
- Логи БД:
	docker compose logs -f db

Остановка:
- Остановить контейнеры:
 	docker compose down

Если нужно полностью удалить образы и тома (ВНИМАНИЕ — удаляет данные БД):
- Остановить и удалить контейнеры, образы и тома:
	docker compose down --rmi --volumes --remove-orphans


## Конфигурация
Конфиги находятся в `Biogenom.Api/appsettings.json`
- Пример нужных настроек (без секретов):
  { "ConnectionStrings":
    {
      "DefaultConnection": "Host=db;Port=5432;Database=biogenomdb;Username=biogenom;Password=biogenom_pass"
    },
    "GigaChat":
    {
     "AuthUrl": "https://ngw.../oauth", "ApiUrl": "https://gigachat.../api/v1/chat/completions", "ClientId": "...", "ClientSecret": "...", "Scope": "...", "Model": "GigaChat-2-Max", "FilesUploadPurpose": "general" // либо другое значение по документации

     }
   }
- Swagger UI будет доступен по адресу:
  http://localhost:8080/swagger


## Промпты (хранятся в конфиге)
Промпты вынесены в `Biogenom.Infostructure\ServiceOptions\GigaChatPromptsOptions.cs` / секцию `GigaChatPrompts` в `appsettings.json`.

Текущие шаблоны:

- `PredictMainObjects`:
> Ты — система компьютерного зрения. Определи основной предмет или предметы, если таких несколько, на изображении. Игнорируй фон, мелкие детали и аксессуары. Сосредоточься на центральных объектах, ради которых сделано фото. Верни ТОЛЬКО JSON-массив строк с названиями предметов на русском языке. Пример: [\"стол\",\"стул\"]. Ответ только на русском

- `PredictMaterialsTemplate`:
> Ты — система компьютерного зрения. Для каждого из перечисленных предметов на изображении определи, из каких материалов они сделаны. Предметы: {items}. Верни ТОЛЬКО JSON-массив объектов вида: [{\"ItemName\":\"название предмета\",\"Materials\":[\"материал1\",\"материал2\"]}]. Пример: [{\"ItemName\":\"ручка\",\"Materials\":[\"пластик\",\"металл\", \"чернила\"]}]. Если для предмета материал не ясен, верни [\\\"неизвестно\\\"]. Перечисли все видимые материалы, из которых состоит предмет (например, корпус — пластик, перо — металл). Если предмет отсутствует на изображении, верни пустой массив. Ответ только на русском.

Вы можете править эти строки в `appsettings.json` или в классе `GigaChatPromptsOptions`.

## Архитектура (коротко)
Приложение разделено на слои, соответствующие чистой архитектуре (Clean Architecture):
- API слой (контроллеры, модели запросов/ответов)
- Сервисный слой (логика работы с GigaChat, построение промптов
- Инфраструктурный слой (клиенты для GigaChat API, загрузка файлов)
- Модель данных (EF Core, миграции)

- `GigaChatService` — оркестратор: получает токен, загружает изображение в GigaChat (если требуется), строит промпт и отправляет chat‑запрос.
- `GigaChatFileUploader` — отвечает за загрузку файлов в `/api/v1/files`.
- `GigaChatChatClient` — отправляет chat/completions и возвращает текст ответа.
- Логика по изменению/добавлению данных вынесена в команды и обработчики (MediatR), например `AnalyzeImageCommandHandler` и `ConfirmItemsCommandHandler` (паттерн CQRS).
- Логика работы с БД вынесена в репозитории (например, `AnalysisRequestRepository`), который инкапсулирует EF Core и обеспечивает абстрактный интерфейс для работы с данными.
- Промпты вынесены в `GigaChatPromptsOptions`.
- UploadedFileRef (в `AnalysisRequest`) хранит `fileRef` от GigaChat, чтобы не загружать изображение повторно.

## ER-диаграмма базы данных

```mermaid
erDiagram
    AnalysisRequest {
        int Id PK
        string ImageUrl
        datetime CreatedAt
        string RawAiResponse
        string UploadedFileRef
    }
    Item {
        int Id PK
        string Name
        int AnalysisRequestId FK
    }
    Material {
        int Id PK
        string Name
    }
    ItemMaterial {
        int ItemId PK, FK
        int MaterialId PK, FK
    }
    AnalysisRequest ||--o{ Item : "has"
    Item ||--o{ ItemMaterial : "has"
    Material ||--o{ ItemMaterial : "has"

Коротко:
- `AnalysisRequest` 1 → * `Item`  (One-to-Many)
- `Item` * ↔ * `Material` через `ItemMaterial` (Many-to-Many через промежуточную таблицу)


## API (пример использования)
- Swagger — подробная документация и ручное тестирование: `http://localhost:8080/swagger`
- Основные сценарии:
  - Отправка изображения на анализ (через UI/контроллеры проекта), через `AnalyzeImageCommandHandler`, который использует `IAiService.PredictMainObjectsAsync`
  - Подтверждение предметов вызывает `ConfirmItemsCommandHandler`, который использует `IAiService.PredictMaterialsAsync`.
  
Точные маршруты и структуры запросов смотрите в `Biogenom.Api\Controllers\AnalyzeController.cs` и DTO в `Biogenom.Application\DTOs`.
