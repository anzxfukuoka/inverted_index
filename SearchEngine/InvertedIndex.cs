/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * SearchEngine.cs
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace SearchEngine
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
        public List<Document> docs;

        /// <summary>
        /// Number of documents with this word
        /// </summary>
        public int docCount { get { return docs.Count; } }

        /// <summary>
        /// Inverse document frequency
        /// </summary>
        public double idf = 1;

        public void Combine(WordData other)
        {
            // todo: fix race cond
            //docs.Concat(other.docs);
            docs.AddRange(other.docs);
            //Console.WriteLine("new combine");
        }

        public WordData(int firstDocId, int firstDocOccurrsCount, int firstDocWordCount) 
        {
            docs = new List<Document>() { new Document(firstDocId, firstDocOccurrsCount, firstDocWordCount) };
        }
    }

    /// <summary>
    /// Multi-threading document indexer 
    /// implemented via MapReduce
    /// </summary>
    public class InvertedIndex
    {
        private const string FILE_SEARCH_FORMAT = "{0}_*.txt";

        public ConcurrentDictionary<string, WordData> indexedData { get; private set; }

        private int indexedDocumentsCount = 0;

        public string folderPath = "";

        /// <summary>
        /// Builds inverted index for words in each file fit FILE_SEARCH_FORMAT 
        /// (non reqursive)
        /// </summary>
        /// <param name="folderPath"> Path to indexing folder </param>
        /// <param name="startIndex"> Index of first file </param>
        /// <param name="stopIndex"> Index of last file </param>
        /// <param name="processCount"> Count of threads on Map and Reduce stages</param>
        public void IndexFolder(string folderPath, int startIndex, int stopIndex, int processCount = 10)
        {
            this.folderPath = folderPath;

            Console.WriteLine($"Indexing... [{startIndex} - {stopIndex}] \t {folderPath}");

            //Console.WriteLine("Map started");

            var mapedData = Map(folderPath, startIndex, stopIndex, processCount);

            //Console.WriteLine($"Map finished\tdictionary size: {mapedData.Count}");
            Console.WriteLine($"dictionary size: {mapedData.Count}");

            //Console.WriteLine("Reduce started");

            var reducedData = Reduce(mapedData, processCount);

            //Console.WriteLine("Reduce finished");

            Console.WriteLine($"Indexing [{startIndex} - {stopIndex}] finished");

            indexedDocumentsCount = stopIndex - startIndex;

            indexedData = reducedData;
        }

        /// <summary>
        /// For every word in query finds in indexed documents 
        /// list of ids of relevant to this word documents.  
        /// </summary>
        /// <param name="query"> query to search </param>
        /// <returns> Sorted descending by relevance {int id: double relavance} </returns>
        public Dictionary<int, double> FindInDocs(string query)
        {
            var words = CleanText(query).Split(" ");

            var foundDocuments = new Dictionary<int, double>();

            foreach (var word in words)
            {
                if (word.Equals(String.Empty) || !indexedData.ContainsKey(word)) { continue; }

                var docsList = indexedData[word].docs;

                double idf = Math.Log(indexedDocumentsCount / indexedData[word].docCount);

                foreach (var doc in docsList)
                {
                    double relavance = doc.tf * idf;

                    if (foundDocuments.ContainsKey(doc.id))
                    {

                        foundDocuments[doc.id] += relavance;
                    }
                    else
                    {
                        foundDocuments.Add(doc.id, relavance);
                    }
                }

            }

            var sorted = (from entery in foundDocuments orderby entery.Value descending select entery)
                .ToDictionary(x => x.Key, x => x.Value );

            return sorted;
        }

        /// <summary>
        /// Map stage of MapReduce
        /// </summary>
        /// <param name="folderPath"> Path to indexing folder </param>
        /// <param name="startIndex"> Index of first file </param>
        /// <param name="stopIndex"> Index of last file </param>
        /// <param name="processCount"> Count of threads </param>
        /// <returns> Documents maped to words {string word: WordData data}</returns>
        private ConcurrentDictionary<string, ConcurrentBag<WordData>> Map(string folderPath, int startIndex, int stopIndex, int processCount = 10)
        {
            var mapThreads = new List<Thread>();

            var results = new ConcurrentDictionary<string, ConcurrentBag<WordData>>();

            int docsCount = (stopIndex - startIndex);
            int step = docsCount / processCount;

            int docIndex;

            for (docIndex = startIndex; docIndex < stopIndex; docIndex += step)
            {
                int start = docIndex;
                int end = docIndex + step >= stopIndex ? stopIndex : docIndex + step; 

                var t = StartMapThread(folderPath, start, end, results);
                mapThreads.Add(t);
            }

            foreach (var process in mapThreads)
            {
                process.Join();
            }

            return results;
        }

        /// <summary>
        /// Starts new Map process
        /// </summary>
        /// <param name="folderPath"> Path to indexing folder </param>
        /// <param name="startIndex"> Index of first file </param>
        /// <param name="stopIndex"> Index of last file </param>
        /// <param name="results"> Dictionary for mapping results {string word: WordData data} </param>
        /// <returns></returns>
        private Thread StartMapThread(string folderPath, int startIndex, int stopIndex, ConcurrentDictionary<string, ConcurrentBag<WordData>> results)
        {
            var t = new Thread(() => MapProccess(folderPath, startIndex, stopIndex, results));
            t.Name = $"MapThread [{startIndex} - {stopIndex}]";
            t.Start();

            return t;
        }

        /// <summary>
        /// Map thread inner function
        /// </summary>
        /// <param name="folderPath"> Path to indexing folder </param>
        /// <param name="startIndex"> Index of first file </param>
        /// <param name="stopIndex"> Index of last file </param>
        /// <param name="results"> Dictionary for mapping results {string word: WordData data} </param>
        private void MapProccess(string folderPath, int startIndex, int stopIndex, ConcurrentDictionary<string, ConcurrentBag<WordData>> results) 
        {
            for (int docIndex = startIndex; docIndex < stopIndex; docIndex++)
            {
                string docContent = GetDocContentByIndex(folderPath, docIndex);

                MapDocument(docIndex, docContent, results);
            }
        }

        /// <summary>
        /// Maps document to words it's contains 
        /// </summary>
        /// <param name="docId"> Index of document </param>
        /// <param name="docContent"> Document content </param>
        /// <param name="results"> Dictionary for mapping results {string word: WordData data} </param>
        /// <exception cref="InvalidOperationException"></exception>
        private void MapDocument(int docId, string docContent, ConcurrentDictionary<string, ConcurrentBag<WordData>> results)
        {
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
                    // на этом этапе в WordData.docs лежит только один Document, заданый через конструктор
                    wd.docs[0].occurrsCount += 1;
                    
                }
                else
                {
                    wd = new WordData(docId, 1, wordsCount);
                    mapData.Add(word, wd);
                }
            }

            //Console.WriteLine($"doc{docId} mapped, sending data to results...");

            //foreach (KeyValuePair<string, WordData> pair in mapData)
            Parallel.ForEach(mapData, pair =>
            {
                var word = pair.Key;
                var newWD = pair.Value;

                ConcurrentBag<WordData> wdList;

                if (!results.TryAdd(word, new ConcurrentBag<WordData>() { newWD }))
                {
                    if (results.TryGetValue(word, out wdList))
                    {
                        wdList.Add(newWD);
                    }
                    else
                    {
                        throw new InvalidOperationException($"{word} data is corrupted");
                    }
                }
            });

            //Console.WriteLine($"doc{docId} map finished");
        }

        /// <summary>
        /// Reduce stage of MapReduce
        /// </summary>
        /// <param name="mapedData"> Dictionary of mapped data {string word: WordData data} </param>
        /// <param name="processCount"> Count of threads </param>
        /// <returns></returns>
        private ConcurrentDictionary<string, WordData> Reduce(ConcurrentDictionary<string, ConcurrentBag<WordData>> mapedData, int processCount = 10)
        {
            var reduceThreads = new List<Thread>();

            var results = new ConcurrentDictionary<string, WordData>();

            var splittedMapedData = new Dictionary<string, ConcurrentBag<WordData>>[processCount];

            foreach (var pair in mapedData) 
            {
                var word = pair.Key;
                var dataList = pair.Value;

                int wordStartChar = word[0];
                var reduceProcessIndex = wordStartChar % processCount;

                if (splittedMapedData[reduceProcessIndex] == null)
                {
                    splittedMapedData[reduceProcessIndex] = new Dictionary<string, ConcurrentBag<WordData>>();
                }

                splittedMapedData[reduceProcessIndex].Add(word, dataList);
            }

            foreach (var dataPart in splittedMapedData)
            {
                Thread t = StartReduceThread(new ConcurrentDictionary<string, ConcurrentBag<WordData>>(dataPart), results);
                reduceThreads.Add(t);
            }

            foreach (var t in reduceThreads)
            {
                t.Join();
            }

            return results;
        }

        /// <summary>
        /// Starts new Reduce process
        /// </summary>
        /// <param name="data"> Dictionary of mapped data {string word: WordData data} </param>
        /// <param name="results"> Dictionary for reducing results {string word: WordData data} </param>
        /// <returns></returns>
        private Thread StartReduceThread(ConcurrentDictionary<string, ConcurrentBag<WordData>> data, ConcurrentDictionary<string, WordData> results)
        {
            Thread t = new Thread(() => ReduceProcess(data, results));
            t.Name = $"ReduceThread [{data.First().Key} - {data.Last().Key}]";
            t.Start();
            return t;
        }

        /// <summary>
        /// Reduce thread inner function
        /// </summary>
        /// <param name="data"> Dictionary of mapped data {string word: WordData data} </param>
        /// <param name="results"> Dictionary for reducing results {string word: WordData data} </param>
        public void ReduceProcess(ConcurrentDictionary<string, ConcurrentBag<WordData>> data, ConcurrentDictionary<string, WordData> results) 
        {
            foreach (var pair in data)
            {
                var word = pair.Key;
                var docList = pair.Value;

                ReduceWord(word, docList, results);
            }
        }

        /// <summary>
        /// Reduces spicific word
        /// </summary>
        /// <param name="word"> Word </param>
        /// <param name="data"> Documents data list </param>
        /// <param name="results"> Dictionary for reducing results {string word: WordData data} </param>
        public void ReduceWord(string word, ConcurrentBag<WordData> data, ConcurrentDictionary<string, WordData> results)
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

        // todo: move to utils
        /// <summary>
        /// Founds document by index
        /// </summary>
        /// <param name="folderPath"> Path to folder </param>
        /// <param name="index"> Document index </param>
        /// <returns> Document content </returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string GetDocContentByIndex(string folderPath, int index)
        {
            string fileQuery = String.Format(FILE_SEARCH_FORMAT, index);

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

        // todo: move to utils
        /// <summary>
        /// Cleans texts from extra symbols
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CleanText(string text)
        {
            string cleanedText = text.ToLower();
            cleanedText = Regex.Replace(cleanedText, @"[,():?]", "");
            cleanedText = Regex.Replace(cleanedText, "\"", "");
            cleanedText = Regex.Replace(cleanedText, @"['.]", " ");
            cleanedText = Regex.Replace(cleanedText, @" -- ", " ");
            cleanedText = Regex.Replace(cleanedText, @"`", "'");
            cleanedText = Regex.Replace(cleanedText, @"<br />", " ");
            cleanedText = Regex.Replace(cleanedText, @"[\n\t]", " ");
            return cleanedText;
        }
    }
}
