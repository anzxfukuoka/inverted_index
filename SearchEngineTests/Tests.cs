using SearchEngine;
using System.IO;

namespace SearchEngineTests
{
    internal class Tests
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

        private static string multiClientAccessResultsPath =
            @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\InvertedIndex\InvertedIndex\Results\client_access.csv";

        static async Task Main(string[] args)
        {
            await IndexingTest();
        }

        public static async Task IndexingTest() 
        {
            // csv header
            await WriteCSVLineAsync(indexingResultsPath, 
                new List<string>(){ "Process count", "dir250.1", "dir250.2", "dir250.3", "dir250.4", "dir1000"});
            
            Task csvWritingTask = Task.CompletedTask;

            for (int processCount = 1; processCount < 32; processCount = (int)Math.Pow(2, processCount))
            {
                List<string> results = new List<string>();
                var indexer = new InvertedIndex();

                foreach (var path in folderPaths_125)
                {
                    Console.WriteLine($"folder {path} \t processCount = {processCount}");
                    var result = InvertedIndexTests.TestFolderIndexingTime(ref indexer, path, START_INDEX_125, STOP_INDEX_125, processCount);
                    results.Add(result.ToString());
                    Console.WriteLine();
                }

                foreach (var path in folderPaths_500)
                {
                    Console.WriteLine($"folder {path} \t processCount = {processCount}");
                    var result = InvertedIndexTests.TestFolderIndexingTime(ref indexer, path, START_INDEX_500, STOP_INDEX_500, processCount);
                    results.Add(result.ToString());
                    Console.WriteLine();
                }

                await csvWritingTask;
                csvWritingTask = WriteCSVLineAsync(indexingResultsPath, results);
            }
        }

        private static async Task WriteCSVLineAsync(string path, List<string> values, char sep = ';', string end = "\n") 
        {
            using (StreamWriter file = new StreamWriter(path, false))
            {
                await file.WriteAsync("");
            }

            using (StreamWriter file = new StreamWriter(path, true)) 
            {
                var line = string.Join(sep, values) + end;

                await file.WriteLineAsync(line);

            }
                
        }

    }
}