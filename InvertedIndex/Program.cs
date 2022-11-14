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

public abstract class Doc 
{
    int docID;

    public Doc(int docID)
    {
        this.docID = docID;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        //if (GetType() == obj.GetType()) 
        //{
        //    return base.Equals(obj);
        //}

        return ((Doc)obj).docID.Equals(this.docID);
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        //return base.GetHashCode();
        return this.docID.GetHashCode();
    }
}

public class DocMap : Doc
{
    
    int occurrNumber; // колво вхождений слова в документ
    int docLenght; // колво слов в документе

    public DocMap(int docID, int occurrNumber, int docLenght) : base(docID)
    {
        this.occurrNumber = occurrNumber;
        this.docLenght = docLenght;
    }
}

public class DocMetrics : Doc
{
    // term frequency
    double tf = 0;
    // inverse document frequency
    double idf = 0;

    public DocMetrics(int docID, double tf, double idf) : base(docID)
    {
        this.tf = tf;
        this.idf = idf;
    }
}

public class InvertedIndex 
{
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

    //

    private static ConcurrentDictionary<string, ConcurrentBag<DocMap>> mapData = new ConcurrentDictionary<string, ConcurrentBag<DocMap>>();

    // <docID, wordMetrics>
    private static ConcurrentDictionary<string, HashSet<DocMetrics>> reduceData = new ConcurrentDictionary<string, HashSet<DocMetrics>>();

    public static int Main(string[] args) 
    {
        //Console.WriteLine("Hello, World!");

        foreach (string folder in folderPaths_125) 
        {
            Map(folder, START_INDEX_125, STOP_INDEX_125);
        }

        foreach (string word in mapData.Keys) 
        {
            Reduce(word);
        }

        return 0;
    }

    private static string GetDocByIndex(string folderPath, int index) 
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

    public static void Map(string folderPath, int startIndex, int stopIndex) 
    {

        for (int docIndex = startIndex; docIndex < stopIndex; docIndex++)
        {
            string text = GetDocByIndex(folderPath, docIndex);

            text = Regex.Replace(text, @"[,]", "");
            text = Regex.Replace(text, @"<br />", " <br/> ");

            string[] words = text.Split(" ");

            for (int i = 0; i < words.Length; i++)
            {
                if (!words[i].Equals(String.Empty)) 
                {
                    ProcessMap(words[i], docIndex, words.Length);
                }
            }

        }
    }

    public static void ProcessMap(string word, int docID, int docLenght) 
    {
        if (!mapData.ContainsKey(word))
        {
            mapData.TryAdd(word, new ConcurrentBag<DocMap>());
        }

        mapData.TryGetValue(word, out var list);
        list.Add(new DocMap(docID, 1, docLenght));
    }

    public static void Reduce(string word) 
    {
        var map = mapData[name];
    }
}