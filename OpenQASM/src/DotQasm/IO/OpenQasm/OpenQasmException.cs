using System.IO;

namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Base class for all OpenQASM exceptions
/// </summary>
public class OpenQasmException: System.Exception {
    
    private static string fmt = 
@"{0} (pos {6}, ln {2}, col {5}): {1}
>  {2} | {3}
   {4}   ^--Here.";

    /// <summary>
    /// Character position of the error occurrence
    /// </summary>
    /// <value>character position</value>
    public int Position {get; private set;}

    public OpenQasmException(int pos, string msg) : base (msg) {
        Position = pos;
    }

    /// <summary>
    /// Get the row, column, and line text for a given character position
    /// </summary>
    /// <returns>tuple with row number, column number, and row text</returns>
    private static (int, int, string) Get(int position, string text) {
        StringReader reader = new StringReader(text);
        string line = null; int row = 1; int column = 0; int current = 0;

        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++) {
            line = lines[i];
            bool isLine = false;
            for (column = 0; column < line.Length; column++, current++) {
                if (current >= position) {
                    isLine = true; break;
                }
            }
            if (isLine)
                break;
            row++;
            current++;
        }

        return (row, column, line);
    }

    /// <summary>
    /// Create a human readable formatted error message
    /// </summary>
    /// <param name="name">name of the source file</param>
    /// <param name="text">content of the source file</param>
    /// <returns>formatted message</returns>
    public string Format(string name, string text) {
        (int row, int column, string line) = Get(Position, text);
        string rowString = row.ToString();
        string spacer = new string(' ', rowString.Length + column);

        return string.Format(
            fmt,
            name,
            this.Message,
            rowString,
            line,
            spacer,
            column,
            Position
        );
    }
}

}