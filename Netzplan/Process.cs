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

        public Process(string name)
        {
            Title = name;
        }
    }
}
