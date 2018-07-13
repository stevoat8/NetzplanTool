using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netzplan
{
    internal class Node
    {
        internal string ID { get; set; }
        internal string Description { get; set; }
        internal int Duration { get; set; }
        internal IList<Node> Predecessors { get; set; }
        internal IList<Node> Ancestors { get; set; }

        /// <summary>
        /// Frühester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        internal int FAZ { get; set; }

        /// <summary>
        /// Frühester Endzeitpunkt des Teilprozesses.
        /// </summary>
        internal int FEZ { get; set; }

        /// <summary>
        /// Spätester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        internal int SAZ { get; set; }

        /// <summary>
        /// Spätester Endzeitpunkt des Teilprozesses.
        /// </summary>
        internal int SEZ { get; set; }

        /// <summary>
        /// Gesamtpuffer des Teilprozesses.
        /// </summary>
        internal int GP { get; set; }

        /// <summary>
        /// Freier Puffer des Teilprozesses.
        /// </summary>
        internal int FP { get; set; }

        internal Node(string id, string description, int duration, IList<Node> predecessors)
        {
            ID = id;
            Description = description;
            Duration = duration;
            Predecessors = predecessors;
            Ancestors = new List<Node>();
        }

        public override string ToString()
        {

            string predecessors = String.Join(",", Predecessors.Select(n => n.ID));
            string ancestors = String.Join(",", Ancestors.Select(n => n.ID));
            predecessors = String.IsNullOrWhiteSpace(predecessors) ? "-" : predecessors;
            ancestors = String.IsNullOrWhiteSpace(ancestors) ? "-" : ancestors;

            return $"{ID} | {Description} | {Duration} | {predecessors} | {ancestors}";
        }

        internal static bool IsInitialNode(Node node)
        {
            return node.Predecessors.Count == 0;
        }

        internal static bool IsFinalNode(Node node)
        {
            return node.Ancestors.Count == 0;
        }
    }

    //internal static class NodeExtension
    //{
    //    internal static bool IsInitialNode(this Node node)
    //    {
    //        return node.Ancestors.Count == 0;
    //    }

    //    internal static bool IsFinalNode(this Node node)
    //    {
    //        return node.Predecessors.Count == 0;
    //    }
    //}
}
