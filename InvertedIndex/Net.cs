using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

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

        public async Task<Dictionary<int, double>> GetResponse(string message)
        {
            Dictionary<int, double> foundDocs = new Dictionary<int, double>();

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

                    var response = Encoding.UTF8.GetString(buffer, 0, received);



                    if (response == NetMSG.ACK)
                    {
                        Console.WriteLine(
                            $"Socket client received acknowledgment: \"{response}\"");
                        //break;
                    }

                    // Receive response.
                    buffer = new byte[1_024];
                    received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    //response = Encoding.UTF8.GetString(buffer, 0, received);

                    var binFormatter = new BinaryFormatter();
                    var mStream = new MemoryStream(buffer);

                    foundDocs = (Dictionary<int, double>)binFormatter.Deserialize(mStream);
                    break;

                    //if (response.IndexOf(NetMSG.EOM) > -1 /* is end of message */)
                    //{
                    //    Console.WriteLine(
                    //        $"Socket client received response: \"{response}\"");
                    //    break;
                    //}
                }

            }
            catch (SocketException e)
            {
                //todo: remove hided exception
            }

            return foundDocs;//response.Replace(NetMSG.EOM, "");
        }

    }

    internal class Server
    {
        private IPEndPoint iPEndPoint;

        private HashSet<Socket> connections = new HashSet<Socket>();

        public delegate Dictionary<int, double> GetResponse(string recivedMessage);
        private GetResponse getResponse;

        public bool IsRunning { private get; set; } = false;

        private Socket listener;

        private Thread serverThread;

        private int timeout = 30_000;

        internal Server(string host, int port, GetResponse getResponse)
        {
            var addr = Dns.GetHostAddresses(host)[0];
            iPEndPoint = new IPEndPoint(addr, port);

            this.getResponse = getResponse;
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

                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    Dictionary<int, double> foundDocs = getResponse(response.Replace(NetMSG.EOM, ""));

                    var binFormatter = new BinaryFormatter();
                    var mStream = new MemoryStream();

                    binFormatter.Serialize(mStream, foundDocs);

                    var replyBytes = mStream.ToArray();  

                    client.Send(replyBytes, 0);

                    Console.WriteLine(
                       $"Socket server sent response: \"{foundDocs}\"");

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
}
