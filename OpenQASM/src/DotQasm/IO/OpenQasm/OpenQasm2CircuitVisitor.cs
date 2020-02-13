using System.Linq;
using System.Collections.Generic;
using DotQasm.IO.OpenQasm.Ast;
using DotQasm.Scheduling;

namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Visitor for OpenQASM to convert OpenQASM AST to quantum circuits
/// </summary>
public class OpenQasm2CircuitVisitor : IOpenQasmVisitor {

    public Circuit Circuit {get; set;}
    public bool CanEditCircuit => Circuit != null;    

    public OpenQasmSemanticAnalyser Analyser {get; private set;}

    private Dictionary<string, IEnumerable<Qubit>> qubitMap = new Dictionary<string, IEnumerable<Qubit>>();
    private Dictionary<string, IEnumerable<Cbit>> cbitMap = new Dictionary<string, IEnumerable<Cbit>>();

    private Dictionary<string, Gate> pre_declared_gates = new Dictionary<string, Gate>();

    public OpenQasm2CircuitVisitor() {
        this.Circuit = new Circuit();
        this.Analyser = new OpenQasmSemanticAnalyser();
    }

    public OpenQasm2CircuitVisitor(Circuit circuit) {
        this.Circuit = circuit;
        this.Analyser = new OpenQasmSemanticAnalyser();
    }

    protected IEnumerable<Qubit> GetQRegister(string varname) {
        return qubitMap[varname];
    }

    protected IEnumerable<Cbit> GetCRegister(string varname) {
        return cbitMap[varname];
    }

    private Qubit GetQubit(string varname) {
        return qubitMap[varname].ElementAt(0);
    }

    private Cbit GetCbit(string varname) {
        return cbitMap[varname].ElementAt(0);
    }

    private Qubit GetQubit(string varname, int element) {
        return qubitMap[varname].ElementAt(element);
    }

    private Cbit GetCbit(string varname, int element) {
        return cbitMap[varname].ElementAt(element);
    }

    private IEnumerable<Qubit> GetQubitsForArgument(ArgumentContext ctx) {
        if (ctx.IsArrayMember) {
            return new Qubit[]{ GetQubit(ctx.ArgumentName, ctx.ArgumentOffset.Value) };
        } else {
            return GetQRegister(ctx.ArgumentName);
        }
    }

    private IEnumerable<Cbit> GetCbitsForArgument(ArgumentContext ctx) {
        if (ctx.IsArrayMember) {
            return new Cbit[]{ GetCbit(ctx.ArgumentName, ctx.ArgumentOffset.Value) };
        } else {
            return GetCRegister(ctx.ArgumentName);
        }
    }

    private Qubit GetQubitForArgument(ArgumentContext ctx) {
        if (ctx.IsArrayMember) {
            return GetQubit(ctx.ArgumentName, ctx.ArgumentOffset.Value);
        } else {
            return GetQubit(ctx.ArgumentName);
        }
    }

    private Cbit GetCbitForArgument(ArgumentContext ctx) {
        if (ctx.IsArrayMember) {
            return GetCbit(ctx.ArgumentName, ctx.ArgumentOffset.Value);
        } else {
            return GetCbit(ctx.ArgumentName);
        }
    }

    public void VisitDeclaration(DeclContext declaration) {
        Analyser.VisitDeclaration(declaration);

        switch (declaration.Type) {
            case DeclType.Classical: 
                var register = Circuit.AllocateCbits(declaration.Amount);
                cbitMap.Add(declaration.VariableName, register);
                break;
            case DeclType.Quantum:
                var qubits = Circuit.AllocateQubits(declaration.Amount);
                qubitMap.Add(declaration.VariableName, qubits); 
                break;
        }
    }

    public void VisitGateDeclaration(GateDeclContext declaration) {
        Analyser.VisitGateDeclaration(declaration);
    }

    public bool RegisterGate(Gate gate) {
        if (Analyser.IsDeclared(gate.Symbol)) {
            return false;
        } else {
            pre_declared_gates.Add(gate.Symbol, gate);
            Analyser.DeclareExternGate(gate.Symbol);
            return true;
        }
    }

    public void VisitOpaqueGateDeclaration(OpaqueGateDeclContext declaration) {
        Analyser.VisitOpaqueGateDeclaration(declaration);
        throw new System.NotImplementedException();
    }

    public void VisitStatement(StatementContext stmt) {
        Analyser.StatementCount++;
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

    public void VisitProgram(ProgramContext program) {
        foreach (var stmt in program.Statements) {
            VisitStatement(stmt);
        }
    }

    private void ExpandUnitaryQuantumOperatorEvents(UnitaryOperationContext qop, List<IEvent> list, Dictionary<string, double> vars = null, List<ArgumentContext> actualParametres = null) {
        vars = vars ?? new Dictionary<string, double>();
        var declaredParametres = qop.QuantumParametres;

        switch (qop.OperationName) {
            case "U": {
                IEnumerable<double> values = qop.ClassicalParametres.Select((x) => x.Evaluate(vars)); 
                var realGate = Gate.U3(values.ElementAt(0), values.ElementAt(1), values.ElementAt(2));

                // ----
                // See if the gate is one of the "base" gates
                // ----
                /*if (realGate.Equals(Gate.Hadamard)) {
                    realGate = Gate.Hadamard;
                } else if (realGate.Equals(Gate.Identity)) {
                    realGate = Gate.Identity;
                } else if (realGate.Equals(Gate.PauliX)) {
                    realGate = Gate.PauliX;
                } else if (realGate.Equals(Gate.PauliY)) {
                    realGate = Gate.PauliY;
                } else if (realGate.Equals(Gate.PauliZ)) {
                    realGate = Gate.PauliZ;
                }*/
                // ----

                list.Add(new GateEvent(
                    realGate,
                    (actualParametres ?? declaredParametres).SelectMany((x) => GetQubitsForArgument(x))
                ));
            } break;
            case "CX": {
                var qubits = (actualParametres ?? declaredParametres).SelectMany((x) => GetQubitsForArgument(x));
                list.Add(new ControlledGateEvent(
                    Gate.PauliX,
                    qubits.FirstOrDefault(),
                    qubits.Skip(1)
                ));
            } break;
            default: {
                if (pre_declared_gates.ContainsKey(qop.OperationName)) {
                    list.Add(new GateEvent(
                        pre_declared_gates[qop.OperationName],
                            (actualParametres ?? declaredParametres).SelectMany((x) => GetQubitsForArgument(x))
                    ));
                } else {
                    IEnumerable<double> values = qop.ClassicalParametres.Select((x) => x.Evaluate(vars));  
                    Dictionary<string, ArgumentContext> formalActualMap = new Dictionary<string, ArgumentContext>();
                    GateDeclContext gate = Analyser.GetGateDefinition(qop.OperationName);
                    for (int i = 0; i < gate.QuantumArguments.Count; i++) {
                        var formal = gate.QuantumArguments[i];
                        var actual = (actualParametres ?? qop.QuantumParametres)[i];
                        formalActualMap.Add(formal, actual);
                    }

                    foreach (var appl in gate.Operations) {
                        switch (appl) {
                            case UnitaryOperationContext uop: {
                                // Create new values for passing onto next func
                                Dictionary<string, double> newVars = new Dictionary<string, double>();
                            
                                foreach (var (value, key) in values.Select((x, y) => (x, y))) {
                                    if (key < gate.ClassicalArguments.Count)
                                        newVars.Add(gate.ClassicalArguments[key], value);
                                }

                                // Construct actual parametre list
                                var actual = uop.QuantumParametres.Select((x) => formalActualMap[x.ArgumentName]).ToList();

                                // Can be a unitary operator or a barrier
                                ExpandUnitaryQuantumOperatorEvents(uop, list, newVars, actual);
                            } break;
                            case BarrierContext barrier: {
                                list.Add(GetBarrierEvent(barrier));
                            } break;
                            default: {
                                throw new System.Exception("Unsupported gate operation");
                            }
                        }
                    }
                }
            } break;
        }
    }

    public void VisitUnitaryQuantumOperator(UnitaryOperationContext qop) {
        Analyser.VisitUnitaryQuantumOperator(qop);

        List<IEvent> events = new List<IEvent>();
        ExpandUnitaryQuantumOperatorEvents(qop, events);
        foreach (var evt in events) {
           Circuit.GateSchedule.ScheduleEvent(evt);
        }

    }

    private IEvent GetMeasurementEvent(MeasurementContext measure) {
        var qubits = GetQubitsForArgument(measure.QuatumArgument);
        var cbits = GetCbitsForArgument(measure.ClassicalArgument);

        return new MeasurementEvent(
            cbits, 
            qubits
        );
    }

    public void VisitMeasurement (MeasurementContext measure) {
        Analyser.VisitMeasurement(measure);

        Circuit.GateSchedule.ScheduleEvent(
            GetMeasurementEvent(measure)
        );
    }

    private IEvent GetResetEvent(ResetContext reset) {
        IEnumerable<Qubit> qubits = GetQubitsForArgument(reset.Argument);

        return new ResetEvent(qubits);
    }

    public void VisitReset (ResetContext reset) {
        Analyser.VisitReset(reset);

        Circuit.GateSchedule.ScheduleEvent(
            GetResetEvent(reset)
        );
    }

    private IEvent GetBarrierEvent(BarrierContext barrier) {
        IEnumerable<Qubit> qubits = barrier.Arguments.SelectMany((x) => GetQubitsForArgument(x));
        return new BarrierEvent(qubits);
    }

    public void VisitBarrier(BarrierContext barrier) {
        Analyser.VisitBarrier(barrier);

        Circuit.GateSchedule.ScheduleEvent(
            GetBarrierEvent(barrier)
        );
    }

    public void VisitClassicalIf(IfContext @if) {
        Analyser.VisitClassicalIf(@if);

        switch (@if.Operation) {
            case MeasurementContext context: {
                Circuit.GateSchedule.ScheduleEvent(
                    new IfEvent(
                        GetCRegister(@if.ClassicalVariableName),
                        @if.ClassicalVariableValue,
                        GetMeasurementEvent(context)
                    )
                );
                break;
            }
            case ResetContext context: {
                Circuit.GateSchedule.ScheduleEvent(
                    new IfEvent(
                        GetCRegister(@if.ClassicalVariableName),
                        @if.ClassicalVariableValue,
                        GetResetEvent(context)
                    )
                );
                break;
            }
            case UnitaryOperationContext context: {
                List<IEvent> events = new List<IEvent>();
                ExpandUnitaryQuantumOperatorEvents(context, events);
                foreach (var evt in events) {
                    Circuit.GateSchedule.ScheduleEvent(
                        new IfEvent(
                            GetCRegister(@if.ClassicalVariableName), 
                            @if.ClassicalVariableValue, 
                            evt
                        )
                    );
                }
                break;
            }
            default: {
                throw new System.Exception("Unsupported operation in IF statement");
            }
        }
    }

}

}