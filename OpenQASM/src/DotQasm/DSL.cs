using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace DotQasm {

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

    /*
    // DSL commands for easily manipulating quantum circuits
    public static void H(this Circuit.Qubit qubit) {
        var evt = new Scheduling.GateEvent(Gate.Hadamard, new Circuit.Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    public static void H(this IEnumerable<Circuit.Qubit> qubits) {
        var evt = new Scheduling.GateEvent(Gate.Hadamard, qubits);
        qubits.FirstOrDefault()?.ParentCircuit?.GateSchedule?.ScheduleEvent(evt);
    }

    public static void CNot(this Circuit.Qubit qubit, Circuit.Qubit control) {
        var evt = new Scheduling.ControlledGateEvent(Gate.PauliX, control, new Circuit.Qubit[]{ qubit });
        qubit.ParentCircuit.GateSchedule.ScheduleEvent(evt);
    }
    */

}

}