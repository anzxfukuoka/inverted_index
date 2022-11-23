using SearchEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineTests
{
    internal class InvertedIndexTests
    {
        public static long TestFolderIndexingTime(string path, int startIndex, int stopIndex, int processCount) 
        {
            var stopwatch = new Stopwatch();
            var indexer = new InvertedIndex();

            stopwatch.Start();

            indexer.IndexFolder(path, startIndex, stopIndex, processCount);

            stopwatch.Stop();

            long result = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"Time ms: \t{result}");

            return result;
        }

    }
}
