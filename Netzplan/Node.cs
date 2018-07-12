using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netzplan
{
    public class Node
    {
        private string[] knodeProps;

        public string ID { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public IEnumerable<Node> Predecessors { get; set; }
        public IEnumerable<Node> Ancestors { get; set; }

        public int FAZ { get; set; }
        public int FEZ { get; set; }
        public int SAZ { get; set; }
        public int SEZ { get; set; }

        public Node()
        {
        }

        public Node(string id, string description, int duration, IEnumerable<Node> predecessors)
        {
            ID = id;
            Description = description;
            Duration = duration;
            Predecessors = predecessors;
        }
    }
}
