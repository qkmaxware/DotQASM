namespace DotQasm.IO.OpenQasm {

public class Token {
    public int Position {get; private set;}
    public string Lexeme {get; private set;}
    public TokenType Type {get; private set;}

    public Token(TokenType type, int pos, string lexeme) {
        this.Type = type;
        this.Position = pos;
        this.Lexeme = lexeme;
    }

    public Token(TokenType type, int pos, char lexeme) {
        this.Type = type;
        this.Position = pos;
        this.Lexeme = lexeme.ToString();
    }
}

}