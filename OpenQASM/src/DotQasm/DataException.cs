namespace DotQasm {

public class DataException<T>: System.Exception {

    public T ExceptionData {get; private set;}

    public DataException(string message, T data) : base(message) {
        this.ExceptionData = data;
    }

    public override string ToString() {
        return this.Message + System.Environment.NewLine + (ExceptionData?.ToString() ?? string.Empty) + System.Environment.NewLine + this.StackTrace;
    }
}

}