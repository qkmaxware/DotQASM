using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DotQasm.Hardware {
    
public class HardwareConfiguration {
    public string Name {get; set;}

    public ConnectivityGraph ConnectivityGraph {get; set;}
    public int PhysicalQubitCount => ConnectivityGraph.Vertices.Count();

    private string _dsl;
    public string Connectivity {
        get {
            return _dsl;
        } 
        set {
            Parse(value);
            _dsl = value;
        }
    } 

    private enum Type {
        Identifier, 
        UndirectedEdge,
        DirectedEdge,
        LeftSquare,
        RightSquare,
        Equals,
        Semicolon
    }

    private struct Token {
        public int Position {get; set;}
        public string Lexeme {get; set;}
        public Type Type {get; set;}
    }

    private static List<Token> Tokenize(string dsl) {
        int position = 0;
        List<Token> tokens = new List<Token>();

        while (position < dsl.Length) {
            // skip whitespace
            while (position < dsl.Length && char.IsWhiteSpace(dsl[position])) {
                position++;
            }
            if (position >= dsl.Length)
                break;

            // Tokenize
            switch (dsl[position]) {
                case ';': tokens.Add(new Token(){ Position = position++, Lexeme = ";", Type = Type.Semicolon }); break;
                case '[': tokens.Add(new Token(){ Position = position++, Lexeme = "[", Type = Type.LeftSquare }); break;
                case ']': tokens.Add(new Token(){ Position = position++, Lexeme = "]", Type = Type.RightSquare }); break;
                case '=': tokens.Add(new Token(){ Position = position++, Lexeme = "=", Type = Type.Equals }); break;
                case '-': {
                    if (position + 1 < dsl.Length) {
                        if (dsl[position + 1] == '-') {
                            tokens.Add(new Token(){ Position = position, Lexeme = "--", Type = Type.UndirectedEdge });
                            position += 2;
                            break;
                        } else if (dsl[position + 1] == '>') {
                            tokens.Add(new Token(){ Position = position, Lexeme = "->", Type = Type.DirectedEdge });
                            position += 2;
                            break;
                        } 
                    }
                    // Invalid symbol
                    throw new System.Exception("Invalid edge symbol at " + position);
                }
                default : {
                    StringBuilder sb = new StringBuilder();
                    int start = position;
                    sb.Append(dsl[position]);
                    position++;
                    while (
                        position < dsl.Length 
                        && !char.IsWhiteSpace(dsl[position])
                        && dsl[position] != '[' && dsl[position] != ']' && dsl[position] != '=' && dsl[position] != '-' && dsl[position] != ';'
                    ) {
                        sb.Append(dsl[position]);
                        position++;
                    }
                    tokens.Add(new Token(){ Position = start, Lexeme = sb.ToString(), Type = Type.Identifier });
                    break;
                }
            }
        }

        return tokens;
    }

    private static Dictionary<string, string> ParseAttributes(ref int position, List<Token> tokens) {
        Dictionary<string, string> attrs = new Dictionary<string, string>();
        if (tokens[position].Type != Type.LeftSquare) {
            throw new System.Exception("Attributes must start with '['");
        }
        position++;
        while (position < tokens.Count && tokens[position].Type != Type.RightSquare) {
            //id=id
            if (position >= tokens.Count || tokens[position].Type != Type.Identifier) {
                throw new System.Exception("Missing attribute name");
            }
            var name = tokens[position++].Lexeme;
            if (position >= tokens.Count || tokens[position].Type != Type.Equals) {
                throw new System.Exception("Missing attribute assignment '='");
            }
            position++;
            if (position >= tokens.Count || tokens[position].Type != Type.Identifier) {
                throw new System.Exception("Missing attribute value");
            }
            var value = tokens[position++].Lexeme;
            attrs.Add(name, value);
        }
        if (tokens[position].Type != Type.RightSquare) {
            throw new System.Exception("Attributes must end with ']'");
        }
        position++;
        return attrs;
    }

    private static PhysicalQubit ParseNode(Dictionary<string, PhysicalQubit> graph, ref int position, List<Token> tokens) {
        if (tokens[position].Type != Type.Identifier) {
            throw new System.Exception("Node must begin with an identifier");
        }

        var nodeName = tokens[position].Lexeme; 
        position++;
        if (position < tokens.Count && tokens[position].Type == Type.LeftSquare) {
            var attrs = ParseAttributes(ref position, tokens);
        }
        if (!graph.ContainsKey(nodeName)) {
            var qubit = new PhysicalQubit();
            graph.Add(nodeName, qubit);   
            return qubit;
        } else {
            return graph[nodeName];
        }
    }

    private void Parse(string dsl) {
        try {
            var tokens = Tokenize(dsl);
            int position = 0;
            ConnectivityGraph graph = new ConnectivityGraph();
            Dictionary<string, PhysicalQubit> qubitMap = new Dictionary<string, PhysicalQubit>();
            List<KeyValuePair<PhysicalQubit, PhysicalQubit>> edgeMap = new List<KeyValuePair<PhysicalQubit, PhysicalQubit>>();
            
            while (position < tokens.Count) {
                // Start
                var startNode = ParseNode(qubitMap, ref position, tokens);
                
                while (position < tokens.Count && (tokens[position].Type == Type.UndirectedEdge || tokens[position].Type == Type.DirectedEdge)) {
                    // Edge type
                    var undirected = false;
                    if (tokens[position].Type == Type.UndirectedEdge) {
                        undirected = true;
                    } else if (tokens[position].Type == Type.DirectedEdge) {
                        undirected = false;
                    } else {
                        throw new System.Exception("Edge must be directed or undirected");
                    }
                    position++;

                    // End
                    var endNode = ParseNode(qubitMap, ref position, tokens);

                    // Make Edge
                    edgeMap.Add(new KeyValuePair<PhysicalQubit, PhysicalQubit>(startNode, endNode));
                    if (undirected) {
                        edgeMap.Add(new KeyValuePair<PhysicalQubit, PhysicalQubit>(endNode, startNode));
                    }

                    // End becomes the new begining
                    startNode = endNode;
                }

                // Check for semicolon
                if (position < tokens.Count && tokens[position].Type == Type.Semicolon) {
                    position++;
                }
            }
            
            foreach (var pair in qubitMap) {
                graph.Add(pair.Value);
            }
            foreach (var pair in edgeMap) {
                graph.DirectedEdge(pair.Key, pair.Value, new Channel());
            }

            this.ConnectivityGraph = graph;
        } catch (System.Exception ex) {
            throw new System.Exception("Failed to parse connectivity DSL", ex);
        }
    }
    
}

}