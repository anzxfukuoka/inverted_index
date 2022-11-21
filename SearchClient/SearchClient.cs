using SearchEngine;

namespace SearchClient
{
    internal class SearchClient
    {
        private static string hostAddress = "127.0.0.1";
        private static int port = 1337;

        static void Main(string[] args)
        {
            Client client = new Client(hostAddress, port);
        }
    }
}