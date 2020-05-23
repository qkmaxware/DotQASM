using System;
using System.Collections.Generic;
using DotQasm.Compile.Generators;

namespace DotQasm.Compile.Templates {

public class QftTemplate : ICircuitTemplate {
    public string TemplateName => "QTF" + QubitCount;

    public int QubitCount = 5;

    public QftTemplate(int Qubits = 5) {
        this.QubitCount = Qubits;
    }

    public Circuit GetTemplateCircuit() {
        return (new QftGenerator()).Generate(new QftGeneratorArguments() {
            Qubits = QubitCount
        });
    }
}

}