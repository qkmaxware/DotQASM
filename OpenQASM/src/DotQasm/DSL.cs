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

    /// <summary>
    /// Create an imaginary number from a scalar value
    /// </summary>
    /// <param name="value">scalar value</param>
    /// <returns>imaginary number</returns>
    public static Complex i(this IConvertible value) {
        return new Complex(0, value.ToDouble(System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Modulus squared magnitude of complex vector
    /// </summary>
    /// <param name="value">complex vector</param>
    /// <returns>squared magnitude</returns>
    public static double SqrMagnitude(this Complex value) {
        return value.Real * value.Real + value.Imaginary * value.Imaginary;
    }

    // -----------------------------------------------------------------------------------------------
    // DSL commands for easily manipulating quantum circuits
    // -----------------------------------------------------------------------------------------------
    /// <summary>
    /// Reset a qubit
    /// </summary>
    /// <param name="qubit">qubit</param>
    public static void Reset(this Qubit qubit) {
        var evt = new Scheduling.ResetEvent(new Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
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