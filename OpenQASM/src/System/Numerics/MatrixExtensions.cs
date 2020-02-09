using System.Text;

namespace System.Numerics {
    
public class DimensionMismatchException : ArithmeticException {
    public DimensionMismatchException() : base() {}
}

public interface IGenerator<T> {
    T Next();
    T Next(T min, T max);
}

public interface IArithmetic<T, R> {
    R Add(T b);
    R Subtract(T b);
    R Multiply(T b);
    R Divide(T b);
}

public static class MatrixExtensions {

    private static readonly Random generator = new Random();

    // MATRIX PROPERTIES -----------------------------------------------------------------
    #region Properties
    public static int Rows<T> (this T[,] m1) {
        return m1.GetLength(0);
    }
 
    public static int Columns<T> (this T[,] m1) {
        return m1.GetLength(1);
    }

    public static bool Equivalent<T1,T2>(this T1[,] m1, T2[,] m2) {
        if(m1.Rows() != m2.Rows() || m1.Columns() != m2.Columns())
            return false;
        for(int k=0;k < m1.GetLength(0); k++) {
            for(int l=0;l < m1.GetLength(1); l++) {
                if( !m1[k,l].Equals(m2[k,l]) )
                    return false;
            }
        }
        return true;
    }

    public static bool ApproximatelyEquivalent<T1,T2>(this T1[,] m1, T2[,] m2, Func<T1, T2, bool> fn) {
        if(m1.Rows() != m2.Rows() || m1.Columns() != m2.Columns())
            return false;
        for(int k=0;k < m1.GetLength(0); k++) {
            for(int l=0;l < m1.GetLength(1); l++) {
                if( !fn(m1[k,l], m2[k,l]) )
                    return false;
            }
        }
        return true;
    }
    #endregion

    // MATRIX ASSERTIONS -----------------------------------------------------------------
    #region Assertions
    public static void AssertSquare<T>(T[,] mtx) {
        if(!mtx.IsSquare()) {
            throw new DimensionMismatchException();
        }
    }

    public static void AssertSameDimensions<T1,T2>(T1[,] m1, T2[,] m2) {
        if(m1.Rows() != m2.Rows() || m1.Columns() != m2.Columns()) {
            throw new DimensionMismatchException();
        }
    }

    public static void AssertCanMultiply<T1,T2>(T1[,] m1, T2[,] m2) {
        if(m1.Columns() != m2.Rows()) {
            throw new DimensionMismatchException();
        }
    }

    public static void AssertValidColumn<T>(T[,] m1, int column) {
        if(m1.Columns() < column) {
            throw new IndexOutOfRangeException();
        }
    }

    public static void AssertValidRow<T>(T[,] m1, int row) {
        if(m1.Rows() < row) {
            throw new IndexOutOfRangeException();
        }
    }
    #endregion

    // MATRIX TEST -----------------------------------------------------------------
    #region Tests
    public static bool IsSquare<T>(this T[,] source) {
        return source.Rows() == source.Columns();
    }

    public static bool IsRowMatrix<T>(this T[,] source) {
        return source.Rows() == 1;
    }

    public static bool IsColumnMatrix<T>(this T[,] source) {
        return source.Columns() == 1;
    }
    #endregion

    // FILLING MATRIX CONTENT -----------------------------------------------------------------
    #region Fill
    public static T[,] Clone<T> (this T[,] source) {
        T[,] res = new T[source.GetLength(0), source.GetLength(1)];
        for (int i = 0; i < source.GetLength(0); i++)
            for (int j = 0; j < source.GetLength(1); j++)
                res[i,j] = source[i, j];
        return res;
    }

    public static int[,] Identity(this int[,] source) {
        return source.FillDiagonal(1);
    }
    public static uint[,] Identity(this uint[,] source) {
        return source.FillDiagonal(1u);
    }
    public static long[,] Identity(this long[,] source) {
        return source.FillDiagonal(1);
    }
    public static ulong[,] Identity(this ulong[,] source) {
        return source.FillDiagonal(1u);
    }
    public static float[,] Identity(this float[,] source) {
        return source.FillDiagonal(1.0f);
    }
    public static double[,] Identity(this double[,] source) {
        return source.FillDiagonal(1.0);
    }
    public static decimal[,] Identity(this decimal[,] source) {
        return source.FillDiagonal(1.0m);
    }

    public static T[,] FillValue<T>(this T[,] source, T value) {
        for (int i = 0; i < source.GetLength(0); i++)
            for (int j = 0; j < source.GetLength(1); j++)
                source[i, j] = value;
        return source;
    }
    public static T[,] FillRows<T>(this T[,] source, params T[][] rows) {
        for(int i = 0; i < Math.Min(source.Rows(), rows.Length); i++) {
            T[] row = rows[i];
            for(int j = 0; j < Math.Min(source.Columns(), row.Length); j++) {
                source[i,j] = row[j];
            }
        }
        return source;
    }
    public static T[,] FillDiagonal<T>(this T[,] source, T value) {
        for (int i = 0; i < source.GetLength(0); i++)
            for (int j = 0; j < source.GetLength(1); j++)
                if (i == j)
                    source[i, j] = value;
                else
                    source[i, j] = default(T);
        return source;
    }
    
    public static int[,] FillRandom(this int[,] m1, int limit0, int limit1) {
        int MinValue = Math.Min(limit0, limit1);
        int MaxValue = Math.Max(limit0, limit1);
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                m1[i,j] = generator.Next(MinValue, MaxValue);
            }
        }
        return m1;
    }

    public static uint[,] FillRandom(this uint[,] m1, uint limit0, uint limit1) {
        int MinValue = Math.Min((int)limit0, (int)limit1);
        int MaxValue = Math.Max((int)limit0, (int)limit1);
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                m1[i,j] = (uint)generator.Next(MinValue, MaxValue);
            }
        }
        return m1;
    }

    public static long[,] FillRandom(this long[,] m1, int limit0, int limit1) {
        int MinValue = Math.Min(limit0, limit1);
        int MaxValue = Math.Max(limit0, limit1);
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                m1[i,j] = generator.Next(MinValue, MaxValue);
            }
        }
        return m1;
    }

    public static ulong[,] FillRandom(this ulong[,] m1, uint limit0, uint limit1) {
        int MinValue = Math.Min((int)limit0, (int)limit1);
        int MaxValue = Math.Max((int)limit0, (int)limit1);
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                m1[i,j] = (uint)generator.Next(MinValue, MaxValue);
            }
        }
        return m1;
    }

    public static long[,] FillRandom(this long[,] m1, long limit0, long limit1) {
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                double t = generator.NextDouble();
                m1[i,j] =  (long)((1 - t) * limit0 + t * limit1);
            }
        }
        return m1;
    }

    public static double[,] FillRandom(this double[,] m1, double limit0, double limit1) {
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                double t = generator.NextDouble();
                m1[i,j] =  ((1 - t) * limit0 + t * limit1);
            }
        }
        return m1;
    }

    public static decimal[,] FillRandom(this decimal[,] m1, double limit0, double limit1) {
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                double t = generator.NextDouble();
                m1[i,j] =  (decimal)((1 - t) * limit0 + t * limit1);
            }
        }
        return m1;
    }

    public static T[,] FillRandom<T>(this T[,] m1, IGenerator<T> gen, T limit0, T limit1) {
        for (int i = 0; i < m1.GetLength(0); i++) {
            for (int j = 0; j < m1.GetLength(1); j++) {
                m1[i,j] =  gen.Next(limit0, limit1);
            }
        }
        return m1;
    }

    public static T[,] Zero<T>(this T[,] source) {
        for (int i = 0; i < source.GetLength(0); i++)
            for (int j = 0; j < source.GetLength(1); j++)
                source[i, j] = default(T);
        return source;
    }
    #endregion

    // CONVERSION BETWEEN TYPES -----------------------------------------------------------------
    #region Conversion
    public static R[,] Map<T,R> (this T[,] m1, System.Func<T, R> fn) {
        R[,] rs = new R[m1.Rows(), m1.Columns()];
        for (var i = 0; i < m1.Rows(); i++) {
            for (var j = 0; j < m1.Columns(); j++) {
                rs[i,j] = fn.Invoke(m1[i,j]);
            }
        }
        return rs;
    }

    public static R[,] To<T,R> (this T[,] source) 
        where T : IConvertible
        where R : IConvertible 
    {
        R[,] res = new R[source.GetLength(0),source.GetLength(1)];
        for (int i = 0; i < source.GetLength(0); i++)
            for (int j = 0; j < source.GetLength(1); j++)
                res[i,j] = (R)Convert.ChangeType(source[i,j], typeof(R));
        return res;
    }
    #endregion

    // MATRIX SPECIFIC OPERATIONS -----------------------------------------------------------------
    #region Matrix Operations
        #region Elementary Row/Column Operations
        public static void SwapColumns<T>(this T[,] m1, int c1, int c2) {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            T temp ;
            for(int i = 0; i < m1.Rows(); i++) {
                temp = m1[i, c1];
                m1[i,c1] = m1[i,c2];
                m1[i,c2] = temp;
            }
        }
        
        public static void SwapRows<T>(this T[,] m1, int r1, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            T temp ;
            for(int i = 0; i < m1.Columns(); i++) {
                temp = m1[r1, i];
                m1[r1,i] = m1[r2,i];
                m1[r2,i] = temp;
            }
        }
        
        public static void MultiplyColumn<T>(this T[,] m1, int c1, T value) where T:IArithmetic<T,T> {
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value.Multiply(m1[i,c1]);
            }
        }
        public static void MultiplyColumn(this int[,] m1, int c1, int value){
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value * (m1[i,c1]);
            }
        }
        public static void MultiplyColumn(this uint[,] m1, int c1, uint value){
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value * (m1[i,c1]);
            }
        }
        public static void MultiplyColumn(this long[,] m1, int c1, long value){
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value * (m1[i,c1]);
            }
        }
        public static void MultiplyColumn(this ulong[,] m1, int c1, ulong value){
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value * (m1[i,c1]);
            }
        }
        public static void MultiplyColumn(this float[,] m1, int c1, float value){
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value * (m1[i,c1]);
            }
        }
        public static void MultiplyColumn(this double[,] m1, int c1, double value){
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value * (m1[i,c1]);
            }
        }
        public static void MultiplyColumn(this decimal[,] m1, int c1, decimal value){
            AssertValidColumn(m1, c1);
            for (int i = 0; i < m1.Rows(); i++) {
                m1[i,c1] = value * (m1[i,c1]);
            }
        }
        
        public static void MultiplyRow<T>(this T[,] m1, int r1, T value) where T:IArithmetic<T,T> {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value.Multiply(m1[r1,i]);
            }
        }
        public static void MultiplyRow(this int[,] m1, int r1, int value) {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value * (m1[r1,i]);
            }
        }
        public static void MultiplyRow(this uint[,] m1, int r1, uint value) {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value * (m1[r1,i]);
            }
        }
        public static void MultiplyRow(this long[,] m1, int r1, long value) {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value * (m1[r1,i]);
            }
        }
        public static void MultiplyRow(this ulong[,] m1, int r1, ulong value) {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value * (m1[r1,i]);
            }
        }
        public static void MultiplyRow(this float[,] m1, int r1, float value) {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value * (m1[r1,i]);
            }
        }
        public static void MultiplyRow(this double[,] m1, int r1, double value) {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value * (m1[r1,i]);
            }
        }
        public static void MultiplyRow(this decimal[,] m1, int r1, decimal value) {
            AssertValidRow(m1, r1);
            for (int i = 0; i < m1.Columns(); i++) {
                m1[r1,i] = value * (m1[r1,i]);
            }
        }
        
        // add value * row(r1) to row(r2)
        public static void AddRowMultiple<T>(this T[,] m1, int r1, T value, int r2) where T:IArithmetic<T,T> {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i].Add(m1[r1,i].Multiply(value));
            }
        }
        public static void AddRowMultiple(this int[,] m1, int r1, int value, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i] + (m1[r1,i] * (value));
            }
        }
        public static void AddRowMultiple(this uint[,] m1, int r1, uint value, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i] + (m1[r1,i] * (value));
            }
        }
        public static void AddRowMultiple(this long[,] m1, int r1, long value, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i] + (m1[r1,i] * (value));
            }
        }
        public static void AddRowMultiple(this ulong[,] m1, int r1, ulong value, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i] + (m1[r1,i] * (value));
            }
        }
        public static void AddRowMultiple(this float[,] m1, int r1, float value, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i] + (m1[r1,i] * (value));
            }
        }
        public static void AddRowMultiple(this double[,] m1, int r1, double value, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i] + (m1[r1,i] * (value));
            }
        }
        public static void AddRowMultiple(this decimal[,] m1, int r1, decimal value, int r2) {
            AssertValidRow(m1, r1);
            AssertValidRow(m1, r2);
            for(int i = 0; i < m1.Columns(); i++) {
                m1[r2, i] = m1[r2,i] + (m1[r1,i] * (value));
            }
        }
        
        // add value * column(c1) to column(c2)
        public static void AddColumnMultiple<T>(this T[,] m1, int c1, T value, int c2) where T:IArithmetic<T,T> {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            for(int i = 0; i < m1.Rows(); i++) {
                m1[i, c2] = m1[i, c2].Add(m1[i, c1].Multiply(value));
            }
        }
        public static void AddColumnMultiple(this int[,] m1, int c1, int value, int c2) {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            for(int i = 0; i < m1.Rows(); i++) {
                m1[i, c2] = m1[i, c2] + (m1[i, c1] * (value));
            }
        }
        public static void AddColumnMultiple(this uint[,] m1, int c1, uint value, int c2) {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            for(int i = 0; i < m1.Rows(); i++) {
                m1[i, c2] = m1[i, c2] + (m1[i, c1] * (value));
            }
        }
        public static void AddColumnMultiple(this ulong[,] m1, int c1, ulong value, int c2) {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            for(int i = 0; i < m1.Rows(); i++) {
                m1[i, c2] = m1[i, c2] + (m1[i, c1] * (value));
            }
        }
        public static void AddColumnMultiple(this long[,] m1, int c1, long value, int c2) {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            for(int i = 0; i < m1.Rows(); i++) {
                m1[i, c2] = m1[i, c2] + (m1[i, c1] * (value));
            }
        }
        public static void AddColumnMultiple(this double[,] m1, int c1, double value, int c2) {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            for(int i = 0; i < m1.Rows(); i++) {
                m1[i, c2] = m1[i, c2] + (m1[i, c1] * (value));
            }
        }
        public static void AddColumnMultiple(this decimal[,] m1, int c1, decimal value, int c2) {
            AssertValidColumn(m1, c1);
            AssertValidColumn(m1, c2);
            for(int i = 0; i < m1.Rows(); i++) {
                m1[i, c2] = m1[i, c2] + (m1[i, c1] * (value));
            }
        }
        #endregion
        #region Trace
        public static int Trace (this int[,] m1) {
            AssertSquare(m1);
            int value = default(int);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value + (m1[i,i]);
                }
            }
            return value;
        }
        public static long Trace (this long[,] m1) {
            AssertSquare(m1);
            long value = default(long);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value + (m1[i,i]);
                }
            }
            return value;
        }
        public static ulong Trace (this ulong[,] m1) {
            AssertSquare(m1);
            ulong value = default(ulong);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value + (m1[i,i]);
                }
            }
            return value;
        }
        public static uint Trace (this uint[,] m1) {
            AssertSquare(m1);
            uint value = default(uint);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value + (m1[i,i]);
                }
            }
            return value;
        }
        public static float Trace (this float[,] m1) {
            AssertSquare(m1);
            float value = default(float);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value + (m1[i,i]);
                }
            }
            return value;
        }
        public static double Trace (this double[,] m1) {
            AssertSquare(m1);
            double value = default(double);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value + (m1[i,i]);
                }
            }
            return value;
        }
        public static decimal Trace (this decimal[,] m1) {
            AssertSquare(m1);
            decimal value = default(decimal);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value + (m1[i,i]);
                }
            }
            return value;
        }
        public static T Trace<T> (this T[,] m1) where T: IArithmetic<T,T> {
            AssertSquare(m1);
            T value = default(T);
            for(int i = 0; i < m1.Rows(); i++) {
                if(i == 0) {
                    value = m1[i,i];
                } else {
                    value = value.Add(m1[i,i]);
                }
            }
            return value;
        }
        #endregion
        #region Transpose
        public static T[,] Transpose<T> (this T[,] source) {
            T[,] res = new T[source.GetLength(1),source.GetLength(0)];
            for (int i = 0; i < source.GetLength(0); i++)
                for (int j = 0; j < source.GetLength(1); j++)
                    res[j,i] = source[i,j];
            return res;
        }
        #endregion
    #endregion

    // MULTIPLICATION WITH SCALAR VALUE -----------------------------------------------------------------
    #region Scalar Multiplication
    public static int[,] Multiply (this int[,] m1, int scalar) {
        int[,] rs = new int[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static ulong[,] Multiply (this ulong[,] m1, ulong scalar) {
        ulong[,] rs = new ulong[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static long[,] Multiply (this long[,] m1, long scalar) {
        long[,] rs = new long[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static uint[,] Multiply (this uint[,] m1, uint scalar) {
        uint[,] rs = new uint[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static float[,] Multiply (this float[,] m1, float scalar) {
        float[,] rs = new float[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static double[,] Multiply (this double[,] m1, double scalar) {
        double[,] rs = new double[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static decimal[,] Multiply (this decimal[,] m1, decimal scalar) {
        decimal[,] rs = new decimal[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static Complex[,] Multiply (this Complex[,] m1, Complex scalar) {
        Complex[,] rs = new Complex[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] * (scalar);
            }
        }
        return rs;
    }

    public static R[,] Multiply <T1, T2, R> (this T1[,] m1, T2 scalar) where T1: IArithmetic<T2, R> {
        R[,] rs = new R[m1.Rows(), m1.Columns()];
        for (int i = 0; i < m1.Rows(); i++) {
            for (int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j].Multiply(scalar);
            }
        }
        return rs;
    }

    #endregion

    // MULTIPLICATION BETWEEN MATRICES -----------------------------------------------------------------
    #region Matrix Multiplication
    public static int[,] Multiply (this int[,] m1, int[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        int[,] rs = new int[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                int sum = default(int);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static ulong[,] Multiply (this ulong[,] m1, ulong[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        ulong[,] rs = new ulong[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                ulong sum = default(ulong);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static long[,] Multiply (this long[,] m1, long[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        long[,] rs = new long[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                long sum = default(long);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static uint[,] Multiply (this uint[,] m1, uint[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        uint[,] rs = new uint[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                uint sum = default(uint);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static float[,] Multiply (this float[,] m1, float[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        float[,] rs = new float[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                float sum = default(float);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static double[,] Multiply (this double[,] m1, double[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        double[,] rs = new double[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                double sum = default(double);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static decimal[,] Multiply (this decimal[,] m1, decimal[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        decimal[,] rs = new decimal[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                decimal sum = default(decimal);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static Complex[,] Multiply (this Complex[,] m1, Complex[,] m2) {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        Complex[,] rs = new Complex[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                Complex sum = default(Complex);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k] * (m2[k,j]); //Row Column Format
                    } else {
                        sum = sum + (m1[i,k] * (m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 

    public static T1[,] Multiply <T1> (this T1[,] m1, T1[,] m2) where T1: IArithmetic<T1, T1> {
        int rows = m1.Rows();
        int columns = m2.Columns();
        AssertCanMultiply(m1, m2);
        T1[,] rs = new T1[rows,columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                T1 sum = default(T1);
                for (int k = 0; k < m1.Columns(); k++) {
                    if (k == 0) {
                        sum = m1[i,k].Multiply(m2[k,j]); //Row Column Format
                    } else {
                        sum = sum.Add(m1[i,k].Multiply(m2[k,j])); //Row Column Format
                    }
                }
                rs[i,j] = sum; //Row Column Format
            }
        }
        return rs;
    } 
    #endregion

    // ADD -----------------------------------------------------------------
    #region Addition
    public static int[,] Add(this int[,] m1, int[,] m2) {
        AssertSameDimensions(m1, m2);
        int[,] rs = new int[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static uint[,] Add(this uint[,] m1, uint[,] m2) {
        AssertSameDimensions(m1, m2);
        uint[,] rs = new uint[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static long[,] Add(this long[,] m1, long[,] m2) {
        AssertSameDimensions(m1, m2);
        long[,] rs = new long[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static ulong[,] Add(this ulong[,] m1, ulong[,] m2) {
        AssertSameDimensions(m1, m2);
        ulong[,] rs = new ulong[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static float[,] Add(this float[,] m1, float[,] m2) {
        AssertSameDimensions(m1, m2);
        float[,] rs = new float[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static double[,] Add(this double[,] m1, double[,] m2) {
        AssertSameDimensions(m1, m2);
        double[,] rs = new double[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static decimal[,] Add(this decimal[,] m1, decimal[,] m2) {
        AssertSameDimensions(m1, m2);
        decimal[,] rs = new decimal[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static Complex[,] Add(this Complex[,] m1, Complex[,] m2) {
        AssertSameDimensions(m1, m2);
        Complex[,] rs = new Complex[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] + (m2[i,j]);
            }
        }
        return rs;
    }

    public static R[,] Add<T1,T2,R> (this T1[,] m1, T2[,] m2) where T1: IArithmetic<T2, R> {
        AssertSameDimensions<T1,T2>(m1, m2);
        R[,] rs = new R[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j].Add(m2[i,j]);
            }
        }
        return rs;
    }
    #endregion

    // SUBTRACT -----------------------------------------------------------------
    #region Subtraction
    public static int[,] Subtract(this int[,] m1, int[,] m2) {
        AssertSameDimensions(m1, m2);
        int[,] rs = new int[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static ulong[,] Subtract(this ulong[,] m1, ulong[,] m2) {
        AssertSameDimensions(m1, m2);
        ulong[,] rs = new ulong[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static long[,] Subtract(this long[,] m1, long[,] m2) {
        AssertSameDimensions(m1, m2);
        long[,] rs = new long[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static uint[,] Subtract(this uint[,] m1, uint[,] m2) {
        AssertSameDimensions(m1, m2);
        uint[,] rs = new uint[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static float[,] Subtract(this float[,] m1, float[,] m2) {
        AssertSameDimensions(m1, m2);
        float[,] rs = new float[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static double[,] Subtract(this double[,] m1, double[,] m2) {
        AssertSameDimensions(m1, m2);
        double[,] rs = new double[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static decimal[,] Subtract(this decimal[,] m1, decimal[,] m2) {
        AssertSameDimensions(m1, m2);
        decimal[,] rs = new decimal[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static Complex[,] Subtract(this Complex[,] m1, Complex[,] m2) {
        AssertSameDimensions(m1, m2);
        Complex[,] rs = new Complex[m1.Rows(), m1.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j] - (m2[i,j]);
            }
        }
        return rs;
    }

    public static R[,] Subtract<T1,T2,R>(this T1[,] m1, T2[,] m2) where T1: IArithmetic<T2, R> {
        AssertSameDimensions<T1,T2>(m1, m2);
        R[,] rs = new R[m1.Rows(), m2.Columns()];
        for(int i = 0; i < m1.Rows(); i++) {
            for(int j = 0; j < m1.Columns(); j++) {
                rs[i,j] = m1[i,j].Subtract(m2[i,j]);
            }
        }
        return rs;
    }
    #endregion

    // PRINTING / STRINGIFY -----------------------------------------------------------------
    #region Printing
    public enum MatrixItemPrintOrder {
        RowWise, ColumnWise,
    }

    public static string FormatWolfram<T>(this T[,] m1, string format = "{0}") {
        return m1.Format(
            MatrixItemPrintOrder.RowWise,
            "{",
                "{",format,",", "}", ",",
            "}"
        );
    }
 
    public static string FormatMatlab<T>(this T[,] m1, string format = "{0}") {
        return m1.Format(
            MatrixItemPrintOrder.RowWise,
            "[",
                string.Empty,format,",", string.Empty, ";",
            "]"
        );
    }
 
    public static string FormatMaple<T>(this T[,] m1, string format = "{0}") {
        return m1.Format(
            MatrixItemPrintOrder.ColumnWise,
            "<",
                "<",format,",", ">", "|",
            ">"
        );
    }

    public static string FormatRaw<T>(this T[,] m1, string format = "{0}") {
        return m1.Format(
            MatrixItemPrintOrder.RowWise,
            string.Empty,
                string.Empty,format," ", string.Empty, System.Environment.NewLine,
            string.Empty
        );
    }

    public static string FormatLatex<T>(this T[,] m1, string format = "{0}") {
        return 
        @"\begin{bmatrix}" + 
        m1.Format(
            MatrixItemPrintOrder.RowWise,
            string.Empty,
                string.Empty,format,"&", string.Empty, @" \\",
            string.Empty
        ) + 
        @"\end{bmatrix}";
    }

    public static string Format<T>(this T[,] m1, MatrixItemPrintOrder format = MatrixItemPrintOrder.RowWise, string prefix = "[", string itemPrefix = "", string elementFormat = "{0}", string elementSeparator = ",", string itemPostfix = "", string itemSeparator = ";", string postfix = "]") {
        StringBuilder sb = new StringBuilder();
        sb.Append(prefix);
        if (format == MatrixItemPrintOrder.RowWise) {
            // Iterate over rows first
            for(int i = 0; i < m1.Rows(); i++){
                if (i != 0) {
                    sb.Append(itemSeparator);
                }
                sb.Append(itemPrefix);
                for(int j = 0; j < m1.Columns(); j++) {
                    if (j != 0) {
                        sb.Append(elementSeparator);
                    }
                    sb.Append(string.Format(elementFormat, m1[i,j]));
                }
                sb.Append(itemPostfix);
            }
        } else {
            // Iterate over columns first
            for(int i = 0; i < m1.Columns(); i++){
                if (i != 0) {
                    sb.Append(itemSeparator);
                }
                sb.Append(itemPrefix);
                for(int j = 0; j < m1.Rows(); j++) {
                    if (j != 0) {
                        sb.Append(elementSeparator);
                    }
                    sb.Append(string.Format(elementFormat, m1[j,i]));
                }
                sb.Append(itemPostfix);
            }
        }
        sb.Append(postfix);
        return sb.ToString();
    }
    #endregion
}
    
}