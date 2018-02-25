using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ProcessIfNotEmpty
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            int? result = GetItemsLinq().ProcessIfNotEmpty(items => items.Sum(), () => (int?)null);

            sw.Stop();

            Console.WriteLine(result);
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadKey();
        }

        private static IEnumerable<int> GetItemsLinq()
        {
            return Enumerable
                   .Range(0, 100)
                   .Reverse()
                   .Select(
                       i =>
                       {
                           Thread.Sleep(100);
                           return i;
                       })
                   .Where(i => i < 10);
        }
    }
}
