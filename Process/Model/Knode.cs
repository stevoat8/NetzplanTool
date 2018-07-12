using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Process.Model
{
    public class Knode
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public Knode Predecessor { get; set; }

        public Knode(string title, string description, int duration, Knode predecessor)
        {
            Title = title;
            Description = description;
            Duration = duration;
            Predecessor = predecessor;
        }


    }
}
