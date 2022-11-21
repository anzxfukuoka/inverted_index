/*
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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InvertedIndex
{
    struct NetMSG
    {
        public const string EOM = "<|EOM|>";
        public const string ACK = "<|ACK|>";
    }
    internal class Client
    {
        private IPEndPoint iPEndPoint;

        private Socket client;

        internal Client(string host, int port)
        {
            var addr = Dns.GetHostAddresses(host)[0];

            iPEndPoint = new IPEndPoint(addr, port);
        }

        public async Task<string> GetResponse(string message) 
        {
            string response = null;

            try
            {
                client = new(
                iPEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

                await client.ConnectAsync(iPEndPoint);

                

                while (true)
                {
                    // Send message.

                    message += NetMSG.EOM;

                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    await client.SendAsync(messageBytes, SocketFlags.None);
                    Console.WriteLine($"Socket client sent message: \"{message}\"");

                    // Receive ack.
                    var buffer = new byte[1_024];
                    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    response = Encoding.UTF8.GetString(buffer, 0, received);
                    
                    if (response == NetMSG.ACK)
                    {
                        Console.WriteLine(
                            $"Socket client received acknowledgment: \"{response}\"");
                        //break;
                    }

                    // Receive response.
                    buffer = new byte[1_024];
                    received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    response = Encoding.UTF8.GetString(buffer, 0, received);

                    if (response.IndexOf(NetMSG.EOM) > -1 /* is end of message */)
                    {
                        Console.WriteLine(
                            $"Socket client received response: \"{response}\"");
                        break;
                    }
                }

            }
            catch (SocketException e) 
            {
                //todo: remove hided exception
            }

            return response.Replace(NetMSG.EOM, "");
        }

    }

    internal class Server 
    {
        private IPEndPoint iPEndPoint;

        private HashSet<Socket> connections = new HashSet<Socket>();

        public delegate string GetResponseMessage(string recivedMessage);
        private GetResponseMessage GetResponseMsg;

        public bool IsRunning { private get; set; } = false;

        private Socket listener;
        
        private Thread serverThread;

        private int timeout = 30_000;

        internal Server(string host, int port, GetResponseMessage GetResponseMsg)
        {
            var addr = Dns.GetHostAddresses(host)[0];
            iPEndPoint = new IPEndPoint(addr, port);

            this.GetResponseMsg = GetResponseMsg;
        }

        private void AddConnection(Socket client) 
        {
            Thread connHanler = new Thread(() => HandleConnection(client));
            connHanler.Start();
        }

        private void HandleConnection(Socket client) 
        {
            connections.Add(client);

            client.ReceiveTimeout = timeout;
            client.SendTimeout = timeout;

            while (client.Connected)
            {
                //Console.WriteLine(handler.Connected + " " + handler.Poll(1, SelectMode.SelectRead));
                // Receive message.
                var buffer = new byte[1_024];

                int received;
                string response;
                try
                {
                    received = client.Receive(buffer, SocketFlags.None);
                    response = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.Write(response);
                }
                catch (System.Net.Sockets.SocketException e) 
                {
                    //Console.WriteLine($"Connection {client.RemoteEndPoint} is down");
                    Console.WriteLine($"Connection {client.GetHashCode} is down");

                    break;

                }
                
                if (response.IndexOf(NetMSG.EOM) > -1 /* is end of message */)
                {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(NetMSG.EOM, "")}\"");

                    
                    var ackMessage = NetMSG.ACK;
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);

                    client.Send(echoBytes, 0);

                    var replyMsg = GetResponseMsg(response);
                    var replyBytes = Encoding.UTF8.GetBytes(replyMsg);

                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    client.Send(replyBytes, 0);

                    Console.WriteLine(
                       $"Socket server sent response: \"{response}\"");

                    //client.Disconnect(false);

                    //break;
                }

            }

            client.Close();

            connections.Remove(client);
        }

        private void MainLoop() 
        {
            while (IsRunning) 
            {
                try
                {
                    listener.Listen();

                    var conn = listener.Accept();

                    Console.WriteLine($"New connection: {conn.RemoteEndPoint}");

                    AddConnection(conn);
                }
                catch (SocketException e) 
                {
                    break;
                }
                
            }
        }


        public void Start()
        {
            IsRunning = true;

            listener = new Socket(
               iPEndPoint.AddressFamily,
               SocketType.Stream,
               ProtocolType.Tcp
               );

            listener.Bind(iPEndPoint);

            serverThread = new Thread(MainLoop);
            serverThread.Start();
        }

        public void Stop() 
        {
            IsRunning = false;

            if (listener != null) 
            {
                listener.Close();
            }

            foreach (var conn in connections)
            {
                conn.Close();
                connections.Remove(conn);
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

        public static int Main(string[] args)
        {
            SearchEngine searchEngine = new SearchEngine();

            //searchEngine.Index();
            searchEngine.StartServer();
            //searchEngine.Query();
            
            return 0;
        }

        private async void StartServer() 
        {
            var addr = "127.0.0.1";
            var port = 1337;

            server = new Server(addr, port, (msg) => $"ECHO[{msg}]");
            server.Start();

            Thread.Sleep(2_000);

            var client = new Client(addr, port);

            Console.WriteLine(await client.GetResponse("Horror dramma"));

            server.Stop();
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

