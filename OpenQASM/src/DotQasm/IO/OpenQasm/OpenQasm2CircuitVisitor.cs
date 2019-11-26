using System.Linq;
using System.Collections.Generic;
using DotQasm.IO.OpenQasm.Ast;

namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Visitor for OpenQASM to convert OpenQASM AST to quantum circuits
/// </summary>
public class OpenQasm2CircuitVisitor : IOpenQasmVisitor {

    public Circuit Circuit {get; set;}
    public bool CanEditCircuit => Circuit != null;    

    private Dictionary<string, IEnumerable<Circuit.Qubit>> qubitMap = new Dictionary<string, IEnumerable<Circuit.Qubit>>();
    private Dictionary<string, IEnumerable<Circuit.Cbit>> cbitMap = new Dictionary<string, IEnumerable<Circuit.Cbit>>();

    public OpenQasm2CircuitVisitor(Circuit circuit) {
        this.Circuit = circuit;
    }

    private bool IsDeclared(string varname) {
        return qubitMap.ContainsKey(varname) || cbitMap.ContainsKey(varname);
    }

    private IEnumerable<Circuit.Qubit> GetQRegister(string varname) {
        return qubitMap[varname];
    }

    private IEnumerable<Circuit.Cbit> GetCRegister(string varname) {
        return cbitMap[varname];
    }

    private Circuit.Qubit GetQubit(string varname) {
        return qubitMap[varname].ElementAt(0);
    }

    private Circuit.Cbit GetCbit(string varname) {
        return cbitMap[varname].ElementAt(0);
    }

    public void VisitBarrier(BarrierContext barrier) {
        throw new System.NotImplementedException();
    }

    public void VisitClassicalIf(IfContext @if) {
        throw new System.NotImplementedException();
    }

    public void VisitDeclaration(DeclContext declaration) {
        if (IsDeclared(declaration.VariableName)) {
            // Throw
            throw new OpenQasmSemanticException(declaration, "Local variable already defined in current scope");
        }

        switch (declaration.Type) {
            case DeclType.Classical: 
                Circuit.Cbit[] register = Circuit.CreateRegister(declaration.Amount);
                cbitMap.Add(declaration.VariableName, register);
                break;
            case DeclType.Quantum:
                Circuit.Qubit[] qubits = Circuit.Allocate(declaration.Amount);
                qubitMap.Add(declaration.VariableName, qubits); 
                break;
        }
    }

    public void VisitGateDeclaration(GateDeclContext declaration) {
        throw new System.NotImplementedException();
    }

    public void VisitOpaqueGateDeclaration(OpaqueGateDeclContext declaration) {
        throw new System.NotImplementedException();
    }

    public void VisitProgram(ProgramContext program) {
        foreach (var stmt in program.Statements) {
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
            }
        }
    }

    public void VisitUnitaryQuantumOperator(UnitaryOperationContext qop) {
        throw new System.NotImplementedException();
    }

    public void VisitMeasurement (MeasurementContext measure) {
        throw new System.NotImplementedException();
    }

    public void VisitReset (ResetContext measure) {
        throw new System.NotImplementedException();
    }

}

}