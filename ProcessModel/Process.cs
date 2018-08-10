using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProcessModel
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
            Title = title;
            Tasks = CreateTasks(processPlan);
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
        /// Erzeugt gemäß des Prozessplans die einzelnen Vorgänge des Prozesses inkl. der
        /// Verbindungen der Vorgänge untereinander und der berechneten Fristen.
        /// </summary>
        /// <param name="processPlan">Prozessplan, der die einzelnen Vörgänge beschreibt.</param>
        /// <returns>Die Vörgänge des Prozesses.</returns>
        private static List<Task> CreateTasks(string[] processPlan)
        {
            List<Task> tasks = new List<Task>();
            int lineCount = 1;

            //Kopfzeile eliminieren. Außer bei der Kopfzeile ist der 3. Wert immer eine Ziffer
            if (Regex.IsMatch(processPlan[0], @"\w+;\w+;\w+;\w+"))
            {
                processPlan = processPlan.Skip(1).ToArray();
                lineCount++;
            }

            foreach (string line in processPlan)
            {
                string[] properties = line.Split(';');
                string id = properties[0].Trim();
                string description = properties[1].Trim().Replace("\t", " ");
                string[] predecessorIds = Regex.Replace(properties[3], @"\s+", "").Split(',');

                string errorMsgHeader = $"Formatfehler in Zeile {lineCount} (Vorgang \"{id}\"):";

                if (properties.Length != 4)
                {
                    throw new FormatException($"{errorMsgHeader} \"{line}\" definiert nicht genau 4 Werte.");
                }

                if (Regex.IsMatch(id, @"^[A-Za-z0-9]+$") == false)
                {
                    throw new FormatException($"{errorMsgHeader} \"{id}\" ist keine zulässige Vorgangs-ID." +
                        $" Zulässige Zeichen sind: A-Z, a-z und 0-9. Keine Leerzeichen.");
                }

                int duration = -1;
                try
                {
                    duration = Int32.Parse(properties[2]);
                }
                catch (Exception)
                {
                    throw new FormatException($"{errorMsgHeader} " +
                        $"\"{properties[2]}\" stellt keine Vorgangsdauer dar. Nur Ziffern erlaubt.");
                }
                if (duration <= 0)
                {
                    throw new FormatException($"{errorMsgHeader} Die Dauer des Vorgangs muss größer 0 sein.");
                }

                Task task = new Task(id, description, duration);

                try
                {
                    task.Predecessors.AddRange(
                        GetPredecessors(tasks, predecessorIds));
                }
                catch (FormatException ex)
                {
                    throw new FormatException(errorMsgHeader + " " + ex.Message);
                }

                tasks.Add(task);

                lineCount++;
            }

            SetSuccessors(tasks);

            ScheduleTasks(tasks);

            return tasks;
        }

        private static List<Task> GetPredecessors(List<Task> tasks, string[] predecessorIds)
        {
            List<Task> predecessors = new List<Task>();
            if (predecessorIds[0] == "-")
            {
                if (tasks.Count > 0)
                {
                    throw new FormatException($"Es darf nur einen Startknoten geben.");
                }
            }
            else
            {
                foreach (string predecessorId in predecessorIds)
                {
                    Task tempTask = tasks.Where(t => t.ID == predecessorId).FirstOrDefault();
                    if (tempTask == null)
                    {
                        throw new FormatException($"An dieser Stelle wurde noch kein " +
                                $"Vorgang mit der ID \"{predecessorId}\" definiert.");
                    }
                    predecessors.Add(tempTask);
                }
            }
            return predecessors;
        }

        private static void ScheduleTasks(List<Task> tasks)
        {
            //Schedule forward
            foreach (Task task in tasks)
            {
                task.ForwardPassCalculation();
            }

            //Schedule backward
            tasks.Reverse();
            foreach (Task task in tasks)
            {
                task.BackwardPassCalculation();
            }
            tasks.Reverse();
        }

        /// <summary>
        /// Weist jedem Vorgang seine Nachfolger zu.
        /// </summary>
        /// <param name="tasks">Die Vorgänge deren Nachfolger ermittelt und gesetzt werden.</param>
        private static void SetSuccessors(List<Task> tasks)
        {
            foreach (Task task in tasks)
            {
                task.Successors.AddRange(
                    tasks.Where(t => t.Predecessors.Contains(task)));
            }

            List<Task> finalTasks = tasks.FindAll(t => t.IsFinal);
            if (finalTasks.Count > 1)
            {
                string ids = String.Join(",", finalTasks.Select(t => t.ID));
                throw new FormatException($"Formatfehler: Es gibt mehr als einen Endknoten. Folgende Vorgänge haben keine Nachfolger: {ids}.");
            }
        }
    }
}