namespace DotQasm.IO.OpenQasm {

/// <summary>
/// All OpenQASM token classifications
/// </summary>
public enum TokenType {
    // Value Types
    ID,
    REAL,
    NNINTEGER,
    STRING,

    // Keywords
    OPENQASM,
    U,
    CX,
    IF,
    OPAQUE,
    BARRIER,
    GATE,
    MEASURE,
    RESET,
    CREG,
    QREG,
    PI,
    SIN,
    COS,
    TAN,
    EXP,
    LN,
    SQRT,
    INCLUDE,

    // Operators
    SEMICOLON,
    COMMA,
    LPAREN,
    RPAREN,
    LBRACE,
    RBRACE,
    LSQUARE,
    RSQUARE,
    EQUALS,
    MAP,
    PLUS,
    MINUS,
    TIMES,
    DIVIDE,
    POW,
}


}