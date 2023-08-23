namespace CustomAuthenticationApp.Extensions
{
    internal static class EnumExtensions
    {
        public static string? ToUpper<T>(this T? obj) where T : Enum
        {
            string? value = null;
            var all = EnumHelper<T>.KeyValuePairsUppercase;
            if (obj is T key && !all.TryGetValue(key, out value))
            {
                value = key.ToString().ToUpperInvariant();
                all.TryAdd(key, value);
            }
            return value;
        }

        public static string? ToLower<T>(this T? obj) where T : Enum
        {
            string? value = null;
            var all = EnumHelper<T>.KeyValuePairsLowercase;
            if (obj is T key && !all.TryGetValue(key, out value))
            {
                value = key.ToString().ToLowerInvariant();
                all.TryAdd(key, value);
            }
            return value;
        }

        public static T[] AllValues<T>(this T _) where T : Enum =>
            EnumHelper<T>.AllValues.Value;

        public static string[] AllNames<T>(this T _) where T : Enum =>
            EnumHelper<T>.AllNames.Value;

        public static string[] AllNamesUppercase<T>(this T _) where T : Enum =>
            EnumHelper<T>.AllNamesUppercase.Value;

        public static string[] AllNamesLowercase<T>(this T _) where T : Enum =>
            EnumHelper<T>.AllNamesLowercase.Value;
    }
}
