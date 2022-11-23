using SearchEngine;
using System.Diagnostics;
using System.IO;

namespace SearchEngineTests
{
    internal class Tests
    {
        static async Task Main(string[] args)
        {
            await InvertedIndexTests.IndexingTest();
            await SearchClientTests.MultiClientAccessTest();

            var kostil = Console.ReadLine();
        }
    }

    public class CSVWriter 
    {
        public static async Task WriteCSVLineAsync(string path, List<string> values, char sep = ';', string end = "\n", bool rewrite = false)
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