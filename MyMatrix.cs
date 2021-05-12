using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Import;
using SCOI_3;

namespace SCOI_4
{
    public class MyMatrix
    {
        double[][] _matrix;

        int _round = 8;

        /// <summary>
        /// Матрица
        /// </summary>
        public double[][] Matrix { get => _matrix; }

        /// <summary>
        /// Проверет нечетность измерений и прямоугольность матрицы
        /// </summary>
        public bool IsOddNumber { get => CheckOdd(); }

        /// <summary>
        /// Количество строк
        /// </summary>
        public int RowCount { get => _matrix.GetLength(0); }

        /// <summary>
        /// Количество столбцов
        /// </summary>
        public int ColCount { get => _matrix[0].Length; }

        /// <summary>
        /// Представление матрицы в виде массива
        /// </summary>
        public double[] Array { get => toArray(); }

        /// <summary>
        /// Количество цифр после запятой
        /// </summary>
        public int Round { get => _round; set => _round = value; }

        /// <summary>
        /// Парсит строку в матрицу
        /// null, если распарсить не удалось
        /// </summary>
        /// <param name="str"></param>
        public MyMatrix(string str)
        {
            str = str.Replace('.', ',');

            var Lines = str.Split('\n');
            Lines = _helper.DeleteNoise(Lines);

            _matrix = new double[Lines.Length][];

            for(int i = 0; i < Lines.Length; i++)
            {
                try
                {

                    var Words = Lines[i].Split(' ');
                    _matrix[i] = new double[Words.Length];
                    for (int j = 0; j < Words.Length; j++)
                    {
                        _matrix[i][j] = Fraction.Parse(Words[j]);
                    }
                }
                catch (Exception ex)
                {
                    _matrix = null;
                    return;
                }
            }
            
        }

        public MyMatrix(double[] arr, int r)
        {
            _matrix = new double[r][];
            for (int i = 0; i < r; i++)
            {
                _matrix[i] = new double[r];
                for (int j = 0; j < r; j++)
                {

                    _matrix[i][j] = arr[i * r + j];
                }
            }
        }

        public MyMatrix(int r, double sigm)
        {
            double[] arr = new double[(r * 2 + 1) * (r * 2 + 1)];
            FilterCpp.GaussMatrix(r, sigm, arr);

            _matrix = new double[(r * 2 + 1)][];
            for (int i = 0; i < (r * 2 + 1); i++)
            {
                _matrix[i] = new double[(r * 2 + 1)];
                for (int j = 0; j < (r * 2 + 1); j++)
                {
                    _matrix[i][j] = arr[i * (r * 2 + 1) + j];
                }
            }
        }

        bool CheckOdd()
        {
            if(_matrix.GetLength(0) % 2 != 1)
            {
                return false;
            }

            if(_matrix[0] == null)
            {
                return false;
            }

            int l = _matrix[0].Length;

            for (int i = 1; i < _matrix.Length; i++)
            {
                if (_matrix[i].Length != l)
                    return false;
            }

            if(l % 2 != 1)
            {
                return false;
            }

            return true;
        }

        double[] toArray()
        {
            List<double> res = new List<double>();

            for(int i = 0; i < _matrix.GetLength(0); i++)
            {
                for(int j = 0; j < _matrix[i].Length; j++)
                {
                    res.Add(_matrix[i][j]);
                }
            }

            return res.ToArray();
        }

        override public string ToString()
        {
            string res = "";
            for(int i = 0; i < RowCount; i++)
            {
                for(int j = 0; j < ColCount; j++)
                {
                    res += Math.Round(_matrix[i][j], Round).ToString() + " ";
                }
                res += "\n";
            }
            return res;
        }
    }

    class Fraction
    {
        public static double Parse(string str)
        {
            var Numbers = str.Split('/');
            double Numerator = double.Parse(Numbers[0]);
            double DeNumerator = 1;
            if (Numbers.Length > 1)
                DeNumerator = double.Parse(Numbers[1]);
            return Numerator / DeNumerator;
        }
    }
}
