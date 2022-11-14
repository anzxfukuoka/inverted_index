/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * Program.cs
 */

using System.Diagnostics.Tracing;
using System.Text;
using System.Text.RegularExpressions;

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

    public static int Main(string[] args) 
    {
        //Console.WriteLine("Hello, World!");

        foreach (string folder in folderPaths_125) 
        {
            Map(folder, START_INDEX_125, STOP_INDEX_125);
        }

        return 0;
    }

    private static string GetTextByIndex(string folderPath, int index) 
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

        for (int index = startIndex; index < stopIndex; index++)
        {
            string text = GetTextByIndex(folderPath, index);

            text = Regex.Replace(text, @"[,]", "");
            text = Regex.Replace(text, @"<br />", " <br/> ");

            string[] words = text.Split(" ");

            for (int i = 0; i < words.Length; i++)
            {
                //Console.WriteLine(word);
            }

        }
    }

    public void Reduce() 
    {

    }
}