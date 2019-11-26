namespace DotQasm.IO.OpenQasm {

/// <summary>
/// OpenQASM token
/// </summary>
public class Token {
    /// <summary>
    /// Starting character position of the token
    /// </summary>
    /// <value>character position</value>
    public int Position {get; private set;}
    /// <summary>
    /// Literal value of the token from the source
    /// </summary>
    /// <value>source value</value>
    public string Lexeme {get; private set;}
    /// <summary>
    /// OpenQASM token classification
    /// </summary>
    /// <value>classification type of the token</value>
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