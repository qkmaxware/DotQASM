using System;
using System.Linq;
using System.Text;
using DotQasm.Scheduling;

namespace DotQasm.IO.Qiskit {

/// <summary>
/// Compiler from OpenQASM circuits to QIS kit source code
/// </summary>
public class QiskitTranspiler : IFileConverter<Circuit, string> {

    public string FormatName => "QISkit";
    public string FormatExtension => "py";

    private static string tab = "    ";

    public string Convert(Circuit circuit) {
        StringBuilder sb = new StringBuilder();

        // Imports
        sb.AppendLine("# Code generated from DotQasm");
        sb.AppendLine("from qiskit import QuantumRegister, ClassicalRegister, QuantumCircuit, Aer, execute");
        sb.AppendLine();

        // Init
        sb.AppendLine("def backend():");
        sb.AppendLine(tab + "return Aer.backends(name='statevector_simulator')[0]");
        sb.AppendLine();

        sb.AppendLine("def circuit():");
        sb.AppendLine(tab + $"qreg =  QuantumRegister({circuit.QubitCount})");
        sb.AppendLine(tab + $"creg = ClassicalRegister({circuit.BitCount})");
        sb.AppendLine(tab + "circ = QuantumCircuit(qreg, creg)");
        sb.AppendLine();
        foreach (var statement in circuit.GateSchedule) {
            EncodeStatement(sb, statement);
        }
        sb.AppendLine();
        sb.AppendLine(tab + "job = execute(circ, backend())");
        sb.AppendLine(tab + "result = job.result()");
        sb.AppendLine(tab + "return result");
        sb.AppendLine();

        // Cleanup
        sb.AppendLine("if __name__ == \"__main__\":");
        sb.AppendLine(tab + "result = circuit()");
        sb.AppendLine(tab + "print(result.get_statevector())");


        return sb.ToString();
    }

    private void EncodeStatement(StringBuilder sb, IEvent statement) {
        switch (statement) {
            case BarrierEvent barrierEvent: break;
            case GateEvent gateEvent:
                foreach (var qubit in gateEvent.QuantumDependencies) {
                    sb.AppendLine(tab + $"circ.u3({gateEvent.Operator.Parametres.Item1}, {gateEvent.Operator.Parametres.Item2}, {gateEvent.Operator.Parametres.Item3}, qreg[{qubit.QubitId}])");
                }
                break;
            case ControlledGateEvent controlledGate: 
                foreach (var qubit in controlledGate.TargetQubits) {
                    sb.AppendLine(tab + $"circ.cu3({controlledGate.Operator.Parametres.Item1}, {controlledGate.Operator.Parametres.Item2}, {controlledGate.Operator.Parametres.Item3}, qreg[{controlledGate.ControlQubit.QubitId}], qreg[{qubit.QubitId}])");
                }
                break;
            case IfEvent ifEvent: // TODO handle this correctly (convert register to number)
                sb.AppendLine(tab + $"if {ifEvent.ClassicalDependencies} == {ifEvent.LiteralValue}:");
                sb.Append(tab); EncodeStatement(sb, ifEvent.Event);
                break;
            case MeasurementEvent measurement:
                foreach (var measure in measurement.QuantumDependencies.Zip(measurement.ClassicalDependencies, (qubit, cbit) => new { Qubit = qubit, Cbit = cbit})) {
                    sb.AppendLine(tab + $"circ.measure(qreg[{measure.Qubit.QubitId}], creg[{measure.Cbit.ClassicalBitId}])");
                }
                break;
            case ResetEvent reset:
                foreach (var qubit in reset.QuantumDependencies) {
                    sb.AppendLine(tab + $"circ.reset(qreg[{qubit.QubitId}])");
                }
                break;
            default:
                throw new InvalidOperationException(statement.GetType() + " is not supported by " + this.GetType());
        }
    }

}

}