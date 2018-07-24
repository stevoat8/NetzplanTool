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
        internal List<Task> Successors { get; }

        #region Fristen

        /// <summary>
        /// Frühester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        internal int EarliestStartingPoint { get; set; }

        /// <summary>
        /// Frühester Endzeitpunkt des Teilprozesses.
        /// </summary>
        internal int EarliestFinishingPoint { get; set; }

        /// <summary>
        /// Spätester Anfangzeitpunkt des Teilprozesses.
        /// </summary>
        internal int LatestStartingPoint { get; set; }

        /// <summary>
        /// Spätester Endzeitpunkt des Teilprozesses.
        /// </summary>
        internal int LatestFinishingPoint { get; set; }

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
        internal bool IsInitial { get { return Predecessors.Count == 0; } }

        /// <summary>
        /// Gibt an, ob der Knoten der Endknoten eines Prozesses ist.
        /// </summary>
        internal bool IsFinal { get { return Successors.Count == 0; } }

        /// <summary>
        /// Erzeugt einen Prozessvorgang ohne Verbindungen zu Nachfolgern oder Vorgängern und ohne
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
            Successors = new List<Task>();
        }

        /// <summary>
        /// Berechnet den frühesten Anfangs- und Endzeitpunkt (Vorwärtsterminierung).
        /// </summary>
        internal void SetStartingPoints()
        {
            EarliestStartingPoint = IsInitial
                ? 0
                : Predecessors.Select(pre => pre.EarliestStartingPoint + pre.Duration).Max();
            EarliestFinishingPoint = EarliestStartingPoint + Duration;
        }

        /// <summary>
        /// Berechnet den spätesten Anfangs- und Endzeitpunkt (Teil der Rückwärtsterminierung).
        /// </summary>
        internal void SetFinishingPoints()
        {
            LatestFinishingPoint = IsFinal 
                ? EarliestFinishingPoint 
                : Successors.Select(successor => successor.LatestStartingPoint).Min();
            LatestStartingPoint = LatestFinishingPoint - Duration;
        }

        /// <summary>
        /// Berechnet den freien und gesamten Puffer (Teil der Rückwärtsterminierung).
        /// </summary>
        internal void SetFloat()
        {
            TotalFloat = LatestFinishingPoint - EarliestFinishingPoint;
            FreeFloat = IsFinal
                ? 0
                : Successors.Select(successor => successor.EarliestStartingPoint).Min() - EarliestFinishingPoint;
        }

        /// <summary>
        /// Erzeugt einen Text im DOT-Format, der den Vorgang als Grahpknoten beschreibt.
        /// </summary>
        /// <returns>Beschreibung als Graphknoten im DOT-Format.</returns>
        internal string GetNodeDot()
        {
            return
                $"\"proc{ID}\" [label=\"" +
                $"{{FAZ={EarliestStartingPoint}|FEZ={EarliestFinishingPoint}}}|" +
                $"{{{ID}|{Description}}}|" +
                $"{{{Duration}|GP={TotalFloat}|FP={FreeFloat}}}|" +
                $"{{SAZ={LatestStartingPoint}|SEZ={LatestFinishingPoint}}}" +
                $"\"];";
        }

        /// <summary>
        /// Erzeugt einen Text im DOT-Format, der die Verbindungen des Vorgangs zu seinen
        /// Vorgängern als Graphkante beschreibt.
        /// </summary>
        /// <returns>Beschreibung der Graphkanten im DOT-Format.</returns>
        internal string GetEdgeDot()
        {
            StringBuilder dotBuilder = new StringBuilder();
            foreach (Task successor in Successors)
            {
                string edgeDot = $"proc{ID} -> proc{successor.ID}";
                if (IsCritical && successor.IsCritical)
                {
                    edgeDot += " [color=\"red\"]";
                }
                dotBuilder.AppendLine(edgeDot + ";");
            }
            return dotBuilder.ToString();
        }

        /// <summary>
        /// Erzeugt eine textuelle Repräsentation des Vorgangs. Angelehnt an die Syntax des Prozessplans.
        /// </summary>
        /// <returns>Textuelle Repräsentation des Vorgangs.</returns>
        internal string GetInfo()
        {
            string predecessors = String.Join(",", Predecessors.Select(n => n.ID));
            string ancestors = String.Join(",", Successors.Select(n => n.ID));
            predecessors = String.IsNullOrWhiteSpace(predecessors) ? "-" : predecessors;
            ancestors = String.IsNullOrWhiteSpace(ancestors) ? "-" : ancestors;

            return $"{ID} | {Description} | {Duration} | {predecessors} | {ancestors}";
        }
    }
}