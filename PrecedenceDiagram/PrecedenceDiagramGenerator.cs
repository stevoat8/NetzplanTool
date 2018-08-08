using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using ProcessModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrecedenceDiagram
{
    /// <summary>
    /// Generiert aus einem Prozessablaufplan den gerenderten Netzplan.
    /// </summary>
    public class PrecedenceDiagramGenerator
    {
        private GraphGeneration wrapper;

        /// <summary>
        /// Erzeugt einen NetzplanGenerator.
        /// </summary>
        public PrecedenceDiagramGenerator()
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(
                getProcessStartInfoQuery, getStartProcessQuery);

            wrapper = new GraphGeneration(
                getStartProcessQuery,
                getProcessStartInfoQuery,
                registerLayoutPluginCommand);
        }

        /// <summary>
        /// Generiert einen gerenderten Netzplan zu dem im übergebenen Prozessablaufplan
        /// beschriebenen Prozess.
        /// </summary>
        /// <param name="processTitle">Titel des im Prozessplan beschriebenen Prozesses.</param>
        /// <param name="processPlan">
        /// Prozessablaufplan der den Prozess beschreibt, welcher gerendert wird.
        /// </param>
        /// <param name="format">Grafikformat der Netzplangrafik.</param>
        /// <returns>Gerenderter Netzplan.</returns>
        public byte[] GeneratePrecedenceDiagram(string processTitle, string[] processPlan, GraphicFormat format)
        {
            processPlan = CheckForFormalErrors(processPlan);
            Process process = new Process(processTitle, processPlan);
            string diagramDot = process.GetDot();
            Enums.GraphReturnType gVFormat = ConvertToGraphVizEnum(format);
            return wrapper.GenerateGraph(diagramDot, gVFormat);
        }

        private static Enums.GraphReturnType ConvertToGraphVizEnum(GraphicFormat format)
        {
            switch (format)
            {
                case GraphicFormat.Png:
                    return Enums.GraphReturnType.Png;

                case GraphicFormat.Jpg:
                    return Enums.GraphReturnType.Jpg;

                case GraphicFormat.Pdf:
                    return Enums.GraphReturnType.Pdf;

                case GraphicFormat.Svg:
                    return Enums.GraphReturnType.Svg;

                case GraphicFormat.Dot:
                    return Enums.GraphReturnType.Plain;

                default:
                    return Enums.GraphReturnType.Png;
            }
        }

        /// <summary>
        /// Prüft ob der übergebene Prozessablaufplan syntaktisch und semantisch korrkt formuliert
        /// ist. Außerdem wird die Kopfzeile, soweit vorhanden, entfernt.
        /// Bei Fehlern werden entsprechende Exceptions ausgelöst.
        /// </summary>
        /// <param name="processPlan">Zu prüfendere Prozessablaufplan.</param>
        /// <returns></returns>
        private static string[] CheckForFormalErrors(string[] processPlan)
        {
            //Kopfzeile eliminieren
            if (Regex.IsMatch(processPlan[0], @"\w+;\w+;\w+;\w+"))
            {
                processPlan = processPlan.Skip(1).ToArray();
            }

            return processPlan;
        }
    }
}