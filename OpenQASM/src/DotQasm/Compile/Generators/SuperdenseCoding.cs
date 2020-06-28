namespace DotQasm.Compile.Generators {

public class SuperdenseCodingGenerator : ICircuitGenerator<int> {
    
    private static void Entangle(Qubit q1, Qubit q2) {
        q1.H();
        q1.CX(q2);
    }

    private static void Encode(Qubit q1, int value) {
        value = value & 0b11;
        if (value == 0) {
            q1.I();
        }
        else if (value == 1) {
            q1.X();
        }
        else if (value == 2) {
            q1.Z();
        }
        else if (value == 3) {
            q1.X();
            q1.Z();
        }
    }
    
    private static void Alice(Qubit q1, int value) {
        Encode(q1, value);
    }

    private static void Decode(Qubit q1, Cbit c1, Qubit q2, Cbit c2) {
        q1.CX(q2);
        q1.H();
        
        q1.Measure(c1);
        q2.Measure(c2);
    }

    private static void Bob(Qubit alice, Qubit bob, Register<Cbit> cbits) {
        Decode(alice, cbits[0], bob, cbits[1]);
        // Resulting value is cbits[0] | cbits[1] << 1;
    }

    public Circuit Generate(int arg) {
        // https://qiskit.org/textbook/ch-algorithms/superdense-coding.html
        var circ = new Circuit($"Superdense coding of {arg}");

        // Allocate qubits
        var qubits = circ.AllocateQubits(2);
        var cbits = circ.AllocateCbits(2);

        // Prepare qubits
        Entangle(qubits[0], qubits[1]);

        // Do Alice's part
        Alice(qubits[0], arg);

        // Send Alice's qubit to Bob

        // Bob's part
        Bob(qubits[0], qubits[1], cbits);

        return circ;
    }
}

}