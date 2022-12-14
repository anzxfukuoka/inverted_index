/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * Tests.cs
 */

using SearchEngine;
using System.Diagnostics;
using System.IO;

namespace SearchEngineTests
{
    /// <summary>
    /// Tests for SearchEngine and SearchClient projects
    /// </summary>
    internal class Tests
    {
        static async Task Main(string[] args)
        {
            await InvertedIndexTests.IndexingTest();
            //await SearchClientTests.MultiClientAccessTest();

            var kostil = Console.ReadLine();
        }
    }

    // todo: move to utils
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