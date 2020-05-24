namespace DotQasm.Compile {

/// <summary>
/// Indicates that a the class can generate a circuit template for new projects
/// </summary>
public interface ICircuitTemplate {
    /// <summary>
    /// Name of the template circuit
    /// </summary>
    string TemplateName {get;}
    /// <summary>
    /// Get the template circuit
    /// </summary>
    Circuit GetTemplateCircuit();
}

}