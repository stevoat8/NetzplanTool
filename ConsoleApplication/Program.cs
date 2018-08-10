using Fclp;
using PrecedenceDiagram;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication
{
    internal class Program
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
                .SetDefault(GraphicFormat.Png);

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
                CreatePrecedenceDiagram(parser.Object);
            }
        }

        /// <summary>
        /// Erzeugt und speichert den Netzplan aus der übergebenen Prozessplan-Datei.
        /// </summary>
        /// <param name="args">Die Startparameter der Netzplan Tool Programms.</param>
        private static void CreatePrecedenceDiagram(NetzplanToolArguments args)
        {
            PrecedenceDiagramGenerator generator = new PrecedenceDiagramGenerator();
            try
            {
                string processTitle = Path.GetFileNameWithoutExtension(args.ProcessPlanPath);
                string[] processPlan = ReadProcessPlan(args.ProcessPlanPath);

                byte[] precedenceDiagram = generator.GeneratePrecedenceDiagram(processTitle, processPlan, args.OutputFileFormat);

                string outputFileName = processTitle + "." + args.OutputFileFormat.ToString().ToLowerInvariant();
                string absoluteOutputPath = Path.Combine(args.OutputDirectory, outputFileName);
                Directory.CreateDirectory(args.OutputDirectory);
                File.WriteAllBytes(absoluteOutputPath, precedenceDiagram);

                ShowSuccess($"Netzplan als \"{absoluteOutputPath}\" gespeichert.");

#if (DEBUG)
                Process.Start(absoluteOutputPath);
#endif
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Liest den Prozessplan im CSV-Format unter dem übergebenen Dateipfad ein.
        /// </summary>
        /// <param name="fileName">Dateipfad der Prozessplan-CSV-Datei.</param>
        /// <returns>Die einzelnen Prozessvorgänge.</returns>
        private static string[] ReadProcessPlan(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName, Encoding.UTF7);
            lines = lines.Where(line => string.IsNullOrWhiteSpace(line) == false).ToArray();
            return lines;
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
            Debug.Print(message);
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
            Console.WriteLine(message);
            Console.ResetColor();
            Debug.Print(message);
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
            /// Dateiformat in welches der Graph gespeichert werden soll (png, jpg, svg, pdf, dot).
            /// </summary>
            internal GraphicFormat OutputFileFormat { get; set; }
        }
    }
}