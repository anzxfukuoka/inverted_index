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
    public class SearchClient
    {
        private static string hostAddress = "127.0.0.1";
        private static int port = 1337;

        private static Client client;

        public static async Task Main(string[] args)
        {
            client = new Client(hostAddress, port);

            while (true)
            {
                Console.Write("query: ");
                var query = Console.ReadLine();

                Console.WriteLine($"Search query: {query}");

                var queryResult = await client.GetResponse(query);

                Console.WriteLine($"Search result: \n{queryResult}");
            }
        }
    }
}