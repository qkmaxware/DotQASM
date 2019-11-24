using System;

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

    public int QubitCount {get; private set;}
    public int BitCount {get; private set;}
    public Schedule GateSchedule {get; set;}

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