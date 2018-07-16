using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netzplan
{
    public class Node
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public IList<Node> Predecessors { get; set; }
        public IList<Node> Ancestors { get; set; }

        /// <summary>
        /// Frühester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        public int FAZ { get; set; }

        /// <summary>
        /// Frühester Endzeitpunkt des Teilprozesses.
        /// </summary>
        public int FEZ { get; set; }

        /// <summary>
        /// Spätester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        public int SAZ { get; set; }

        /// <summary>
        /// Spätester Endzeitpunkt des Teilprozesses.
        /// </summary>
        public int SEZ { get; set; }

        /// <summary>
        /// Gesamtpuffer des Teilprozesses.
        /// </summary>
        public int GP { get; set; }

        /// <summary>
        /// Freier Puffer des Teilprozesses.
        /// </summary>
        public int FP { get; set; }

        /// <summary>
        /// Der Knoten is Teil des kritishen Pfads. (GesamtPuffer = 0)
        /// </summary>
        public bool IsCritical { get { return GP == 0; } }

        /// <summary>
        /// Gibt an, ob der Knoten der Ursprungsknoten eines Graphen ist.
        /// </summary>
        public bool IsInitialNode { get { return Predecessors.Count == 0; } }

        /// <summary>
        /// Gibt an, ob der Knoten der Endknoten eines Graphen ist.
        /// </summary>
        public bool IsFinalNode { get { return Ancestors.Count == 0; } }

        public Node(string id, string description, int duration, IList<Node> predecessors)
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
    }
}
