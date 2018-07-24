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
    public class Program
    {
        private static void Main(string[] args)
        {
            var parser = new FluentCommandLineParser<NetzplanToolArguments>();

            parser.Setup(arg => arg.ProcessPlanPath)
                .As('i', "input")
                .Required();

            parser.Setup(arg => arg.OutputDirectory)
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
        /// Startet das Netzplan Tool Programm, das eine Prozess-Grafik erstellt.
        /// </summary>
        /// <param name="args">Die nötigen Parameter.</param>
        private static void StartNetzplanTool(NetzplanToolArguments args)
        {
            try
            {
                string processTitle = Path.GetFileNameWithoutExtension(args.ProcessPlanPath);
                string[] processPlan = ReadProcessPlan(args.ProcessPlanPath);
                CheckSyntax(processPlan);

                Process process = new Process(processTitle, processPlan);
                byte[] precedenceDiagram = GeneratePrecedenceDiagram(process, args.OutputFileFormat);

                string outputFileName = processTitle + "." + args.OutputFileFormat.ToString().ToLowerInvariant();
                string absoluteOutputPath = Path.Combine(args.OutputDirectory, outputFileName);
                Directory.CreateDirectory(args.OutputDirectory);
                File.WriteAllBytes(absoluteOutputPath, precedenceDiagram);

                ShowSuccess($"Netzplan generiert unter \"{absoluteOutputPath}\"");

#if (DEBUG)
                System.Diagnostics.Process.Start(absoluteOutputPath);
#endif
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private static void CheckSyntax(string[] processPlan)
        {
            char[] invalidChars = new char[] { '{', '}', '|', '"' };
            foreach (string task in processPlan)
            {
                if (task.IndexOfAny(invalidChars) != 1)
                {
                    throw new FormatException("Der Prozessplan enthält eines der unerlaubten Zeichen '{', '}', '\"', oder '|'");
                }
            }
        }

        /// <summary>
        /// Erzeugt aus einem Prozess einen Netzplan im übergebenen Dateiformat, das den Prozess
        /// mit allen Vorgängen und Fristen darstellt.
        /// </summary>
        /// <param name="process">Der Prozess, zu dem der Netzplan erzeugt wird.</param>
        /// <param name="fileFormat">Dateiformat in dem die Diagramm-Grafik erzeugt wird.</param>
        /// <returns>Netzplan-Grafik des Prozesses.</returns>
        private static byte[] GeneratePrecedenceDiagram(Process process, Enums.GraphReturnType fileFormat)
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

            string digramDot = process.GetDot();
            return wrapper.GenerateGraph(digramDot, fileFormat);
        }

        /// <summary>
        /// Liest den Prozessplan im CSV-Format unter dem übergebenen Dateipfad ein.
        /// </summary>
        /// <param name="fileName">Dateipfad der Prozessplan-CSV-Datei.</param>
        /// <returns>Die einzelnen Prozessvorgänge.</returns>
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
                //throw new FormatException("Die CSV-Datei hat keine Kopfzeile.");
                return fileLines.ToArray();
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
        /// Die Startparameter der Netzplan Tool Programms.
        /// </summary>
        internal class NetzplanToolArguments
        {
            /// <summary>
            /// Speicherpfad des Prozessplans (Pfad + Dateiname).
            /// </summary>
            internal string ProcessPlanPath { get; set; }

            /// <summary>
            /// Speicherpfad unter dem der erstellte Graph gespeichert wird.
            /// </summary>
            internal string OutputDirectory { get; set; }

            /// <summary>
            /// Dateiformat in welches der Graph gespeichert werden soll (jpg, png, svg, pdf, plain, plainext).
            /// </summary>
            internal Enums.GraphReturnType OutputFileFormat { get; set; }
        }
    }
}