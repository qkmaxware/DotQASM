using System;
using System.IO;
using System.Linq;
using DotQasm.Scheduling;
using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm {

public class OpenQasmEmitter: IEmitter<Circuit> {

    private static string space = " ";
    private static string stop = ";";

    public static void EmitCircuit(Circuit circuit, TextWriter writer) {
        (new OpenQasmEmitter()).Emit(circuit, writer);
    }

    private string ConvertQubit(Qubit qubit) {
        return "qubits" + qubit.Owner.RegisterId + "[" + qubit.QubitId + "]";
    }

    private string ConvertCbit(Cbit qubit) {
        return "bits" + qubit.Owner.RegisterId + "[" + qubit.ClassicalBitId + "]";
    }

    private string ConvertCreg(Cbit qubit) {
        return "bits" + qubit.Owner.RegisterId;
    }

    private string ConvertQubitList(IEnumerable<Qubit> qubits) {
        var names = qubits.Select((qubit) => ConvertQubit(qubit));
        return string.Join(',', names);
    }

    private void EmitBarrier(BarrierEvent evt, TextWriter writer) {
        foreach (var qubit in evt.QuantumDependencies) {
            writer.Write("barrier ");
            writer.Write(ConvertQubit(qubit));
            writer.WriteLine(stop);
        }
    }

    private void EmitControlledGate(ControlledGateEvent evt, TextWriter writer) {
        if (evt.Operator == Gate.PauliX) {
            foreach (var qubit in evt.TargetQubits) {
                writer.Write("cx ");
                writer.Write(ConvertQubit(evt.ControlQubit));
                writer.Write(',');
                writer.Write(space);
                writer.Write(ConvertQubit(qubit));
                writer.WriteLine(stop);
            }
        } else {
            throw new InvalidOperationException("controlled-" + evt.Operator.Symbol + " is not supported by OpenQASM 2.0");
        }
    }

    private void EmitGate(GateEvent evt, TextWriter writer) {
        foreach (var qubit in evt.QuantumDependencies) {
            // Write name
            writer.Write("u3"); // all gates are u3 representable

            // Write params (if any)
            writer.Write(evt.Operator.Parametres);
            writer.Write(space);

            // Write args (if any) (comma separated list of qubits)
            writer.Write(ConvertQubit(qubit));

            writer.WriteLine(stop);
        }
    }

    private void EmitIf(IfEvent evt, TextWriter writer) {
        // Measurement events and IFs use register ids (or id[index]) for qubits, we only have one register
        writer.Write("if(");
        writer.Write(ConvertCreg(evt.ClassicalDependencies.First()));
        writer.Write("==");
        writer.Write(evt.LiteralValue);
        writer.Write(") ");
        Emit(evt.Event, writer);
    }

    private IEnumerable<Register<Qubit>> GetWholeRegisters(IEnumerable<Qubit> qubits) {
        var potentialRegisters = qubits.Select(qubit => qubit.Owner).Distinct();
        return potentialRegisters.Where(reg => reg.All(qubit => qubits.Contains(qubit)));
    }
    private IEnumerable<Qubit> GetIndividualQubits(IEnumerable<Register<Qubit>> registers, IEnumerable<Qubit> qubits) {
        return qubits.Where(qubit => registers.Any(reg => reg.Contains(qubit)));
    }

    private void EmitMeasure(MeasurementEvent evt, TextWriter writer) {
        // Measurement events and IFs use register ids (or id[index]) for qubits, we only have one register
        //var registersToMeasure  = GetWholeRegisters(evt.QuantumDependencies);
        //var qubitsToMeasure     = GetIndividualQubits(registersToMeasure, evt.QuantumDependencies);
        for (int i = 0; i < evt.QuantumDependencies.Count(); i++) {
            writer.Write("measure ");
            writer.Write(ConvertQubit(evt.QuantumDependencies.ElementAt(i)));
            writer.Write(" -> ");
            writer.Write(ConvertCbit(evt.ClassicalDependencies.ElementAt(i))); 
            writer.WriteLine(stop);
        }
    }

    private void EmitReset(ResetEvent evt, TextWriter writer) {
        writer.Write("reset ");
        writer.Write(ConvertQubitList(evt.QuantumDependencies));
        writer.WriteLine(stop);
    }

    private void Emit(IEvent evt, TextWriter writer) {
        switch (evt) {
            case BarrierEvent be: {
                EmitBarrier(be, writer);
            } break;
            case ControlledGateEvent cge: {
                EmitControlledGate(cge, writer);
            } break;
            case GateEvent ge: {
                EmitGate(ge, writer);
            } break;
            case IfEvent ife: {
                EmitIf(ife, writer);
            } break;
            case MeasurementEvent me: {
                EmitMeasure(me, writer);
            } break;
            case ResetEvent re: {
                EmitReset(re, writer);
            } break;
            default: {
                throw new InvalidOperationException(evt.GetType() + " is not supported by OpenQASM 2.0");
            }
        }
    }

    public void Emit(Circuit circuit, TextWriter writer) {
        // TODO actually emit a quantum circuit
        writer.WriteLine("OPENQASM 2.0;");
        writer.WriteLine("include \"qelib1.inc\";");
        writer.WriteLine();

        foreach (var reg in circuit.QuantumRegisters) {
            writer.WriteLine("qreg qubits" + reg.RegisterId + "[" + reg.Count + "];");
        }
        foreach (var reg in  circuit.ClassicalRegisters) {
            writer.WriteLine("creg bits" + reg.RegisterId + "[" + reg.Count + "];");
        }
        writer.WriteLine();

        // Go over schedule and output the correct instructions
        foreach (var evt in circuit.GateSchedule) {
            Emit(evt, writer);
        }
    }
}

}