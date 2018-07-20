using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PrecedenceDiagram
{
    /// <summary>
    /// Stellt einen Prozess dar, der aus einzelnen Vorgängen besteht und von einem Netzplan repräsentiert wird.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Process
    {
        /// <summary>
        /// Der Titel eines Prozesses.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Die einzelnen Vorgänge des Prozesses.
        /// </summary>
        internal List<Task> Tasks { get; }

        /// <summary>
        /// Erzeugt eien textuelle Repräsentation des Prozesses.
        /// </summary>
        private string DebuggerDisplay
        {
            get
            {
                int count = (Tasks == null) ? 0 : Tasks.Count;
                return $"{Title}: Count={count}";
            }
        }

        /// <summary>
        /// Erzeugt aus dem Prozessplan (welcher die einzelnen Vorgänge beschreibt) einen Gesamtprozeess und berechnet die Fristen.
        /// </summary>
        /// <param name="title">Der Titel des Prozesses.</param>
        /// <param name="processPlan">Beschreibungen der einzelnen Prozessvorgänge./// </param>
        public Process(string title, string[] processPlan)
        {
            try
            {
                Title = title;

                Tasks = CreateTasks(processPlan);
                SetPredecessors(processPlan);
                SetSuccessors();

                //Schedule forward
                foreach (Task task in Tasks)
                {
                    task.SetStartingPoints();
                }

                //Schedule backward
                Tasks.Reverse();
                foreach (Task task in Tasks)
                {
                    task.SetFinishingPoints();
                    task.SetFloat();
                }
                Tasks.Reverse();
            }
            catch (Exception ex)
            {
                throw new FormatException("Der Prozessplan hat ein ungültiges Format.", ex);
            }
        }

        /// <summary>
        /// Erzeugt einen Text im DOT-Format, der den gesamten Prozess beschreibt.
        /// </summary>
        /// <returns>Beschreibung des gesamten Prozesses im DOT-Format.</returns>
        public string GetDot()
        {
            StringBuilder dotBuilder = new StringBuilder();
            dotBuilder.AppendLine($"digraph \"{Title}\" {{");
            dotBuilder.AppendLine("node[shape=record];");
            dotBuilder.AppendLine("rankdir=LR;");

            foreach (Task task in Tasks)
            {
                dotBuilder.AppendLine(task.GetNodeDot());
            }

            foreach (Task task in Tasks)
            {
                string edgeDot = task.GetEdgeDot();
                if (String.IsNullOrWhiteSpace(edgeDot) == false)
                {
                    dotBuilder.Append(edgeDot);
                }
            }
            dotBuilder.AppendLine("}");
            return dotBuilder.ToString();
        }

        /// <summary>
        /// Erzeugt gemäß des Prozessplans die einzelnen Vorgänge des Prozesses.
        /// Die Vorgänge sind noch nicht verbunden und die Fristen nicht berechnet.
        /// </summary>
        /// <param name="processPlan">Prozessplan, der die einzelnen Vörgänge beschreibt.</param>
        /// <returns>Die Vörgänge des Prozesses.</returns>
        private static List<Task> CreateTasks(string[] processPlan)
        {
            List<Task> taskList = new List<Task>();

            foreach (string task in processPlan)
            {
                string[] properties = task.Split(';');
                taskList.Add(new Task(properties[0], properties[1], Int32.Parse(properties[2])));
            }
            return taskList;
        }

        /// <summary>
        /// Weist jedem Vorgang, entsprechend dem Projektplan, seine Vorgänger zu.
        /// </summary>
        /// <param name="processPlan">Der Projektplans bzw, der Inhalt der CSV-Datei</param>
        private void SetPredecessors(string[] processPlan)
        {
            foreach (string line in processPlan)
            {
                string[] properties = line.Split(';');

                string id = properties[0];
                Task task = Tasks.Where(t => t.ID == id).First();

                string[] predecessorIds = properties[3].Split(',');

                foreach (string preId in predecessorIds)
                {
                    task.Predecessors.AddRange(Tasks.Where(t => t.ID == preId));
                }
            }
        }

        /// <summary>
        /// Weist jedem Vorgang seine Nachfolger zu.
        /// </summary>
        private void SetSuccessors()
        {
            foreach (Task task in Tasks)
            {
                task.Successors.AddRange(
                    Tasks.Where(t => t.Predecessors.Contains(task)));
            }
        }
    }
}