using System;
using System.IO;

namespace SVGConverter
{
    class Program
    {
        private const string FILE_IN_NAME = "in2.txt";
        private const string FILE_OUT_NAME = "out.svg";

        static void Main(string[] args)
        {
            Console.WriteLine("Attempting to convert - " + Directory.GetCurrentDirectory());
            TextReader textReader = new TextReader(FILE_OUT_NAME);
        }
    }
}
