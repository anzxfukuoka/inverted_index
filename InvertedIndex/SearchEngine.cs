﻿/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * Program.cs
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InvertedIndex
{

    internal class Client 
    {
        private string host;
        private int port;

        private IPEndPoint iPEndPoint;

        internal Client(string host, int port)
        {
            this.host = host;
            this.port = port;

            var addr = Dns.GetHostAddresses(host)[0];

            iPEndPoint = new IPEndPoint(addr, port);
        }

        public async Task SendAsync() 
        {
            using Socket client = new(
                iPEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            await client.ConnectAsync(iPEndPoint);

            while (true)
            {
                // Send message.
                var message = "Dramma<|EOM|>";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                Console.WriteLine($"Socket client sent message: \"{message}\"");

                // Receive ack.
                var buffer = new byte[1_024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response == "<|ACK|>")
                {
                    Console.WriteLine(
                        $"Socket client received acknowledgment: \"{response}\"");
                    break;
                }
                // Sample output:
                //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                //     Socket client received acknowledgment: "<|ACK|>"
            }

            client.Shutdown(SocketShutdown.Both);
        }

    }

    internal class Server 
    {
        private string host;
        private int port;

        private IPEndPoint iPEndPoint;

        private List<Socket> connections = new List<Socket>();

        internal Server(string host, int port) 
        {
            this.host = host;
            this.port = port;

            var addr = Dns.GetHostAddresses(host)[0];

            iPEndPoint = new IPEndPoint(addr, port);
        }

        public async Task StartAsync()
        {
            using Socket listener = new Socket(
                iPEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            listener.Bind(iPEndPoint);
            listener.Listen();

            Console.WriteLine("lstn...");

            var handler = await listener.AcceptAsync();
            
            while (handler.Connected)
            {
                Console.WriteLine("msg");
                // Receive message.
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1 /* is end of message */)
                {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");

                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    break;
                }

            }
        }
    }

    public class SearchEngine
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

        private List<InvertedIndex> indexedFolders = new List<InvertedIndex>();

        private Server server;

        public static async Task<int> Main(string[] args)
        {
            SearchEngine searchEngine = new SearchEngine();

            //searchEngine.Index();
            await searchEngine.StartServerAsync();
            //searchEngine.Query();
            
            return 0;
        }

        private async Task StartServerAsync() 
        {
            var addr = "127.0.0.1";
            var port = 1337;

            server = new Server(addr, port);
            var serverTask = server.StartAsync();

            var client = new Client(addr, port);

            Thread.Sleep(10_000);

            await client.SendAsync();

            await serverTask;
        }

        public void Index() 
        {
            Console.WriteLine("++++++++++ INDEXING STARTED ++++++++++");

            foreach (var folderPath in folderPaths_125)
            {
                var indexer = new InvertedIndex();

                indexer.IndexFolder(folderPath, START_INDEX_125, STOP_INDEX_125);

                indexedFolders.Add(indexer);
            }

            foreach (var folderPath in folderPaths_500)
            {
                var indexer = new InvertedIndex();

                indexer.IndexFolder(folderPath, START_INDEX_500, STOP_INDEX_500);

                indexedFolders.Add(indexer);
            }

            Console.WriteLine("++++++++++ INDEXING FINISHED ++++++++++");

        }

        public void Query() 
        {
            var searchQueries = new Queue<string>();

            searchQueries.Enqueue("The");
            searchQueries.Enqueue("Hadoop");
            searchQueries.Enqueue("Good comedy movie");
            searchQueries.Enqueue("Imagine back to when you were 11 years old.");
            searchQueries.Enqueue("Vampire horror");

            Dictionary<int, double> queryResults = new Dictionary<int, double>();

            while (searchQueries.TryDequeue(out string query))
            {
                Console.WriteLine($"Search query: {query}");

                foreach (var indexer in indexedFolders)
                {
                    queryResults = indexer.FindInDocs(query);
                    Console.WriteLine($"Indexer: {indexer.GetHashCode()}\t Count: {queryResults.Count}");

                    if (queryResults.Count <= 0) { continue; }

                    var best = queryResults.First();
                    var docId = best.Key;
                    var relevance = best.Value;

                    Console.WriteLine($"Best match:\tdocId: {docId}, relevance: {relevance}");
                    Console.WriteLine($"\t{InvertedIndex.GetDocContentByIndex(indexer.folderPath, docId).Substring(0, 80)}...");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Last query results:");

            foreach (var pair in queryResults)
            {
                var docId = pair.Key;
                var relevance = pair.Value;

                Console.WriteLine($"docId: {docId}, relevance: {relevance}");
                Console.WriteLine($"\t{InvertedIndex.GetDocContentByIndex(folderPaths_500[0], docId).Substring(0, 80)}...");
                Console.WriteLine();
            }

        }
    }

    
}
