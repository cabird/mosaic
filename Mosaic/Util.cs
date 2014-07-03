using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    public static class Util
    {
        public static void Shuffle<T>(this List<T> list)
        {
            T tmp;
            Random rand = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                int j = rand.Next(i, list.Count);
                tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
