using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netzplan
{
    public class Process
    {
        public string Title { get; set; }
        public Node IntitalNode { get; set; }
        public Node FinalNode { get; set; }
        public Dictionary<string, Node> Nodes { get; }

        public Process(string title, Dictionary<string, Node> nodes)
        {
            Title = title;
            Nodes = nodes;
            IntitalNode = nodes.Values.Where(n => n.Predecessors is null).First();
            FinalNode = nodes.Values.Where(n => n.Ancestors is null).First();
        }
    }
}
