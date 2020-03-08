namespace System {

/// <summary>
/// Utility classes for mathematical extensions
/// </summary>
public static class MathExtensions {

    public static TimeSpan Max(TimeSpan first, params TimeSpan[] rest) {
        var max = first;
        foreach (var time in rest) {
            if (time > first) {
                max = time;
            }
        }
        return max;
    }

}

}