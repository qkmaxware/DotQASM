namespace DotQasm.Compile {
    
/// <summary>
/// Interface representing a class that can generate full quantum circuits
/// </summary>
public interface ICircuitGenerator<T> {
    /// <summary>
    /// Generate a circuit given specific args
    /// </summary>
    /// <param name="generatorArgs">Arguments for the generator</param>
    /// <returns>Generated circuit</returns>
    Circuit Generate(T generatorArgs);
}

/// <summary>
/// Interface representing a class that can generate full quantum circuits
/// </summary>
public interface ICircuitGenerator {
    /// <summary>
    /// Generate a circuit given specific args
    /// </summary>
    /// <returns>Generated circuit</returns>
    Circuit Generate();
}

}