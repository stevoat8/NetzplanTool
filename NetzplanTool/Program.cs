using Fclp;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using PrecedenceDiagram;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetzplanTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = new FluentCommandLineParser<NetzplanToolArguments>();

            parser.Setup(arg => arg.CsvPath)
                .As('i', "input")
                .Required();

            parser.Setup(arg => arg.OutputPath)
                .As('o', "output")
                .Required();

            parser.Setup(arg => arg.OutputFileFormat)
                .As('f', "format")
                .SetDefault(Enums.GraphReturnType.Png);

            parser.SetupHelp("h", "help")
                 .Callback(text => ShowHelp());

            var parseResult = parser.Parse(args);

            if (parseResult.HasErrors)
            {
                ShowError(parseResult.ErrorText);
                return;
            }

            if (parseResult.HelpCalled == false)
            {
                StartNetzplanTool(parser.Object);
            }
        }

        /// <summary>
        /// Erzeugt aus einem Projektplan ein Diagramm im übergebenen Dateiformat, das den Prozess
        /// mit allen Teilprozessen und Fristen darstellt.
        /// </summary>
        /// <param name="processTitle">Titel des Prozesses</param>
        /// <param name="processPlan">Der Prozessplan, nach dem das Diagramm erzeugt wird.</param>
        /// <param name="fileFormat">Dateiformat in dem die Diagramm-Grafik erzeugt wird.</param>
        /// <returns>Digramm-Grafik des Prozesses.</returns>
        private static byte[] GenerateDiagram(
            string processTitle, string[] processPlan, Enums.GraphReturnType fileFormat)
        {
            #region Arrange graphViz wrapper

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(
                getProcessStartInfoQuery, getStartProcessQuery);

            GraphGeneration wrapper = new GraphGeneration(
                getStartProcessQuery,
                getProcessStartInfoQuery,
                registerLayoutPluginCommand);

            #endregion Arrange graphViz wrapper

            Process process = new Process(processTitle, processPlan);

            string digramDot = process.GetDot();
            byte[] digramGraphic = wrapper.GenerateGraph(digramDot, fileFormat);
            return digramGraphic;
        }

        /// <summary>
        /// Liest den Prozessplan im CSV-Format unter dem übergebenen Dateipfad ein.
        /// </summary>
        /// <param name="fileName">Dateipfad der Prozessplan-CSV-Datei.</param>
        /// <returns>Die Teilprozesse des Prozessplans.</returns>
        private static string[] ReadProcessPlan(string fileName)
        {
            string[] fileLines = File.ReadAllLines(fileName, Encoding.UTF7);
            string firstLine = fileLines[0];
            if (Regex.IsMatch(firstLine, @"\w+;\w+;\w+;\w+"))
            {
                return fileLines.Skip(1).ToArray();
            }
            else
            {
                throw new FormatException("Die CSV-Datei hat keine Kopfzeile.");
            }
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
        /// Startet das Netzplan Tool Programm, das eine Prozess-Grafik erstellt.
        /// </summary>
        /// <param name="args">Die nötigen Parameter.</param>
        private static void StartNetzplanTool(NetzplanToolArguments args)
        {
            try
            {
                string[] processPlan = ReadProcessPlan(args.CsvPath);
                string processTitle = Path.GetFileNameWithoutExtension(args.CsvPath);

                byte[] digramGraphic = GenerateDiagram(processTitle, processPlan, args.OutputFileFormat);

                string outputFileName = processTitle + "." + args.OutputFileFormat.ToString().ToLowerInvariant();
                string fullOutputFileName = Path.Combine(args.OutputPath, outputFileName);
                File.WriteAllBytes(fullOutputFileName, digramGraphic);

                ShowSuccess($"Netzplan generiert unter \"{fullOutputFileName}\"");

#if (DEBUG)
                System.Diagnostics.Process.Start(fullOutputFileName);
#endif
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Die Startparameter der Netzplan Tool Programms.
        /// </summary>
        internal class NetzplanToolArguments
        {
            /// <summary>
            /// Speicherpfad des Projektplans (Pfad + Dateiname).
            /// </summary>
            internal string CsvPath { get; set; }

            /// <summary>
            /// Dateiformat in welches der Graph gespeichert werden soll (jpg, png, svg, pdf, plain, plainext).
            /// </summary>
            internal Enums.GraphReturnType OutputFileFormat { get; set; }

            /// <summary>
            /// Speicherpfad unter dem der erstellte Graph gespeichert wird.
            /// </summary>
            internal string OutputPath { get; set; }
        }
    }
}