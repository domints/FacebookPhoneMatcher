using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

            if (args[0] == "friends")
            {
                FindInFriendsCsv(args);
                return;
            }

            if (args[0] == "singlePhone")
            {
                FindSingle(args, phone: true);
                return;
            }

            if (args[0] == "singleId")
            {
                FindSingle(args, phone: false);
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
                .SelectMany(s => GetPhoneEntries(s, args[0]))
                .DistinctBy(s => s.phoneNumber)
                .ToDictionary(s => s.phoneNumber, s => s.name);

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
                var record = new FacebookLeakRecord(line);
                if (contactsCsv.ContainsKey(record.PhoneNumber))
                {
                    Console.WriteLine($"{contactsCsv[record.PhoneNumber]}, {record.PhoneNumber}, {record.Name} {record.Surname}");
                }
            }
        }

        private static void FindInFriendsCsv(string[] args)
        {
            var friends = File.ReadAllLines(args[1])
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Split(";"))
                .Select(l => new {UserId = l[0], UserName = l[1], FullName = l[2]})
                .DistinctBy(l => l.UserId)
                .ToDictionary(l => l.UserId);
            
            StreamReader facebook = new(args[2]);
            string line;
            while ((line = facebook.ReadLine()) != null)
            {
                var record = new FacebookLeakRecord(line);
                if (friends.ContainsKey(record.UserId))
                {
                    var friend = friends[record.UserId];
                    //Console.WriteLine($"{friend.FullName}, {record.PhoneNumber}, {record.Name} {record.Surname}");

                    Console.WriteLine($"Phone number: {record.PhoneNumber}");
                    Console.WriteLine($"UserId: {record.UserId}");
                    Console.WriteLine($"Name: {record.Name} {record.Surname}");
                    Console.WriteLine($"E-mail: {record.EmailAddress}");
                    Console.WriteLine($"Birthdate: {record.BirthDate}");
                    Console.WriteLine($"Gender: {record.Gender}");
                    Console.WriteLine($"Workplace: {record.Workplace}");
                    Console.WriteLine($"Living in: {record.LivingIn}");
                    Console.WriteLine($"Coming from: {record.ComingFrom}");
                    Console.WriteLine($"Relationship status: {record.RelationshipStatus}");

                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        private static void FindSingle(string[] args, bool phone = false)
        {
            string checkValue = args[1];
            if(phone)
                checkValue = NormalizeNumber(args[1]);

            var found = false;
            StreamReader facebook = new(args[2]);
            string line;
            while ((line = facebook.ReadLine()) != null)
            {
                var record = new FacebookLeakRecord(line);
                if (phone ? record.PhoneNumber == checkValue : record.UserId == checkValue)
                {
                    found = true;
                    Console.WriteLine($"Phone number: {record.PhoneNumber}");
                    Console.WriteLine($"UserId: {record.UserId}");
                    Console.WriteLine($"Name: {record.Name} {record.Surname}");
                    Console.WriteLine($"E-mail: {record.EmailAddress}");
                    Console.WriteLine($"Birthdate: {record.BirthDate}");
                    Console.WriteLine($"Gender: {record.Gender}");
                    Console.WriteLine($"Workplace: {record.Workplace}");
                    Console.WriteLine($"Living in: {record.LivingIn}");
                    Console.WriteLine($"Coming from: {record.ComingFrom}");
                    Console.WriteLine($"Relationship status: {record.RelationshipStatus}");
                    return;
                }
            }
            if (!found)
            {
                Console.WriteLine("No such data found.");
            }
        }

        private static IEnumerable<(string phoneNumber, string name)> GetPhoneEntries(string[] row, string countryCode)
        {
            var result = new List<(string phoneNumber, string name)>();

            var number1 = NormalizeNumber(row[34], countryCode);
            var number2 = NormalizeNumber(row[36], countryCode);

            if(!string.IsNullOrWhiteSpace(number1))
                result.Add((number1, row[0]));

            if(!string.IsNullOrWhiteSpace(number2))
                result.Add((number2, row[0]));

            return result;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("You need three args: country code, contacts file name and leak file name!");
            Console.WriteLine("Usage: dotnet run -- countryCode contactsFile leakFile");
            Console.WriteLine("For example: dotnet run -- 48 contacts.csv Poland.txt");
            Console.WriteLine("contacts.csv is contacts exported from Google Contacts, as Google CSV.");
        }

        private static string NormalizeNumber(this string number, string countryCode = null)
        {
            var clean = number.Trim().Replace(" ", "").Replace("-", "").Replace("+", "");
            while (clean.StartsWith("0"))
                clean = clean[1..];
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                if (!clean.StartsWith(countryCode) && !string.IsNullOrWhiteSpace(clean))
                    clean = countryCode + clean;
            }

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

    public class FacebookLeakRecord
    {
        public string PhoneNumber { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public string LivingIn { get; set; }
        public string ComingFrom { get; set; }
        public string RelationshipStatus { get; set; }
        public string Workplace { get; set; }
        public string EmailAddress { get; set; }
        public string BirthDate { get; set; }
        public FacebookLeakRecord(string row)
        {
            var record = row.Split(":");
            PhoneNumber = record[0];
            UserId = record[1];
            Name = record[2];
            Surname = record[3];
            Gender = record[4];
            LivingIn = record[5];
            ComingFrom = record[6];
            RelationshipStatus = record[7];
            Workplace = record[8];
            // Some date
            EmailAddress = record[10];
            BirthDate = record[11];
            // year. Graduation?
            // bool
            // bool
            // date
            // date
        }
    }
}
