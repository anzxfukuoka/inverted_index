/*
 * Айайа Г. ДА-92
 * Вар 1 (34)
 * Program.cs
 */

public class InvertedIndex 
{
    // вар
    public const int V = 34;
    
    public const int COUNT_1 = 12500;  
    public const int COUNT_2 = 50000; 
    
    public const int STRAT_INDEX_1 = (COUNT_1 / 50) * (V - 1); 
    public const int STRAT_INDEX_2 = (COUNT_2 / 50) * (V - 1); 
    
    public const int STOP_INDEX_1 = (COUNT_1 / 50) * V; 
    public const int STOP_INDEX_2 = (COUNT_2 / 50) * V;

    public static List<string> folder_paths_1 = new List<string>{ 
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\test\neg",
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\test\pos",
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\train\neg",
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\train\pos"
    };

    public static List<string> folder_paths_2 = new List<string>{
        @"C:\Users\Anzx\MISC\SYNC\LABS\par_course_work\datasets\aclImdb\train\unsup",
    };

    public static int Main(string[] args) 
    {
        Console.WriteLine("Hello, World!");

        foreach (string folder in folder_paths_1) 
        {
            Map(folder);
        }

        return 0;
    }
    public static void Map(string folder_path) 
    {
        Console.WriteLine(folder_path);
    }
    public void Reduce() 
    {

    }
}