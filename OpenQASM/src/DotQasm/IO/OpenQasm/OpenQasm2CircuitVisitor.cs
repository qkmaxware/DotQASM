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

    private Dictionary<string, IEnumerable<Circuit.Qubit>> qubitMap = new Dictionary<string, IEnumerable<Circuit.Qubit>>();
    private Dictionary<string, IEnumerable<Circuit.Cbit>> cbitMap = new Dictionary<string, IEnumerable<Circuit.Cbit>>();

    public OpenQasm2CircuitVisitor() {
        this.Circuit = new Circuit();
        this.Analyser = new OpenQasmSemanticAnalyser();
    }

    public OpenQasm2CircuitVisitor(Circuit circuit) {
        this.Circuit = circuit;
        this.Analyser = new OpenQasmSemanticAnalyser();
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

    private Circuit.Qubit GetQubit(string varname, int element) {
        return qubitMap[varname].ElementAt(element);
    }

    private Circuit.Cbit GetCbit(string varname, int element) {
        return cbitMap[varname].ElementAt(element);
    }

    private IEnumerable<Circuit.Qubit> GetQubitsForArgument(ArgumentContext ctx) {
        if (ctx.IsArrayMember) {
            return new Circuit.Qubit[]{ GetQubit(ctx.ArgumentName, ctx.ArgumentOffset.Value) };
        } else {
            return GetQRegister(ctx.ArgumentName);
        }
    }

    private IEnumerable<Circuit.Cbit> GetCbitsForArgument(ArgumentContext ctx) {
        if (ctx.IsArrayMember) {
            return new Circuit.Cbit[]{ GetCbit(ctx.ArgumentName, ctx.ArgumentOffset.Value) };
        } else {
            return GetCRegister(ctx.ArgumentName);
        }
    }

    private Circuit.Qubit GetQubitForArgument(ArgumentContext ctx) {
        if (ctx.IsArrayMember) {
            return GetQubit(ctx.ArgumentName, ctx.ArgumentOffset.Value);
        } else {
            return GetQubit(ctx.ArgumentName);
        }
    }

    private Circuit.Cbit GetCbitForArgument(ArgumentContext ctx) {
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
        Analyser.VisitGateDeclaration(declaration);
    }

    public void VisitOpaqueGateDeclaration(OpaqueGateDeclContext declaration) {
        Analyser.VisitOpaqueGateDeclaration(declaration);
        throw new System.NotImplementedException();
    }

    public void VisitProgram(ProgramContext program) {
        foreach (var stmt in program.Statements) {
            Analyser.InstructionCount++;
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

    private void ExpandUnitaryQuantumOperatorEvents(UnitaryOperationContext qop, List<IEvent> list, Dictionary<string, double> vars = null) {
        // TODO add to circuit schedule, expand operator if need be
        vars = vars ?? new Dictionary<string, double>();
        switch (qop.OperationName) {
            case "U": {
                IEnumerable<double> values = qop.ClassicalParametres.Select((x) => x.Evaluate(vars)); 

                list.Add(new GateEvent(
                    Gate.U(values.ElementAt(0), values.ElementAt(1), values.ElementAt(2)),
                    qop.QuantumParametres.SelectMany((x) => GetQubitsForArgument(x))
                ));
            } break;
            case "CX": {
                list.Add(new GateEvent(
                    Gate.CNot,
                    qop.QuantumParametres.SelectMany((x) => GetQubitsForArgument(x))
                ));
            } break;
            default: {
                GateDeclContext gate = Analyser.GetGateDefinition(qop.OperationName);
                IEnumerable<double> values = qop.ClassicalParametres.Select((x) => x.Evaluate(vars));  

                foreach (var appl in gate.Operations) {
                    switch (appl) {
                        case UnitaryOperationContext uop: {
                            // Create new values for passing onto next func
                            //Dictionary<string, double> newVars;
                            
                            //TODO

                            // Can be a unitary operator or a barrier
                            //ExpandUnitaryQuantumOperatorEvents(uop, list, newVars);
                        } break;
                        case BarrierContext barrier: {
                            list.Add(GetBarrierEvent(barrier));
                        } break;
                        default: {
                            throw new System.Exception("Unsupported gate operation");
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
        IEnumerable<Circuit.Qubit> qubits = GetQubitsForArgument(reset.Argument);

        return new ResetEvent(qubits);
    }

    public void VisitReset (ResetContext reset) {
        Analyser.VisitReset(reset);

        Circuit.GateSchedule.ScheduleEvent(
            GetResetEvent(reset)
        );
    }

    private IEvent GetBarrierEvent(BarrierContext barrier) {
        IEnumerable<Circuit.Qubit> qubits = barrier.Arguments.SelectMany((x) => GetQubitsForArgument(x));
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
                    GetMeasurementEvent(context)
                );
                break;
            }
            case ResetContext context: {
                Circuit.GateSchedule.ScheduleEvent(
                    GetResetEvent(context)
                );
                break;
            }
            case UnitaryOperationContext context: {
                List<IEvent> events = new List<IEvent>();
                ExpandUnitaryQuantumOperatorEvents(context, events);
                foreach (var evt in events) {
                    Circuit.GateSchedule.ScheduleEvent(
                        new IfEvent(GetCbit(@if.ClassicalVariableName), @if.ClassicalVariableValue, evt)
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