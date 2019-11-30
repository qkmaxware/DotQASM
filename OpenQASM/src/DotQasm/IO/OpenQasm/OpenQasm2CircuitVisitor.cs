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

    private OpenQasmSemanticAnalyser Semantics = new OpenQasmSemanticAnalyser();

    private Dictionary<string, IEnumerable<Circuit.Qubit>> qubitMap = new Dictionary<string, IEnumerable<Circuit.Qubit>>();
    private Dictionary<string, IEnumerable<Circuit.Cbit>> cbitMap = new Dictionary<string, IEnumerable<Circuit.Cbit>>();

    public OpenQasm2CircuitVisitor(Circuit circuit) {
        this.Circuit = circuit;
    }

    protected IEnumerable<Circuit.Qubit> GetQRegister(string varname) {
        return qubitMap[varname];
    }

    protected IEnumerable<Circuit.Cbit> GetCRegister(string varname) {
        return cbitMap[varname];
    }

    private Circuit.Qubit GetQubit(string varname) {
        return qubitMap[varname].ElementAt(0);
    }

    private Circuit.Cbit GetCbit(string varname) {
        return cbitMap[varname].ElementAt(0);
    }

    public void VisitDeclaration(DeclContext declaration) {
        Semantics.VisitDeclaration(declaration);

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
        Semantics.VisitGateDeclaration(declaration);
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
        Semantics.VisitUnitaryQuantumOperator(qop);
        throw new System.NotImplementedException();
        // TODO add to circuit schedule, expand operator if need be
    }

    public void VisitMeasurement (MeasurementContext measure) {
        Semantics.VisitMeasurement(measure);
        throw new System.NotImplementedException();
        // TODO add to circuit schedule
    }

    public void VisitReset (ResetContext reset) {
        Semantics.VisitReset(reset);
        throw new System.NotImplementedException();
        // TODO add to circuit schedule
    }

    public void VisitBarrier(BarrierContext barrier) {
        Semantics.VisitBarrier(barrier);
        throw new System.NotImplementedException();
        // TODO add to circuit schedule
    }

    public void VisitClassicalIf(IfContext @if) {
        Semantics.VisitClassicalIf(@if);
        throw new System.NotImplementedException();
        //TODO add to circuit schedule
    }

}

}