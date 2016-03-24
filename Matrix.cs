using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Figures
{
    public class Matrix
    {
        internal double[,] Array;

        public int Rows => Array.GetLength(0);
        public int Columns => Array.GetLength(1);

        public bool IsRow => Rows == 1;
        public bool IsColumn => Columns == 1;
        public bool IsSquare => Rows == Columns;

        public double X
        {
            get
            {
                if (IsRow || IsColumn) return Array[0, 0];
                throw new InvalidOperationException();
            }
            set
            {
                if (IsRow || IsColumn) Array[0, 0] = value;
                else throw new InvalidOperationException();
            }
        }
        public double Y
        {
            get
            {
                if (IsRow) return Array[0, 1];
                if (IsColumn) return Array[1, 0];
                throw new InvalidOperationException();
            }
            set
            {
                if (IsRow) Array[0, 1] = value;
                else if (IsColumn) Array[1, 0] = value;
                else throw new InvalidOperationException();
            }
        }
        public double Z
        {
            get
            {
                if (IsRow) return Array[0, 2];
                if (IsColumn) return Array[2, 0];
                throw new InvalidOperationException();
            }
            set
            {
                if (IsRow) Array[0, 2] = value;
                else if (IsColumn) Array[2, 0] = value;
                else throw new InvalidOperationException();
            }
        }
        public double W
        {
            get
            {
                if (IsRow) return Array[0, 3];
                if (IsColumn) return Array[3, 0];
                throw new InvalidOperationException();
            }
            set
            {
                if (IsRow) Array[0, 3] = value;
                else if (IsColumn) Array[3, 0] = value;
                else throw new InvalidOperationException();
            }
        }

        public double this[int i, int j]
        {
            get { return Array[i, j]; }
            set { Array[i, j] = value; }
        }
        public double this[int i]
        {
            get
            {
                if (IsRow) return Array[0, i];
                if (IsColumn) return Array[i, 0];
                throw new InvalidOperationException();
            }
            set
            {
                if (IsRow) Array[0, i] = value;
                else if (IsColumn) Array[i, 0] = value;
                else throw new InvalidOperationException();
            }
        }

        public Matrix Transposed => Clone().Transpose();
        public Matrix Inverse => Solve(Diagonal(Rows, Rows, 1.0));

        public Matrix Normalized
        {
            get
            {
                if (!IsColumn) throw new InvalidOperationException();
                double sum = 0;
                for (var i = 0; i < Rows; i++)
                    sum += Array[i, 0] * Array[i, 0];
                sum = Math.Sqrt(sum);
                var m = new double[Rows, 1];
                for (var i = 0; i < Rows; i++)
                    m[i, 0] = Array[i, 0] / sum;
                return new Matrix(m);
            }
        }

        public Matrix(double[,] array)
        {
            this.Array = array;
        }

        public Matrix(int n, int m) : this(new double[n, m])
        {
        }

        public Matrix(int width, params double[] elements)
        {
            if (elements.Length % width != 0)
                throw new ArgumentOutOfRangeException(nameof(elements));

            var height = elements.Length / width;

            Array = new double[height, width];
            for (int i = 0, p = 0; i < height; i++)
                for (var j = 0; j < width; j++, p++)
                    Array[i, j] = elements[p];
        }

        public static Matrix operator -(Matrix m)
        {
            var elements = new double[m.Rows, m.Columns];
            for (var i = 0; i < m.Rows; i++)
                for (var j = 0; j < m.Columns; j++)
                    elements[i, j] = -m.Array[i, j];
            return new Matrix(elements);
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.Columns != b.Columns || a.Rows != b.Rows)
                throw new InvalidOperationException();

            var elements = new double[a.Columns, b.Columns];
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Columns; j++)
                    elements[i, j] = a.Array[i, j] + b.Array[i, j];

            return new Matrix(elements);
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.Columns != b.Columns || a.Rows != b.Rows)
                throw new InvalidOperationException();

            var elements = new double[a.Rows, a.Columns];
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Columns; j++)
                    elements[i, j] = a.Array[i, j] - b.Array[i, j];

            return new Matrix(elements);
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
                throw new InvalidOperationException();

            var elements = new double[a.Rows, b.Columns];

            for (var i = 0; i < elements.GetLength(0); i++)
                for (var j = 0; j < elements.GetLength(1); j++)
                    for (var k = 0; k < a.Columns; k++)
                        elements[i, j] += a[i, k] * b[k, j];

            return new Matrix(elements);
        }

        public static Matrix operator *(Matrix a, double b)
        {
            var m = new Matrix(a.Rows, a.Columns);
            for (var i = 0; i < m.Rows; i++)
                for (var j = 0; j < m.Columns; j++)
                    m[i, j] *= b;
            return m;
        }
        public static Matrix operator /(Matrix a, double b)
        {
            var m = new Matrix(a.Rows, a.Columns);
            for (var i = 0; i < m.Rows; i++)
                for (var j = 0; j < m.Columns; j++)
                    m[i, j] /= b;
            return m;
        }

        public static Matrix Identity(int size)
        {
            var m = new Matrix(size, size);
            for (var i = 0; i < size; i++) m[i, i] = 1;
            return m;
        }

        public static Matrix Diagonal(int rows, int columns, double value)
        {
            Matrix X = new Matrix(rows, columns);
            double[,] x = X.Array;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i, j] = ((i == j) ? value : 0.0);
                }
            }
            return X;
        }

        public static Matrix Column(params double[] elements)
        {
            return new Matrix(1, elements);
        }

        public static double Dot(Matrix a, Matrix b)
        {
            if (!a.IsColumn || !b.IsColumn || a.Rows != b.Rows)
                throw new ArgumentException();

            double sum = 0;
            for (var i = 0; i < a.Rows; i++)
                sum += a[i, 0] * b[i, 0];

            return sum;
        }

        public static Matrix Cross(Matrix u, Matrix v)
        {
            if (!u.IsColumn || !v.IsColumn || u.Rows != 3 || v.Rows != 3)
                throw new ArgumentException();

            return Column(u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0]);
        }

        public static Matrix PerspectiveProjectionMatrix(double fovy, double aspect)
        {
            var cos = Math.Cos(fovy / 2);

            return new Matrix(new[,]
            {
                { cos / aspect, 0, 0, 0 },
                { 0, cos, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 1, 0 }
            });
        }

        public static Matrix OrthographicProjectionMatrix(int dimension, int axis)
        {
            var m = new double[dimension, dimension + 1];
            for (var i = 0; i < dimension - 1; i++)
                m[i, i >= axis ? i + 1 : i] = 1;
            m[dimension - 1, dimension] = 1;
            return new Matrix(m);
        }

        public static Matrix LookAtMatrix(Matrix eye, Matrix at)
        {
            var zaxis = (at - eye).Normalized;
            var xaxis = Cross(Column(0, 1, 0), zaxis).Normalized;
            var yaxis = Cross(zaxis, xaxis);

            return new Matrix(new[,]
            {
                {xaxis[0, 0], xaxis[1, 0], xaxis[2, 0], Dot(xaxis, -eye)},
                {yaxis[0, 0], yaxis[1, 0], yaxis[2, 0], Dot(yaxis, -eye)},
                {zaxis[0, 0], zaxis[1, 0], zaxis[2, 0], Dot(zaxis, -eye)},
                {0, 0, 0, 1}
            });
        }

        public static Matrix ScaleMatrix(params double[] scale)
        {
            var m = new double[scale.Length + 1, scale.Length + 1];
            for (var i = 0; i < scale.Length; i++)
                m[i, i] = scale[i];
            m[scale.Length, scale.Length] = 1;
            return new Matrix(m);
        }

        public static Matrix RotationMatrix(int i, int j, int dimension, double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            var m = Identity(dimension + 1);
            m[i, i] = cos;
            m[i, j] = -sin;
            m[j, i] = sin;
            m[j, j] = cos;

            return m;
        }

        public static Matrix TranslationMatrix(params double[] translation)
        {
            var m = Identity(translation.Length + 1);
            for (var i = 0; i < translation.Length; i++)
                m[i, m.Columns - 1] = translation[i];
            return m;
        }

        public Matrix Submatrix(int startRow, int endRow, int startColumn, int endColumn)
        {
            if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) || (startRow >= this.Rows) || (endRow < 0) || (endRow >= this.Rows) || (startColumn < 0) || (startColumn >= this.Columns) || (endColumn < 0) || (endColumn >= this.Columns))
                throw new ArgumentException("Argument out of range.");

            Matrix X = new Matrix(endRow - startRow + 1, endColumn - startColumn + 1);
            double[,] x = X.Array;
            for (int i = startRow; i <= endRow; i++)
                for (int j = startColumn; j <= endColumn; j++)
                    x[i - startRow, j - startColumn] = Array[i, j];

            return X;
        }
        public Matrix Submatrix(int[] r, int j0, int j1)
        {
            if ((j0 > j1) || (j0 < 0) || (j0 >= Columns) || (j1 < 0) || (j1 >= Columns))
            {
                throw new ArgumentException("Argument out of range.");
            }

            Matrix X = new Matrix(r.Length, j1 - j0 + 1);
            double[,] x = X.Array;
            for (int i = 0; i < r.Length; i++)
            {
                for (int j = j0; j <= j1; j++)
                {
                    if ((r[i] < 0) || (r[i] >= this.Rows))
                    {
                        throw new ArgumentException("Argument out of range.");
                    }

                    x[i, j - j0] = Array[r[i], j];
                }
            }

            return X;
        }

        public Matrix Solve(Matrix rightHandSide)
        {
            return (Rows == Columns) ? new LuDecomposition(this).Solve(rightHandSide) : new QrDecomposition(this).Solve(rightHandSide);
        }

        public Matrix Transpose()
        {
            var elements = new double[Columns, Rows];
            for (var i = 0; i < Rows; i++)
                for (var j = 0; j < Columns; j++)
                    elements[j, i] = Array[i, j];

            Array = elements;

            return this;
        }

        public Matrix Clone()
        {
            return new Matrix((double[,])Array.Clone());
        }
    }
}
