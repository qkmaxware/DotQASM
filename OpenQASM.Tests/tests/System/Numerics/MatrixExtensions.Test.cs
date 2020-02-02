using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace msharp.test {
    [TestClass]
    public class Matrix {
        [TestMethod]
        public void IsSquare() {
            int[,] xs = new int[1,1];
            int[,] ys = new int[1,2];
            int[,] zs = new int[2,1];

            Assert.AreEqual(true, xs.IsSquare());
            Assert.AreEqual(false, ys.IsSquare());
            Assert.AreEqual(false, zs.IsSquare());
        }
        [TestMethod]
        public void IsRowMatrix() {
            int[,] xs = new int[1,1];
            int[,] ys = new int[1,2];
            int[,] zs = new int[2,1];

            Assert.AreEqual(true, xs.IsRowMatrix());
            Assert.AreEqual(true, ys.IsRowMatrix());
            Assert.AreEqual(false, zs.IsRowMatrix());
        }
        [TestMethod]
        public void IsColumnMatrix() {
            int[,] xs = new int[1,1];
            int[,] ys = new int[1,2];
            int[,] zs = new int[2,1];

            Assert.AreEqual(true, xs.IsColumnMatrix());
            Assert.AreEqual(false, ys.IsColumnMatrix());
            Assert.AreEqual(true, zs.IsColumnMatrix());
        }
        [TestMethod]
        public void FillValue() {
            int[,] xs = new int[2,2].FillValue(2);

            Assert.AreEqual(2, xs[0,0]);
            Assert.AreEqual(2, xs[0,1]);
            Assert.AreEqual(2, xs[1,0]);
            Assert.AreEqual(2, xs[1,1]);
        }
        [TestMethod]
        public void FillDiagonal() {
            int[,] xs = new int[2,2].FillDiagonal(2);

            Assert.AreEqual(2, xs[0,0]);
            Assert.AreEqual(0, xs[0,1]);
            Assert.AreEqual(0, xs[1,0]);
            Assert.AreEqual(2, xs[1,1]);
        }
        [TestMethod]
        public void Zero() {
            int[,] xs = new int[2,2].Zero();

            Assert.AreEqual(0, xs[0,0]);
            Assert.AreEqual(0, xs[0,1]);
            Assert.AreEqual(0, xs[1,0]);
            Assert.AreEqual(0, xs[1,1]);

            object[,] ys = new object[2,2].Zero();

            Assert.AreEqual(null, ys[0,0]);
            Assert.AreEqual(null, ys[0,1]);
            Assert.AreEqual(null, ys[1,0]);
            Assert.AreEqual(null, ys[1,1]);
        }
        [TestMethod]
        public void To() {
            int[,] xs = new int[2,2].FillValue(2);
            float[,] ys = xs.To<int,float>();
        }

        [TestMethod]
        public void Map() {
            int[,] xs = new int[2,2].FillValue(2);

            string[,] zs = xs.Map<int, string>((x) => x.ToString());
        }

        [TestMethod]
        public void Transpose() {
            int[,] xs = new int[2,1].FillValue(2);
            int[,] ys = xs.Transpose();

            Assert.AreEqual(1, ys.GetLength(0));
            Assert.AreEqual(2, ys.GetLength(1));
            Assert.AreEqual(2, ys[0,0]);
            Assert.AreEqual(2, ys[0,1]);
        }

        [TestMethod]
        public void Trace() {
            int[,] xs = new int[2,2].FillValue(2);
            Assert.AreEqual(4, xs.Trace());
        }

        [TestMethod]
        public void ScalarMultiplication() {
            int[,] xs = new int[2,2].FillValue(2);
            int[,] ys = xs.Multiply(4);

            Assert.AreEqual(8, ys[0,0]);
            Assert.AreEqual(8, ys[0,1]);
            Assert.AreEqual(8, ys[1,0]);
            Assert.AreEqual(8, ys[1,1]);
        }

        [TestMethod]
        public void SwapRows() {
            double[,] m = new double[2,2].FillRows(
                new double[] {1,2},
                new double[] {3,4}
            );
            m.SwapRows(0,1);

            Assert.AreEqual(3, m[0,0]);
            Assert.AreEqual(4, m[0,1]);
            Assert.AreEqual(1, m[1,0]);
            Assert.AreEqual(2, m[1,1]);
        }
    }
}