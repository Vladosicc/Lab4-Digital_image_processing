using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SCOI_3
{
    public static class _helper
    {
        /// <summary>
        /// Возвращает массив строк, до \n
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string[] GetAllLine(this string strOrig)
        {
            var str = strOrig.Clone() as string;
            List<string> res = new List<string>();
            while (true)
            {
                int indexOfEnter = str.IndexOf('\n');
                if(indexOfEnter == -1)
                {
                    if(str.Length != 0)
                    {
                        str = Regex.Replace(str, @"\s{2,}", " ", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                        if (str.Last() == ' ' || str.Last() == '\t')
                            str = str.Remove(str.Length - 1);
                        res.Add(str);
                    }
                    break;
                }
                var newstr = str.Substring(0, indexOfEnter);
                newstr = Regex.Replace(newstr, @"\s{2,}", " ", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                newstr = newstr.Replace("\r", "");
                if (newstr.Last() == ' ' || newstr.Last() == '\t')
                    newstr = newstr.Remove(newstr.Length - 1);
                res.Add(newstr);
                str = str.Substring(indexOfEnter + 1);
            }
            return res.ToArray();
        }

        /// <summary>
        /// Возвращает следующее слово в строке (до пробела)
        /// Скипает строку на начало следующего слова
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetWord(this string str)
        {
            string res;
            if (str == null)
                return null;
            int index = str.IndexOf(' ');
            if (index == -1)
            {
                if(str != null && str != " " && str != "")
                {
                    return str;
                }
                return null;
            }
            res = str.Substring(0, index);
            return res;
        }

        /// <summary>
        /// /// Скипает строку на начало следующего слова
        /// </summary>
        /// <returns></returns>
        public static string SkipWord(this string str)
        {
            int index = str.IndexOf(' ');
            if (index == -1)
                return null;
            return str.Substring(index + 1);
        }

        public static string[] DeleteNoise(string[] Lines)
        {
            List<string> res = new List<string>();
            if (Lines.Length == 0)
                return res.ToArray();
            for(int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = Regex.Replace(Lines[i], @"\s{2,}", " ", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                Lines[i] = Regex.Replace(Lines[i], @"\t{2,}", " ", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                Lines[i] = Lines[i].Replace("\r", "");
                if (Lines[i] == "" || Lines[i] == " ")
                    continue;
                if (Lines[i].Last() == ' ' || Lines[i].Last() == '\t')
                    Lines[i] = Lines[i].Remove(Lines[i].Length - 1);
                res.Add(Lines[i]);
            }
            return res.ToArray();
        }

        public static double Sum(this double[] arr, int finallyIndex = 255)
        {
            double res = 0;
            for (int i = 0; i < finallyIndex; i++)
            {
                res += arr[i];
            }
            return res;
        }

        public static double SumMult(this double[] arr, int finallyIndex = 255)
        {
            double res = 0;
            for (int i = 0; i < finallyIndex; i++)
            {
                res += arr[i] * i;
            }
            return res;
        }

        public static byte[][] To2xArray(this byte[] arr, int height, int width, int bitPerPix)
        {
            byte[][] res = new byte[width][];
            List<int> ad = new List<int>();
            for (int i = 0; i < width; i++)
            {
                res[i] = new byte[height];
                for (int j = 0; j < height * 4; j += 4)
                {
                    res[i][j / 4] = arr[j * width + i * (bitPerPix / 8)];
                    ad.Add(j * width + i * (bitPerPix / 8));
                }
            }
            return res;
        }

        public static byte[][] To2xArray2(this byte[] arr, int height, int width, int bitPerPix)
        {
            int bInPix = (bitPerPix / 8);
            byte[][] res = new byte[height][];
            for (int i = 0; i < height; i++)
            {
                res[i] = new byte[width];
                for (int j = 0; j < width; j++)
                {
                    res[i][j] = arr[i * width * bInPix + j * bInPix];
                }
            }
            return res;
        }

        public static byte ClampByte(double d)
        {
            if (d > 255)
                return 255;
            if (d < 0)
                return 0;
            return (byte)d;
        }

        //Байты идут по столбцам
        public static double SumMatrix(byte[][] arr, int indexByte, int height, int width, int a, int BitsPerPix)
        {
            int sum = 0;
            int rowIndex = (indexByte / (BitsPerPix / 8)) / width;
            int colIndex = (indexByte / (BitsPerPix / 8)) % width;

            int rowNow = rowIndex - a / 2;
            int colNow = colIndex - a / 2;

            int countPixInA = 0;

            for (int i = 0; i < a; i++)
            {
                if (colNow < 0)
                {
                    colNow++;
                    continue;
                }
                if (colNow >= width)
                {
                    colNow--;
                    continue;
                }
                rowNow = rowIndex - a / 2;
                for (int j = 0; j < a; j++)
                {
                    if (rowNow < 0)
                    {
                        rowNow++;
                        continue;
                    }
                    if (rowNow >= height)
                    {
                        rowNow--;
                        continue;
                    }
                    sum += arr[rowNow][colNow];
                    rowNow++;
                    countPixInA++;
                }
                colNow++;
            }
            return sum / (double)countPixInA;
        }

        public static double SumMatrixSqr(byte[][] arr, int indexByte, int height, int width, int a, int BitsPerPix)
        {
            int sum = 0;
            int rowIndex = (indexByte / (BitsPerPix / 8)) / width;
            int colIndex = (indexByte / (BitsPerPix / 8)) % width;

            int rowNow = rowIndex - a / 2;
            int colNow = colIndex - a / 2;

            int countPixInA = 0;

            for (int i = 0; i < a; i++)
            {
                if (colNow < 0)
                {
                    colNow++;
                    continue;
                }
                if (colNow >= width)
                {
                    colNow--;
                    continue;
                }
                rowNow = rowIndex - a / 2;
                for (int j = 0; j < a; j++)
                {
                    if (rowNow < 0)
                    {
                        rowNow++;
                        continue;
                    }
                    if (rowNow >= height)
                    {
                        rowNow--;
                        continue;
                    }
                    sum += arr[rowNow][colNow] * arr[rowNow][colNow];
                    rowNow++;
                    countPixInA++;
                }
                colNow++;
            }
            return sum / (double)countPixInA;
        }
    }

}



