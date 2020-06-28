namespace DotQasm.Compile.Generators {

public class BernsteinVazraniGenerator : ICircuitGenerator<(int qubits, int value)> {
    public Circuit Generate((int qubits, int value) args) {
        // https://qiskit.org/textbook/ch-algorithms/bernstein-vazirani.html
        Circuit circ = new Circuit($"Bernstein Vazrani for value {args.value} with {args.qubits} qubits");

        var inputs = circ.AllocateQubits(args.qubits);
        var output = circ.AllocateQubit();
        var creg = circ.AllocateCbits(args.qubits);

        // Initialize output qubit to |−⟩
        output.H();
        output.Z();

        // Apply hadamard gates to the input register
        foreach (var input in inputs) {
            input.H();
        }

        // Barrier?

        // Query the "inner-product" oracle
        for (var i = 0; i < args.qubits; i++) {
            var mask = 1 << i;
            if ((mask & args.value) != 0) {
                // is set
                inputs[i].CX(output);
            } else {
                // is not set
                inputs[i].I();
            } 
        }

        // Barrier?

        // Apply hadamard gates to the input register
        foreach (var input in inputs) {
            input.H();
        }

        // Measure
        for (var i = 0; i < inputs.Count; i++) {
            inputs[i].Measure(creg[i]);
        }

        return circ;
    }
}

}