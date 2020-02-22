namespace System {

public static class UtilityExtensions {
    /// <summary>
    /// Quick access to the string.format function
    /// </summary>
    /// <param name="template">value to format</param>
    /// <param name="values">values to insert</param>
    /// <returns>formatted string</returns>
    public static string Format(this string template, params object[] values) {
        return string.Format(template, values);
    }
}

}