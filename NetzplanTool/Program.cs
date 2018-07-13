using System;
using System.IO;
using System.Linq;
using System.Text;
using Fclp;
using Netzplan;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;

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

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(
                getProcessStartInfoQuery, getStartProcessQuery);

            GraphGeneration wrapper = new GraphGeneration(
                getStartProcessQuery,
                getProcessStartInfoQuery,
                registerLayoutPluginCommand);

            try
            {
                string[] lines = File.ReadAllLines(csvPath, Encoding.UTF7)
                    .Skip(1).ToArray();
                string graphTitle = Path.GetFileNameWithoutExtension(csvPath);
                Graph graph = new Graph(graphTitle, lines);

                string dot = graph.GetDot();
                //TODO: FileFormat anpassen
                byte[] graphBytes = wrapper.GenerateGraph(dot, Enums.GraphReturnType.Png);
                string outputFileName = Path.Combine(outputPath, graph.Title) + ".png";
                //TODO: Prüfen, ob Ordner existiert
                File.WriteAllBytes(outputFileName, graphBytes);

                Console.WriteLine("Graph erfolgreich generiert");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
                Console.ResetColor();
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
    }
}
