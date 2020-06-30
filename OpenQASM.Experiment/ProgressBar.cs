using System;
using System.Text;

public class ProgressBar {

    private int left;
    private int top;

    private static int length = 20;
    public static char endcap = '|';
    private static char bar = '-';
    private static char space = ' ';

    private float max;
    private int lastLength = 0;

    public ProgressBar() {
        this.left = Console.CursorLeft;
        this.top = Console.CursorTop;
        this.max = 1f;
    }
    public ProgressBar(float max) : this(){
        this.max = max;
    }

    private void ResetCursor() {
        Console.SetCursorPosition(this.left, this.top);
    }

    private void ClearLine() {
        ResetCursor();
        Console.Write(new String(space, lastLength));
    }

    public void Update(float value) {
        ClearLine();
        ResetCursor();
        var percent = value / max;
        percent = percent > 1 ? 1 : (percent < 0 ? 0 : percent); // clamp value

        StringBuilder sb = new StringBuilder();

        // Draw bar
        sb.Append(endcap);
        var bars = (int)(percent * length); // compute number of bars
        var spaces = length - bars;         // compute number of blanks
        for (var i = 0; i < bars; i++) {
            sb.Append(bar);
        } 
        for (var i = 0; i < spaces; i++) {
            sb.Append(space);
        }
        sb.Append(endcap);
        sb.Append(space);

        // Print Percent
        sb.Append(value + "/" + max);
        lastLength = sb.Length;

        Console.Write(sb.ToString());
    }

}