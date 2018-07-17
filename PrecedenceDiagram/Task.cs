﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedenceDiagram
{
    public class Task
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public IList<Task> Predecessors { get; set; }
        public IList<Task> Ancestors { get; set; }

        /// <summary>
        /// Frühester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        public int EarliestStart { get; set; }

        /// <summary>
        /// Frühester Endzeitpunkt des Teilprozesses.
        /// </summary>
        public int EarliestFinish { get; set; }

        /// <summary>
        /// Spätester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        public int LatestStart { get; set; }

        /// <summary>
        /// Spätester Endzeitpunkt des Teilprozesses.
        /// </summary>
        public int LatestFinish { get; set; }

        /// <summary>
        /// Gesamtpuffer des Teilprozesses. Mögliche Verzögerung, ohne dass 
        /// sich der gesamte Prozess verzögert.
        /// </summary>
        public int TotalFloat { get; set; }

        /// <summary>
        /// Freier Puffer des Teilprozesses. Mögliche Verzögerung, ohne dass 
        /// sich direkt folgende Teilprozesse verzögern.
        /// </summary>
        public int FreeFloat { get; set; }

        /// <summary>
        /// Der Knoten is Teil des kritischen Pfads. Verzögerungen des Teilprozesses
        /// verzögern des gesamten Prozess. (TotalFloat/Gesamtpuffer = 0)
        /// </summary>
        public bool IsCritical { get { return TotalFloat == 0; } }

        /// <summary>
        /// Gibt an, ob der Knoten der Startknoten eines Prozesses ist.
        /// </summary>
        public bool IsInitialTask { get { return Predecessors.Count == 0; } }

        /// <summary>
        /// Gibt an, ob der Knoten der Endknoten eines Prozesses ist.
        /// </summary>
        public bool IsFinalTask { get { return Ancestors.Count == 0; } }

        public Task(string id, string description, int duration, IList<Task> predecessors)
        {
            ID = id;
            Description = description;
            Duration = duration;
            Predecessors = predecessors;
            Ancestors = new List<Task>();
        }

        public override string ToString()
        {

            string predecessors = String.Join(",", Predecessors.Select(n => n.ID));
            string ancestors = String.Join(",", Ancestors.Select(n => n.ID));
            predecessors = String.IsNullOrWhiteSpace(predecessors) ? "-" : predecessors;
            ancestors = String.IsNullOrWhiteSpace(ancestors) ? "-" : ancestors;

            return $"{ID} | {Description} | {Duration} | {predecessors} | {ancestors}";
        }
    }
}