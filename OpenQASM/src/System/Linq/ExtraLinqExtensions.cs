using System.Collections.Generic;

namespace System.Linq {

public static class ExtraLinqExtensions {
    public static int IndexOf<T> (this IEnumerable<T> ls, T value) {
        try {
            return ls.Select((item, i) => new {
                Item = item,
                Position = i
            }).Where(m => m.Item?.Equals(value) ?? false).First().Position;
        } catch (InvalidOperationException) {
            return -1;
        }
    }

    public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source) {
            if (seenKeys.Add(keySelector(element))) {
                yield return element;
            }
        }
    }
}

}