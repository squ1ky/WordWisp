using WordWisp.API.Constants;

namespace WordWisp.API.Constants

{
    public static class ErrorMessages
    {
        // General
        public const string AccessDenied = "Доступ запрещен";
        public const string NotFound = "Ресурс не найден";
        public const string InternalServerError = "Внутренняя ошибка сервера";
        public const string ConnectionError = "Не удалось подключиться к серверу";

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

    }
}
