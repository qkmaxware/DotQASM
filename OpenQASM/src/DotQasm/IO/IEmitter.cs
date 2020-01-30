using System.IO;

namespace DotQasm.IO {

/// <summary>
/// Interface representing something that can emit a string representation for a given type
/// </summary>
/// <typeparam name="C">type to stringify</typeparam>
public interface IEmitter<C> {
    /// <summary>
    /// Convert the given program into a string and output to the given text writer
    /// </summary>
    /// <param name="program">program to convert</param>
    /// <param name="writer">stream to write to</param>
    void Emit(C program, TextWriter writer);
}

}