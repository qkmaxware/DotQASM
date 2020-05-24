namespace DotQasm.Optimization {

/// <summary>
/// Interface for indicating that a given class must use features of another class
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IUsing<T> {
    /// <summary>
    /// Set the object whose features are to be used
    /// </summary>
    void Use(T value);
}

}