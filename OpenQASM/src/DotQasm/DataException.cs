namespace DotQasm {

/// <summary>
/// Generic exception which can store a data object representing the details of the exception
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataException<T>: System.Exception {
    /// <summary>
    /// Data stored with the exception
    /// </summary>
    public T ExceptionData {get; private set;}

    public DataException(string message, T data) : base(message) {
        this.ExceptionData = data;
    }

    public override string ToString() {
        return this.Message + System.Environment.NewLine + (ExceptionData?.ToString() ?? string.Empty) + System.Environment.NewLine + this.StackTrace;
    }
}

}