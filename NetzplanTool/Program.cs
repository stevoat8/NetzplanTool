using System;
using System.IO;
using System.Linq;
using System.Text;
using Fclp;
using PrecedenceDiagram;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;

namespace NetzplanTool
{
    class Program
    {
        /// <summary>
        /// Die Startparameter der Netzplan Tool Programms.
        /// </summary>
        public class ApplicationArguments
        {
            /// <summary>
            /// Speicherpfad des Projektplans (Pfad + Dateiname).
            /// </summary>
            public string CsvPath { get; set; }

            /// <summary>
            /// Speicherpfad unter dem der erstellte Graph gespeichert wird.
            /// </summary>
            public string OutputPath { get; set; }

            /// <summary>
            /// Dateiformat in welches der Graph gespeichert werden soll (jpg, png, svg, pdf, plain, plainext).
            /// </summary>
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
                ApplicationArguments appArgs = parser.Object;
                GenerateDiagram(appArgs.CsvPath, appArgs.OutputPath, appArgs.FileFormat);
            }
        }

        /// <summary>
        /// Erzeugt aus einem Projektplan im CSV-Format einen Graphen, der den 
        /// Prozess mit allen Teilprozessen und Fristen darstellt und speichert diesen.
        /// </summary>
        /// <param name="csvPath">Speicherpfad des Projektplans (Pfad + Dateiname).</param>
        /// <param name="outputPath">Speicherpfad unter dem der erstellte Graph gespeichert wird.</param>
        /// <param name="fileFormat">
        /// Dateiformat in welches der Graph gespeichert werden soll (jpg, png, svg, pdf, plain, plainext).
        /// </param>
        private static void GenerateDiagram(
            string csvPath, string outputPath, Enums.GraphReturnType fileFormat)
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
                string title = Path.GetFileNameWithoutExtension(csvPath);
                Process process = new Process(title, lines);

                string dot = process.GetDot();
                byte[] graphBytes = wrapper.GenerateGraph(dot, fileFormat);

                string outputFileName = Path.Combine(
                    outputPath,
                    process.Title + "." + fileFormat.ToString().ToLowerInvariant());
                File.WriteAllBytes(outputFileName, graphBytes);

                ShowSuccess($"Graph \"{process.Title}\" erfolgreich generiert unter \" {outputFileName}\"");

                System.Diagnostics.Process.Start(outputFileName);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Gibt das Hilfemenü auf der Konsole aus.
        /// </summary>
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

        /// <summary>
        /// Gibt eine Erfolgsmeldung auf der Konsole aus.
        /// </summary>
        /// <param name="message">Die Nachricht, die ausgegeben wird.</param>
        private static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }
        /// <summary>
        /// Gibt eine Fehlermeldung auf der Konsole aus.
        /// </summary>
        /// <param name="message">Die Nachricht, die ausgegeben wird.</param>
        private static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }

    }
}
