using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedenceDiagram
{
    /// <summary>
    /// Stellt einen Prozess dar, der von einem Netzplan repräsentiert wird.
    /// </summary>
    public class Process
    {
        /// <summary>
        /// Der Titel eines Prozesses.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Der erste Teilprozess. Es kann immer nur einen geben.
        /// </summary>
        public Task IntitialTask { get; set; }

        /// <summary>
        /// Der letzte Teilprozess. Es kann immer nur einen geben.
        /// </summary>
        public Task FinalTask { get; set; }

        /// <summary>
        /// Die Teilprozesse, aus denen der Gesamtprozess besteht.
        /// </summary>
        public Dictionary<string, Task> Tasks { get; }

        /// <summary>
        /// Erzeugt aus den Beschreibungen der Teilprozesse einen Gesamtprozeess und berechnet die Fristen.
        /// </summary>
        /// <param name="title">Der Titel des erstellten Prozesses.</param>
        /// <param name="subtasks">Beschreibungen der Teilprozesse, aus denen der Gesamtprozess besteht.</param>
        public Process(string title, string[] subtasks)
        {
            Title = title;

            Dictionary<string, Task> tasks = CreateTasks(subtasks);

            IntitialTask = tasks.Values.Where(n => n.IsInitialTask).First();
            FinalTask = tasks.Values.Where(n => n.IsFinalTask).First();

            List<string> keys = tasks.Keys.ToList();
            foreach (string key in keys)
            {
                ScheduleForward(tasks[key]);
            }

            keys.Reverse();
            foreach (string key in keys)
            {
                ScheduleBackwards(tasks[key]);
            }

            Tasks = tasks;
        }

        /// <summary>
        /// Erzeugt aus Beschreibungstexten die Teilprozesse des Prozesses.
        /// </summary>
        /// <param name="subtasks">Beschreibungstext eines Teilprozesses.</param>
        /// <returns>Die Teilprozesse des Prozesses. (Ohne berechneten Fristen.)</returns>
        private static Dictionary<string, Task> CreateTasks(string[] subtasks)
        {
            Dictionary<string, Task> tasks = new Dictionary<string, Task>();
            for (int i = 0; i < subtasks.Length; i++)
            {
                string[] props = subtasks[i].Split(';');

                //Vorgänger setzen
                List<Task> predecessors = new List<Task>();
                if (props[3] == "-")
                {
                    continue;
                }
                string[] predecessorStrings = props[3].Split(',');

                foreach (string pre in predecessorStrings)
                {
                    tasks.TryGetValue(pre, out Task preTask);
                    if (preTask != null)
                    {
                        predecessors.Add(preTask);
                    }
                }

                tasks.Add(
                    props[0],
                    new Task(props[0], props[1], Int32.Parse(props[2]), predecessors));
            }

            //Nachfolger setzen
            foreach (string key in tasks.Keys)
            {
                foreach (Task predecessor in tasks[key].Predecessors)
                {
                    tasks[predecessor.ID].Ancestors.Add(tasks[key]);
                }
            }
            return tasks;
        }

        /// <summary>
        /// Vorwärtsterminierung berechnet den frühesten Anfangs- und Endzeitpunkt.
        /// </summary>
        /// <param name="task"></param>
        private static void ScheduleForward(Task task)
        {
            task.EarliestStart = (task.IsInitialTask)
                ? 0
                : task.Predecessors.Select(n => n.EarliestStart + n.Duration).Max();
            task.EarliestFinish = task.EarliestStart + task.Duration;
        }

        /// <summary>
        /// Rückwärtsterminierung berechnet den spätesten Anfangs- und Endzeitpunkt, sowie den
        /// freien und gesamten Puffer.
        /// </summary>
        /// <param name="task"></param>
        private static void ScheduleBackwards(Task task)
        {
            task.LatestFinish = (task.IsFinalTask)
                            ? task.EarliestFinish
                            : task.Ancestors.Select(n => n.LatestStart).Min();
            task.LatestStart = task.LatestFinish - task.Duration;

            task.TotalFloat = task.LatestFinish - task.EarliestFinish;
            task.FreeFloat = (task.IsFinalTask)
                ? 0
                : task.Ancestors.Select(n => n.EarliestStart).Min() - task.EarliestFinish;
        }

        /// <summary>
        /// Erzeugt einen Text im DOT-Format, der den Prozess samt Teilprozessen beschreibt.
        /// DOT ist eine Beschreibungssprache für die visuelle Darstellung von Graphen.
        /// </summary>
        /// <returns>Beschreibung der Prozessstruktur im DOT-Format.</returns>
        public string GetDot()
        {
            StringBuilder dotBuilder = new StringBuilder();
            dotBuilder.AppendLine($"digraph {Title} {{");
            dotBuilder.AppendLine("node [shape=record]");
            dotBuilder.AppendLine("rankdir=LR");

            foreach (Task n in Tasks.Values)
            {
                string structure =
                    $"proc{n.ID} [label=\"" +
                    $"{{FAZ={n.EarliestStart}|FEZ={n.EarliestFinish}}}|" +
                    $"{{{n.ID}|{n.Description}}}|" +
                    $"{{{n.Duration}|GP={n.TotalFloat}|FP={n.FreeFloat}}}|" +
                    $"{{SAZ={n.LatestStart}|SEZ={n.LatestFinish}}}" +
                    $"\"]";
                dotBuilder.AppendLine(structure);
            }

            foreach (Task task in Tasks.Values)
            {
                foreach (Task anc in task.Ancestors)
                {
                    string edge = $"proc{task.ID} -> proc{anc.ID}";
                    if (task.IsCritical && anc.IsCritical)
                    {
                        edge += " [color=\"red\"]";
                    }
                    dotBuilder.AppendLine(edge);
                }
            }
            dotBuilder.AppendLine("}");
            return dotBuilder.ToString();
        }
    }
}
