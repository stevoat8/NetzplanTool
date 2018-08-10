using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using ProcessModel;

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
            Process process = new Process(processTitle, processPlan);
            string diagramDot = process.GetDot();
            Enums.GraphReturnType gVFormat = ConvertToGraphVizEnum(format);
            return wrapper.GenerateGraph(diagramDot, gVFormat);
        }

        /// <summary>
        /// Konvertiert das <see cref="GraphicFormat"/>-Enum in das GraphViz <see cref="Enums.GraphReturnType"/>-Enum.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
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
    }
}