using System;
using System.Linq;
using System.Text;
using DotQasm.Scheduling;

namespace DotQasm.IO.QSharp {

public class QSharpTranspiler : IFileConverter<Circuit, string> {
    public string FormatName => "q#";
    public string FormatExtension => "qs";

    private static string tab = "    ";

    public string Convert(Circuit circuit) {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("// Code generated from DotQasm");
        sb.AppendLine("namespace OpenQASM {");

        sb.AppendLine("open Microsoft.Quantum.Intrinsic as Gates;");
        sb.AppendLine("open Microsoft.Quantum.Math as Math;");
        sb.AppendLine();

        sb.AppendLine("operation U1(lambda: Double, qubit: Qubit) {");
        sb.AppendLine(tab + "U3(0, 0, lambda, qubit);");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("operation U3(theta: Double, phi: Double, lambda: Double, qubit: Qubit) {");
        sb.AppendLine(tab + "Rz(phi, qubit); Ry(theta, qubit); Rz(lambda, qubit);");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("operation CU3(theta: Double, phi: Double, lambda: Double, control: Qubit, target: Qubit) {");
        sb.AppendLine(tab + "U1((lambda-phi)/2, target);");
        sb.AppendLine(tab + "Gates.CNOT(control, target);");
        sb.AppendLine(tab + "U3(-theta/2, 0, -(phi+lambda)/2, target);");
        sb.AppendLine(tab + "Gates.CNOT(control, target);");
        sb.AppendLine(tab + "U3(theta/2, phi, 0, target);");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("operation Circuit() : Unit {");
        sb.AppendLine(tab + $"mutable creg = new Result[{circuit.BitCount}];");
        sb.AppendLine(tab + $"using(qreg = Qubit[{circuit.QubitCount}]) {{");
        foreach (var statement in circuit.GateSchedule) {
            EncodeStatement(sb, statement);
        }
        sb.AppendLine(tab + "}");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.Append("}");

        return sb.ToString();
    }

    private void EncodeStatement(StringBuilder sb, IEvent statement) {
        switch (statement) {
            case BarrierEvent barrierEvent: break;
            case GateEvent gateEvent:
                foreach (var qubit in gateEvent.QuantumDependencies) {
                    sb.AppendLine(tab + tab + $"U3({gateEvent.Operator.Parametres.Item1}, {gateEvent.Operator.Parametres.Item2}, {gateEvent.Operator.Parametres.Item3}, qreg[{qubit.QubitId}]);");
                }
                break;
            case ControlledGateEvent controlledGate: 
                foreach (var qubit in controlledGate.TargetQubits) {
                    sb.AppendLine(tab + tab + $"CU3({controlledGate.Operator.Parametres.Item1}, {controlledGate.Operator.Parametres.Item2}, {controlledGate.Operator.Parametres.Item3}, qreg[{controlledGate.ControlQubit.QubitId}], qreg[{qubit.QubitId}]);");
                }
                break;
            case IfEvent ifEvent: // TODO handle this correctly (convert register to number)
                sb.AppendLine(tab + tab + $"if ({ifEvent.ClassicalDependencies} == {ifEvent.LiteralValue}) {{");
                sb.Append(tab); EncodeStatement(sb, ifEvent.Event);
                sb.AppendLine(tab + tab + "}");
                break;
            case MeasurementEvent measurement:
                foreach (var measure in measurement.QuantumDependencies.Zip(measurement.ClassicalDependencies, (qubit, cbit) => new { Qubit = qubit, Cbit = cbit})) {
                    sb.AppendLine(tab + tab + $"set creg w/= {measure.Cbit.ClassicalBitId} <- Gates.M(qreg[{measure.Qubit.QubitId}]);");
                }
                break;
            case ResetEvent reset:
                foreach (var qubit in reset.QuantumDependencies) {
                    sb.AppendLine(tab + tab + $"Gates.Reset(qreg[{qubit.QubitId}]);");
                }
                break;
            default:
                throw new InvalidOperationException(statement.GetType() + " is not supported by " + this.GetType());
        }
    }
}

}