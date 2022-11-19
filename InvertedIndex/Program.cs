﻿/*
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
            var invidx = new InvertedIndex();

            invidx.IndexFolder(folderPaths_125[0], START_INDEX_125, STOP_INDEX_125);

            var query = "comedy"; // good comedy movie

            Console.WriteLine($"Search query: {query}");

            var queryResults = invidx.FindInDocs(query);

            Console.WriteLine($"Results: \t Count: {queryResults.Count}");

            foreach (var queryResult in queryResults)
            {
                Console.WriteLine($"docId: {queryResult.id}, tf: {queryResult.tf}");
                Console.WriteLine($"\t{InvertedIndex.GetDocContentByIndex(folderPaths_125[0], queryResult.id).Substring(0, 80)}...");
                Console.WriteLine();
            }

            return 0;
        }
    }

    
}

