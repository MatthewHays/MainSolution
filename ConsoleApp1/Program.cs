// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        //Console.WriteLine(NumDistinct("babgbag", "bag"));
        Console.WriteLine(NumDistinct("rabbbit", "rabbit", new HashSet<int>(), new HashSet<string>()));
        Console.WriteLine(NumDistinct("babgbag", "bag", new HashSet<int>(), new HashSet<string>()));
        Console.WriteLine(NumDistinct("bdacbaddcbcbacbbadcaddacdadbaccbb", "cdddc", new HashSet<int>(), new HashSet<string>())); //152
        
        var list = new List<string>();
        GenerateCombinations("babgbag", 0, new Stack<char>(), list);

        var a= list.Where(a => a=="bag").Count();
        
    }
    /*public static HashSet<int> ignore = new();*/
    //public static int matches = 0;


    public static int NumDistinct2(string s, string t, HashSet<int> ignore, HashSet<string> dupes)
    {
        List<int> combinations = new();
        int index = 0;

        int outer = 0;
        int inner = 0;

        while (outer < s.Length && inner < t.Length)
        { 
            var s1 = s[outer];
            var s2 = s[inner];

            while(s1 == s2)
            {
                outer++;
                var s1 = s[outer];
            }
            
        }
    }
    public static void GenerateCombinations(string s, int index, Stack<char> current, List<string> list)
    {
        if (index == s.Length)
        {
            var curString = String.Join("", current.Reverse());
            list.Add(curString);
            return;
        }
        current.Push(s[index]);
        GenerateCombinations(s, index+1, current, list);
        current.Pop();
        GenerateCombinations(s, index+1, current, list);
    }

    public static int NumDistinct(string s, string t, HashSet<int> ignore, HashSet<string> dupes)
    {
        LinkedList<int> matches = new();
        int totalMatches = 0;

        int outer = 0;
        int inner = 0;

        while (outer < s.Length && inner < t.Length)
        {
            if (ignore.Contains(outer))
            { 
                outer++;
                continue;
            }
            
            if (s[outer] == t[inner])
            {
                matches.AddLast(outer);
                if (matches.Count == t.Length)
                {
                    string matchKey = String.Join("", s.Select((item, index) => matches.Contains(index) ? $"[{item}]" : $"{item}"));

                    if (dupes.Contains(matchKey))
                        return 0;
                    dupes.Add(matchKey);

                    Console.WriteLine(String.Join("", s.Select((item, index) => ignore.Contains(index) ? $"*" : $"{item}")));
                    Console.WriteLine(matchKey);
                    Console.WriteLine("---");

                    totalMatches = 1;
                    
                    List<string> options = new List<string>();
                    foreach (var matchedInt in matches.Reverse())
                    {
                        ignore.Add(matchedInt);
                        totalMatches += NumDistinct(s, t, ignore, dupes);
                        ignore.Remove(matchedInt);
                    }

                    foreach (var matchedInt in matches.SkipLast(1).Reverse())
                    {
                        ignore.Add(matchedInt);
                        totalMatches += NumDistinct(s, t, ignore, dupes);
                        ignore.Remove(matchedInt);
                    }

                }
                inner++;
            }
            outer++;

        }
        //Console.WriteLine();
        return totalMatches;
    }


}

