namespace DotQasm.Compile {

/// <summary>
/// Indicates that a the class can generate a circuit template for new projects
/// </summary>
public interface ICircuitTemplate {
    string TemplateName {get;}
    Circuit GetTemplateCircuit();
}

}