/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * SearchClient.cs
 */

using SearchEngine;
using System;

namespace SearchClient
{
    /// <summary>
    /// Network client for SearchEngine 
    /// </summary>
    internal class SearchClient
    {
        private static string hostAddress = "127.0.0.1";
        private static int port = 1337;

        public static int Main(string[] args)
        {
            Thread t = null;

            for (int i = 0; i < 3; i++)
            {
               t  = new Thread(() => TestConn());
               t.Start();
            }

            t.Join();

            var kostil = Console.ReadLine();
            return 0;
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

                queryResult = await client.GetResponse(query);

                Console.WriteLine($"Search result: \n{queryResult}");
            }
        }
    }
}