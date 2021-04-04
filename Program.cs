using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NumberFinder
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                PrintUsage();
                return;
            }

            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Provided contacts file does not exist!");
                PrintUsage();
                return;
            }

            var contactsCsv = File.ReadAllLines(args[1])
                .Select(s => s.Split(","))
                .DistinctBy(s => NormalizeNumber(s[34], args[0]))
                .ToDictionary(s => NormalizeNumber(s[34], args[0]), s => s[0]);

            if (!File.Exists(args[2]))
            {
                Console.WriteLine("Provided leak file does not exist!");
                PrintUsage();
                return;
            }
            StreamReader facebook = new(args[2]);
            string line;
            while ((line = facebook.ReadLine()) != null)
            {
                var record = line.Split(":");
                if (contactsCsv.ContainsKey(record[0]))
                {
                    Console.WriteLine($"{contactsCsv[record[0]]}, {record[0]}, {record[2]} {record[3]}");
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("You need three args: country code, contacts file name and leak file name!");
            Console.WriteLine("Usage: dotnet run -- countryCode contactsFile leakFile");
            Console.WriteLine("For example: dotnet run -- 48 contacts.csv Poland.txt");
            Console.WriteLine("contacts.csv is contacts exported from Google Contacts, as Google CSV.");
        }

        private static string NormalizeNumber(this string number, string countryCode)
        {
            var clean = number.Replace(" ", "").Replace("-", "").Replace("+", "");
            while (clean.StartsWith("0"))
                clean = clean[1..];
            if (!clean.StartsWith(countryCode))
                clean = countryCode + clean;
            return clean;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
