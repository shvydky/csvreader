using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Breeze.Data.Csv;
using System.IO;
using System.Diagnostics;

namespace Csv {
    class Program {
        static void Main(string[] args) {
            int n = 0, i = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach (var x in new CsvReader(new StreamReader(@"..\..\..\..\etc\sample.csv"))) {
                Console.WriteLine("Row {0}:", ++n);
                i = 0;
                foreach (var c in x) {
                    Console.WriteLine("Column {0}: {1}", i++, c);
                }
            }
            watch.Stop();
            Console.WriteLine("Ellapsed: {0}", watch.Elapsed.ToString());
        }
    }
}
