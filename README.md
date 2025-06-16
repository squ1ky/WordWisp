# WordWisp 🎓

**Адаптивная платформа для изучения английского языка с персональными словарями и тестированием уровня**

WordWisp - это современная веб-платформа, которая помогает пользователям изучать английский язык через создание персональных словарей и точное определение уровня владения языком с помощью адаптивного тестирования.

## ✨ Основные возможности

- **📚 Персональные словари** - Создание и управление собственными словарями с переводами, транскрипциями и примерами использования
- **🎯 Адаптивное тестирование** - Точное определение уровня английского языка (A1-C2) через комплексный тест из 110 вопросов
- **📊 Детальная аналитика** - Подробные результаты по грамматике (50 вопросов), лексике (50 вопросов) и чтению (10 вопросов)
- **📧 Цифровые сертификаты** - Автоматическая генерация и отправка сертификатов на email с результатами тестирования
- **📱 Адаптивный дизайн** - Полная поддержка мобильных устройств и планшетов
- **🔐 Безопасная авторизация** - JWT токены с подтверждением email адреса

## 🛠 Технологический стек

### Backend
- **C#**
- **ASP.NET Core 7.0**
- **Entity Framework Core**
- **PostgreSQL**

### Frontend
- **Razor Pages**
- **Bootstrap 5**
- **JavaScript (ES6+)**
- **Font Awesome**

## 🚀 Быстрый старт

### Предварительные требования

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/)

### Установка

1. **Клонируйте репозиторий**
   ```bash
   git clone https://github.com/squ1ky/WordWisp.git
   cd WordWisp
   ```

2. **Настройте базу данных**
   ```bash
   # Создайте базу данных PostgreSQL
   createdb wordwisp_db
   
   # Примените миграции
   cd WordWisp.API
   dotnet ef database update
   ```

3. **Создайте .env в WordWisp.API**
   
   Создайте файл `.env` в проекте `WordWisp.API`:
   ```json
   DB_CONNECTION_STRING=Host=localhost;Database=wordwisp_db;Username=password;Password=password
   JWT_SECRET_KEY=[secret key with 32 len at least]
    
   # Email
    
   EMAIL_HOST=smtp.yandex.com
   EMAIL_PORT=587
   EMAIL_USERNAME=[yandex почта]
   EMAIL_PASSWORD=[пароль приложения]
   EMAIL_FROM=[yandex почта]
   ```

4. **Запустите приложение**
   ```bash
   # Запуск API
   cd WordWisp.API
   dotnet run
   
   # Запуск веб-приложения (в отдельном терминале)
   cd WordWisp.Web
   dotnet run
   ```

## 📁 Структура проекта

```
WordWisp/
├── WordWisp.API/              # Backend API
│   ├── Controllers/           # API контроллеры
│   ├── Services/             # Бизнес-логика
│   ├── Repositories/         # Доступ к данным
│   ├── Models/               # Модели данных и DTO
│   └── Data/                 # Контекст базы данных
├── WordWisp.Web/             # Frontend веб-приложение
│   ├── Pages/                # Razor Pages
│   ├── wwwroot/              # Статические файлы
│   └── Models/               # View модели
└── README.md
```

## 🎯 Особенности адаптивного тестирования

Система использует современные подходы к оценке языкового уровня:

- **Секционная структура**: Грамматика (50 вопросов), Словарь (50 вопросов), Чтение (10 вопросов)
- **Градация сложности**: Вопросы распределены по уровням A1, A2, B1, B2, C1, C2
- **Справедливая оценка**: Неотвеченные вопросы засчитываются как неправильные
- **Взвешенное оценивание**: Учитывается количество вопросов в каждой секции

## 🔧 API Endpoints задокументированы в Swagger

## 📄 Лицензия

Этот проект лицензирован под MIT License - см. файл [LICENSE](LICENSE) для деталей.

## 👥 Авторы

- **Сабирзянов Артур Фидаэлевич** - *team lead, developer* - [squ1ky](https://github.com/squ1ky)
- **Жидких Никита Олегович** - *developer* - [nekitoksik](https://github.com/nekitoksik)
- **Марфин Михаил Евгеньевич** - *designer, developer* - [tenine](https://github.com/tenineee)
