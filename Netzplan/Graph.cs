using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netzplan
{
    public class Graph
    {
        public static void CreateGraph(string csvPath, string outputPath)
        {
            string processTitle = Path.GetFileNameWithoutExtension(csvPath);

            string[] lines = ReadCheckCsv(csvPath);

            Dictionary<string, Node> nodes;

            try
            {
                nodes = CreateNodes(lines);
                ConnectNodes(nodes);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Bei dem Parsen der CSV-Datei trat ein Fehler auf. " +
                    "Originale Fehlermeldung: " + ex.Message);
            }

            Process process = new Process(processTitle, nodes);

            ScheduleForward(process.IntitalNode);
            ScheduleBackwards(process.FinalNode);


            //string graphDot = CreateGraphDot(process);

            //ExportGraph(outputPath, graphDot);
        }

        private static void ScheduleForward(Node node)
        {
            if (node.Ancestors != null)
            {
                node.FAZ = (node.Predecessors is null)
                    ? 0
                    : node.Predecessors.Select(n => n.FEZ).Max();
                node.FEZ = node.FAZ + node.Duration;
                foreach (Node ancestor in node.Ancestors)
                {
                    ScheduleForward(ancestor);
                }
            }
        }

        private static void ScheduleBackwards(Node node)
        {
            int a = 3;
        }

        private static string[] ReadCheckCsv(string csvPath)
        {
            string[] lines = new string[0];
            try
            {
                lines = File.ReadAllLines(csvPath, Encoding.UTF7);
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

        private static void ConnectNodes(Dictionary<string, Node> nodes)
        {
            foreach (string key in nodes.Keys)
            {
                if (nodes[key].Predecessors != null)
                {
                    foreach (Node predecessor in nodes[key].Predecessors)
                    {
                        nodes[key].Ancestors.Add(predecessor);
                    }
                }
            }
        }

        private static Dictionary<string, Node> CreateNodes(string[] lines)
        {
            Dictionary<string, Node> nodes = new Dictionary<string, Node>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] props = lines[i].Split(';');

                List<Node> predecessors = null;
                if (props[3] != "-")
                {
                    predecessors = new List<Node>();

                    string[] predecessorStrings = props[3].Split(',');

                    foreach (string pre in predecessorStrings)
                    {
                        nodes.TryGetValue(pre, out Node preNode);
                        if (preNode != null)
                        {
                            predecessors.Add(preNode);
                        }
                    }
                }

                nodes.Add(
                    props[0],
                    new Node()
                    {
                        ID = props[0],
                        Description = props[1],
                        Duration = Int32.Parse(props[2]),
                        Predecessors = predecessors
                    });
            }
            return nodes;
        }
    }
}
