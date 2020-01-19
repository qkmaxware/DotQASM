using System;
using DotQasm.Scheduling;

namespace DotQasm {

[Serializable]
public class Circuit {

    public class Qubit {      
        public int QubitId {get; private set;}
        public Circuit ParentCircuit {get; private set;}

        public Qubit(Circuit circuit) {
            this.ParentCircuit = circuit;
            this.QubitId = circuit.QubitCount++;
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

    public class Cbit {
        public int ClassicalBitId {get; private set;}
        public Circuit ParentCircuit {get; private set;}

        public Cbit(Circuit circuit) {
            this.ParentCircuit = circuit;
            this.ClassicalBitId = circuit.BitCount++;
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
    public ISchedule GateSchedule {get; set;}

    public Circuit() {
        this.GateSchedule = new LinearSchedule(); // Default to a simple empty linear schedule
    }

    public Cbit[] CreateRegister(int classicalCount) {
        Cbit[] cbits = new Cbit[classicalCount];
        for (int i = 0; i < classicalCount; i++) {
            cbits[i] = new Cbit(this);
        }
        return cbits;
    }

    public Qubit Allocate() {
        return new Qubit(this);
    }

    public Qubit[] Allocate(int qubitCount) {
        Qubit[] qubits = new Qubit[qubitCount];
        for (int i = 0; i < qubitCount; i++) {
            qubits[i] = Allocate();
        }
        return qubits;
    }


}

}