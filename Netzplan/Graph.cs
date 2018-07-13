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
        internal Node IntitalNode { get; set; }
        internal Node FinalNode { get; set; }
        internal Dictionary<string, Node> Nodes { get; }

        public Graph(string[] lines)
        {
            Dictionary<string, Node> nodes = CreateNodes(lines);
            ConnectNodes(nodes);
            Nodes = nodes;

            IntitalNode = nodes.Values.Where(n => Node.IsInitialNode(n)).First();
            FinalNode = nodes.Values.Where(n => Node.IsFinalNode(n)).First();

            ScheduleForward(IntitalNode);
            ScheduleBackwards(FinalNode);
            DetermineBuffers(IntitalNode);
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

            Node node = IntitalNode;

            while (Node.IsFinalNode(node) == false)
            {
                critPath.Add(node);
                node = node.Ancestors?.Where(n => n.SAZ == node.SEZ).First();
            }
            return String.Join(",", critPath.Select(n => n.ID));
        }

        public string GetDot()
        {
            throw new NotImplementedException();
        }
    }
}
