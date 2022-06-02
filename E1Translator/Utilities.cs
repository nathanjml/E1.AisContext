namespace E1Translator
{
    public static class Utilities
    {
        public static string? TryGetFormOid(string formName)
        {
            var substrings = SplitFormName(formName);
            return HasFormOid(formName) ? substrings[1] : null;
        }

        public static bool HasFormOid(string formName)
        {
            var substrings = SplitFormName(formName);
            return substrings.Length == 2;
        }

        private static string[] SplitFormName(string formName) => formName.Split('_', 2, System.StringSplitOptions.RemoveEmptyEntries);
    }
}
