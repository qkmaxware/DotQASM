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
}

}