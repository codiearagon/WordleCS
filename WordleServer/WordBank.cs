using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordleServer
{
    static class WordBank
    {
        private static string[] words = File.ReadAllLines("words.txt");

        public static string GetRandomWord()
        {
            Random random = new Random();
            string randomWord = words[random.Next(random.Next(words.Length))];

            return randomWord;
        }
    }
}
