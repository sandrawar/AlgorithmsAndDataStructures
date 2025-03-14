using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using ASD.Graphs;

namespace ASD2
{
    public class GraphColorer_4_5_FailedMycielski : MarshalByRefObject
    {
        private class GraphColoringInfo
        {
            public GraphColoringInfo(Graph g, int[] coloring, HashSet<int>[] neighboursColors, int[] uncoloredNeighbours, int colorCount, int usedColorsCount)
            {
                this.g = g;
                this.coloring = coloring;
                this.neighboursColors = neighboursColors;
                this.uncoloredNeighbours = uncoloredNeighbours;
                this.colorCount = colorCount;
                this.usedColorsCount = usedColorsCount;
            }

            public Graph g;
            public int[] coloring;
            public HashSet<int>[] neighboursColors;
            public int[] uncoloredNeighbours;
            public int colorCount;
            public int usedColorsCount;
        }


        /// <summary>
        /// Metoda znajduje kolorowanie zadanego grafu g używające najmniejsze możliwej liczby kolorów.
        /// </summary>
        /// <param name="g">Graf (nieskierowany)</param>
        /// <returns>Liczba użytych kolorów i kolorowanie (coloring[i] to kolor wierzchołka i). Kolory mogą być dowolnymi liczbami całkowitymi.</returns>
        public (int numberOfColors, int[] coloring) FindBestColoring(Graph g)
        {
            //var totalTimeStopwatch = DebugHelper.StartStopwatch();
            //DebugHelper.DebugInfo();
            //DebugHelper.DebugInfo($"graph size {g.VertexCount}");

            int[] bestColoring = new int[g.VertexCount];

            if (g.EdgeCount == 0)
            {
                for (int i = 0; i < g.VertexCount; i++)
                {
                    bestColoring[i] = 1;
                }
                return (1, bestColoring);
            }

            var isClique = g.EdgeCount == g.VertexCount * (g.VertexCount - 1) / 2;
            if (isClique)
            {
                for (int i = 0; i < g.VertexCount; i++)
                {
                    bestColoring[i] = i + 1;
                }
                return (g.VertexCount, bestColoring);
            }

            var maxDegree = 0;
            var minDegree = int.MaxValue;
            int[] coloring = new int[g.VertexCount];
            int[] uncoloredNeighbours = new int[g.VertexCount];
            HashSet<int>[] neighboursColors = new HashSet<int>[g.VertexCount];

            for (var v = 0; v < g.VertexCount; v++)
            {
                neighboursColors[v] = new HashSet<int>();
                maxDegree = Math.Max(maxDegree, g.Degree(v));
                minDegree = Math.Min(minDegree, g.Degree(v));
            }

            //var n = (double)g.VertexCount;
            //var x = ((n - 1) * n) / (n * (n - 1) - 2 * g.EdgeCount);
            //var minColorCount = (int)Math.Ceiling(x);

            int notEnoughColorCount = 1;
            int enoughColorCount = maxDegree + 1;

            //DebugHelper.DebugInfo($"minDegree {minDegree}");
            //DebugHelper.DebugInfo($"maxDegree {maxDegree}");
            //DebugHelper.DebugInfo($"notEnoughColorCount {notEnoughColorCount}");
            //DebugHelper.DebugInfo($"enoughColorCount {enoughColorCount}");

            var initCheck = true;

            while (notEnoughColorCount < enoughColorCount - 1)
            {
                //var iterationStopwatch = DebugHelper.StartStopwatch();

                var colorCount = initCheck
                    ? enoughColorCount
                    : enoughColorCount - 1;
                //: Math.Min((notEnoughColorCount + enoughColorCount) / 2, enoughColorCount - 1);

                initCheck = false;

                var coloringInfo = new GraphColoringInfo(g, coloring, neighboursColors, uncoloredNeighbours, colorCount, 0);
                for (var v = 0; v < g.VertexCount; v++)
                {
                    ChangeVertexColor(coloringInfo, v, 0);
                }

                if (ColorGraph(coloringInfo))
                {
                    HashSet<int> usedColors = new HashSet<int>();
                    for (int i = 0; i < g.VertexCount; i++)
                    {
                        bestColoring[i] = coloring[i];
                        if (!usedColors.Contains(coloring[i]))
                        {
                            usedColors.Add(coloring[i]);
                        }
                    }
                    enoughColorCount = usedColors.Count;
                    //DebugHelper.DebugInfo($"Completed check for {colorCount}, ok, found {enoughColorCount} in {iterationStopwatch.Elapsed}");
                }
                else
                {
                    notEnoughColorCount = colorCount;
                    //DebugHelper.DebugInfo($"Completed check for {colorCount}, not enough in {iterationStopwatch.Elapsed}");
                }
            }

            //DebugHelper.DebugInfo($"Found mono {enoughColorCount} in {totalTimeStopwatch.Elapsed}");
            //DebugHelper.DebugInfo();
            return (enoughColorCount, bestColoring);
        }

        private static bool ColorGraph(GraphColoringInfo info)
        {
            int nextVertex = NextVertex(info);
            if (nextVertex == -1)
            {
                return true;
            }

            var g = info.g;
            var coloring = info.coloring;
            var neighboursColors = info.neighboursColors;
            var uncoloredNeighbours = info.uncoloredNeighbours;
            var colorCount = info.colorCount;
            var usedColorsCount = info.usedColorsCount;

            var colorLimit = Math.Min(colorCount, usedColorsCount + 1);
            for (int color = 1; color <= colorLimit; color++)
            {
                var canUseColor = CanUseColor(g, coloring, nextVertex, color);
                if (canUseColor)
                {
                    ChangeVertexColor(info, nextVertex, color);

                    var nextUsedColorsCount = color > usedColorsCount ? usedColorsCount + 1 : usedColorsCount;
                    var nextInfo = new GraphColoringInfo(g, coloring, neighboursColors, uncoloredNeighbours, colorCount, nextUsedColorsCount);
                    if (ColorGraph(nextInfo))
                    {
                        return true;
                    }

                    ChangeVertexColor(info, nextVertex, 0);

                }
            }
            return false;
        }

        private static int NextVertex(GraphColoringInfo info)
        {
            var g = info.g;
            var coloring = info.coloring;
            var neighboursColors = info.neighboursColors;
            var uncoloredNeighbours = info.uncoloredNeighbours;
            var colorCount = info.colorCount;

            var nextVertex = -1;
            var currentMinAvailableColors = int.MaxValue;
            var currentMaxAvailableColors = 0;
            var currentMinDegree = int.MaxValue;
            var currentMaxDegree = 0;
            var currentMinUncolorodNeighbours = int.MaxValue;
            var currentMaxUncolorodNeighbours = 0;
            var currentMinColorodNeighbours = int.MaxValue;
            var currentMaxColorodNeighbours = 0;

            for (var v = 0; v < g.VertexCount; v++)
            {
                if (coloring[v] == 0)
                {
                    var availableColors = colorCount - neighboursColors[v].Count;
                    var vertexDegree = g.Degree(v);
                    var uncoloredNeighboursCount = uncoloredNeighbours[v];
                    var coloredNeighbours = vertexDegree - uncoloredNeighboursCount;

                    void SetVals()
                    {
                        nextVertex = v;
                        currentMinAvailableColors = availableColors;
                        currentMaxAvailableColors = availableColors;
                        currentMinDegree = vertexDegree;
                        currentMaxDegree = vertexDegree;
                        currentMinUncolorodNeighbours = uncoloredNeighboursCount;
                        currentMaxUncolorodNeighbours = uncoloredNeighboursCount;
                        currentMinColorodNeighbours = coloredNeighbours;
                        currentMaxColorodNeighbours = coloredNeighbours;
                    }

                    if (availableColors < currentMinAvailableColors)
                    {
                        SetVals();
                    }
                    else if (availableColors == currentMinAvailableColors)
                    {
                        if (vertexDegree > currentMaxDegree)
                        {
                            SetVals();
                        }
                        else if (vertexDegree == currentMaxDegree)
                        {
                            if (coloredNeighbours > currentMaxColorodNeighbours)
                            {
                                SetVals();
                            }
                            else if (coloredNeighbours == currentMaxColorodNeighbours)
                            {
                                if (uncoloredNeighboursCount > currentMaxUncolorodNeighbours)
                                {
                                    SetVals();
                                }
                            }
                        }
                    }
                }
            }
            return nextVertex;
        }

        private static void ChangeVertexColor(GraphColoringInfo info, int vertex, int color)
        {
            var g = info.g;
            var coloring = info.coloring;
            var neighboursColors = info.neighboursColors;
            var uncoloredNeighbours = info.uncoloredNeighbours;

            coloring[vertex] = color;
            foreach (var neighbour in g.OutNeighbors(vertex))
            {
                //CalcNeighboursColors(g, coloring, neighboursColors, uncoloredNeighbours, neighbour);
                neighboursColors[neighbour].Clear();
                uncoloredNeighbours[neighbour] = 0;

                foreach (var n in g.OutNeighbors(neighbour))
                {
                    if (coloring[n] == 0)
                    {
                        uncoloredNeighbours[n]++;
                    }
                    else
                    {
                        if (!neighboursColors[neighbour].Contains(coloring[n]))
                        {
                            neighboursColors[neighbour].Add(coloring[n]);
                        }
                    }
                }
            }
        }

        private static bool CanUseColor(Graph g, int[] coloring, int vertex, int color)
        {
            foreach (var neighbor in g.OutNeighbors(vertex))
            {
                if (coloring[neighbor] == color)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
