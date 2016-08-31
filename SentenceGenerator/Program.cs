using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MarkovLib;

namespace SentenceGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: SentenceGenerator <input lines> <output file>");
                return;
            }

            var chain = new InMemoryMarkovChain<WordPair>();
            var generator = new Order2SentenceGenerator(chain);
            using (var reader = File.OpenText(args[0]))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    generator.FeedLine(line.Trim());
            }

            var rand = new Random();
            using (var writer = File.CreateText(args[1]))
            {
                for (var i = 0; i < 1000; i++)
                {
                    var line = string.Join(" ", generator.GenerateSentence(rand));
                    writer.WriteLine(line);
                }
            }
        }
    }
}
