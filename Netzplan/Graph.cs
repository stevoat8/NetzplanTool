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
        public string Title { get; set; }
        internal Node IntitialNode { get; set; }
        internal Node FinalNode { get; set; }
        internal Dictionary<string, Node> Nodes { get; }

        public Graph(string title, string[] lines)
        {
            Title = title;
            Dictionary<string, Node> nodes = CreateNodes(lines);
            ConnectNodes(nodes);
            Nodes = nodes;

            IntitialNode = nodes.Values.Where(n => Node.IsInitialNode(n)).First();
            FinalNode = nodes.Values.Where(n => Node.IsFinalNode(n)).First();

            ScheduleForward(IntitialNode);
            ScheduleBackwards(FinalNode);
            DetermineBuffers(IntitialNode);
        }

        private static Dictionary<string, Node> CreateNodes(string[] lines)
        {
            //TODO: CreateNodes und ConnectNodes zusammenfügen
            Dictionary<string, Node> nodes = new Dictionary<string, Node>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] props = lines[i].Split(';');

                List<Node> predecessors = new List<Node>();
                if (props[3] != "-")
                {
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
                    new Node(props[0], props[1], Int32.Parse(props[2]), predecessors));
            }
            return nodes;
        }

        private static void ConnectNodes(Dictionary<string, Node> nodes)
        {
            foreach (string key in nodes.Keys)
            {
                foreach (Node predecessor in nodes[key].Predecessors)
                {
                    nodes[predecessor.ID].Ancestors.Add(nodes[key]);
                }
            }
        }

        private static void ScheduleForward(Node node)
        {
            node.FAZ = (Node.IsInitialNode(node))
                ? 0
                : node.Predecessors.Select(n => n.FAZ + n.Duration).Max();
            node.FEZ = node.FAZ + node.Duration;

            foreach (Node ancestor in node.Ancestors)
            {
                ScheduleForward(ancestor);
            }
        }

        private static void ScheduleBackwards(Node node)
        {
            node.SEZ = (Node.IsFinalNode(node))
                ? node.FEZ
                : node.Ancestors.Select(n => n.FAZ).Min();
            node.SAZ = node.SEZ - node.Duration;
            foreach (Node predecessor in node.Predecessors)
            {
                ScheduleBackwards(predecessor);
            }
        }

        private static void DetermineBuffers(Node node)
        {
            node.GP = node.SEZ - node.FEZ;
            node.FP = (Node.IsFinalNode(node))
                ? 0
                : node.Ancestors.Select(n => n.FAZ).Min() - node.FEZ;

            foreach (Node ancestor in node.Ancestors)
            {
                DetermineBuffers(ancestor);
            }
        }

        private string GetCriticalPath()
        {
            List<Node> critPath = new List<Node>();

            Node node = IntitialNode;

            while (Node.IsFinalNode(node) == false)
            {
                critPath.Add(node);
                node = node.Ancestors?.Where(n => n.SAZ == node.SEZ).First();
            }
            return String.Join(",", critPath.Select(n => n.ID));
        }

        public string GetDot()
        {
            StringBuilder dotBuilder = new StringBuilder();
            dotBuilder.AppendLine($"digraph {Title}" + "{");
            dotBuilder.AppendLine("node[shape=record]");
            dotBuilder.AppendLine("rankdir = LR");

            foreach (Node n in Nodes.Values)
            {
                string structure =
                    $"{n.ID} [label:\"" +
                    $"{{FAZ={n.FAZ}|FEZ={n.FEZ}}}|" +
                    $"{{{n.ID}|{n.Description}}}|" +
                    $"{{{n.Duration}|GP={n.GP}|FP={n.FP}}}|" +
                    $"{{FAZ={n.SAZ}|FEZ={n.SEZ}}}" +
                    $"\"]";
                dotBuilder.AppendLine(structure);
            }

            foreach (Node node in Nodes.Values)
            {
                foreach (Node anc in node.Ancestors)
                {
                    //TODO: Check if crit path -> then red
                    string edge = $"{node.ID} -> {anc.ID}";
                    edge += (node.IsCritical) ? " [color=\"red\"]" : "";
                    dotBuilder.AppendLine(edge);
                }
            }
            dotBuilder.AppendLine("}");
            return dotBuilder.ToString();
        }
    }
}
