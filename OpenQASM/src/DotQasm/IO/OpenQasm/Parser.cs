using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using DotQasm.IO.OpenQasm.Ast;

namespace DotQasm.IO.OpenQasm {

/* https://github.com/Qiskit/openqasm/blob/master/spec/qasm2.rst

productionlist::
    mainprogram: "OPENQASM" real ";" program
    program: statement | program statement
    statement: decl
                                            :| gatedecl goplist }
                                            :| gatedecl }
                                            :| "opaque" id idlist ";"
                                            :| "opaque" id "( )" idlist ";"
                                            :| "opaque" id "(" idlist ")" idlist ";"
                                            :| qop
                                            :| "if (" id "==" nninteger ")" qop
                                            :| "barrier" anylist ";"
    decl: "qreg" id [ nninteger ] ";" | "creg" id [ nninteger ] ";"
    gatedecl: "gate" id idlist {
                                    :| "gate" id "( )" idlist {
                                    :| "gate" id "(" idlist ")" idlist {
    goplist: uop
                                    :| "barrier" idlist ";"
                                    :| goplist uop
                                    :| goplist "barrier" idlist ";"
    qop: uop
                    :| "measure" argument "->" argument ";"
                    :| "reset" argument ";"
    uop: "U (" explist ")" argument ";"
                    :| "CX" argument "," argument ";"
                    :| id anylist ";" | id "( )" anylist ";"
                    :| id "(" explist ")" anylist ";"
    anylist: idlist | mixedlist
    idlist: id | idlist "," id
    mixedlist: id [ nninteger ] | mixedlist "," id
                                            :| mixedlist "," id [ nninteger ]
                                            :| idlist "," id [ nninteger ]
    argument: id | id [ nninteger ]
    explist: exp | explist "," exp
    exp: real | nninteger | "pi" | id
                    :| exp + exp | exp - exp | exp * exp
                    :| exp / exp | -exp | exp ^ exp
                    :| "(" exp ")" | unaryop "(" exp ")"
    unaryop: "sin" | "cos" | "tan" | "exp" | "ln" | "sqrt"
*/

/// <summary>
/// Parser for OpenQASM formatted text
/// </summary>
public class Parser {

    /// <summary>
    /// Internal token queue
    /// </summary>
    private List<Token> queue;
    /// <summary>
    /// Current position in the queue
    /// </summary>
    /// <value>position in the queue</value>
    public int Position {get; private set;}
    /// <summary>
    /// Check if the queue has been entirely parsed
    /// </summary>
    public bool IsDone => Position >= queue.Count;
    /// <summary>
    /// Current token in the queue
    /// </summary>
    /// <returns>The token at the current position</returns>
    private Token Current => Position < queue.Count ? queue.ElementAt(Position) : queue.Last();

    /// <summary>
    /// Directory to search when including files
    /// </summary>
    public string IncludeSearchPath;

    /// <summary>
    /// Create a new parser for the given token stream
    /// </summary>
    /// <param name="tokens">list of OpenQASM tokens</param>
    public Parser(List<Token> tokens) {
        this.queue = tokens;
        this.Position = 0;
    }

    /// <summary>
    /// Check that the next token is of the given type
    /// </summary>
    /// <param name="type">Type of token</param>
    /// <returns>true if the next token exists and is of the given type</returns>
    private bool Next(TokenType type) {
        if (!IsDone && Current.Type == type) {
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Move onto the next token
    /// </summary>
    private void Inc() {
        if (!IsDone)
            Position++;
    }

    /// <summary>
    /// Require that a token is next and consume it if it is
    /// </summary>
    /// <param name="type">type of token</param>
    private void Require(TokenType type, string symbol = null) {
        if (Next(type)) {
            Inc();
        } else {
            throw new OpenQasmSyntaxException(Current, "Missing '" + (symbol ?? type.ToString()) + "'");
        }
    }

    /// <summary>
    /// Require a semicolon
    /// </summary>
    private void Semicolon() {
        if (!Next(TokenType.SEMICOLON)) {
            throw new OpenQasmSyntaxException(Current, "Missing semicolon");
        }
        Inc();
    }

    /// <summary>
    /// Parse an OpenQASM include directive
    /// </summary>
    /// <returns>include path string if found</returns>
    public string ParseInclude() {
        if (!Next(TokenType.INCLUDE)) {
            return null;
        } 
        Inc();

        if (!Next(TokenType.STRING)) {
            throw new OpenQasmSyntaxException(Current, "Missing include filename");
        }
        string filename = Current.Lexeme;
        Inc();
        Semicolon();
        return filename;
    }

    /// <summary>
    /// Evaluate a classical expression atomic element
    /// </summary>
    /// <returns>double</returns>
    private double EvaluateAtomicExpression() {
        if (Next(TokenType.REAL)) {
            var ret = double.Parse(Current.Lexeme);
            Inc();
            return ret;
        } else if (Next(TokenType.NNINTEGER)) {
            var ret = int.Parse(Current.Lexeme);
            Inc();
            return ret;
        } else if (Next(TokenType.PI)) {
            return Math.PI;
        } else if (Next(TokenType.LPAREN)) {
            Inc();
            var ret = EvaluateClassicalExpression();
            if (!Next(TokenType.RPAREN)) {
                throw new OpenQasmSyntaxException(Current, "Missing ')'");
            }
            Inc();
            return ret;
        } 
        else {
            switch (Current.Type) {
                case TokenType.SIN:
                    Inc();
                    return Math.Sin(EvaluateClassicalExpression());
                case TokenType.COS:
                    Inc();
                    return Math.Cos(EvaluateClassicalExpression());
                case TokenType.TAN:
                    Inc();
                    return Math.Tan(EvaluateClassicalExpression());
                case TokenType.EXP:
                    Inc();
                    return Math.Exp(EvaluateClassicalExpression());
                case TokenType.LN:
                    Inc();
                    return Math.Log(EvaluateClassicalExpression());
                default:
                    throw new OpenQasmSyntaxException(Current, "Missing expression literal or function call");
            }
        }
    }

    /// <summary>
    /// Evaluate a unitary classical expression
    /// </summary>
    /// <returns>double</returns>
    private double EvaluateUnitaryExpression() {
        // PLUS signedAtom | MINUS signedAtom | func | atom
        if (Next(TokenType.PLUS)) {
            Inc();
            return EvaluateAtomicExpression();
        } else if (Next(TokenType.MINUS)) {
            Inc();
            return -1 * EvaluateAtomicExpression();
        } else {
            return EvaluateAtomicExpression();
        }
    }

    /// <summary>
    /// Evaluate a classical expression with exponents
    /// </summary>
    /// <returns>double</returns>
    private double EvaluatePowExpression() {
        // signedAtom (POW signedAtom)*
        List<double> pows = new List<double>();
        pows.Add(EvaluateUnitaryExpression());
        while(Next(TokenType.POW)) {
            Inc();
            pows.Add(EvaluateUnitaryExpression());
        }
        double root = pows[pows.Count - 1];
        for (int i = pows.Count - 1; i > 0; i++) {
            root = Math.Pow(pows[i-1], root);
        }
        return root;
    }

    /// <summary>
    /// Evaluate a classical expression with multiplication and division
    /// </summary>
    /// <returns>double</returns>
    private double EvaluateMulDivExpression() {
        // powExpression ((TIMES | DIV) powExpression)*
        double x = EvaluatePowExpression();
        while(Next(TokenType.TIMES) || Next(TokenType.DIVIDE)) {
            switch (Current.Type) {
                case TokenType.TIMES:
                    x = x * EvaluatePowExpression();
                    break;
                case TokenType.DIVIDE:
                    x = x / EvaluatePowExpression();
                    break;
            }
        }
        return x;
    }

    /// <summary>
    /// Evaluate a classical expression with addition and subtraction
    /// </summary>
    /// <returns>double</returns>
    private double EvaluateAddSubExpression() {
        // multiplyingExpression ((PLUS | MINUS) multiplyingExpression)*
        double x = EvaluateMulDivExpression();
        while(Next(TokenType.TIMES) || Next(TokenType.DIVIDE)) {
            switch (Current.Type) {
                case TokenType.TIMES:
                    x = x * EvaluateMulDivExpression();
                    break;
                case TokenType.DIVIDE:
                    x = x / EvaluateMulDivExpression();
                    break;
            }
        }
        return x;
    }

    /// <summary>
    /// Evaluate a classical expression
    /// </summary>
    /// <returns>double</returns>
    public double EvaluateClassicalExpression() {
        return EvaluateAddSubExpression();
    }

    /// <summary>
    /// Parse OpenQASM header
    /// </summary>
    public double ParseHeader() {
        if (!Next(TokenType.OPENQASM)) {
            throw new OpenQasmSyntaxException(Current, "OpenQASM files must start with 'OPENQASM <version>'");
        }
        Inc();

        double version;

        if (!Next(TokenType.REAL)) {
            throw new OpenQasmSyntaxException(Current, "OpenQASM version not specified");
        }
        double.TryParse(Current.Lexeme, out version);
        Inc();

        Semicolon();
        return version;
    }

    /// <summary>
    /// Parse an OpenQASM declaration
    /// </summary>
    /// <returns>Declaration AST context</returns>
    public DeclContext ParseDecl() {
        int pos = Current.Position;
        DeclType type = DeclType.Quantum;
        if (Next(TokenType.QREG)) {
            type = DeclType.Quantum;
            Inc();
        } else if (Next(TokenType.CREG)) {
            type = DeclType.Classical;
            Inc();
        } else {
            return null;
        }

        if (!Next(TokenType.ID)) {
            throw new OpenQasmSyntaxException(Current, "Declaration missing identifier");
        }
        string name = Current.Lexeme;
        Inc();

        if (!Next(TokenType.LSQUARE)) {
            throw new OpenQasmSyntaxException(Current, "Missing '['");
        }
        Inc();

        if (!Next(TokenType.NNINTEGER)) {
            throw new OpenQasmSyntaxException(Current, "Integer size required for register");
        }
        int amt = int.Parse(Current.Lexeme);
        Inc();

        if (!Next(TokenType.RSQUARE)) {
            throw new OpenQasmSyntaxException(Current, "Missing ']'");
        }
        Inc();

        var ctx = new DeclContext(pos, type, name, amt);

        Semicolon();

        return ctx;
    }

    /// <summary>
    /// Parse an OpenQASM argument
    /// </summary>
    /// <returns>Argument AST context</returns>
    public ArgumentContext ParseArgument() {
        int pos = Current.Position;
        // id | id [ nninteger ]
        if (!Next(TokenType.ID)) {
            throw new OpenQasmSyntaxException(Current, "Missing argument identifier");
        }
        string name = Current.Lexeme;
        Inc();

        if (Next(TokenType.LSQUARE)) {
            Inc();
            if (!Next(TokenType.NNINTEGER)) {
                throw new OpenQasmSyntaxException(Current, "Missing register index");
            }
            int index = int.Parse(Current.Lexeme);
            Inc();
            if (!Next(TokenType.RSQUARE)) {
                throw new OpenQasmSyntaxException(Current, "Missing ']'");
            }
            Inc();
            return new ArgumentContext(pos, name, index);
        } else {
            return new ArgumentContext(pos, name);
        }
    }

    /// <summary>
    /// Parse an OpenQASM measurement operation
    /// </summary>
    /// <returns>Measurement AST context</returns>
    public MeasurementContext ParseMeasurement() {
        int pos = Current.Position;
        if (!Next(TokenType.MEASURE)) {
            return null;
        }
        Inc();

        var quantum = ParseArgument();

        if (!Next(TokenType.MAP)) {
            throw new OpenQasmSyntaxException(Current, "Missing '->'");
        }
        Inc();

        var classical = ParseArgument();

        Semicolon();

        return new MeasurementContext(pos, quantum, classical);
    }

    /// <summary>
    /// Parse an OpenQASM reset operation
    /// </summary>
    /// <returns>Reset AST context</returns>
    public ResetContext ParseReset() {
        int pos = Current.Position;
        if (!Next(TokenType.RESET)) {
            return null;
        }
        Inc();

        var quantum = ParseArgument();

        return new ResetContext(pos, quantum);
    }

    private List<ArgumentContext> ParseArgumentList() {
        var ls = new List<ArgumentContext>();
        ls.Add(ParseArgument());
        while (Next(TokenType.COMMA)) {
            Inc();
            ls.Add(ParseArgument());
        }
        return ls;
    }

    private List<double> ParseExpressionList() {
        var ls = new List<double>();
        ls.Add(EvaluateClassicalExpression());
        while (Next(TokenType.COMMA)) {
            Inc();
            ls.Add(EvaluateClassicalExpression());
        }
        return ls;
    }

    /// <summary>
    /// Parse an OpenQASM unitary operation
    /// </summary>
    /// <returns>UnitaryOperation AST context</returns>
    public UnitaryOperationContext ParseUnitaryOperation() {
        // Built-IN
        // "U"|"CX" "(" explist ")" argument ;
        int pos = Current.Position;
        if (Next(TokenType.U) || Next(TokenType.CX)) {
            string name = Current.Type.ToString();
            Inc();

            if (!Next(TokenType.LPAREN)) {
                throw new OpenQasmSyntaxException(Current, "Missing '('");
            }
            Inc();
            
            List<double> classicalArgs = ParseExpressionList();

            if (!Next(TokenType.RPAREN)) {
                throw new OpenQasmSyntaxException(Current, "Missing ')'");
            }
            Inc();

            return new UnitaryOperationContext(pos, name, classicalArgs, new List<ArgumentContext>());
        }
        // User-Defined
        // id "(" explist ")" anylist ";"
        else if (Next(TokenType.ID)) {
            string name = Current.Lexeme;
            Inc();

            List<double> classicalArgs = null;
            if (Next(TokenType.LPAREN)) {
                Inc();

                classicalArgs = ParseExpressionList();

                if (!Next(TokenType.RPAREN)) {
                    throw new OpenQasmSyntaxException(Current, "Missing ')'");
                }
                Inc();
            }

            List<ArgumentContext> quantumArgs = ParseArgumentList();

            Semicolon();

            return new UnitaryOperationContext(pos, name, classicalArgs ?? new List<double>(), quantumArgs);
        } else {
            throw new OpenQasmSyntaxException(Current, "Expecting unitary expression");
        }
    }

    /// <summary>
    /// Parse a OpenQASM quantum operation
    /// </summary>
    /// <returns>QuantumOperation AST context</returns>
    public QuantumOperationContext ParseQuantumOperation() {
        var measure = ParseMeasurement();
        if (measure != null)
            return measure;
        var reset = ParseReset();
        if (reset != null) 
            return reset;
        var op = ParseUnitaryOperation();
        if (op != null)
            return op;
        else 
            throw new OpenQasmSyntaxException(Current, "Expecting measurement, reset, or unitary operation");
    }

    /// <summary>
    /// Parse OpenQASM barrier statement
    /// </summary>
    /// <returns>Barrier AST context</returns>
    public BarrierContext ParseBarrier() {
        int pos = Current.Position;
        if (!Next(TokenType.BARRIER)) {
            return null;
        }
        Inc();

        BarrierContext ctx = new BarrierContext(pos, ParseArgumentList());

        Semicolon();
        return ctx;
    }

    /// <summary>
    /// Parse OpenQASM if statment
    /// </summary>
    /// <returns>If statement AST context</returns>
    public IfContext ParseIf() {
        int pos = Current.Position;
        if (!Next(TokenType.IF)) {
            return null;
        }
        Inc();

        Require(TokenType.LPAREN, "(");

        if (!Next(TokenType.ID)) {
            throw new OpenQasmSyntaxException(Current, "Missing variable name");
        }
        string name = Current.Lexeme;
        Inc();

        Require(TokenType.EQUALS, "==");

        if (!Next(TokenType.NNINTEGER)) {
            throw new OpenQasmSyntaxException(Current, "Missing integer literal");
        }
        int value = int.Parse(Current.Lexeme);
        Inc();

        Require(TokenType.RPAREN, ")");

        QuantumOperationContext stmt = ParseQuantumOperation();

        return new IfContext(pos, name, value, stmt);
    }

    private List<string> ParseIdList() {
        var ls = new List<string>();
        if (!Next(TokenType.ID)) {
            return ls;
        }
        ls.Add(Current.Lexeme);
        Inc();

        while (Next(TokenType.COMMA)) {
            Inc();
            if (!Next(TokenType.ID)) {
                throw new OpenQasmSyntaxException(Current, "Expecting additional identifier after ','");
            }
            ls.Add(Current.Lexeme);
            Inc();
        }
        return ls;
    }

    public GateDeclContext ParseGateDecl() {
        int pos = Current.Position;
        // "gate" id 
        if (!Next(TokenType.GATE)) {
            return null;
        }
        Inc();

        if (!Next(TokenType.ID)) {
            throw new OpenQasmSyntaxException(Current, "Missing gate name");
        }
        string name = Current.Lexeme;
        Inc();

        // "(" idlist ")"
        List<string> classical = null;
        if (Next(TokenType.LPAREN)) {
            classical = ParseIdList();
            Require(TokenType.RPAREN, ")");
        }

        // idlist
        List<string> quantum = ParseIdList();

        // { goplist }
        // goplist = list of (barrier | unitaryop )
        Require(TokenType.LBRACE, "{");

        List<ICustomGateOperation> ops = new List<ICustomGateOperation>();
        while(!Next(TokenType.RBRACE)) {
            var barrier = ParseBarrier();
            if (barrier != null) {
                ops.Add(barrier);
                continue;
            }
            var uop = ParseUnitaryOperation();
            if (uop != null) {
                ops.Add(uop);
                continue;
            }

            throw new OpenQasmSyntaxException(Current, "Expecting barrier or unitary operation in gate's body");
        }

        Require(TokenType.RBRACE, "}");

        return new GateDeclContext(pos, name, classical ?? new List<string>(), quantum, ops);
    }

    public OpaqueGateDeclContext ParseOpaqueGateDecl() {
        int pos = Current.Position;
        // "opaque" id 
        if (!Next(TokenType.OPAQUE)) {
            return null;
        }
        Inc();

        if (!Next(TokenType.ID)) {
            throw new OpenQasmSyntaxException(Current, "Missing gate name");
        }
        string name = Current.Lexeme;
        Inc();

        // "(" idlist ")"
        List<string> classical = null;
        if (Next(TokenType.LPAREN)) {
            classical = ParseIdList();
            Require(TokenType.RPAREN, ")");
        }

        // idlist
        List<string> quantum = ParseIdList();

        return new OpaqueGateDeclContext(pos, name, classical ?? new List<string>(), quantum);
    }

    /// <summary>
    /// Parse and OpenQASM statement
    /// </summary>
    /// <returns>Statement AST context</returns>
    public StatementContext ParseStatement() {
        // Try decl
        var decl = ParseDecl();
        if (decl != null)
            return decl;
        // Try gatedel
        var gatedecl = ParseGateDecl();
        if (gatedecl != null)
            return gatedecl;
        // Try opaque
        var opaque = ParseOpaqueGateDecl();
        if (opaque != null) 
            return opaque;
        // Try classical if
        var iff = ParseIf();
        if (iff != null)
            return iff;
        // Try barrier
        var barrier = ParseBarrier();
        if (barrier != null)
            return barrier;
        // Try quantum op
        var op = ParseQuantumOperation();
        if (op != null) 
            return op;
        // Else error
        throw new OpenQasmSyntaxException(Current, "Expecting declaration, quantum operation, or instruction");
    }   

    /// <summary>
    /// Parse an OpenQASM program
    /// </summary>
    /// <returns>Program AST context</returns>
    public ProgramContext ParseProgram() {
        int pos = Current.Position;
        ProgramContext ctx = new ProgramContext(pos);
        while (!IsDone) {
            var position = Position;
            var inc = ParseInclude();
            if (inc != null && IncludeSearchPath != null) {
                var path = Path.Join(IncludeSearchPath, inc);
                if (File.Exists(path)) {
                    using (var reader = new StreamReader(path)) {
                        try {
                            Parser sub = new Parser(Lexer.Tokenize(reader));
                            ctx.Statements.AddRange(sub.ParseProgram().Statements);
                        } catch (OpenQasmException ex) {
                            throw new OpenQasmSyntaxException(position, string.Format("Syntax error in include '{0}' at pos '{2}', '{1}'", path, ex.Message, ex.Position));
                        }
                    }
                } else {
                    throw new OpenQasmIncludeException(position, path);
                }
                continue;
            }

            var stmt = ParseStatement();
            if (stmt != null) {
                ctx.Statements.Add(stmt);
            } else {
                break;
            }
        }
        return ctx;
    }

    /// <summary>
    /// Parse an OpenQASM file
    /// </summary>
    /// <returns></returns>
    public ProgramContext ParseFile() {
        var version = ParseHeader();
        var context = ParseProgram();
        if (context != null)
            context.Version = version;
        return context;
    }
    

    /// <summary>
    /// Convert a OpenQASM abstract syntax tree to a quantum circuit representation
    /// </summary>
    /// <param name="ast"></param>
    /// <returns></returns>
    public static Circuit Ast2Circuit(ProgramContext ast) {
        Circuit circuit = new Circuit();
        var visitor = new OpenQasm2CircuitVisitor(circuit);
        visitor.VisitProgram(ast);
        return circuit;
    }

    /// <summary>
    /// Parses an OpenQASM program using the grammar found at: https://github.com/Qiskit/openqasm/blob/master/spec/qasm2.rst
    /// </summary>
    /// <param name="program">Text reader containing an OpenQASM program</param>
    /// <returns>Quantum circuit</returns>
    public static Circuit Parse(TextReader program) {
        var tokens = Lexer.Tokenize(program);
        Parser p = new Parser(tokens);
        var ast = p.ParseFile();
        return Ast2Circuit(ast);
    }
    
    /// <summary>
    /// Parses an OpenQASM program using the grammar found at: https://github.com/Qiskit/openqasm/blob/master/spec/qasm2.rst
    /// </summary>
    /// <param name="program">OpenQASM program</param>
    /// <returns>Quantum circuit</returns>
    public static Circuit Parse(string program) {
        Circuit circuit;
        using (StringReader reader = new StringReader(program)) {
            circuit = Parse(reader);
        }
        return circuit;
    }

    /// <summary>
    /// Convert a quantum circuit to an OpenQASM program
    /// </summary>
    /// <param name="program">Quantum circuit</param>
    /// <returns>OpenQASM program</returns>
    public static string Stringify(Circuit program) {
        StringBuilder builder = new StringBuilder();
        using (StringWriter writer = new StringWriter(builder)) {
            Stringify(program, writer);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Convert a quantum circuit to an OpenQASM program
    /// </summary>
    /// <param name="program">Quantum circuit</param>
    /// <param name="writer">TextWriter to output OpenQASM program to</param>
    /// <returns>OpenQASM program</returns>
    public static void Stringify(Circuit program, TextWriter writer) {
        writer.WriteLine("OPENQASM 2.0;");
        // TODO
    }
}

}