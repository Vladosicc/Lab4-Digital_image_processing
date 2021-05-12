using ImgConvert;
using Import;
using SCOI_3;
using SCOI_4;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace ImgBytes
{
    public interface IImgBytes
    {
        PixelFormat PixelFormat { get; }
        int BitsPerPixel { get; }

        BitmapSource BitmapSourseOrig { get; }
        Bitmap BitmapOrig { get; }
    }

    public class ImgBytes : IImgBytes
    {
        protected byte[] bytes1;

        protected int _stride;
        protected int _height;
        protected int _width;
        private PixelFormat _pixelFormat;
        private int _bitsPerPixel;

        public bool HasMinValue = false;

        public PixelFormat PixelFormat { get => _pixelFormat; }
        public int BitsPerPixel { get => _bitsPerPixel; }

        public BitmapSource BitmapSourseOrig { get => GetBitmapSource(bytes1); }
        public Bitmap BitmapOrig { get => GetBitmap(bytes1); }


        /// <summary>
        /// Инициализация. 
        /// bool - Применяет Bitmap.Dispose в конце функции или нет
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="DisposeThisBitmap"></param>
        public ImgBytes(Bitmap Source, bool DisposeThisBitmap = true)
        {
            var bmpData = Source.LockBits(new Rectangle(0, 0, Source.Width, Source.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Source.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            _stride = bmpData.Stride;
            _height = Source.Height;
            _width = Source.Width;
            bytes1 = new byte[Size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes1, 0, Size);
            Source.UnlockBits(bmpData);
            _pixelFormat = Source.ToBitmapSource().Format;
            _bitsPerPixel = PixelFormat.BitsPerPixel;
            if (DisposeThisBitmap)
                Source.Dispose();

        }

        public ImgBytes(BitmapSource Source)
        {
            if (Source.Format.BitsPerPixel < 24)
            {
                ImageConvert.To24rgb(ref Source);
            }
            bytes1 = Source.ToByte(out _stride, out _height, out _width, out _pixelFormat);
            _bitsPerPixel = _pixelFormat.BitsPerPixel;
        }

        private BitmapSource GetBitmapSource(byte[] b)
        {
            var Source = b.ToBitmapSource(_stride, _width, _height, _pixelFormat);
            return Source;
        }

        private Bitmap GetBitmap(byte[] b, bool min = false)
        {
            var Source = b.ToBitmapSource(_stride, _width, _height, _pixelFormat);
            return Source.ToBitmap();
        }
    }

    public class ImgMatrix : ImgBytes
    {
        public ImgMatrix(Bitmap source, bool DisposeThisBitmap = true) : base(source, DisposeThisBitmap)
        {
        }

        public ImgMatrix(BitmapSource source) : base(source)
        {
        }

        BitmapSource ApplyMatrixOld(double[][] filterMatrix)
        {
            int ink = BitsPerPixel / 8;
            byte[] bClone = new byte[bytes1.Length];

            int RowCount = filterMatrix.GetLength(0);
            int ColCount = filterMatrix[0].Length;

            //Метод с раздельной обработкой каждой стороны изображения (там где матрица выходит за пределы изображения) и центра
            //Работает чуть быстрее общего метода, но сложней для понимания и отладки
            #region Method1
            //Количество ячеек матрицы, которые могут выходить за пределы по вертикали
            int ErrorRow = RowCount / 2;
            //Количество ячеек матрицы, которые могут выходить за пределы по горизонтали
            int ErrorCol = ColCount / 2;
            //Верхние и нижние грани
            for (int j = 0; j <= ErrorCol; j++)
            {
                //Обработка верхнего края
                for (int i = _width * ink * j; i < _width * ink; i += ink)
                {
                    int rowIndex = (i / ink) / _width;
                    int colIndex = (i / ink) % _width;

                    Point LeftUpper = new Point(colIndex - ColCount / 2, rowIndex - RowCount / 2);

                    //Производим действие с матрицей
                    double[] res = new double[3];
                    int rownow = LeftUpper.Y;
                    int colnow = LeftUpper.X;
                    for (int imatrix = 0; imatrix < RowCount; imatrix++)
                    {
                        colnow = LeftUpper.X;
                        for (int jmatrix = 0; jmatrix < ColCount; jmatrix++)
                        {
                            int rownowtmp = rownow < 0 ? -rownow - 1 : rownow;
                            int colnowtmp = colnow < 0 ? -colnow - 1 : colnow;
                            if (rownowtmp >= _height)
                            {
                                rownowtmp -= 1 + (rownowtmp - _height);
                            }
                            if (colnowtmp >= _width)
                            {
                                colnowtmp -= 1 + (colnowtmp - _width);
                            }
                            res[0] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink];
                            res[1] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 1];
                            res[2] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 2];
                            colnow++;
                        }
                        rownow++;
                    }
                    bClone[i] = _helper.ClampByte(res[0]);
                    bClone[i + 1] = _helper.ClampByte(res[1]);
                    bClone[i + 2] = _helper.ClampByte(res[2]);
                }

                //Обработка нижнего края
                for (int i = _width * (_height - 1) * ink - (j * _width); i < bytes1.Length; i += ink)
                {
                    int rowIndex = (i / ink) / _width;
                    int colIndex = (i / ink) % _width;

                    Point LeftUpper = new Point(colIndex - ColCount / 2, rowIndex - RowCount / 2);

                    //Производим действие с матрицей
                    double[] res = new double[3];
                    int rownow = LeftUpper.Y;
                    int colnow = LeftUpper.X;
                    for (int imatrix = 0; imatrix < RowCount; imatrix++)
                    {
                        colnow = LeftUpper.X;
                        for (int jmatrix = 0; jmatrix < ColCount; jmatrix++)
                        {
                            int rownowtmp = rownow < 0 ? -rownow - 1 : rownow;
                            int colnowtmp = colnow < 0 ? -colnow - 1 : colnow;
                            if (rownowtmp >= _height)
                            {
                                rownowtmp -= 1 + (rownowtmp - _height);
                            }
                            if (colnowtmp >= _width)
                            {
                                colnowtmp -= 1 + (colnowtmp - _width);
                            }
                            res[0] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink];
                            res[1] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 1];
                            res[2] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 2];
                            colnow++;
                        }
                        rownow++;
                    }
                    bClone[i] = _helper.ClampByte(res[0]);
                    bClone[i + 1] = _helper.ClampByte(res[1]);
                    bClone[i + 2] = _helper.ClampByte(res[2]);
                }
            }

            //Левые и правые грани
            for (int j = 0; j < ErrorRow; j++)
            {
                //Левая грань
                for (int i = _width * ErrorCol * ink + j * ink; i < (_height - ErrorCol * 2) * ink * _width + 1; i += _width * ink)
                {
                    int rowIndex = (i / ink) / _width;
                    int colIndex = (i / ink) % _width;

                    Point LeftUpper = new Point(colIndex - ColCount / 2, rowIndex - RowCount / 2);

                    //Производим действие с матрицей
                    double[] res = new double[3];
                    int rownow = LeftUpper.Y;
                    int colnow = LeftUpper.X;
                    for (int imatrix = 0; imatrix < RowCount; imatrix++)
                    {
                        colnow = LeftUpper.X;
                        for (int jmatrix = 0; jmatrix < ColCount; jmatrix++)
                        {
                            int rownowtmp = rownow < 0 ? -rownow - 1 : rownow;
                            int colnowtmp = colnow < 0 ? -colnow - 1 : colnow;
                            if (rownowtmp >= _height)
                            {
                                rownowtmp -= 1 + (rownowtmp - _height);
                            }
                            if (colnowtmp >= _width)
                            {
                                colnowtmp -= 1 + (colnowtmp - _width);
                            }
                            res[0] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink];
                            res[1] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 1];
                            res[2] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 2];
                            colnow++;
                        }
                        rownow++;
                    }
                    bClone[i] = _helper.ClampByte(res[0]);
                    bClone[i + 1] = _helper.ClampByte(res[1]);
                    bClone[i + 2] = _helper.ClampByte(res[2]);
                }

                //Правая грань
                for (int i = _width * ErrorCol * ink + (_width * ink - (j + 1) * ink); i < (_height - ErrorCol * 2) * ink * _width + _width * ink; i += _width * ink)
                {
                    int rowIndex = (i / ink) / _width;
                    int colIndex = (i / ink) % _width;

                    Point LeftUpper = new Point(colIndex - ColCount / 2, rowIndex - RowCount / 2);

                    //Производим действие с матрицей
                    double[] res = new double[3];
                    int rownow = LeftUpper.Y;
                    int colnow = LeftUpper.X;
                    for (int imatrix = 0; imatrix < RowCount; imatrix++)
                    {
                        colnow = LeftUpper.X;
                        for (int jmatrix = 0; jmatrix < ColCount; jmatrix++)
                        {
                            int rownowtmp = rownow < 0 ? -rownow - 1 : rownow;
                            int colnowtmp = colnow < 0 ? -colnow - 1 : colnow;
                            if (rownowtmp >= _height)
                            {
                                rownowtmp -= 1 + (rownowtmp - _height);
                            }
                            if (colnowtmp >= _width)
                            {
                                colnowtmp -= 1 + (colnowtmp - _width);
                            }
                            res[0] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink];
                            res[1] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 1];
                            res[2] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 2];
                            colnow++;
                        }
                        rownow++;
                    }
                    bClone[i] = _helper.ClampByte(res[0]);
                    bClone[i + 1] = _helper.ClampByte(res[1]);
                    bClone[i + 2] = _helper.ClampByte(res[2]);
                }
            }


            for (int j = 0; j < _height - ErrorCol * 2; j++)
            {
                int LastIndex = ErrorRow * _width * ink + (j + 1) * _width * ink - ErrorCol * ink;
                for (int i = ErrorRow * _width * ink + ErrorCol * ink + j * _width * ink; i < LastIndex; i += ink)
                {
                    int rowIndex = (i / ink) / _width;
                    int colIndex = (i / ink) % _width;

                    Point LeftUpper = new Point(colIndex - ColCount / 2, rowIndex - RowCount / 2);

                    //Производим действие с матрицей
                    double[] res = new double[3];
                    int rownow = LeftUpper.Y;
                    int colnow = LeftUpper.X;
                    for (int imatrix = 0; imatrix < RowCount; imatrix++)
                    {
                        colnow = LeftUpper.X;
                        for (int jmatrix = 0; jmatrix < ColCount; jmatrix++)
                        {
                            int rownowtmp = rownow;
                            int colnowtmp = colnow;
                            res[0] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink];
                            res[1] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 1];
                            res[2] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 2];
                            colnow++;
                        }
                        rownow++;
                    }
                    bClone[i] = _helper.ClampByte(res[0]);
                    bClone[i + 1] = _helper.ClampByte(res[1]);
                    bClone[i + 2] = _helper.ClampByte(res[2]);
                }
            }
            #endregion

            //Общий метод
            #region Method2
            ////Обработка всей матрицы
            //for (int i = 0; i < bytes1.Length; i += ink)
            //{
            //    int rowIndex = (i / ink) / _width;
            //    int colIndex = (i / ink) % _width;

            //    Point LeftUpper = new Point(colIndex - ColCount / 2, rowIndex - RowCount / 2);
            //    Point RightBot = new Point(colIndex + ColCount / 2, rowIndex + RowCount / 2);

            //    //Производим действие с матрицей
            //    double[] res = new double[3];
            //    int rownow = LeftUpper.Y;
            //    int colnow = LeftUpper.X;
            //    for (int imatrix = 0; imatrix < RowCount; imatrix++)
            //    {
            //        colnow = LeftUpper.X;
            //        for (int jmatrix = 0; jmatrix < ColCount; jmatrix++)
            //        {
            //            int rownowtmp = rownow < 0 ? -rownow - 1 : rownow;
            //            int colnowtmp = colnow < 0 ? -colnow - 1 : colnow;
            //            if (rownowtmp >= _height)
            //            {
            //                rownowtmp -= 1 + (rownowtmp - _height);
            //            }
            //            if (colnowtmp >= _width)
            //            {
            //                colnowtmp -= 1 + (colnowtmp - _width);
            //            }
            //            res[0] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink];
            //            res[1] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 1];
            //            res[2] += filterMatrix[imatrix][jmatrix] * bytes1[rownowtmp * _width * ink + colnowtmp * ink + 2];
            //            colnow++;
            //        }
            //        rownow++;
            //    }
            //    bClone[i] = _helper.ClampByte(res[0]);
            //    bClone[i + 1] = _helper.ClampByte(res[1]);
            //    bClone[i + 2] = _helper.ClampByte(res[2]);
            //}
            #endregion

            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource ApplyMatrix(MyMatrix filterMatrix, int ThreadUse = 1, int percentheight = 100)
        {
            int ink = BitsPerPixel / 8;

            int heightUse = (int)(_height * (percentheight / 100.0));

            byte[] bClone = new byte[bytes1.Length];

            bytes1.CopyTo(bClone, 0);

            int RowCount = filterMatrix.RowCount;
            int ColCount = filterMatrix.ColCount;

            FilterCpp.ApplyMatrix(bytes1, bClone, bytes1.Length - ((_height - heightUse) * _width * ink), filterMatrix.Array, _width, heightUse, BitsPerPixel, RowCount, ColCount, ThreadUse);

            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource MedianFilter(int r, int ThreadUse = 1, int percentheight = 100)
        {
            int ink = BitsPerPixel / 8;

            int heightUse = (int)(_height * (percentheight / 100.0));

            byte[] bClone = new byte[bytes1.Length];

            bytes1.CopyTo(bClone, 0);

            FilterCpp.MedianFilter(bytes1, bClone, bytes1.Length, _width, _height, r, BitsPerPixel, ThreadUse);

            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }
    }

    public class ImgBinary : ImgBytes
    {
        ulong[] _integralMatrix;
        ulong[] _integralMatrixSqr;

        byte _minPixel = 255;

        public bool RememberIntegralMatrix = true;
        public bool RememberIntegralSqrMatrix = false;

        public void DeleteMatrix()
        {
            _integralMatrix = null;
        }

        public void DeleteMatrixSqr()
        {
            _integralMatrixSqr = null;
        }

        public void CheckAndDeleteMatrix()
        {
            if (RememberIntegralMatrix == false)
                DeleteMatrix();
            if (RememberIntegralSqrMatrix == false)
                DeleteMatrixSqr();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        public BinaryOption BinaryOption { get; set; }

        public ImgBinary(Bitmap source, bool DisposeThisBitmap = true) : base(source, DisposeThisBitmap)
        {
            BinaryCPP.ToGray(bytes1, bytes1.Length, BitsPerPixel, ref _minPixel);
        }

        public ImgBinary(BitmapSource source) : base(source)
        {
            BinaryCPP.ToGray(bytes1, bytes1.Length, BitsPerPixel, ref _minPixel);
        }

        public BitmapSource BinaryGlobal()
        {
            int ink = BitsPerPixel / 8;
            byte[] bClone = new byte[bytes1.Length];

            //Задаем альфа-канал, если он есть
            if (BitsPerPixel == 32)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < bytes1.Length; i += ink)
                    {
                        bClone[i + 3] = bytes1[i + 3];
                    }
                });
            }

            BinaryCPP.Gavrilov(bytes1, bClone, bClone.Length, _width, _height, BitsPerPixel);

            BinaryOption = BinaryOption.GLobal;
            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource BinaryOtsu()
        {
            int ink = BitsPerPixel / 8;
            byte[] bClone = new byte[bytes1.Length];

            //Задаем альфа-канал, если он есть
            if (BitsPerPixel == 32)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < bytes1.Length; i += ink)
                    {
                        bClone[i + 3] = bytes1[i + 3];
                    }
                });
            }

            BinaryCPP.Otsu(bytes1, bClone, bClone.Length, _width, _height, BitsPerPixel);

            BinaryOption = BinaryOption.Otsu;
            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource BinaryNiblek(int a = 3, double k = -0.2)
        {
            int ink = BitsPerPixel / 8;

            if (_integralMatrix == null && _integralMatrixSqr == null)
            {
                _integralMatrix = new ulong[_width * _height];
                _integralMatrixSqr = new ulong[_width * _height];
                BinaryCPP.CalcIntegralMatrix2(bytes1, _width, _height, BitsPerPixel, _integralMatrix, _integralMatrixSqr);
            }
            else
            {
                if (_integralMatrix == null)
                {
                    _integralMatrix = new ulong[_width * _height];
                    BinaryCPP.CalcIntegralMatrix(bytes1, _width, _height, BitsPerPixel, _integralMatrix);
                }
                if (_integralMatrixSqr == null)
                {
                    _integralMatrixSqr = new ulong[_width * _height];
                    BinaryCPP.CalcIntegralSqrMatrix(bytes1, _width, _height, BitsPerPixel, _integralMatrixSqr);
                }
            }

            byte[] bClone = new byte[bytes1.Length];

            //Задаем альфа-канал, если он есть
            if (BitsPerPixel == 32)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < bytes1.Length; i += ink)
                    {
                        bClone[i + 3] = bytes1[i + 3];
                    }
                });
            }

            BinaryCPP.Niblek(bytes1, bClone, bytes1.Length, _integralMatrix, _integralMatrixSqr, _width, _height, BitsPerPixel, a, k);

            BinaryOption = BinaryOption.Niblek;
            if (RememberIntegralMatrix == false)
                DeleteMatrix();
            if (RememberIntegralSqrMatrix == false)
                DeleteMatrixSqr();
            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource BinarySauvola(int a = 3, double k = 0.2)
        {
            int ink = BitsPerPixel / 8;

            if (_integralMatrix == null && _integralMatrixSqr == null)
            {
                _integralMatrix = new ulong[_width * _height];
                _integralMatrixSqr = new ulong[_width * _height];
                BinaryCPP.CalcIntegralMatrix2(bytes1, _width, _height, BitsPerPixel, _integralMatrix, _integralMatrixSqr);
            }
            else
            {
                if (_integralMatrix == null)
                {
                    _integralMatrix = new ulong[_width * _height];
                    BinaryCPP.CalcIntegralMatrix(bytes1, _width, _height, BitsPerPixel, _integralMatrix);
                }
                if (_integralMatrixSqr == null)
                {
                    _integralMatrixSqr = new ulong[_width * _height];
                    BinaryCPP.CalcIntegralSqrMatrix(bytes1, _width, _height, BitsPerPixel, _integralMatrixSqr);
                }
            }

            byte[] bClone = new byte[bytes1.Length];

            //Задаем альфа-канал, если он есть
            if (BitsPerPixel == 32)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < bytes1.Length; i += ink)
                    {
                        bClone[i + 3] = bytes1[i + 3];
                    }
                });
            }

            BinaryCPP.Sauvola(bytes1, bClone, bClone.Length, _integralMatrix, _integralMatrixSqr, _width, _height, BitsPerPixel, a, k);

            BinaryOption = BinaryOption.Sauvola;
            if (RememberIntegralMatrix == false)
                DeleteMatrix();
            if (RememberIntegralSqrMatrix == false)
                DeleteMatrixSqr();
            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource BinaryKristianWolf(int a = 3, double k = 0.5)
        {
            int ink = BitsPerPixel / 8;

            if (_integralMatrix == null && _integralMatrixSqr == null)
            {
                _integralMatrix = new ulong[_width * _height];
                _integralMatrixSqr = new ulong[_width * _height];
                BinaryCPP.CalcIntegralMatrix2(bytes1, _width, _height, BitsPerPixel, _integralMatrix, _integralMatrixSqr);
            }
            else
            {
                if (_integralMatrix == null)
                {
                    _integralMatrix = new ulong[_width * _height];
                    BinaryCPP.CalcIntegralMatrix(bytes1, _width, _height, BitsPerPixel, _integralMatrix);
                }
                if (_integralMatrixSqr == null)
                {
                    _integralMatrixSqr = new ulong[_width * _height];
                    BinaryCPP.CalcIntegralSqrMatrix(bytes1, _width, _height, BitsPerPixel, _integralMatrixSqr);
                }
            }

            byte[] bClone = new byte[bytes1.Length];

            //Задаем альфа-канал, если он есть
            if (BitsPerPixel == 32)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < bytes1.Length; i += ink)
                    {
                        bClone[i + 3] = bytes1[i + 3];
                    }
                });
            }

            BinaryCPP.KristianWolf(bytes1, bClone, bClone.Length, _integralMatrix, _integralMatrixSqr, _width, _height, BitsPerPixel, a, k, _minPixel);

            BinaryOption = BinaryOption.KristianWolf;
            if (RememberIntegralMatrix == false)
                DeleteMatrix();
            if (RememberIntegralSqrMatrix == false)
                DeleteMatrixSqr();
            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource BinaryBradley(int a = 3, double k = 0.15)
        {
            int ink = BitsPerPixel / 8;

            if (_integralMatrix == null)
            {
                _integralMatrix = new ulong[_width * _height];
                BinaryCPP.CalcIntegralMatrix(bytes1, _width, _height, BitsPerPixel, _integralMatrix);
            }

            byte[] bClone = new byte[bytes1.Length];

            //Задаем альфа-канал, если он есть
            if (BitsPerPixel == 32)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < bytes1.Length; i += ink)
                    {
                        bClone[i + 3] = bytes1[i + 3];
                    }
                });
            }

            BinaryCPP.Bradley(bytes1, bClone, bClone.Length, _integralMatrix, _width, _height, BitsPerPixel, a, k);

            BinaryOption = BinaryOption.Bradley;
            if (RememberIntegralMatrix == false)
                DeleteMatrix();
            if (RememberIntegralSqrMatrix == false)
                DeleteMatrixSqr();
            return bClone.ToBitmapSource(_stride, _width, _height, PixelFormat);
        }

        public BitmapSource Binary(BinaryOption option)
        {
            switch (option)
            {
                case BinaryOption.GLobal:
                    return BinaryGlobal();
                case BinaryOption.Otsu:
                    return BinaryOtsu();
                case BinaryOption.Niblek:
                    return BinaryNiblek();
                case BinaryOption.Sauvola:
                    return BinarySauvola();
                case BinaryOption.KristianWolf:
                    return BinaryKristianWolf();
                case BinaryOption.Bradley:
                    return BinaryBradley();
                default:
                    return BinaryOtsu();
            }
        }

        public BitmapSource Binary(BinaryOption option, int rect, double param)
        {
            switch (option)
            {
                case BinaryOption.GLobal:
                    return BinaryGlobal();
                case BinaryOption.Otsu:
                    return BinaryOtsu();
                case BinaryOption.Niblek:
                    return BinaryNiblek(rect, param);
                case BinaryOption.Sauvola:
                    return BinarySauvola(rect, param);
                case BinaryOption.KristianWolf:
                    return BinaryKristianWolf(rect, param);
                case BinaryOption.Bradley:
                    return BinaryBradley(rect, param);
                default:
                    return BinaryOtsu();
            }
        }


        private int SumIntegralMatrix(int[][] matrix, int rect, int indexByte, out int C)
        {
            int rowIndex = (indexByte / (BitsPerPixel / 8)) / _width;
            int colIndex = (indexByte / (BitsPerPixel / 8)) % _width;

            Point LeftUpper = new Point(colIndex - rect / 2, rowIndex - rect / 2);
            Point RightBot = new Point(colIndex + rect / 2, rowIndex + rect / 2);

            int a = -1;
            int b = a;
            int c = a;
            int d = a;

            if (LeftUpper.X <= 0)
            {
                LeftUpper.X = 0;
                a = b = 0;
            }

            if (LeftUpper.Y <= 0)
            {
                LeftUpper.Y = 0;
                a = c = 0;
            }

            if (RightBot.X >= _width)
            {
                RightBot.X = _width - 1;
            }

            if (RightBot.Y >= _height)
            {
                RightBot.Y = _height - 1;
            }

            if (d == -1)
                d = matrix[RightBot.Y][RightBot.X];
            if (a == -1)
                a = matrix[LeftUpper.Y - 1][LeftUpper.X - 1];
            if (c == -1)
                c = matrix[LeftUpper.Y - 1][RightBot.X];
            if (b == -1)
                b = matrix[RightBot.Y][LeftUpper.X - 1];

            C = (RightBot.X - LeftUpper.X + 1) * (RightBot.Y - LeftUpper.Y + 1);

            return (a + d - b - c);
        }

        private double MiddleIntegralMatrix(int[][] matrix, int rect, int indexByte)
        {
            int rowIndex = (indexByte / (BitsPerPixel / 8)) / _width;
            int colIndex = (indexByte / (BitsPerPixel / 8)) % _width;

            Point LeftUpper = new Point(colIndex - rect / 2, rowIndex - rect / 2);
            Point RightBot = new Point(colIndex + rect / 2, rowIndex + rect / 2);

            int a = -1;
            int b = a;
            int c = a;
            int d = a;

            if (LeftUpper.X <= 0)
            {
                LeftUpper.X = 0;
                a = b = 0;
            }

            if (LeftUpper.Y <= 0)
            {
                LeftUpper.Y = 0;
                a = c = 0;
            }

            if (RightBot.X >= _width)
            {
                RightBot.X = _width - 1;
            }

            if (RightBot.Y >= _height)
            {
                RightBot.Y = _height - 1;
            }

            if (d == -1)
                d = matrix[RightBot.Y][RightBot.X];
            if (a == -1)
                a = matrix[LeftUpper.Y - 1][LeftUpper.X - 1];
            if (c == -1)
                c = matrix[LeftUpper.Y - 1][RightBot.X];
            if (b == -1)
                b = matrix[RightBot.Y][LeftUpper.X - 1];

            int CountSumPix = (RightBot.X - LeftUpper.X + 1) * (RightBot.Y - LeftUpper.Y + 1);

            return (a + d - b - c) / (double)CountSumPix;
        }

        private double MiddleSqrIntegralMatrix(int[][] matrix, int rect, int indexByte)
        {
            int rowIndex = (indexByte / (BitsPerPixel / 8)) / _width;
            int colIndex = (indexByte / (BitsPerPixel / 8)) % _width;

            Point LeftUpper = new Point(colIndex - rect / 2, rowIndex - rect / 2);
            Point RightBot = new Point(colIndex + rect / 2, rowIndex + rect / 2);

            int a = -1;
            int b = a;
            int c = a;
            int d = a;

            if (LeftUpper.X <= 0)
            {
                LeftUpper.X = 0;
                a = b = 0;
            }

            if (LeftUpper.Y <= 0)
            {
                LeftUpper.Y = 0;
                a = c = 0;
            }

            if (RightBot.X >= _width)
            {
                RightBot.X = _width - 1;
            }

            if (RightBot.Y >= _height)
            {
                RightBot.Y = _height - 1;
            }

            if (d == -1)
                d = matrix[RightBot.Y][RightBot.X];
            if (a == -1)
                a = matrix[LeftUpper.Y - 1][LeftUpper.X - 1];
            if (c == -1)
                c = matrix[LeftUpper.Y - 1][RightBot.X];
            if (b == -1)
                b = matrix[RightBot.Y][LeftUpper.X - 1];

            int CountSumPix = (RightBot.X - LeftUpper.X + 1) * (RightBot.Y - LeftUpper.Y + 1);

            return (a + d - b - c) / (double)CountSumPix;
        }

        private double DispersionIntegralMatrix(int[][] matrix, int[][] matrixsqr, int rect, int indexByte, out double M)
        {
            int rowIndex = (indexByte / (BitsPerPixel / 8)) / _width;
            int colIndex = (indexByte / (BitsPerPixel / 8)) % _width;

            Point LeftUpper = new Point(colIndex - rect / 2, rowIndex - rect / 2);
            Point RightBot = new Point(colIndex + rect / 2, rowIndex + rect / 2);

            int a = -1;
            int b = a;
            int c = a;
            int d = a;


            int a1 = -1;
            int b1 = a;
            int c1 = a;
            int d1 = a;

            if (LeftUpper.X <= 0)
            {
                LeftUpper.X = 0;
                a = b = a1 = b1 = 0;
            }

            if (LeftUpper.Y <= 0)
            {
                LeftUpper.Y = 0;
                a = c = a1 = c1 = 0;
            }

            if (RightBot.X >= _width)
            {
                RightBot.X = _width - 1;
            }

            if (RightBot.Y >= _height)
            {
                RightBot.Y = _height - 1;
            }

            if (d == -1)
            {
                d = matrix[RightBot.Y][RightBot.X];
                d1 = matrixsqr[RightBot.Y][RightBot.X];
            }
            if (a == -1)
            {
                a = matrix[LeftUpper.Y - 1][LeftUpper.X - 1];
                a1 = matrixsqr[LeftUpper.Y - 1][LeftUpper.X - 1];
            }
            if (c == -1)
            {
                c = matrix[LeftUpper.Y - 1][RightBot.X];
                c1 = matrixsqr[LeftUpper.Y - 1][RightBot.X];
            }
            if (b == -1)
            {
                b = matrix[RightBot.Y][LeftUpper.X - 1];
                b1 = matrixsqr[RightBot.Y][LeftUpper.X - 1];
            }

            int CountSumPix = (RightBot.X - LeftUpper.X + 1) * (RightBot.Y - LeftUpper.Y + 1);
            M = (a + d - b - c) / (double)CountSumPix;

            return ((a1 + d1 - b1 - c1) / (double)CountSumPix) - M * M;
        }

        private int[][] calcIntegralMatrix(byte[][] bytes)
        {
            var res = new int[bytes.Length][];

            for (int i = 0; i < bytes.Length; i++)
            {
                res[i] = new int[bytes[0].Length];
            }

            res[0][0] = bytes[0][0];

            //первый столбец
            for (int i = 1; i < bytes.Length; i++)
            {
                res[i][0] = bytes[i][0] + res[i - 1][0];
            }

            //первая строка
            for (int i = 1; i < bytes[0].Length; i++)
            {
                res[0][i] = bytes[0][i] + res[0][i - 1];
            }

            for (int i = 1; i < bytes.Length; i++)
            {
                for (int j = 1; j < bytes[0].Length; j++)
                {
                    res[i][j] = bytes[i][j] + res[i - 1][j] + res[i][j - 1] - res[i - 1][j - 1];
                }
            }
            return res;
        }

        private int[][] calcIntegralMatrixSqr(byte[][] bytes)
        {
            var sqrMatrix = new int[bytes.Length][];

            for (int i = 0; i < bytes.Length; i++)
            {
                sqrMatrix[i] = new int[bytes[0].Length];
            }

            sqrMatrix[0][0] = bytes[0][0] * bytes[0][0];

            //первый столбец
            for (int i = 1; i < bytes.Length; i++)
            {
                sqrMatrix[i][0] = bytes[i][0] * bytes[i][0] + sqrMatrix[i - 1][0];
            }

            //первая строка
            for (int i = 1; i < bytes[0].Length; i++)
            {
                sqrMatrix[0][i] = bytes[0][i] * bytes[0][i] + sqrMatrix[0][i - 1];
            }

            for (int i = 1; i < bytes.Length; i++)
            {
                for (int j = 1; j < bytes[0].Length; j++)
                {
                    sqrMatrix[i][j] = bytes[i][j] * bytes[i][j] + sqrMatrix[i - 1][j] + sqrMatrix[i][j - 1] - sqrMatrix[i - 1][j - 1];
                }
            }
            return sqrMatrix;
        }

        private int[][] calcIntegralMatrix(byte[][] bytes, ref int[][] sqrMatrix)
        {
            var res = new int[bytes.Length][];
            sqrMatrix = new int[bytes.Length][];

            for (int i = 0; i < bytes.Length; i++)
            {
                res[i] = new int[bytes[0].Length];
                sqrMatrix[i] = new int[bytes[0].Length];
            }

            res[0][0] = bytes[0][0];
            sqrMatrix[0][0] = bytes[0][0] * bytes[0][0];

            //первый столбец
            for (int i = 1; i < bytes.Length; i++)
            {
                res[i][0] = bytes[i][0] + res[i - 1][0];
                sqrMatrix[i][0] = bytes[i][0] * bytes[i][0] + sqrMatrix[i - 1][0];
            }

            //первая строка
            for (int i = 1; i < bytes[0].Length; i++)
            {
                res[0][i] = bytes[0][i] + res[0][i - 1];
                sqrMatrix[0][i] = bytes[0][i] * bytes[0][i] + sqrMatrix[0][i - 1];
            }

            for (int i = 1; i < bytes.Length; i++)
            {
                for (int j = 1; j < bytes[0].Length; j++)
                {
                    res[i][j] = bytes[i][j] + res[i - 1][j] + res[i][j - 1] - res[i - 1][j - 1];
                    sqrMatrix[i][j] = bytes[i][j] * bytes[i][j] + sqrMatrix[i - 1][j] + sqrMatrix[i][j - 1] - sqrMatrix[i - 1][j - 1];
                }
            }
            return res;
        }
    }

    public enum BinaryOption
    {
        GLobal,
        Otsu,
        Niblek,
        Sauvola,
        KristianWolf,
        Bradley
    }
}
