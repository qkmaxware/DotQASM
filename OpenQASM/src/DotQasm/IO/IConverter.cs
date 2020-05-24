using System.IO;

namespace DotQasm.IO {

/// <summary>
/// Interface representing something that can convert one object type to another
/// </summary>
/// <typeparam name="From">type to convert</typeparam>
/// <typeparam name="To">type to convert into</typeparam>
public interface IConverter<From, To> {
    /// <summary>
    /// Convert the given program into another form
    /// </summary>
    /// <param name="from">program to convert</param>
    To Convert(From program);
}

public interface IFileConverter<From, To> : IConverter<From, To> {
    /// <summary>
    /// Name of format being converted to
    /// </summary>
    string FormatName {get;}
    /// <summary>
    /// Extension of the format being converted to
    /// </summary>
    /// <value></value>
    string FormatExtension {get;}
}

}