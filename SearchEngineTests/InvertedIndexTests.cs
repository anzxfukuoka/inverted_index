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
        public static void TestFolderIndexingTime(string path, int startIndex, int stopIndex, int processCount) 
        {
            var stopwatch = new Stopwatch();
            var indexer = new InvertedIndex();

            stopwatch.Start();

            indexer.IndexFolder(path, startIndex, stopIndex, processCount);

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedTicks);
        }

    }
}
