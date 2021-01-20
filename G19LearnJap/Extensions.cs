using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace G19LearnJap
{
    public static class Extensions
    {
        /// <summary>
        /// Fisher-Yates shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static T GetRandom<T>(this IList<T> list)
        {
            Random rng = new Random();
            return list[rng.Next(0, list.Count)];
        }
    }
}
