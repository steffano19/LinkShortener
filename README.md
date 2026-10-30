# LinkShortener

REST API для сокращения ссылок на ASP.NET Core 10. Сервис принимает длинную ссылку, выдаёт короткий код, перенаправляет по нему и считает переходы.

Учебный проект для портфолио.

## Возможности

- Создание короткой ссылки со случайным кодом из 6 символов
- Свой код ссылки (например, `/mylink`)
- Проверка адреса: принимаются только `http://` и `https://`
- Защита от дубликатов: занятый код возвращает ошибку 409
- Редирект по короткой ссылке
- Подсчёт переходов и просмотр статистики
- Удаление ссылки
- Автоматические тесты (xUnit)

## Технологии

- C# и ASP.NET Core 10 (Web API с контроллерами)
- Entity Framework Core и SQLite
- OpenAPI (описание API в формате JSON)
- xUnit и WebApplicationFactory для интеграционных тестов

## Запуск

Понадобится [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
# 1. Клонировать репозиторий
git clone https://github.com/steffano19/LinkShortener.git
cd LinkShortener

# 2. Установить инструмент для миграций (один раз)
dotnet tool install --global dotnet-ef

# 3. Создать базу данных (появится файл links.db)
dotnet ef database update --project LinkShortener.Api

# 4. Запустить приложение
dotnet run --project LinkShortener.Api
```

Адрес (например, `http://localhost:5138`) будет написан в консоли в строке `Now listening on`.

Описание API в формате OpenAPI доступно по адресу `/openapi/v1.json` (только в режиме разработки).

## API

| Метод | Адрес | Описание | Успех | Ошибки |
|-------|-------|----------|-------|--------|
| POST | `/api/links` | Создать короткую ссылку | 201 | 400, 409 |
| GET | `/{code}` | Перейти на оригинальную ссылку | 302 | 404 |
| GET | `/api/links/{code}/stats` | Статистика по ссылке | 200 | 404 |
| DELETE | `/api/links/{code}` | Удалить ссылку | 204 | 404 |

### Создание ссылки

Тело запроса:

```json
{
  "originalUrl": "https://github.com",
  "customCode": "mylink"
}
```

Поле `customCode` необязательное. Если его не указать, код сгенерируется автоматически. Свой код может содержать буквы, цифры, `-` и `_`, длина от 3 до 20 символов. Коды `api` и `openapi` зарезервированы.

Ответ:

```json
{
  "code": "mylink",
  "shortUrl": "http://localhost:5138/mylink",
  "originalUrl": "https://github.com"
}
```

### Примеры запросов

PowerShell:

```powershell
# Создать ссылку
Invoke-RestMethod -Method Post -Uri "http://localhost:5138/api/links" -ContentType "application/json" -Body '{"originalUrl":"https://github.com","customCode":"mylink"}'

# Посмотреть статистику
Invoke-RestMethod -Uri "http://localhost:5138/api/links/mylink/stats"

# Удалить ссылку
Invoke-RestMethod -Method Delete -Uri "http://localhost:5138/api/links/mylink"
```

curl:

```bash
curl -X POST http://localhost:5138/api/links \
  -H "Content-Type: application/json" \
  -d '{"originalUrl":"https://github.com","customCode":"mylink"}'

curl http://localhost:5138/api/links/mylink/stats

curl -X DELETE http://localhost:5138/api/links/mylink
```

Переход по короткой ссылке удобнее проверять в браузере: откройте `http://localhost:5138/mylink`.

## Тесты

```bash
dotnet test
```

Тесты запускают приложение в памяти с отдельной временной базой данных и проверяют основные сценарии: создание ссылки, ошибки валидации, дубликаты, зарезервированные коды, редирект со счётчиком кликов и удаление.

## Структура проекта

```
LinkShortener/
├── LinkShortener.Api/
│   ├── Controllers/    # Контроллеры: создание, статистика, удаление, редирект
│   ├── Data/           # AppDbContext (работа с базой данных)
│   ├── Dtos/           # Классы запросов и ответов
│   ├── Models/         # Модель ShortLink
│   ├── Migrations/     # Миграции EF Core
│   └── Program.cs      # Настройка и запуск приложения
└── LinkShortener.Tests/
    └── LinksApiTests.cs  # Интеграционные тесты
```

