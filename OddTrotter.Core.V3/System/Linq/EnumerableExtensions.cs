namespace System.Linq
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class EnumerableExtensions
    {
        public static bool TrySingle<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
        {
            try
            {
                value = source.Single();
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public static bool TryFirst<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
        {
            try
            {
                value = source.First();
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }
    }
}
