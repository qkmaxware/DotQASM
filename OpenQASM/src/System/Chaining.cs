namespace System {

/*
 Usage: var Compiler = new SourceFileSystem().Then(scanner).Then(parser).Then(walk);
 Usage: var Compiler = new SourceFileSystem().Then(scanner).Then(parser).Then(bytecode).Then(interpret);
 Usage: var Compiler = new SourceFileSystem().Then(scanner).Then(parser).Then(bytecode).Then(tox86).then(writeBinaryFile);
*/

/// <summary>
/// Interface representing a chainable operation
/// </summary>
/// <typeparam name="InType">input type</typeparam>
/// <typeparam name="OutType">output type</typeparam>
public interface IChainableAction <InType, OutType> {
    /// <summary>
    /// Chain this operation with another
    /// </summary>
    /// <param name="next">next operation</param>
    /// <typeparam name="T">new output type</typeparam>
    /// <returns>chained operation</returns>
    ChainLink<InType, OutType, T> Then<T> (IChainableAction<OutType, T> next) {
        return new ChainLink<InType, OutType, T>(this, next);
    }
    /// <summary>
    /// Invoke the operation
    /// </summary>
    /// <param name="value">input value</param>
    /// <returns>output value</returns>
    OutType Invoke (InType value);
}

/// <summary>
/// Class representing a link within a chain of operations
/// </summary>
/// <typeparam name="InType">input type</typeparam>
/// <typeparam name="IntermediateType">intermediate type</typeparam>
/// <typeparam name="OutType">output type</typeparam>
public class ChainLink <InType, IntermediateType, OutType> : IChainableAction<InType, OutType> {

    private IChainableAction<InType, IntermediateType> first;
    private IChainableAction<IntermediateType, OutType> last;

    /// <summary>
    /// Create a new link
    /// </summary>
    /// <param name="first"></param>
    /// <param name="last"></param>
    public ChainLink(IChainableAction<InType, IntermediateType> first, IChainableAction<IntermediateType, OutType> last) {
        this.first = first;
        this.last = last;
    }

    /// <summary>
    /// Execute the operations in sequence
    /// </summary>
    /// <param name="value">first input</param>
    /// <returns>last operation</returns>
    public OutType Invoke(InType value) {
        return this.last.Invoke(this.first.Invoke(value));
    }
}

}