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
        public static void CreateGarph(string csvPath, string outputPath)
        {
            string processTitle = Path.GetFileNameWithoutExtension(csvPath);

            string[] lines = ReadCheckCsv(csvPath);

            List<Node> nodes = CreateNodes(lines);

            //Process process = Process.CreateProcess(nodes);

            //string graphDot = CreateGraphDot(process);

            //ExportGraph(outputPath, graphDot);
        }

        private static string[] ReadCheckCsv(string csvPath)
        {
            string[] lines = new string[0];
            try
            {
                lines = File.ReadAllLines(csvPath, Encoding.UTF7);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] props = lines[i].Split(';');
            }

            return lines;
        }

        private static List<Node> CreateNodes(string[] lines)
        {
            List<Node> nodes = new List<Node>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] props = lines[i].Split(';');

                List<Node> predecessors = null;
                if (props[3] != "-")
                {
                    string[] predecessorStrings = props[3].Split(',');

                    foreach (string pre in predecessorStrings)
                    {
                        Node preNode = nodes.Where(n => n.ID.Equals(pre)).FirstOrDefault();
                        if (preNode != null)
                        {
                            predecessors.Add(preNode);
                        }
                    }
                }

                nodes.Add(new Node()
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
