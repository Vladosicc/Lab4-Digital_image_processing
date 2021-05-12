using System.Runtime.InteropServices;

namespace Import
{

    class BinaryCPP
    {
        [DllImport("SCOIDLL.dll", EntryPoint = "ToGray", CallingConvention = CallingConvention.StdCall)]
        static public extern void ToGray(byte[] bytes1, int length, int BitPerPixel, ref byte minPixel);

        [DllImport("SCOIDLL.dll", EntryPoint = "Gavrilov", CallingConvention = CallingConvention.StdCall)]
        static public extern void Gavrilov(byte[] bytes1, byte[] bClone, int length, int width, int height, int BitPerPixel);

        [DllImport("SCOIDLL.dll", EntryPoint = "Otsu", CallingConvention = CallingConvention.StdCall)]
        static public extern void Otsu(byte[] bytes1, byte[] bClone, int length, int width, int height, int BitPerPixel);

        [DllImport("SCOIDLL.dll", EntryPoint = "CalcIntegralMatrix", CallingConvention = CallingConvention.StdCall)]
        static public extern void CalcIntegralMatrix(byte[] bytes, int width, int height, int BitPerPixel, ulong[] Out);

        [DllImport("SCOIDLL.dll", EntryPoint = "CalcIntegralMatrix2", CallingConvention = CallingConvention.StdCall)]
        static public extern void CalcIntegralMatrix2(byte[] bytes, int width, int height, int BitPerPixel, ulong[] Out, ulong[] Out2);

        [DllImport("SCOIDLL.dll", EntryPoint = "CalcIntegralSqrMatrix", CallingConvention = CallingConvention.StdCall)]
        static public extern void CalcIntegralSqrMatrix(byte[] bytes, int width, int height, int BitPerPixel, ulong[] Out);

        [DllImport("SCOIDLL.dll", EntryPoint = "GetDispersion", CallingConvention = CallingConvention.StdCall)]
        static public extern void GetDispersion(ulong[] integralMat, ulong[] integralMatSqr, int height, int width, int BitPerPixel, int indexByte, int rect, ref double M);

        [DllImport("SCOIDLL.dll", EntryPoint = "Niblek", CallingConvention = CallingConvention.StdCall)]
        static public extern void Niblek(byte[] bytes1, byte[] bClone, int length, ulong[] integralMat, ulong[] integralMatSqr, int width, int height, int BitPerPixel, int a, double k);

        [DllImport("SCOIDLL.dll", EntryPoint = "Sauvola", CallingConvention = CallingConvention.StdCall)]
        static public extern void Sauvola(byte[] bytes1, byte[] bClone, int length, ulong[] integralMat, ulong[] integralMatSqr, int width, int height, int BitPerPixel, int a, double k);

        [DllImport("SCOIDLL.dll", EntryPoint = "KristianWolf", CallingConvention = CallingConvention.StdCall)]
        static public extern void KristianWolf(byte[] bytes1, byte[] bClone, int length, ulong[] integralMat, ulong[] integralMatSqr, int width, int height, int BitPerPixel, int a, double k, byte minPixel);

        [DllImport("SCOIDLL.dll", EntryPoint = "Bradley", CallingConvention = CallingConvention.StdCall)]
        static public extern void Bradley(byte[] bytes1, byte[] bClone, int length, ulong[] integralMat, int width, int height, int BitPerPixel, int a, double k);
    }

    class FilterCpp
    {
        [DllImport("SCOIDLL.dll", EntryPoint = "ApplyMatrix", CallingConvention = CallingConvention.StdCall)]
        static public extern void ApplyMatrix(byte[] bytes1, byte[] bClone, int length, double[] filterArray, int width, int height, int BitPerPixel, int RowCount, int ColCount, int ThreadUse = 1);

        [DllImport("SCOIDLL.dll", EntryPoint = "GaussMatrix", CallingConvention = CallingConvention.StdCall)]
        static public extern void GaussMatrix(int r, double sig, double[] output);

        [DllImport("SCOIDLL.dll", EntryPoint = "MedianFilter", CallingConvention = CallingConvention.StdCall)]
        static public extern void MedianFilter(byte[] bytes1, byte[] bClone, int length, int width, int height, int r, int BitPerPixel, int ThreadUse = 1);
    }

    class ThreadCPP
    {
        [DllImport("SCOIDLL.dll", EntryPoint = "ThreadCount", CallingConvention = CallingConvention.StdCall)]
        static public extern int ThreadCount();
    }

}
