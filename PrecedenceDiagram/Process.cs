using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PrecedenceDiagram
{
    /// <summary>
    /// Stellt einen Prozess dar, der von einem Netzplan repräsentiert wird.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Process
    {
        /// <summary>
        /// Der Titel eines Prozesses.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Die Teilprozesse, aus denen der Gesamtprozess besteht.
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
        /// Erzeugt aus den Beschreibungen der Teilprozesse einen Gesamtprozeess und berechnet die Fristen.
        /// </summary>
        /// <param name="title">Der Titel des erstellten Prozesses.</param>
        /// <param name="processPlan">
        /// Beschreibungen der Teilprozesse, aus denen der Gesamtprozess besteht.
        /// </param>
        public Process(string title, string[] processPlan)
        {
            try
            {
                Title = title;

                List<Task> tasks = CreateTasks(processPlan);
                SetPredecessors(tasks, processPlan);
                SetAncestors(tasks);

                //ScheduleForward
                foreach (Task task in tasks)
                {
                    task.SetStartingPoints();
                }

                //ScheduleBackwards
                tasks.Reverse();
                foreach (Task task in tasks)
                {
                    task.SetFinishingPoints();
                    task.SetBuffers();
                }
                tasks.Reverse();

                Tasks = tasks;
            }
            catch (Exception ex)
            {
                throw new FormatException("Der Prozessplan hat ungültiges Format.", ex);
            }
        }

        /// <summary>
        /// Erzeugt einen Text im DOT-Format, der den Prozess samt Teilprozessen beschreibt.
        /// </summary>
        /// <returns>Beschreibung der Prozessstruktur im DOT-Format.</returns>
        public string GetDot()
        {
            StringBuilder dotBuilder = new StringBuilder();
            dotBuilder.AppendLine($"digraph {Title} {{");
            dotBuilder.AppendLine("node [shape=record];");
            dotBuilder.AppendLine("rankdir=LR;");

            foreach (Task task in Tasks)
            {
                dotBuilder.AppendLine(task.GetDot());
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
        /// Erzeugt aus dem Prozessplan die Teilprozesse des Prozesses. Ohne Vorgänger, Nachfolger
        /// und ohne berechnete Fristen.
        /// </summary>
        /// <param name="processPlan">Prozessplan, der die einzelnen Unterprozesse beschreibt.</param>
        /// <returns>Die Teilprozesse des Prozesses. (Ohne berechneten Fristen.)</returns>
        private static List<Task> CreateTasks(string[] processPlan)
        {
            List<Task> taskList = new List<Task>();

            foreach (string task in processPlan)
            {
                string[] props = task.Split(';');
                taskList.Add(new Task(props[0], props[1], Int32.Parse(props[2])));
            }
            return taskList;
        }

        /// <summary>
        /// Weist jedem Teilprozess seine Nachfolger zu.
        /// </summary>
        /// <param name="tasks">Liste der erstellten Teilprozesse.</param>
        private void SetAncestors(List<Task> tasks)
        {
            foreach (Task task in tasks)
            {
                foreach (Task predecessor in task.Predecessors)
                {
                    predecessor.Ancestors.Add(task);
                }
            }
        }

        /// <summary>
        /// Weist jedem Teilprozess, entsprechend dem Projektplan, seinen Vorgänger zu.
        /// </summary>
        /// <param name="tasks">Die Liste der erstellten Teilprozesse.</param>
        /// <param name="processPlan">Der Projektplans bzw, der Inhalt der CSV-Datei</param>
        private void SetPredecessors(List<Task> tasks, string[] processPlan)
        {
            foreach (string line in processPlan)
            {
                string[] properties = line.Split(';');

                string id = properties[0];
                Task task = tasks.Where(t => t.ID == id).First();

                string[] predecessorIds = properties[3].Split(',');
                foreach (string preId in predecessorIds)
                {
                    if (preId == "-")
                    {
                        continue;
                    }
                    //Alle Tasks, die dieselbe ID haben, wie die ID des betrachteten Task
                    task.Predecessors.AddRange(tasks.Where(t => t.ID == preId));
                }
            }
        }
    }
}