namespace PredicateBuilder
{
    public static class StringExtension
    {
        public static string Truncate(this string s, int len)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (len < 0) return s;

            if (s.Length > len) return s.Substring(0, len);
            return s;
        }
    }
}
