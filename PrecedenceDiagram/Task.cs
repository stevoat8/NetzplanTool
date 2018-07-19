using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PrecedenceDiagram
{
    /// <summary>
    /// Stellt einen Teilprozess innerhalb eines Netzplans dar.
    /// </summary>
    [DebuggerDisplay("{GetInfo(),nq}")]
    internal class Task
    {
        /// <summary>
        /// Eindeutige ID des Teilprozesses.
        /// </summary>
        internal string ID { get; }

        /// <summary>
        /// Beschreibung des Aufgabe des Teilprozesses.
        /// </summary>
        internal string Description { get; }

        /// <summary>
        /// Dauer des Teilprozesses.
        /// </summary>
        internal int Duration { get; }

        /// <summary>
        /// Die Teilprozesse die diesem Teilprozess direkt vorausgehen.
        /// </summary>
        internal List<Task> Predecessors { get; }

        /// <summary>
        /// Die Teilprozesse die direkt auf diesen Teilprozess folgen.
        /// </summary>
        internal List<Task> Ancestors { get; }

        #region Fristen

        /// <summary>
        /// Frühester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        internal int EarliestStart { get; set; }

        /// <summary>
        /// Frühester Endzeitpunkt des Teilprozesses.
        /// </summary>
        internal int EarliestFinish { get; set; }

        /// <summary>
        /// Spätester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        internal int LatestStart { get; set; }

        /// <summary>
        /// Spätester Endzeitpunkt des Teilprozesses.
        /// </summary>
        internal int LatestFinish { get; set; }

        /// <summary>
        /// Gesamtpuffer des Teilprozesses. Mögliche Verzögerung, ohne dass sich der gesamte Prozess verzögert.
        /// </summary>
        internal int TotalFloat { get; set; }

        /// <summary>
        /// Freier Puffer des Teilprozesses. Mögliche Verzögerung, ohne dass sich direkt folgende
        /// Teilprozesse verzögern.
        /// </summary>
        internal int FreeFloat { get; set; }

        #endregion Fristen

        /// <summary>
        /// Der Knoten is Teil des kritischen Pfads. Verzögerungen des Teilprozesses verzögern des
        /// gesamten Prozess. (TotalFloat/Gesamtpuffer = 0)
        /// </summary>
        internal bool IsCritical { get { return TotalFloat == 0; } }

        /// <summary>
        /// Gibt an, ob der Knoten der Startknoten eines Prozesses ist.
        /// </summary>
        internal bool IsInitialTask { get { return Predecessors.Count == 0; } }

        /// <summary>
        /// Gibt an, ob der Knoten der Endknoten eines Prozesses ist.
        /// </summary>
        internal bool IsFinalTask { get { return Ancestors.Count == 0; } }

        /// <summary>
        /// Erzeugt einen Teilprozess ohne Verbindungen zu Nachfolgern oder Vorgängern und ohne
        /// berechnete Fristen.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="duration"></param>
        internal Task(string id, string description, int duration)
        {
            ID = id;
            Description = description;
            Duration = duration;
            Predecessors = new List<Task>();
            Ancestors = new List<Task>();
        }

        /// <summary>
        /// Berechnet den frühesten Anfangs- und Endzeitpunkt (Vorwärtsterminierung).
        /// </summary>
        internal void SetStartingPoints()
        {
            EarliestStart = (IsInitialTask)
                ? 0
                : Predecessors.Select(pre => pre.EarliestStart + pre.Duration).Max();
            EarliestFinish = EarliestStart + Duration;
        }

        /// <summary>
        /// Berechnet den spätesten Anfangs- und Endzeitpunkt (Teil der Rückwärtsterminierung).
        /// </summary>
        internal void SetFinishingPoints()
        {
            LatestFinish = (IsFinalTask)
                            ? EarliestFinish
                            : Ancestors.Select(anc => anc.LatestStart).Min();
            LatestStart = LatestFinish - Duration;
        }

        /// <summary>
        /// Berechnet den freien und gesamten Puffer (Teil der Rückwärtsterminierung).
        /// </summary>
        internal void SetBuffers()
        {
            TotalFloat = LatestFinish - EarliestFinish;
            FreeFloat = (IsFinalTask)
                ? 0
                : Ancestors.Select(anc => anc.EarliestStart).Min() - EarliestFinish;
        }

        /// <summary>
        /// Erzeugt einen Text im DOT-Format, der den Teilprozess darstellt - ohne Verbindungen zu
        /// anderen Teilprozessen.
        /// </summary>
        /// <returns>Dot des Teilprozesses.</returns>
        internal string GetDot()
        {
            return
                $"proc{ID} [label=\"" +
                $"{{FAZ={EarliestStart}|FEZ={EarliestFinish}}}|" +
                $"{{{ID}|{Description}}}|" +
                $"{{{Duration}|GP={TotalFloat}|FP={FreeFloat}}}|" +
                $"{{SAZ={LatestStart}|SEZ={LatestFinish}}}" +
                $"\"];";
        }

        /// <summary>
        /// Erzeugt einen Text im DOT-Format, der alle Verbindungen zu seinen Vorgängern darstellt. 
        /// Ist der Teilprozess Teil des kritischen Pfades, wird die Verbindung gekennzeichnet.
        /// </summary>
        /// <returns>Dot des Verbindungen.</returns>
        internal string GetEdgeDot()
        {
            StringBuilder dotBuilder = new StringBuilder();
            foreach (Task ancestor in Ancestors)
            {
                string edgeDot = $"proc{ID} -> proc{ancestor.ID}";
                if (IsCritical && ancestor.IsCritical)
                {
                    edgeDot += " [color=\"red\"]";
                }
                dotBuilder.AppendLine(edgeDot + ";");
            }
            return dotBuilder.ToString();
        }

        /// <summary>
        /// Erzeugt eine textuelle Repräsentation eines Teilprozesses. Angelehnt an die Syntax des Prozessplans.
        /// </summary>
        /// <returns>Textuelle Repräsentation eines Teilprozesses</returns>
        internal string GetInfo()
        {
            string predecessors = String.Join(",", Predecessors.Select(n => n.ID));
            string ancestors = String.Join(",", Ancestors.Select(n => n.ID));
            predecessors = String.IsNullOrWhiteSpace(predecessors) ? "-" : predecessors;
            ancestors = String.IsNullOrWhiteSpace(ancestors) ? "-" : ancestors;

            return $"{ID} | {Description} | {Duration} | {predecessors} | {ancestors}";
        }
    }
}