using SearchEngine;
using System;

namespace SearchClient
{
    internal class SearchClient
    {
        private static string hostAddress = "127.0.0.1";
        private static int port = 1337;

        static async Task Main(string[] args)
        {
            Thread t = null;

            for (int i = 0; i < 3; i++)
            {
               t  = new Thread(() => TestConn());
               t.Start();
            }

            t.Join();

            var kostil = Console.ReadLine();
        }

        public static async void TestConn() 
        {
            Client client = new Client(hostAddress, port);

            var searchQueries = new Queue<string>();

            searchQueries.Enqueue("The");
            searchQueries.Enqueue("Hadoop");
            searchQueries.Enqueue("Good comedy movie");
            searchQueries.Enqueue("Imagine back to when you were 11 years old.");
            searchQueries.Enqueue("Vampire horror");

            string queryResult = "";

            while (searchQueries.TryDequeue(out string query))
            {
                Console.WriteLine($"Search query: {query}");

                //var kostil = Console.ReadLine();

                queryResult = await client.GetResponse(query);

                Console.WriteLine($"Search result: \n{queryResult}");
            }
        }
    }
}