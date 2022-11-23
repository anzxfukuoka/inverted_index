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
        #region Indexing data params

        // вар
        public const int V = 34;

        public const int COUNT_125 = 12500;
        public const int COUNT_500 = 50000;

        public const int START_INDEX_125 = (COUNT_125 / 50) * (V - 1);
        public const int START_INDEX_500 = (COUNT_500 / 50) * (V - 1);

        public const int STOP_INDEX_125 = (COUNT_125 / 50) * V;
        public const int STOP_INDEX_500 = (COUNT_500 / 50) * V;


        public static List<string> folderPaths_125 = new List<string>{
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\test\neg",
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\test\pos",
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\train\neg",
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\train\pos"
        };

        public static List<string> folderPaths_500 = new List<string>{
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\train\unsup",
        };

        #endregion

        private static string indexingResultsPath =
            @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\InvertedIndex\InvertedIndex\Results\indexing.csv";

        public static async Task IndexingTest()
        {
            // csv header
            await CSVWriter.WriteCSVLineAsync(indexingResultsPath,
                new List<string>(){
                    "Process count",
                    "dir250.1",
                    "dir250.2",
                    "dir250.3",
                    "dir250.4",
                    "dir1000"
                },
                rewrite: true);

            Task csvWritingTask = Task.CompletedTask;

            for (int i = 0; i < 5; i++)
            {
                var processCount = (int)Math.Pow(2, i);

                List<string> results = new List<string>();
                var indexer = new InvertedIndex();

                foreach (var path in folderPaths_125)
                {
                    Console.WriteLine($"folder {path} \t processCount = {processCount}");
                    var result = TestFolderIndexingTime(ref indexer, path, START_INDEX_125, STOP_INDEX_125, processCount);
                    results.Add(result.ToString());
                    Console.WriteLine();
                }

                foreach (var path in folderPaths_500)
                {
                    Console.WriteLine($"folder {path} \t processCount = {processCount}");
                    var result = TestFolderIndexingTime(ref indexer, path, START_INDEX_500, STOP_INDEX_500, processCount);
                    results.Add(result.ToString());
                    Console.WriteLine();
                }

                await csvWritingTask;
                csvWritingTask = CSVWriter.WriteCSVLineAsync(indexingResultsPath, results);
            }
        }

        public static long TestFolderIndexingTime(ref InvertedIndex indexer, string path, int startIndex, int stopIndex, int processCount)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            indexer.IndexFolder(path, startIndex, stopIndex, processCount);

            stopwatch.Stop();

            long result = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"Time ms: \t{result}");

            return result;
        }

    }
}
