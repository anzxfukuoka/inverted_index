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
    /// <summary>
    /// Stores info of specific word
    /// </summary>
    public class WordData
    {
        /// <summary>
        /// Stores info of specific document 
        /// </summary>
        public class Document 
        {
            /// <summary>
            /// document id
            /// </summary>
            public readonly int id;

            /// <summary>
            /// number of occurrences of word in a document
            /// </summary>
            public int occurrsCount;

            /// <summary>
            /// count of words in a document
            /// </summary>
            public readonly int wordCount;

            /// <summary>
            /// term frequency
            /// </summary>
            public double tf { get { return (double)occurrsCount / (double)wordCount; } }

            public Document(int id, int occurrsCount, int wordCount)
            {
                this.id = id;
                this.occurrsCount = occurrsCount;
                this.wordCount = wordCount;
            }
        }

        /// <summary>
        /// List of documents
        /// </summary>
        public List<Document> docs = new List<Document>();
        
        /// <summary>
        /// Number of documents with this word
        /// </summary>
        public int docCount { get { return docs.Count; } }

        /// <summary>
        /// Inverse document frequency
        /// </summary>
        public double idf = 1;

        public WordData() 
        {
        }

        public void Combine(WordData other) 
        {
            docs.AddRange(other.docs);
            //Console.WriteLine("new combine");
        }

        public void AddDocument(int id, int occurrsCount, int wordCount) 
        {
            var doc = new Document(id, occurrsCount, wordCount);
            docs.Add(doc);
        }
    }

    public class InvertedIndex
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

        public const string QUERY_FORMAT = "{0}_*.txt";

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

        public Dictionary<string, WordData> indexedData;

        public static int Main(string[] args)
        {
            var invidx = new InvertedIndex();

            invidx.IndexFolder(folderPaths_125[0], START_INDEX_125, STOP_INDEX_125);

            var query = "The";

            Console.WriteLine($"Search query: {query}");

            var queryResults = invidx.FindInDocs(query);

            Console.WriteLine($"Results: \t Count: {queryResults.Count}");

            foreach (var queryResult in queryResults) 
            {
                Console.WriteLine($"docId: {queryResult.id}, tf: {queryResult.tf}");
                Console.WriteLine($"\t{GetDocContentByIndex(folderPaths_125[0], queryResult.id).Substring(0, 80)}...");
                Console.WriteLine();
            }

            return 0;
        }

        public void IndexFolder(string folderPath, int startIndex, int stopIndex)
        {
            Console.WriteLine($"Indexing... [{startIndex} - {stopIndex}]");

            Console.WriteLine("Map started");

            var mapedData = Map(folderPath, startIndex, stopIndex);

            Console.WriteLine($"Map finished\tdictionary size: {mapedData.Count}");

            Console.WriteLine("Reduce started");

            var reducedData = Reduce(mapedData);

            Console.WriteLine("Reduce finished");

            indexedData = reducedData.ToDictionary(entry => entry.Key,
                                                   entry => entry.Value);
        }

        public List<WordData.Document> FindInDocs(string queryWord) 
        {
            var words = CleanText(queryWord);

            var docsList = indexedData[words].docs;

            //var idf = indexedData[words].docCount / 250; //indexedData[words].idf;

            return docsList.OrderByDescending((x) => x.tf).ToList();
        }

        // todo: simplify
        private ConcurrentDictionary<string, List<WordData>> Map(string folderPath, int startIndex, int stopIndex) 
        {
            var mapProcesses = new List<MapProccess<string, WordData>>();

            var results = new ConcurrentDictionary<string, List<WordData>>();

            for (int docIndex = startIndex; docIndex < stopIndex; docIndex++)
            {
                string docContent = GetDocContentByIndex(folderPath, docIndex);

                mapProcesses.Add(new MapProccess<string, WordData>(MapProccessFunc, ref results, docIndex, docContent));
            }

            foreach (var process in mapProcesses)
            {
                process.WaitForResult();
            }

            //Console.WriteLine(results);

            return results;
        }

        private Dictionary<string, WordData> MapProccessFunc(params object[] args) 
        {
            int docId = (int)args[0];
            string docContent = (string)args[1];

            var cleanedText = CleanText(docContent);

            //Console.WriteLine($"doc{docId} map started");

            string[] words = cleanedText.Split(" ");
            int wordsCount = words.Length;

            //Console.WriteLine($"doc{docId} words count: {wordsCount}");

            Dictionary<string, WordData> mapData = new Dictionary<string, WordData>();

            //Thread.Sleep((new Random()).Next(0, 10_000));

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Equals(String.Empty))
                {
                    continue;
                }

                string word = words[i];

                if (mapData.TryGetValue(word, out WordData wd))
                {
                    wd.docs[0].occurrsCount += 1;
                }
                else
                {
                    wd = new WordData();
                    wd.AddDocument(docId, 1, wordsCount);
                    mapData.Add(word, wd);
                }
            }

            //Console.WriteLine($"doc{docId} map finished");

            return mapData;
        }

        private ConcurrentDictionary<string, WordData> Reduce(ConcurrentDictionary<string, List<WordData>> mapedData)
        {
            var reduceThreads = new List<Thread>();

            var results = new ConcurrentDictionary<string, WordData>();
            
            foreach (var pair in mapedData)
            {
                var word = pair.Key;
                var dataList = pair.Value;

                Thread t = new Thread(() => ReduceProcessFunc(word, dataList, ref results));
                t.Start();

                reduceThreads.Add(t);
            }

            foreach (var t in reduceThreads)
            {
                t.Join();
            }

            return results;
        }

        public void ReduceProcessFunc(string word, List<WordData> data, ref ConcurrentDictionary<string, WordData> results)
        {
            //Console.WriteLine($"word [{word}] reduce started");

            //Thread.Sleep((new Random()).Next(0, 10_000));

            foreach (var dataPart in data) 
            {
                if (results.TryGetValue(word, out var storedData))
                {
                    storedData.Combine(dataPart);
                }
                else
                {
                    if (!results.TryAdd(word, dataPart)) 
                    {
                        results[word].Combine(dataPart);
                    }
                }
            }

            //Console.WriteLine($"word [{word}] reduce finished");
        }

        private static string GetDocContentByIndex(string folderPath, int index)
        {
            string fileQuery = String.Format(QUERY_FORMAT, index);

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            FileInfo[] fileEntries = folder.GetFiles(fileQuery);

            FileInfo file = fileEntries[0];

            if (!file.Exists)
            {
                throw new FileNotFoundException(String.Format("text with index {0} not found", index));
            }

            string text;

            // Open the stream and read it.
            using (StreamReader sr = file.OpenText())
            {
                text = sr.ReadToEnd();
            }

            return text;
        }

        private string CleanText(string text)
        {
            string cleanedText = text.ToLower();
            cleanedText = Regex.Replace(cleanedText, @"[,():?]", "");
            cleanedText = Regex.Replace(cleanedText, "\"", "");
            cleanedText = Regex.Replace(cleanedText, @"[.]", " ");
            cleanedText = Regex.Replace(cleanedText, @"--", " ");
            cleanedText = Regex.Replace(cleanedText, @"`", "'");
            cleanedText = Regex.Replace(cleanedText, @"<br />", " ");
            cleanedText = Regex.Replace(cleanedText, @"[\n\t]", " ");
            return cleanedText;
        }
    }
}

