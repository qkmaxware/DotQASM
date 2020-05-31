using System;
using DotQasm.Compile.Generators;

namespace DotQasm.Compile.Templates {

public class DeutschTemplate : ICircuitTemplate {
    public string TemplateName => "Deutsch Algorithm" + (funcName != null ? " for " + funcName : string.Empty);
    public Func<bool, bool> function;
    private string funcName;

    public DeutschTemplate(Func<bool, bool> fn, string @for = null) {
        this.function = fn;
        this.funcName = @for;
    }

    public Circuit GetTemplateCircuit() {
        DeutschGenerator generator = new DeutschGenerator();

        return generator.Generate(this.function);
    }
}

}