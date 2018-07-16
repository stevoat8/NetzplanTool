using System;
using System.IO;
using System.Linq;
using System.Text;
using Fclp;
using Netzplan;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using System.Diagnostics;

namespace NetzplanTool
{
    class Program
    {
        public class ApplicationArguments
        {
            public string CsvPath { get; set; }
            public string OutputPath { get; set; }
            public Enums.GraphReturnType FileFormat { get; set; }
        }

        static void Main(string[] args)
        {

            var parser = new FluentCommandLineParser<ApplicationArguments>();

            parser.Setup(arg => arg.CsvPath)
                .As('i', "input")
                .Required();

            parser.Setup(arg => arg.OutputPath)
                .As('o', "output")
                .Required();

            parser.Setup(arg => arg.FileFormat)
                .As('f', "format")
                .SetDefault(Enums.GraphReturnType.Png);

            parser.SetupHelp("h", "help")
                 .Callback(text => ShowHelp());

            var result = parser.Parse(args);

            if (result.HasErrors)
            {
                ShowError(result.ErrorText);
                return;
            }

            if (result.HelpCalled == false)
            {
                ApplicationArguments a = parser.Object;
                GenerateGraph(a.CsvPath, a.OutputPath, a.FileFormat);
            }
        }

        private static void GenerateGraph(string csvPath, string outputPath, Enums.GraphReturnType fileFormat)
        {
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
                byte[] graphBytes = wrapper.GenerateGraph(dot, fileFormat);

                string outputFileName = Path.Combine(
                    outputPath,
                    graph.Title + "." + fileFormat.ToString().ToLowerInvariant());
                File.WriteAllBytes(outputFileName, graphBytes);

                ShowSuccess($"Graph \"{graph.Title}\" erfolgreich generiert. Gespeichert unter \" {outputFileName}\"");

                Process.Start(outputFileName);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }

        private static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }

        private static void ShowHelp()
        {
            string helpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\help.txt");
            string[] lines = File.ReadAllLines(helpPath);
            Console.WriteLine();
            foreach (string line in lines)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }
    }
}
