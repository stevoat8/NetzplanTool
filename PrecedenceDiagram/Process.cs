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
        public List<Task> Tasks { get; }

        /// <summary>
        /// Erzeugt aus den Beschreibungen der Teilprozesse einen Gesamtprozeess und berechnet die Fristen.
        /// </summary>
        /// <param name="title">Der Titel des erstellten Prozesses.</param>
        /// <param name="subtasks">Beschreibungen der Teilprozesse, aus denen der Gesamtprozess besteht.</param>
        public Process(string title, string[] subtasks)
        {
            Title = title;

            List<Task> tasks = CreateTasks(subtasks);

            IntitialTask = tasks.Where(n => n.IsInitialTask).First();
            FinalTask = tasks.Where(n => n.IsFinalTask).First();

            foreach (Task task in tasks)
            {
                ScheduleForward(task);
            }

            tasks.Reverse();
            foreach (Task task in tasks)
            {
                ScheduleBackwards(task);
            }
            tasks.Reverse();
            Tasks = tasks;
        }

        /// <summary>
        /// Erzeugt aus Beschreibungstexten die Teilprozesse des Prozesses.
        /// </summary>
        /// <param name="tasks">Beschreibungstext eines Teilprozesses.</param>
        /// <returns>Die Teilprozesse des Prozesses. (Ohne berechneten Fristen.)</returns>
        private static List<Task> CreateTasks(string[] tasks)
        {
            List<Task> taskList = new List<Task>();

            //Tasks erzeugen - ohne Vorgänger und Nachfolger
            foreach (string task in tasks)
            {
                string[] props = task.Split(';');
                taskList.Add(new Task(props[0], props[1], Int32.Parse(props[2])));
            }

            //Vorgänger setzen
            foreach (string taskLine in tasks)
            {
                string[] props = taskLine.Split(';');

                string id = props[0];
                Task thisTask = taskList.Where(t => t.ID == id).First();

                string[] predecessors = props[3].Split(',');
                foreach (string pre in predecessors)
                {
                    if (pre == "-")
                    {
                        continue;
                    }
                    foreach (Task task in taskList)
                    {
                        if(task.ID == pre)
                        {
                            thisTask.Predecessors.Add(task);
                        }
                    }
                    //TODO: Dozent fragen: Erzeugt Kopien???
                    //thisTask.Predecessors.Add(taskList.Where(t => t.ID == pre).FirstOrDefault());
                }
            }

            //Nachfolger setzen
            foreach (Task task in taskList)
            {
                foreach (Task predecessor in task.Predecessors)
                {
                    predecessor.Ancestors.Add(task);
                }
            }
            return taskList;
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

            foreach (Task t in Tasks)
            {
                string structure =
                    $"proc{t.ID} [label=\"" +
                    $"{{FAZ={t.EarliestStart}|FEZ={t.EarliestFinish}}}|" +
                    $"{{{t.ID}|{t.Description}}}|" +
                    $"{{{t.Duration}|GP={t.TotalFloat}|FP={t.FreeFloat}}}|" +
                    $"{{SAZ={t.LatestStart}|SEZ={t.LatestFinish}}}" +
                    $"\"]";
                dotBuilder.AppendLine(structure);
            }

            foreach (Task task in Tasks)
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
