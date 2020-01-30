using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DotQasm.Scheduling;

namespace DotQasm {

public interface IOwnedBy<T> {
    T Owner {get;}
}

public interface IRegister<T>: System.Collections.Generic.IEnumerable<T>, IOwnedBy<Circuit> {
    int RegisterId {get;}
}

public class Register<T> : IRegister<T> where T:IOwnedBy<Register<T>> {
    public Circuit Owner {get; private set;}
    public int RegisterId {get; private set;}
    private IEnumerable<T> elements;
    public int Count => elements.Count(); 

    public Register(Circuit circuit, int registerId, IEnumerable<T> Ts) {
        this.Owner = circuit;
        this.RegisterId = registerId;
        this.elements = Ts;
    }

    public T this[int i] => elements.ElementAt(i);

    public IEnumerator<T> GetEnumerator() {
        return elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return elements.GetEnumerator();
    }
}

/// <summary>
/// Quantum bit
/// </summary>
public class Qubit: IOwnedBy<Register<Qubit>> {      
    public int QubitId {get; private set;}
    public Circuit ParentCircuit => Owner.Owner;
    public Register<Qubit> Owner {get; private set;}

    public Qubit(Register<Qubit> circuit, int index) {
        this.Owner = circuit;
        this.QubitId = index;
    }

    public override bool Equals(object obj) {
        if (obj is Qubit) {
            Qubit q = (Qubit)obj;
            return q.QubitId == this.QubitId && q.ParentCircuit == this.ParentCircuit;
        } else {
            return base.Equals(obj);
        }
    }

    public override int GetHashCode() {
        return HashCode.Combine(QubitId, ParentCircuit);
    }
}

/// <summary>
/// Classical bit
/// </summary>
public class Cbit: IOwnedBy<Register<Cbit>> {
    public int ClassicalBitId {get; private set;}
    public Circuit ParentCircuit => Owner.Owner;
    public Register<Cbit> Owner {get; private set;}

    public Cbit(Register<Cbit> circuit, int index) {
        this.Owner = circuit;
        this.ClassicalBitId = index;
    }

    public override bool Equals(object obj) {
        if (obj is Cbit) {
            Cbit q = (Cbit)obj;
            return q.ClassicalBitId == this.ClassicalBitId && q.ParentCircuit == this.ParentCircuit;
        } else {
            return base.Equals(obj);
        }
    }

    public override int GetHashCode() {
        return HashCode.Combine(ClassicalBitId, ParentCircuit);
    }
}   

[Serializable]
public class Circuit {

    /// <summary>
    /// User chosen name for the given circuit
    /// </summary>
    public string Name {get; set;}
    /// <summary>
    /// Number of qubits in the circuit
    /// </summary>
    public int QubitCount {get; private set;}
    /// <summary>
    /// Number of classical in the circuit
    /// </summary>
    public int BitCount {get; private set;}
    /// <summary>
    /// The schedule of quantum gates and events
    /// </summary>
    public LinearSchedule GateSchedule {get; set;}

    private List<Register<Qubit>> quantumRegisters = new List<Register<Qubit>>();
    private List<Register<Cbit>> classicalRegisters = new List<Register<Cbit>>();

    public IEnumerable<Register<Qubit>> QuantumRegisters => quantumRegisters.AsReadOnly();
    public IEnumerable<Register<Cbit>> ClassicalRegisters => classicalRegisters.AsReadOnly();

    public Circuit() {
        this.GateSchedule = new LinearSchedule(); // Default to a simple empty linear schedule
    }

    public Cbit AllocateCbit() {
        return AllocateCbits(1)[0];
    }

    public Register<Cbit> AllocateCbits(int classicalCount) {
        Cbit[] cbits = new Cbit[classicalCount];
        var reg = new Register<Cbit>(this, classicalRegisters.Count, cbits);
        this.classicalRegisters.Add(reg);
        for (int i = 0; i < classicalCount; i++) {
            cbits[i] = new Cbit(reg, this.BitCount++);
        }
        return reg;
    }

    public Qubit AllocateQubit() {
        return AllocateQubits(1)[0];
    }

    public Register<Qubit> AllocateQubits(int qubitCount) {
        Qubit[] qubits = new Qubit[qubitCount];
        var reg = new Register<Qubit>(this,quantumRegisters.Count, qubits);
        this.quantumRegisters.Add(reg);
        for (int i = 0; i < qubitCount; i++) {
            qubits[i] = new Qubit(reg, this.QubitCount++);
        }
        return reg;
    }


}

}