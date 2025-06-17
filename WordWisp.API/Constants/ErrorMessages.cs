namespace WordWisp.API.Constants
{
    public static class ErrorMessages
    {
        // General
        public const string AccessDenied = "Доступ запрещен";
        public const string NotFound = "Ресурс не найден";
        public const string InternalServerError = "Внутренняя ошибка сервера";
        public const string ConnectionError = "Не удалось подключиться к серверу";
        public const string InvalidRequestData = "Неверные данные запроса";

        // Dictionaries
        public const string DictionaryNotFound = "Словарь не найден";
        public const string DictionaryNotBelongsToUser = "Словарь не принадлежит указанному пользователю";
        public const string DictionaryAccessDenied = "У вас нет доступа к этому словарю";
        public const string CanCreateOnlyOwnDictionaries = "Можно создавать словари только для себя";
        public const string CanEditOnlyOwnDictionaries = "Можно редактировать только свои словари";
        public const string CanDeleteOnlyOwnDictionaries = "Можно удалять только свои словари";

        // Words
        public const string WordNotFound = "Слово не найдено";
        public const string CanAddWordsOnlyToOwnDictionaries = "Можно добавлять слова только в свои словари";
        public const string CanEditOnlyOwnWords = "Можно редактировать только свои слова";
        public const string CanDeleteOnlyOwnWords = "Можно удалять только свои слова";
        public const string NoDictionaryAccess = "Нет доступа к словарю";

        // Users
        public const string UserNotFound = "Пользователь не найден";
        public const string UserUpdateError = "Ошибка обновления данных пользователя";
        public const string PasswordChangeError = "Ошибка смены пароля";
        public const string PasswordChangeSuccess = "Пароль успешно изменен";
        public const string UserStatsError = "Ошибка загрузки статистики пользователя";
        public const string UsernameAlreadyExists = "Пользователь с таким именем уже существует";
        public const string EmailAlreadyExists = "Пользователь с таким email уже существует";
        public const string CurrentPasswordIncorrect = "Неверный текущий пароль";


        // Topic
        public const string TopicNotFound = "Топик не найден";
        public const string TopicAccessDenied = "У вас нет доступа к этому топику";
        public const string CanCreateOnlyAsTeacher = "Только преподаватели могут создавать топики";
        public const string CanEditOnlyOwnTopics = "Можно редактировать только свои топики";
        public const string CanDeleteOnlyOwnTopics = "Можно удалять только свои топики";
        public const string TopicCreatedSuccessfully = "Топик успешно создан";
        public const string TopicUpdatedSuccessfully = "Топик успешно обновлен";
        public const string TopicDeletedSuccessfully = "Топик успешно удален";

        // Materials
        public const string MaterialNotFound = "Материал не найден";
        public const string MaterialAccessDenied = "У вас нет доступа к этому материалу";
        public const string CanCreateMaterialsOnlyInOwnTopics = "Можно создавать материалы только в своих топиках";
        public const string CanEditOnlyOwnMaterials = "Можно редактировать только свои материалы";
        public const string CanDeleteOnlyOwnMaterials = "Можно удалять только свои материалы";

        // Exercises
        public const string ExerciseNotFound = "Упражнение не найдено";
        public const string ExerciseNotActive = "Упражнение неактивно";
        public const string MaxAttemptsExceeded = "Превышено максимальное количество попыток";
        public const string AttemptNotFound = "Попытка не найдена";
        public const string AttemptAlreadyCompleted = "Попытка уже завершена";
        public const string ExerciseAccessDenied = "У вас нет доступа к этому упражнению";
        // LevelTest
        public const string RetestingNotAvailable = "Повторное тестирование доступно через 30 дней";
        public const string UnableStartTest = "Невозможно начать тест. Проверьте eligibility или завершите активный тест";
        public const string WrongSection = "Неверная секция. Используйте: Grammar, Vocabulary, Reading";
        public const string FailedSaveAnswer = "Не удалось сохранить ответ";
        public const string UnableCompleteTest = "Не удалось завершить тест";

        public const string FailedSendCertificate = "Не удалось отправить сертификат";
        public const string SuccessCertificateSending = "Сертификат отправлен на почту";
    }
}
