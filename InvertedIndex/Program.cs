/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * Program.cs
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
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
            public double tf { get { return occurrsCount / wordCount; } }

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
        public double idf = 0;

        public WordData() 
        {
        }

        public void Combine(WordData other) 
        {
            docs.AddRange(other.docs);
            Console.WriteLine("new combine");
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

        public static int Main(string[] args)
        {

            var invidx = new InvertedIndex();
            
            var mapedData = invidx.IndexFolder(folderPaths_125[0], START_INDEX_125, STOP_INDEX_125);

            //foreach (string folder in folderPaths_125)
            //{
            //    await invidx.IndexFolderAsync(folder, START_INDEX_125, STOP_INDEX_125);
            //}

            foreach (var pair in mapedData) 
            {
                invidx.Reduce(pair.Key, pair.Value);
            }

            return 0;
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

        public Dictionary<string, List<WordData>> IndexFolder(string folderPath, int startIndex, int stopIndex)
        {
            List<MapProccess<string, WordData>> mapProcesses = new List<MapProccess<string, WordData>>();
            
            Dictionary<string, List<WordData>> results = new Dictionary<string, List<WordData>>();

            for (int docIndex = startIndex; docIndex < stopIndex; docIndex++)
            {
                string docContent = GetDocContentByIndex(folderPath, docIndex);

                mapProcesses.Add(new MapProccess<string, WordData> (Map, ref results, docIndex, docContent));
            }

            foreach (var process in mapProcesses) 
            {
                process.WaitForResult();
            }

            Console.WriteLine(results);

            return results;
        }

        private string CleanText(string text) 
        {
            string cleanedText = Regex.Replace(text, @"[,]", "");
            cleanedText = Regex.Replace(cleanedText, @"<br />", " <br/> ");
            return cleanedText;
        }

        public Dictionary<string, WordData> Map(params object[] args) 
        {
            int docId = (int)args[0];
            string docContent = (string)args[1];

            var cleanedText = CleanText(docContent);

            Console.WriteLine($"doc{docId} map started");

            string[] words = cleanedText.Split(" ");
            int wordsCount = words.Length;

            Console.WriteLine($"doc{docId} words count: {wordsCount}");

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

            Console.WriteLine($"doc{docId} map finished");

            return mapData;
        }

        public Dictionary<string, WordData> Reduce(string word, List<WordData> data)
        {
            Dictionary<string, WordData> results = new Dictionary<string, WordData>();

            foreach (var dataPart in data) 
            {
                ProccessReduce(word, dataPart, ref results);
            }

            return results;
        }

        public void ProccessReduce(string word, WordData data, ref Dictionary<string, WordData> results) 
        {
            if (results.TryGetValue(word, out var storedData))
            {
                storedData.Combine(data);
            }
            else 
            {
                results.Add(word, data);
            }
        }
      
        //public static void Reduce(WordInfo wordInfo)
        //{
        //    if (!reduceData.ContainsKey(wordInfo.word))
        //    {
        //        reduceData.TryAdd(wordInfo.word, wordInfo);
        //    }
        //    else
        //    {
        //        reduceData.TryGetValue(wordInfo.word, out WordInfo storedInfo);

        //        Dictionary<int, DocInfo> data = new Dictionary<int, DocInfo>();

        //        foreach (var docInfo in storedInfo.docs)
        //        {
        //            if (data.ContainsKey(docInfo.docID))
        //            {
        //                data[docInfo.docID] += docInfo;
        //            }
        //            else
        //            {
        //                data.Add(docInfo.docID, docInfo);
        //            }
        //        }

        //        foreach (var docInfo in wordInfo.docs)
        //        {
        //            if (data.ContainsKey(docInfo.docID))
        //            {
        //                data[docInfo.docID] += docInfo;
        //            }
        //            else
        //            {
        //                data.Add(docInfo.docID, docInfo);
        //            }
        //        }

        //        storedInfo.docs = new ConcurrentBag<DocInfo>(data.Values);
        //    }
        //}
    }
}

