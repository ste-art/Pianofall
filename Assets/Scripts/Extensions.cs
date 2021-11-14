using System;
using System.Collections.Generic;

public static class Extensions
{
    public static bool EqualsI(this string a, string b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
    {
        var knownKeys = new HashSet<TKey>(comparer);
        foreach (var element in source)
        {
            if (knownKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static void SafeDispose(this IDisposable obj)
    {
        if (obj != null)
        {
            obj.Dispose();
        }
    }

    public static string Clamp(this string s, int size)
    {
        if (s == null)
        {
            return null;
        }
        if (s.Length <= size)
        {
            return s;
        }
        return s.Substring(0, size);
    }

    public static bool ContainsI(this string s, string value)
    {
        return s.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool StartsWithI(this string s, string value)
    {
        return s.StartsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EndsWithI(this string s, string value)
    {
        return s.EndsWith(value, StringComparison.OrdinalIgnoreCase);
    }
}
