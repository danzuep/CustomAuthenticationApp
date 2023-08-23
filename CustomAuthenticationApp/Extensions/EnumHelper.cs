using System.Collections.Concurrent;

namespace CustomAuthenticationApp.Extensions
{
    internal static class EnumHelper<T> where T : Enum
    {
        public static Lazy<T[]> AllValues =>
            new Lazy<T[]>(() => (T[])Enum.GetValues(typeof(T)));

        public static Lazy<string[]> AllNames =>
            new Lazy<string[]>(() => Enum.GetNames(typeof(T)));

        public static Lazy<string[]> AllNamesUppercase =>
            new Lazy<string[]>(() => AllNames.Value.Select(v => v.ToUpperInvariant()).ToArray());

        public static Lazy<string[]> AllNamesLowercase =>
            new Lazy<string[]>(() => AllNames.Value.Select(v => v.ToLowerInvariant()).ToArray());

        public static readonly ConcurrentDictionary<T, string> KeyValuePairs = new();

        public static readonly ConcurrentDictionary<T, string> KeyValuePairsUppercase = new();

        public static readonly ConcurrentDictionary<T, string> KeyValuePairsLowercase = new();

        static EnumHelper()
        {
            for (int i = 0; i < AllValues.Value.Length; i++)
            {
                KeyValuePairs[AllValues.Value[i]] = AllNames.Value[i];
                KeyValuePairsUppercase[AllValues.Value[i]] = AllNamesUppercase.Value[i];
                KeyValuePairsLowercase[AllValues.Value[i]] = AllNamesLowercase.Value[i];
            }
        }
    }
}
