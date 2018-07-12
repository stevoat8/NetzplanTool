using NetzplanTool.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Process.Model;

namespace NetzplanTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvPath = args[1];
            string outputPath = args[2];

            string processTitle = Path.GetFileNameWithoutExtension(csvPath);

            ReadCsv(csvPath, processTitle);
        }

        private static void ReadCsv(string csvPath, string processTitle)
        {
            string[] lines = File.ReadAllLines(csvPath);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] knotenStr = lines[i].Split(',');
            }
        }
    }
}
