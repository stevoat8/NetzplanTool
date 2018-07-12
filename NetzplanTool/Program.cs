using System;
using Fclp;
using Netzplan;

namespace NetzplanTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvPath = "";
            string outputPath = "";

            var p = new FluentCommandLineParser();

            p.Setup<string>('i', "input")
             .Callback(value => csvPath = value)
             .Required();

            p.Setup<string>('o', "output")
             .Callback(value => outputPath = value)
             .Required();

            p.Setup<string>('h', "help")
             .Callback(ShowHelp);

            p.Parse(args);

            Graph.CreateGarph(csvPath, outputPath);
        }

        private static void ShowHelp(string obj)
        {
            string line = "";
            line += "=================================" + Environment.NewLine;
            line += "===           HILFE           ===" + Environment.NewLine;
            line += "=================================" + Environment.NewLine;

            Console.WriteLine(line);
        }
    }
}
