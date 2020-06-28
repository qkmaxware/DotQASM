using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace System.Numerics {

public class Fraction {
    public int Sign {get; private set;}
    public int Numerator {get; private set;}
    public int Denominator {get; private set;}

    public Fraction(int num, int denom) {
        var sign = Math.Sign(num / denom);
        this.Numerator = Math.Abs(num);
        this.Denominator = Math.Abs(denom);
    }

    public Fraction(double value, double epsilon = 0.0001, int maxIterations = 20) {
        var attempts = 0;
        this.Sign = Math.Sign(value);
        foreach (var fraction in rationalizations(Math.Abs(value))) {
            var approx = fraction.Item1/fraction.Item2;
            if (Math.Abs(value - approx) < epsilon) {
                this.Numerator = fraction.Item1;
                this.Denominator = fraction.Item2;
                break;
            } else if (attempts++ >= maxIterations) {
                this.Numerator = fraction.Item1;
                this.Denominator = fraction.Item2;
                break;
            }
        }
    }

    private static int gcd(int a, int b) {
        while (b != 0) {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public Fraction Reduce() {
        var factor = gcd(Numerator, Denominator);
        var n = Numerator / factor;
        var d = Denominator / factor;
        return new Fraction(Sign * n, d);
    }

    private IEnumerable<(int, int)> rationalizations(double x) {
        var ix = (int)Math.Floor(x); 
        yield return (ix, 1);
        if (x == ix)
            yield break;
        foreach (var (num, denom) in rationalizations(1.0 / (x - ix))) {
            yield return (denom + ix * num, num);
        }
    }

    public override string ToString() {
        return Sign * Numerator + "/" + Denominator;
    }
}

}