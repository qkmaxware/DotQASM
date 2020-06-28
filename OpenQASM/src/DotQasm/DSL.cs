using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace DotQasm {

/// <summary>
/// Domain specific language for manipulating quantum circuits
/// </summary>
public static class DSL {
    // -----------------------------------------------------------------------------------------------
    // DSL commands for easily manipulating quantum circuits
    // -----------------------------------------------------------------------------------------------
    /// <summary>
    /// Apply a specific single qubit gate 
    /// </summary>
    /// <param name="qubit">qubit to apply to</param>
    /// <param name="gate">gate to apply</param>
    public static void Apply(this Qubit qubit, Gate gate) {
        var evt = new Scheduling.GateEvent(gate, qubit);
        qubit.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }

    /// <summary>
    /// Apply a gate to a qubit only if the given classical bit is set
    /// </summary>
    /// <param name="qubit">qubit to apply to</param>
    /// <param name="control">classical control</param>
    /// <param name="gate">gate to apply</param>
    public static void IfApply(this Qubit qubit, Cbit control, Gate gate) {
        var evt = new Scheduling.GateEvent(gate, qubit);
        var iff = new Scheduling.IfEvent( new Cbit[]{ control }, 1, evt );
        qubit.ParentCircuit?.GateSchedule?.ScheduleEvent(iff);
    }   

    /// <summary>
    /// Apply a specific gate to the target qubit if the control qubit is set
    /// </summary>
    /// <param name="control">control qubit</param>
    /// <param name="target">target qubit</param>
    /// <param name="gate">gate to apply</param>
    public static void ControlledApply(this Qubit control, Qubit target, Gate gate) {
        /*
        gate cu3(theta,phi,lambda) c, t
        {
            // implements controlled-U(theta,phi,lambda) with  target t and control c
            u1((lambda-phi)/2) t;
            cx c,t;
            u3(-theta/2,0,-(phi+lambda)/2) t;
            cx c,t;
            u3(theta/2,phi,0) t;
        }
        */
        target.U1((gate.Parametres.Lambda - gate.Parametres.Phi) / 2);
        control.CX(target);
        target.U3(-gate.Parametres.Theta/2, 0, -(gate.Parametres.Phi + gate.Parametres.Lambda) / 2);
        control.CX(target);
        target.U3(gate.Parametres.Theta/2, gate.Parametres.Phi, 0);
    }
    
    /// <summary>
    /// Measure a qubit and put the result in the given classical bit
    /// </summary>
    /// <param name="qubit">qubit to measure</param>
    /// <param name="cbit">cbit to receive the result</param>
    public static void Measure (this Qubit qubit, Cbit cbit) {
        var evt = new Scheduling.MeasurementEvent(new Cbit[]{ cbit }, new Qubit[]{ qubit });
        qubit.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    
    /// <summary>
    /// Reset a qubit
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Reset(this Qubit qubit) {
        var evt = new Scheduling.ResetEvent(new Qubit[]{ qubit });
        qubit.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Reset several qubits
    /// </summary>
    /// <param name="qubits">qubits</param>
    public static void Reset(this IEnumerable<Qubit> qubits) {
        var evt = new Scheduling.ResetEvent(qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Put a barrier on a qubit
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Barrier(this Qubit qubit) {
        var evt = new Scheduling.BarrierEvent(new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Put a barrier across several qubits
    /// </summary>
    /// <param name="qubits">qubits</param>
    public static void Barrier(this IEnumerable<Qubit> qubits) {
        var evt = new Scheduling.BarrierEvent(qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the hadamard gate 
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void H(this Qubit qubit) {
        var evt = new Scheduling.GateEvent(Gate.Hadamard, new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the hadamard gate 
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void H(this IEnumerable<Qubit> qubits) {
        var evt = new Scheduling.GateEvent(Gate.Hadamard, qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the pauli x gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void X(this Qubit qubit) {
        var evt = new Scheduling.GateEvent(Gate.PauliX, new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the pauli x gate
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void X(this IEnumerable<Qubit> qubits) {
        var evt = new Scheduling.GateEvent(Gate.PauliX, qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the pauli y gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Y(this Qubit qubit) {
        var evt = new Scheduling.GateEvent(Gate.PauliY, new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the pauli y gate
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void Y(this IEnumerable<Qubit> qubits) {
        var evt = new Scheduling.GateEvent(Gate.PauliY, qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the pauli z gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Z(this Qubit qubit) {
        var evt = new Scheduling.GateEvent(Gate.PauliZ, new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the pauli z gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Z(this IEnumerable<Qubit> qubits) {
        var evt = new Scheduling.GateEvent(Gate.PauliZ, qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the identity gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void I(this Qubit qubit) {
        var evt = new Scheduling.GateEvent(Gate.Identity, new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply the identity gate
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void I(this IEnumerable<Qubit> qubits) {
        var evt = new Scheduling.GateEvent(Gate.Identity, qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an x rotation
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Rx(this Qubit qubit, double angle) {
        var evt = new Scheduling.GateEvent(Gate.Rx(angle), new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an x rotation
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void Rx(this IEnumerable<Qubit> qubits, double angle) {
        var evt = new Scheduling.GateEvent(Gate.Rx(angle), qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an y rotation
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Ry(this Qubit qubit, double angle) {
        var evt = new Scheduling.GateEvent(Gate.Ry(angle), new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an y rotation
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void Ry(this IEnumerable<Qubit> qubits, double angle) {
        var evt = new Scheduling.GateEvent(Gate.Ry(angle), qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an z rotation
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Rz(this Qubit qubit, double angle) {
        var evt = new Scheduling.GateEvent(Gate.Rz(angle), new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an z rotation
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void Rz(this IEnumerable<Qubit> qubits, double angle) {
        var evt = new Scheduling.GateEvent(Gate.Rz(angle), qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an u1 gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void U1(this Qubit qubit, double lambda) {
        var evt = new Scheduling.GateEvent(Gate.U1(lambda), new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an u1 gate
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void U1(this IEnumerable<Qubit> qubits, double lambda) {
        var evt = new Scheduling.GateEvent(Gate.U1(lambda), qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an u2 gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void U2(this Qubit qubit, double phi, double lambda) {
        var evt = new Scheduling.GateEvent(Gate.U2(phi, lambda), new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an u2 gate
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void U2(this IEnumerable<Qubit> qubits, double phi, double lambda) {
        var evt = new Scheduling.GateEvent(Gate.U2(phi, lambda), qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an u3 gate
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void U3(this Qubit qubit, double theta, double phi, double lambda) {
        var evt = new Scheduling.GateEvent(Gate.U3(theta, phi, lambda), new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply an u3 gate
    /// </summary>
    /// <param name="qubit">qubits</param>
    public static void U3(this IEnumerable<Qubit> qubits, double theta, double phi, double lambda) {
        var evt = new Scheduling.GateEvent(Gate.U3(theta, phi, lambda), qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply a controlled not gate
    /// </summary>
    /// <param name="control">control qubit</param>
    /// <param name="target">target qubit</param>
    public static void CX(this Qubit control, Qubit target) {
        var evt = new Scheduling.ControlledGateEvent(Gate.PauliX, control, new Qubit[]{ target });
        control.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    /// <summary>
    /// Apply a controlled not gate
    /// </summary>
    /// <param name="control">control qubit</param>
    /// <param name="targets">target qubit register</param>
    public static void CX(this Qubit control, IEnumerable<Qubit> targets) {
        var evt = new Scheduling.ControlledGateEvent(Gate.PauliX, control, targets);
        control.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }

    /// <summary>
    /// Apply the swap gate (3 CX gates in a row)
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Swap(this Qubit qubit, Qubit other) {
        qubit.CX(other);
        other.CX(qubit);
        qubit.CX(other);
    }

}

}