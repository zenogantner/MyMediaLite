using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVM
{
    internal static class SVMExtensions
    {
        public static void SwapIndex<T>(this T[] list, int i, int j)
        {
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
