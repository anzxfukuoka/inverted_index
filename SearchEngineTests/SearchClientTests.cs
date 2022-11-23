using SearchEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineTests
{
    internal class SearchClientTests
    {
        private static string multiClientAccessResultsPath =
            @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\InvertedIndex\InvertedIndex\Results\client_access.csv";

        public static async Task MultiClientAccessTest()
        {
            SearchEngine.SearchEngine se = new SearchEngine.SearchEngine();

            string hostAddress = "127.0.0.1";
            int port = 1337;

            se.Index();
            se.StartServer(hostAddress, port);

            await CSVWriter.WriteCSVLineAsync(
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
                csvWritingTask = CSVWriter.WriteCSVLineAsync(
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
    }
}
