namespace System.Numerics {

public static class ComplexExtensions {

    /// <summary>
    /// Create an imaginary number from a literal value
    /// </summary>
    /// <param name="value">real value</param>
    public static Complex i(this IConvertible value) {
        return new Complex(0, Convert.ToDouble(value));
    }

    /// <summary>
    /// Create an complex number with a real value from a literal 
    /// </summary>
    /// <param name="value">real value</param>
    public static Complex re(this IConvertible value) {
        return new Complex(Convert.ToDouble(value), 0);
    }

    /// <summary>
    /// Modulus squared magnitude of complex vector
    /// </summary>
    /// <param name="value">complex vector</param>
    /// <returns>squared magnitude</returns>
    public static double SqrMagnitude(this Complex value) {
        return value.Real * value.Real + value.Imaginary * value.Imaginary;
    }

}

}