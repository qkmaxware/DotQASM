using System.Linq;
using System.Collections.Generic;
using DotQasm.IO.OpenQasm.Ast;

namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Visitor for OpenQASM to convert OpenQASM AST to quantum circuits
/// </summary>
public class OpenQasmSemanticAnalyser : IOpenQasmVisitor {

    public enum OpenQasmType {
        CReg, QReg, Gate
    }

    private Dictionary<string, OpenQasmType> identifiers = new Dictionary<string, OpenQasmType>();
    private Dictionary<string, GateDeclContext> gateMap = new Dictionary<string, GateDeclContext>();

    public int QubitCount {get; set;}

    public int CbitCount {get; set;}

    public int GateUseCount {get; set;}
    public int MeasurementCount {get; set;}
    public int ResetCount {get; set;}
    public int BarrierCount {get; set;}
    public int ClassicalConditionCount {get; set;}
    public int StatementCount {get; set;}

    public OpenQasmSemanticAnalyser() {}

    public bool IsDeclared(string varname) {
        return identifiers.ContainsKey(varname);
    }

    public GateDeclContext GetGateDefinition(string varname) {
        return gateMap[varname];
    }

    public bool IsType(string varname, OpenQasmType type) {
        if (IsDeclared(varname)) {
            return identifiers[varname] == type;
        } else {
            return false;
        }
    }

    public void VisitStatement(StatementContext stmt) {
        StatementCount++;
        switch (stmt) {
            case BarrierContext barrier: 
                VisitBarrier(barrier);
                break;
            case DeclContext decl: 
                VisitDeclaration(decl);
                break;
            case GateDeclContext gate: 
                VisitGateDeclaration(gate);
                break;
            case IfContext @if: 
                VisitClassicalIf(@if);
                break;
            case OpaqueGateDeclContext gate: 
                VisitOpaqueGateDeclaration(gate);
                break;
            case MeasurementContext measure: 
                VisitMeasurement(measure);
                break;
            case UnitaryOperationContext qop:
                VisitUnitaryQuantumOperator(qop);  
                break;
            case ResetContext reset: 
                VisitReset(reset);
                break;
            default:
                throw new OpenQasmSemanticException(stmt, "Statement is not supported in circuit");
        }  
    }

    public void VisitProgram(ProgramContext program) {
        foreach (var stmt in program.Statements) {
            VisitStatement(stmt);
        }
    }

    public void VisitDeclaration(DeclContext declaration) {
        if (IsDeclared(declaration.VariableName)) {
            // Throw
            throw new OpenQasmSemanticException(declaration, "Local variable already defined in current scope");
        }

        switch (declaration.Type) {
            case DeclType.Classical: 
                identifiers.Add(declaration.VariableName, OpenQasmType.CReg);
                QubitCount += declaration.Amount;
                break;
            case DeclType.Quantum:
                identifiers.Add(declaration.VariableName, OpenQasmType.QReg);
                CbitCount += declaration.Amount;
                break;
        }
    }

    public void VisitGateDeclaration(GateDeclContext declaration) {
        // Verify that all used arguments are formal parametres
        if (IsDeclared(declaration.GateName)) {
            throw new OpenQasmSemanticException(declaration, "Duplicate identifier");
        }

        foreach (var stmt in declaration.Operations) {
            switch (stmt) {
                case UnitaryOperationContext op:
                    foreach (var argument in op.ClassicalParametres) {
                        foreach (var id in argument.GetVariables()) {
                            if (!declaration.ClassicalArguments.Contains(id.VariableName)) {
                                throw new OpenQasmSemanticException(id, "Local classical variable is not defined in the current scope");
                            }
                        }
                    }
                    foreach (var argument in op.QuantumParametres) {
                        if (argument.IsArrayMember) {
                            throw new OpenQasmSemanticException(argument, "Cannot index a quantum bit");
                        } else if (!declaration.QuantumArguments.Contains(argument.ArgumentName)) {
                            throw new OpenQasmSemanticException(argument, "Local quantum variable is not defined in the current scope");
                        }
                    }
                    break;
            }
        }

        identifiers.Add(declaration.GateName, OpenQasmType.Gate);
        gateMap.Add(declaration.GateName, declaration);
    }

    public void VisitOpaqueGateDeclaration(OpaqueGateDeclContext declaration) {
        if (IsDeclared(declaration.GateName)) {
            throw new OpenQasmSemanticException(declaration, "Duplicate identifier");
        }

        identifiers.Add(declaration.GateName, OpenQasmType.Gate);
    }

    public void VisitUnitaryQuantumOperator(UnitaryOperationContext qop) {
        switch (qop.OperationName) {
            case "U":
                // Built in U() gate has 3 classical args and 1 quantum arg
                if (qop.ClassicalParametres.Count != 3) {
                    throw new OpenQasmSemanticException(qop, "U gate requires exactly 3 classical arguments");
                }
                if (qop.QuantumParametres.Count != 1) {
                    throw new OpenQasmSemanticException(qop, "U gate requires exactly 1 quantum argument");
                }

                break;
            case "CX":
                // Built in CX gate has 0 classical args and 2 quantum args
                if (qop.ClassicalParametres.Count != 0) {
                    throw new OpenQasmSemanticException(qop, "CX gate requires exactly 0 classical arguments");
                }
                if (qop.QuantumParametres.Count != 2) {
                    throw new OpenQasmSemanticException(qop, "CX gate requires exactly 2 quantum argument");
                }

                break;
            default:
                // User defined gate, verify it exists
                if (!IsDeclared(qop.OperationName)) {
                    throw new OpenQasmSemanticException(qop, "Gate does not exist");
                }
                if (!IsType(qop.OperationName, OpenQasmType.Gate)) {
                    throw new OpenQasmSemanticException(qop, "Identifier does not refer to a quantum gate");
                }
                if (!gateMap.ContainsKey(qop.OperationName)) {
                    throw new OpenQasmSemanticException(qop, "Gate does not have a defined body");
                }

                var gate = gateMap[qop.OperationName];

                // has the correct number of args
                if (qop.ClassicalParametres.Count != gate.ClassicalArguments.Count) {
                    throw new OpenQasmSemanticException(qop, string.Format("'{0}' gate requires exactly {1} classical arguments", qop.OperationName, qop.ClassicalParametres.Count));
                }
                if (qop.QuantumParametres.Count != gate.QuantumArguments.Count) {
                    throw new OpenQasmSemanticException(qop, string.Format("'{0}' gate requires exactly {1} quantum argument", qop.OperationName, qop.QuantumParametres.Count));
                }
                break;
        }
        GateUseCount++;
    }

    public void VisitMeasurement (MeasurementContext measure) {
        if (!IsType(measure.QuatumArgument.ArgumentName, OpenQasmType.QReg)) {
            throw new OpenQasmSemanticException(measure, "First argument of measure must be a quantum variable");
        }

        if (!IsType(measure.ClassicalArgument.ArgumentName, OpenQasmType.CReg)) {
            throw new OpenQasmSemanticException(measure, "Second argument of measure must be a classical variable");
        }

        MeasurementCount++;
    }

    public void VisitReset (ResetContext reset) {
        if (!IsType(reset.Argument.ArgumentName, OpenQasmType.QReg)) {
            throw new OpenQasmSemanticException(reset, "Argument of reset must be a quantum variable");
        }
        
        ResetCount++;
    }

    public void VisitBarrier(BarrierContext barrier) {
        foreach (var argument in barrier.Arguments) {
            if (!IsType(argument.ArgumentName, OpenQasmType.QReg)) {
                throw new OpenQasmSemanticException(argument, "Argument of barrier must be a quantum variable");
            }
        }

        BarrierCount++;
    }

    public void VisitClassicalIf(IfContext @if) {
        if (!IsDeclared(@if.ClassicalVariableName)) {
            throw new OpenQasmSemanticException(@if, string.Format("Variable '{0}' is not defined", @if.ClassicalVariableName));
        }

        switch (@if.Operation) {
            case MeasurementContext measurement:
                VisitMeasurement(measurement);
                break;
            case ResetContext reset:
                VisitReset(reset);
                break;
            case UnitaryOperationContext qop:
                VisitUnitaryQuantumOperator(qop);
                break;
            default:
                throw new OpenQasmSemanticException(@if.Operation, "Operation is not supported in 'if' statement");
        }
        ClassicalConditionCount ++;
    }

}

}