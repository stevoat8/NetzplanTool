using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetzplanTool.Model
{
    public class Process
    {
        public string Title { get; set; }
        public Knode IntitalKnode { get; set; }

        public Process(string name)
        {
            Title = name;
        }
    }
}
