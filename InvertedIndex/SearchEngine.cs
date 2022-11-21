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
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InvertedIndex
{
    public class SearchEngine
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
            Console.WriteLine("++++++++++ INDEXING STARTED ++++++++++");

            List<InvertedIndex> indexedFolders = new List<InvertedIndex>();

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

            return 0;
        }
    }

    
}

