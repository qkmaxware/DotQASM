using System;
using System.Numerics;

namespace DotQasm {

public static class DSL {

    /// <summary>
    /// Create an imaginary number from a scalar value
    /// </summary>
    /// <param name="value">scalar value</param>
    /// <returns>imaginary number</returns>
    public static Complex i(this IConvertible value) {
        return new Complex(0, value.ToDouble(System.Globalization.CultureInfo.InvariantCulture));
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