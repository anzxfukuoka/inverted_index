/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * Program.cs
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;
using System.Text.RegularExpressions;

namespace InvertedIndex
{
    ///// <summary>
    ///// Stores info of specific word regarding specific document 
    ///// </summary>
    //public class DocInfo
    //{
    //    /// <summary>
    //    /// document id
    //    /// </summary>
    //    public int docID;

    //    /// <summary>
    //    /// number of occurrences of word in a doc
    //    /// </summary>
    //    public int occurrNumber;
    //    /// <summary>
    //    /// count of word in a doc
    //    /// </summary>
    //    public int docLenght;

    //    /// <summary>
    //    /// term frequency
    //    /// </summary>
    //    public double tf { get { return occurrNumber / docLenght; } }

    //    /// <summary>
    //    /// constructor
    //    /// </summary>
    //    /// <param name="docID">
    //    /// <inheritdoc cref="DocInfo.docID" path="/summary"></inheritdoc>
    //    /// </param>
    //    /// <param name="occurrNumber">
    //    /// <inheritdoc cref="DocInfo.occurrNumber" path="/summary"></inheritdoc>
    //    /// </param>
    //    /// <param name="docLenght">
    //    /// <inheritdoc cref="DocInfo.docLenght" path="/summary"></inheritdoc>
    //    /// </param>
    //    public DocInfo(int docID, int occurrNumber, int docLenght)
    //    {
    //        this.docID = docID;
    //        this.occurrNumber = occurrNumber;
    //        this.docLenght = docLenght;
    //    }

    //    /// <summary>
    //    /// Combies two DocInfo with same <see cref="docID"></see>
    //    /// </summary>
    //    /// <param name="other"></param>
    //    /// <returns>Combined DocInfo</returns>
    //    /// <exception cref="InvalidOperationException">thrown when objects docIDs are different</exception>
    //    private DocInfo Add(DocInfo other)
    //    {
    //        if (this.docID != other.docID)
    //        {
    //            throw new InvalidOperationException($"Objects docIDs are different: {this.docID} and {other.docID}");
    //        }

    //        this.occurrNumber += other.occurrNumber;

    //        return this;
    //    }

    //    public static DocInfo operator +(DocInfo left, DocInfo right)
    //    {
    //        return left.Add(right);
    //    }
    //}
    ///// <summary>
    ///// Stores info of specific word
    ///// </summary>
    //public class WordInfo
    //{
    //    /// <summary>
    //    /// Lexem
    //    /// </summary>
    //    public string word;

    //    /// <summary>
    //    /// List of documents where word occurrs
    //    /// </summary>
    //    public ConcurrentBag<DocInfo> docs = new ConcurrentBag<DocInfo>();

    //    /// <summary>
    //    /// IDF metric param
    //    /// </summary>
    //    /// <param name="N">total number of documents</param>
    //    /// <returns>Inverse document frequency</returns>
    //    public double GetIDF(int N)
    //    {
    //        return Math.Log(docs.Count / N);
    //    }

    //    public WordInfo(string word)
    //    {
    //        this.word = word;
    //    }
    //}

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

        //private static ConcurrentDictionary<string, WordInfo> mapData = new ConcurrentDictionary<string, WordInfo>();
        //private static ConcurrentDictionary<string, WordInfo> reduceData = new ConcurrentDictionary<string, WordInfo>();

        public static async Task<int> Main(string[] args)
        {

            //Console.WriteLine("Hello, World!");

            foreach (string folder in folderPaths_125)
            {
                await IndexFilesAsync(folder, START_INDEX_125, STOP_INDEX_125);
            }

            //foreach (var word in mapData.Keys)
            //{
            //    Reduce(mapData[word]);
            //}

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

        public static async Task IndexFilesAsync(string folderPath, int startIndex, int stopIndex) 
        {
            List<Task<Dictionary<string, WordData>>> mapTasks = new List<Task<Dictionary<string, WordData>>>();

            for (int docIndex = startIndex; docIndex < stopIndex; docIndex++)
            {
                string docContent = GetDocContentByIndex(folderPath, docIndex);

                var task = new Task<Dictionary<string, WordData>>(
                    () => Map(docIndex, docContent));
                
                task.Start();

                mapTasks.Add(task);
                
            }

            foreach (var task in mapTasks) 
            {
                await task;
            }

            Console.WriteLine(mapTasks[0].Result);
        }

        public static Dictionary<string, WordData> Map(int docId, string docContent) 
        {
            Console.WriteLine($"doc{docId} map started");

            string cleanedText = Regex.Replace(docContent, @"[,]", "");
            cleanedText = Regex.Replace(cleanedText, @"<br />", " <br/> ");

            string[] words = cleanedText.Split(" ");
            int wordsCount = words.Length;

            Console.WriteLine($"doc{docId} words count: {wordsCount}");

            Dictionary<string, WordData> mapData = new Dictionary<string, WordData>();

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

        //public static void Map(string folderPath, int startIndex, int stopIndex)
        //{
        //    for (int docIndex = startIndex; docIndex < stopIndex; docIndex++)
        //    {
        //        string text = GetDocByIndex(folderPath, docIndex);

        //        text = Regex.Replace(text, @"[,]", "");
        //        text = Regex.Replace(text, @"<br />", " <br/> ");

        //        string[] words = text.Split(" ");

        //        Console.WriteLine($"words count {words.Length}");

        //        for (int i = 0; i < words.Length; i++)
        //        {
        //            if (!words[i].Equals(String.Empty))
        //            {
        //                ProcessMap(words[i], docIndex, words.Length);
        //            }
        //        }

        //    }
        //}

        //public static async Task<Dictionary<string, WordData>> MapAsync(int docIndex, string docContent) 
        //{
        //    var result = await Task<Dictionary<string, WordData>>.Run(() =>
        //    {
                
        //    });

        //    return result;
        //}

        //public static void ProcessMap(string word, int docID, int docLenght)
        //{
        //    if (!mapData.ContainsKey(word))
        //    {
        //        var newInfo = new WordInfo(word);
        //        mapData.TryAdd(word, newInfo);
        //    }

        //    mapData.TryGetValue(word, out var info);
        //    info.docs.Add(new DocInfo(docID, 1, docLenght));
        //}

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

