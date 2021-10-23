namespace E1Translator
{
    public static class Utilities
    {
        public static string GetFormOidFromForm(string formName)
        {
            var substrings = formName.Split('_', 2, System.StringSplitOptions.RemoveEmptyEntries);
            if(substrings.Length != 2)
            {
                return null;
            } else
            {
                return substrings[1];
            }
        }
    }
}
