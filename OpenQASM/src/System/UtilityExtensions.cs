namespace System {

public static class UtilityExtensions {
    public static string Format(this string template, params object[] values) {
        return string.Format(template, values);
    }
}

}