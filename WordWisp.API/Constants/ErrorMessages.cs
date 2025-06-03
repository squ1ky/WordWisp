namespace WordWisp.API.Constants
{
    public static class ErrorMessages
    {
        // General
        public const string AccessDenied = "Нет доступа к ресурсу";
        public const string NotFound = "Ресурс не найден";

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
    }
}
