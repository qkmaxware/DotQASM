using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm {

public static class Lexer {
    /*
    https://github.com/Qiskit/openqasm/blob/master/spec/qasm2.rst

    id        := [a-z][A-Za-z0-9_]*
    real      := ([0-9]+\.[0-9]*|[0-9]*\.[0-9]+)([eE][-+]?[0-9]+)?
    nninteger := [1-9]+[0-9]*|0
    OPENQASM  := "OPENQASM"
    semicolon := ";"
    comma     := ","
    lparen    := "("
    rparen    := ")"
    lbrace    := "{"
    rparen    := "}"
    lsquare   := "["
    rsquare   := "]"
    equals    := "=="
    map       := "->"
    plus      := "+"
    minus     := "-"
    times     := "*"
    divide    := "/"
    pow       := "^"
    U         := "U"
    if        := "if"
    opaque    := "opaque"
    barrier   := "barrier"
    gate      := "gate"
    measure   := "measure"
    reset     := "reset"
    creg      := "creg"
    qreg      := "qreg"
    pi        := "pi"
    sin       := "sin"
    cos       := "cos"
    tan       := "tan"
    exp       := "exp"
    ln        := "ln"
    sqrt      := "sqrt"
    */

    public static List<Token> Tokenize(TextReader reader) {
        List<Token> tokens = new List<Token>();
        int position = 0;
        while (reader.Peek() != -1) {
            // Skip whitespace
            while(reader.Peek() != -1 && char.IsWhiteSpace((char)reader.Peek())) {
                reader.Read();
                position++;
            }
            if (reader.Peek() == -1) {
                break;
            }
            // Pattern match the character
            char c = (char)reader.Read();
            int initPosition = position;
            switch (c) {
                case ';':
                    tokens.Add(new Token(TokenType.SEMICOLON, initPosition, c)); break;
                case ',':
                    tokens.Add(new Token(TokenType.COMMA, initPosition, c)); break;
                case '(':
                    tokens.Add(new Token(TokenType.LPAREN, initPosition, c)); break;
                case ')':
                    tokens.Add(new Token(TokenType.RPAREN, initPosition, c)); break;
                case '{':
                    tokens.Add(new Token(TokenType.LBRACE, initPosition, c)); break;
                case '}':
                    tokens.Add(new Token(TokenType.RBRACE, initPosition, c)); break;
                case '[':
                    tokens.Add(new Token(TokenType.LSQUARE, initPosition, c)); break;
                case ']':
                    tokens.Add(new Token(TokenType.RSQUARE, initPosition, c)); break;
                case '+':
                    tokens.Add(new Token(TokenType.PLUS, initPosition, c)); break;
                case '*':
                    tokens.Add(new Token(TokenType.TIMES, initPosition, c)); break;
                case '/': {
                    if (reader.Peek() == '/') {
                        // Is a comment
                        while (reader.Peek() != -1 && reader.Peek() != '\n') {
                            reader.Read(); position++;
                        }
                        break;
                    } else {
                        tokens.Add(new Token(TokenType.DIVIDE, initPosition, c)); 
                        break;
                    }
                }
                case '^':
                    tokens.Add(new Token(TokenType.POW, initPosition, c)); break;
                case '=': {
                    if (reader.Peek() == '=') {
                        position++; reader.Read();
                        tokens.Add(new Token(TokenType.EQUALS, initPosition, "==")); 
                        break;
                    } else {
                        throw new OpenQasmCharacterException(position, "Missing second '=' for equality operator");
                    }
                }
                case '-': {
                    if (reader.Peek() == '>') {
                        position++; reader.Read();
                        tokens.Add(new Token(TokenType.MAP, initPosition, "->")); 
                        break;
                    } else {
                        tokens.Add(new Token(TokenType.MINUS, initPosition, c)); 
                        break;
                    }
                }
                case '"': {
                    StringBuilder builder = new StringBuilder();
                    while(reader.Peek() != -1 && reader.Peek() != '"') {
                        builder.Append((char)reader.Read()); position++;
                    }
                    reader.Read(); position++; // consume trailing "
                    tokens.Add(new Token(TokenType.STRING, initPosition, builder.ToString()));
                    break;
                }
                default: {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(c);
                    if (char.IsNumber(c)) {
                        // Is real or nninteger
                        TokenType type = TokenType.NNINTEGER;
                        while ( reader.Peek() != -1 && char.IsDigit((char)reader.Peek()) ) {
                            builder.Append((char)reader.Read()); position++;
                        }
                        if (reader.Peek() == '.') {
                            type = TokenType.REAL;
                            builder.Append((char)reader.Read()); position++;
                            while ( reader.Peek() != -1 && char.IsDigit((char)reader.Peek()) ) {
                                builder.Append((char)reader.Read()); position++;
                            }
                        }

                        if (reader.Peek() == 'e' || reader.Peek() == 'E') {
                            type = TokenType.REAL;
                            builder.Append((char)reader.Read()); position++;
                            if (reader.Peek() == '+' || reader.Peek() == '-') {
                                builder.Append((char)reader.Read()); position++;
                            }
                            while ( reader.Peek() != -1 && char.IsDigit((char)reader.Peek()) ) {
                                builder.Append((char)reader.Read()); position++;
                            }
                        }

                        tokens.Add(new Token(type, initPosition, builder.ToString()));
                        break;
                    } else if (char.IsLetter(c)) {
                        // Is an id or keyword TODO, id's must be lower-case
                        while (reader.Peek() != -1 && ( char.IsLetter((char)reader.Peek()) || char.IsDigit((char)reader.Peek()) || reader.Peek() == '_' )) {
                            builder.Append((char)reader.Read()); position++;
                        }

                        string text = builder.ToString();
                        tokens.Add(text switch {
                            "OPENQASM"  => new Token(TokenType.OPENQASM, initPosition, text),
                            "U"         => new Token(TokenType.U, initPosition, text),
                            "CX"        => new Token(TokenType.CX, initPosition, text),
                            "if"        => new Token(TokenType.IF, initPosition, text),
                            "opaque"    => new Token(TokenType.OPAQUE, initPosition, text),
                            "barrier"   => new Token(TokenType.BARRIER, initPosition, text),
                            "gate"      => new Token(TokenType.GATE, initPosition, text),
                            "measure"   => new Token(TokenType.MEASURE, initPosition, text),
                            "reset"     => new Token(TokenType.RESET, initPosition, text),
                            "creg"      => new Token(TokenType.CREG, initPosition, text),
                            "qreg"      => new Token(TokenType.QREG, initPosition, text),
                            "pi"        => new Token(TokenType.PI, initPosition, text),
                            "sin"       => new Token(TokenType.SIN, initPosition, text),
                            "cos"       => new Token(TokenType.COS, initPosition, text),
                            "tan"       => new Token(TokenType.TAN, initPosition, text),
                            "exp"       => new Token(TokenType.EXP, initPosition, text),
                            "ln"        => new Token(TokenType.LN, initPosition, text),
                            "sqrt"      => new Token(TokenType.SQRT, initPosition, text),
                            "include"   => new Token(TokenType.INCLUDE, initPosition, text),
                            _           => new Token(TokenType.ID, initPosition, text),
                        });
                        break;
                    } else {
                        // Invalid character
                        throw new OpenQasmCharacterException(
                            initPosition, 
                            string.Format(
                                "Invalid character '{0}' in OpenQASM program",
                                System.Web.HttpUtility.JavaScriptStringEncode(builder.ToString())
                            )
                        );
                    }
                }
            }
            position++;
        }

        return tokens;
    }
}

}