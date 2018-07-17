using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netzplan
{
    public class Process
    {
        public string Title { get; set; }
        public Subtask IntitialNode { get; set; }
        public Subtask FinalNode { get; set; }
        public Dictionary<string, Subtask> Nodes { get; }

        private Process()
        {
        }

        public Process(string title, string[] subtasks)
        {
            Title = title;

            Dictionary<string, Subtask> nodes = CreateNodes(subtasks);
            SetAncestors(nodes);

            IntitialNode = nodes.Values.Where(n => n.IsInitialNode).First();
            FinalNode = nodes.Values.Where(n => n.IsFinalNode).First();

            List<string> keys = nodes.Keys.ToList();
            foreach (string key in keys)
            {
                ScheduleForward(nodes[key]);
            }

            keys.Reverse();
            foreach (string key in keys)
            {
                ScheduleBackwards(nodes[key]);
            }

            Nodes = nodes;
        }

        private static Dictionary<string, Subtask> CreateNodes(string[] subtasks)
        {
            Dictionary<string, Subtask> nodes = new Dictionary<string, Subtask>();
            for (int i = 0; i < subtasks.Length; i++)
            {
                string[] props = subtasks[i].Split(';');

                List<Subtask> predecessors = new List<Subtask>();
                if (props[3] == "-")
                {
                    continue;
                }
                string[] predecessorStrings = props[3].Split(',');

                foreach (string pre in predecessorStrings)
                {
                    nodes.TryGetValue(pre, out Subtask preNode);
                    if (preNode != null)
                    {
                        predecessors.Add(preNode);
                    }
                }

                nodes.Add(
                    props[0],
                    new Subtask(props[0], props[1], Int32.Parse(props[2]), predecessors));
            }
            return nodes;
        }

        private static void SetAncestors(Dictionary<string, Subtask> nodes)
        {
            foreach (string key in nodes.Keys)
            {
                foreach (Subtask predecessor in nodes[key].Predecessors)
                {
                    nodes[predecessor.ID].Ancestors.Add(nodes[key]);
                }
            }
        }

        private static void ScheduleForward(Subtask node)
        {
            node.EarliestStart = (node.IsInitialNode)
                ? 0
                : node.Predecessors.Select(n => n.EarliestStart + n.Duration).Max();
            node.EarliestFinish = node.EarliestStart + node.Duration;
        }

        private static void ScheduleBackwards(Subtask node)
        {
            node.LatestFinish = (node.IsFinalNode)
                            ? node.EarliestFinish
                            : node.Ancestors.Select(n => n.LatestStart).Min();
            node.LatestStart = node.LatestFinish - node.Duration;

            node.TotalFloat = node.LatestFinish - node.EarliestFinish;
            node.FreeFloat = (node.IsFinalNode)
                ? 0
                : node.Ancestors.Select(n => n.EarliestStart).Min() - node.EarliestFinish;
        }

        public string GetDot()
        {
            StringBuilder dotBuilder = new StringBuilder();
            dotBuilder.AppendLine($"digraph {Title} {{");
            dotBuilder.AppendLine("node [shape=record]");
            dotBuilder.AppendLine("rankdir=LR");

            foreach (Subtask n in Nodes.Values)
            {
                string structure =
                    $"proc{n.ID} [label=\"" +
                    $"{{FAZ={n.EarliestStart}|FEZ={n.EarliestFinish}}}|" +
                    $"{{{n.ID}|{n.Description}}}|" +
                    $"{{{n.Duration}|GP={n.TotalFloat}|FP={n.FreeFloat}}}|" +
                    $"{{SAZ={n.LatestStart}|SEZ={n.LatestFinish}}}" +
                    $"\"]";
                dotBuilder.AppendLine(structure);
            }

            foreach (Subtask node in Nodes.Values)
            {
                foreach (Subtask anc in node.Ancestors)
                {
                    string edge = $"proc{node.ID} -> proc{anc.ID}";
                    if (node.IsCritical && anc.IsCritical)
                    {
                        edge += " [color=\"red\"]";
                    }
                    dotBuilder.AppendLine(edge);
                }
            }
            dotBuilder.AppendLine("}");
            return dotBuilder.ToString();
        }
    }
}
