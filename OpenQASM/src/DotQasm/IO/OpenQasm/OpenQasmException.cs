using System.IO;

namespace DotQasm.IO.OpenQasm {

public class OpenQasmException: System.Exception {
    
    private static string fmt = 
@"{0} (pos {6}, ln {2}, col {5}): {1}
>  {2} | {3}
   {4}   ^--Here.";

    public int Position {get; private set;}

    public OpenQasmException(int pos, string msg) : base (msg) {
        Position = pos;
    }

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