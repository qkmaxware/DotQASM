using System;

public class ProgressBar {

    private int left;
    private int top;

    private static int length = 20;
    public static char endcap = '|';
    private static char bar = '-';
    private static char space = ' ';

    private float max;

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
        Console.Write(new String(space, Console.BufferWidth - 1));
    }

    public void Update(float value) {
        ClearLine();
        ResetCursor();
        var percent = value / max;
        percent = percent > 1 ? 1 : (percent < 0 ? 0 : percent); // clamp value

        // Draw bar
        Console.Write(endcap);
        var bars = (int)(percent * length); // compute number of bars
        var spaces = length - bars;         // compute number of blanks
        for (var i = 0; i < bars; i++) {
            Console.Write(bar);
        } 
        for (var i = 0; i < spaces; i++) {
            Console.Write(space);
        }
        Console.Write(endcap);
        Console.Write(space);

        // Print Percent
        Console.Write(string.Format( "{0:0.00}", percent ));
        Console.Write('%');
    }

}