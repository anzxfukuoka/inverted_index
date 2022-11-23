using SearchEngine;
using System.Diagnostics;
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
            await MultiClientAccessTest();

            var kostil = Console.ReadLine();
        }

        public static async Task MultiClientAccessTest() 
        {
            SearchEngine.SearchEngine se = new SearchEngine.SearchEngine();

            string hostAddress = "127.0.0.1";
            int port = 1337;

            se.Index();
            se.StartServer(hostAddress, port);

            await WriteCSVLineAsync(
                    multiClientAccessResultsPath,
                    new List<string>() { "Clients count", "Avarage response time" }, 
                    rewrite: true);

            Task csvWritingTask = Task.CompletedTask;

            for (int i = 0; i < 5; i++)
            {
                List<long> responses = new List<long>();
                Thread t = null;

                int clientCount = (int)Math.Pow(2, i);

                for (int c = 1; c < clientCount; c++)
                {
                    t = new Thread(() => TestClientConn(hostAddress, port, responses));
                    t.Start();
                }

                t.Join();

                var avarageResponseTime = responses.Sum() / responses.Count;

                await csvWritingTask;
                csvWritingTask = WriteCSVLineAsync(
                    multiClientAccessResultsPath,
                    new List<string>() { clientCount.ToString(), avarageResponseTime.ToString() });
            }

        }

        public static async void TestClientConn(string hostAddress, int port, List<long> responses)
        {  
            Client client = new Client(hostAddress, port);

            var searchQueries = new Queue<string>();

            searchQueries.Enqueue("The");
            searchQueries.Enqueue("Hadoop");
            searchQueries.Enqueue("Good comedy movie");
            searchQueries.Enqueue("Imagine back to when you were 11 years old.");
            searchQueries.Enqueue("Vampire horror");

            string queryResult = "";
            Task<string> queryTask;

            while (searchQueries.TryDequeue(out string query))
            {
                Console.WriteLine($"Client [{Thread.CurrentThread.Name}] sending query [{query}]");

                var sw = new Stopwatch();
                sw.Start();

                queryTask = client.GetResponse(query);
                queryResult = await queryTask;

                sw.Stop();

                Console.WriteLine($"Client [{Thread.CurrentThread.Name}] recived response on query [{query}]: {queryResult.Split('\n')[0]}...");

                responses.Add(sw.ElapsedMilliseconds);
            }
        }

        public static async Task IndexingTest() 
        {
            // csv header
            await WriteCSVLineAsync(indexingResultsPath, 
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
                csvWritingTask = WriteCSVLineAsync(indexingResultsPath, results);
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

        private static async Task WriteCSVLineAsync(string path, List<string> values, char sep = ';', string end = "\n", bool rewrite = false) 
        {
            if (rewrite) 
            {
                using (StreamWriter file = new StreamWriter(path, false))
                {
                    await file.WriteAsync("");
                }
            }

            using (StreamWriter file = new StreamWriter(path, true)) 
            {
                var line = string.Join(sep, values) + end;

                await file.WriteLineAsync(line);

            }
                
        }

    }
}