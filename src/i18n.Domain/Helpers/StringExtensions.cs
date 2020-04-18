namespace i18n.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// String extension method to simplify testing for non-null/non-empty values.
        /// </summary>
        public static bool IsSet(
            this string str)
        {
            return !string.IsNullOrEmpty(str);
        }
    }
}
