/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * Program.cs
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

namespace InvertedIndex
{
    public class Program 
    {
        #region Input data params

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

        public static int Main(string[] args)
        {
            var indexer = new InvertedIndex();

            indexer.IndexFolder(folderPaths_125[0], START_INDEX_125, STOP_INDEX_125);

            var query = "funny comedy"; // good comedy movie

            Console.WriteLine($"Search query: {query}");

            var queryResults = indexer.FindInDocs(query);

            Console.WriteLine($"Results: \t Count: {queryResults.Count}");

            foreach (var pair in queryResults)
            {
                var docId = pair.Key;
                var relevance = pair.Value;

                Console.WriteLine($"docId: {docId}, relevance: {relevance}");
                Console.WriteLine($"\t{InvertedIndex.GetDocContentByIndex(folderPaths_125[0], docId).Substring(0, 80)}...");
                Console.WriteLine();
            }

            return 0;
        }
    }

    
}

