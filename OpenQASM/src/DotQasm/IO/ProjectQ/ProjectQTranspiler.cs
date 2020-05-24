using System;
using System.Linq;
using System.Text;
using DotQasm.Scheduling;

namespace DotQasm.IO.ProjectQ {

/// <summary>
/// Compiler from OpenQASM circuits to ProjectQ source code
/// </summary>
public class ProjectQTranspiler : IFileConverter<Circuit, string> {

    public string FormatName => "ProjectQ";
    public string FormatExtension => "py";

    private static string tab = "    ";

    public string Convert(Circuit circuit) {
        StringBuilder sb = new StringBuilder();

        // Imports
        sb.AppendLine("from projectq           import MainEngine               # Import main compiler engine");
        sb.AppendLine("from projectq.ops       import Rx, Ry,Rz, CNOT, Measure # Import quantum operations");
        sb.AppendLine("from projectq.backends  import Simulator                # Import quantum simulator");
        sb.AppendLine("import projectq.setups                                  # Import premade compiler engine lists");
        sb.AppendLine();

        // Init
        sb.AppendLine("eng = MainEngine(backend=Simulator())");
        sb.AppendLine();

        sb.AppendLine("def U3(_theta, _phi, _lambda, qubit):");
        sb.AppendLine(tab + "Rz(_phi)    | qubit");
        sb.AppendLine(tab + "Ry(_theta)  | qubit");
        sb.AppendLine(tab + "Rz(_lambda) | qubit");
        sb.AppendLine();

        sb.AppendLine("def U1(_lambda: Double, qubit: Qubit):");
        sb.AppendLine(tab + "U3(0, 0, lambda, qubit)");
        sb.AppendLine();

        sb.AppendLine("def CU3(_theta, _phi, _lambda, control, target):");
        sb.AppendLine(tab + "U1((_lambda-_phi)/2, target)");
        sb.AppendLine(tab + "CNOT | (control, target)");
        sb.AppendLine(tab + "U3(-_theta/2, 0, -(_phi+_lambda)/2, target)");
        sb.AppendLine(tab + "CNOT | (control, target)");
        sb.AppendLine(tab + "U3(_theta/2, _phi, 0, target)");
        sb.AppendLine();

        sb.AppendLine("def Reset(qubit):");
        sb.AppendLine(tab + "Measure | qubit");
        sb.AppendLine(tab + "eng.flush()");
        sb.AppendLine(tab + "val = int(qubit)");
        sb.AppendLine(tab + "if val:");
        sb.AppendLine(tab + tab + "X | qubit");
        sb.AppendLine();

        sb.AppendLine("def circuit():");
        sb.AppendLine(tab + $"qreg =  eng.allocate_qureg({circuit.QubitCount})");
        sb.AppendLine(tab + $"creg = [0] * {circuit.BitCount}");
        foreach (var statement in circuit.GateSchedule) {
            EncodeStatement(sb, statement);
        }
        sb.AppendLine(tab + "eng.flush()");
        sb.AppendLine();

        // Cleanup
        sb.AppendLine("if __name__ == \"__main__\":");
        sb.AppendLine(tab + "circuit()");

        return sb.ToString();
    }

    private void EncodeStatement(StringBuilder sb, IEvent statement) {
        switch (statement) {
            case BarrierEvent barrierEvent: break;
            case GateEvent gateEvent:
                foreach (var qubit in gateEvent.QuantumDependencies) {
                    sb.AppendLine(tab + $"U3({gateEvent.Operator.Parametres.Item1}, {gateEvent.Operator.Parametres.Item2}, {gateEvent.Operator.Parametres.Item3}, qreg[{qubit.QubitId}])");
                }
                break;
            case ControlledGateEvent controlledGate: 
                foreach (var qubit in controlledGate.TargetQubits) {
                    sb.AppendLine(tab + $"CU3({controlledGate.Operator.Parametres.Item1}, {controlledGate.Operator.Parametres.Item2}, {controlledGate.Operator.Parametres.Item3}, qreg[{controlledGate.ControlQubit.QubitId}], qreg[{qubit.QubitId}])");
                }
                break;
            case IfEvent ifEvent: // TODO handle this correctly (convert register to number)
                sb.AppendLine(tab + $"if {ifEvent.ClassicalDependencies} == {ifEvent.LiteralValue}:");
                sb.Append(tab); EncodeStatement(sb, ifEvent.Event);
                break;
            case MeasurementEvent measurement:
                foreach (var measure in measurement.QuantumDependencies.Zip(measurement.ClassicalDependencies, (qubit, cbit) => new { Qubit = qubit, Cbit = cbit})) {
                    sb.AppendLine(tab + $"Measure | qreg[{measure.Qubit.QubitId}]");
                    sb.AppendLine(tab + "eng.flush()");
                    sb.AppendLine(tab + $"creg[{measure.Cbit.ClassicalBitId}] = int(qreg[{measure.Qubit.QubitId}])");
                }
                break;
            case ResetEvent reset:
                foreach (var qubit in reset.QuantumDependencies) {
                    sb.AppendLine(tab + $"Reset(qreg[{qubit.QubitId}])");
                }
                break;
            default:
                throw new InvalidOperationException(statement.GetType() + " is not supported by " + this.GetType());
        }
    }

}

}