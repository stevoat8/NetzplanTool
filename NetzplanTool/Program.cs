using System;
using System.IO;
using System.Text;
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
            string fileFormat = "";

            var p = new FluentCommandLineParser();

            p.Setup<string>('i', "input")
             .Callback(value => csvPath = value)
             .WithDescription("input file")
             .Required();

            p.Setup<string>('o', "output")
             .Callback(value => outputPath = value)
             .WithDescription("output file")
             .Required();

            p.Setup<string>('f', "format")
             .Callback(value => fileFormat = value.ToLowerInvariant())
             .SetDefault("png")
             .WithDescription("output file format")
             .Required();

            p.Setup<string>('h', "help")
             .Callback(ShowHelp)
             .WithDescription("help");

            p.Parse(args);


            try
            {
                string[] lines = ReadCsv(csvPath);
                Graph graph = new Graph(lines);
                string graphDot = graph.GetDot();

                //ExportGraph(outputPath, graphDot);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("(Beliebige Taste zum Beenden)");
                Console.ReadKey();
            }    
        }

        private static void ShowHelp(string obj)
        {
            string line = "";
            line += "=================================" + Environment.NewLine;
            line += "===           HILFE           ===" + Environment.NewLine;
            line += "=================================" + Environment.NewLine;

            Console.WriteLine(line);
        }


        private static string[] ReadCsv(string path)
        {
            string[] lines = new string[0];
            try
            {
                lines = File.ReadAllLines(path, Encoding.UTF7);
            }
            catch (Exception)
            {
                throw;
            }

            //TODO: Check ob alle Zeilen das richtige Format haben
            for (int i = 1; i < lines.Length; i++)
            {
                string[] props = lines[i].Split(';');
            }

            return lines;
        }
    }
}
