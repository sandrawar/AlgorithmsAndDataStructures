using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using ASD.Graphs;

namespace ASD2
{
    public class GraphColorer : MarshalByRefObject
    {
        /// <summary>
        /// Metoda znajduje kolorowanie zadanego grafu g używające najmniejsze możliwej liczby kolorów.
        /// </summary>
        /// <param name="g">Graf (nieskierowany)</param>
        /// <returns>Liczba użytych kolorów i kolorowanie (coloring[i] to kolor wierzchołka i). Kolory mogą być dowolnymi liczbami całkowitymi.</returns>
        public (int numberOfColors, int[] coloring) FindBestColoring(Graph g)
        {
            var bestColoring = new int[0];

            var colorCount = 0;
            if (ColoringIfEmpty(g, out bestColoring, out colorCount)
                || ColoringIfNoEdges(g, out bestColoring, out colorCount)
                || ColoringIfClique(g, out bestColoring, out colorCount))
            {
                return (colorCount, bestColoring);
            }

            var gci = new GraphColoringInfo(g);

            var notEnoughColorCount = 1;
            var enoughColorCount = gci.MaxDegree + 1;

            var initCheck = true;

            while (notEnoughColorCount < enoughColorCount - 1)
            {

                var colorLimit = initCheck
                    ? enoughColorCount
                    : enoughColorCount - 1;
                //: Math.Min((notEnoughColorCount + enoughColorCount) / 2, enoughColorCount - 1);

                initCheck = false;

                gci.Init(colorLimit);

                if (ColorGraphRecursive(gci, 0))
                {
                    bestColoring = gci.GetColoringCopy(out var usedColorsCount);
                    enoughColorCount = usedColorsCount;
                }
                else
                {
                    notEnoughColorCount = colorLimit;
                }
            }

            return (enoughColorCount, bestColoring);
        }


        private static bool ColorGraphRecursive(GraphColoringInfo gci, int usedColorsCount)
        {
            var nextVertex = gci.GetNextVertexForColoring();
            if (nextVertex == -1)
            {
                return true;
            }

            var vertexColorLimit = Math.Min(gci.ColorLimit, usedColorsCount + 1);
            for (int color = 1; color <= vertexColorLimit; color++)
            {
                var canUseColor = gci.CanUseColor(nextVertex, color);
                if (canUseColor)
                {

                    gci.ChangeVertexColor(nextVertex, color);

                    var nextUsedColorsCount = color > usedColorsCount ? usedColorsCount + 1 : usedColorsCount;
                    if (ColorGraphRecursive(gci, nextUsedColorsCount))
                    {
                        return true;
                    }

                    gci.ChangeVertexColor(nextVertex, 0);
                }
            }

            return false;
        }

        private bool ColoringIfEmpty(Graph g, out int[] coloring, out int colorCount)
        {
            colorCount = 0;
            coloring = new int[0];

            return (g.VertexCount == 0);
        }

        private bool ColoringIfNoEdges(Graph g, out int[] coloring, out int colorCount)
        {
            colorCount = 1;
            coloring = new int[0];

            if (g.EdgeCount > 0)
            {
                return false;
            }

            coloring = new int[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
            {
                coloring[i] = 1;
            }
            return true;
        }

        private bool ColoringIfClique(Graph g, out int[] coloring, out int colorCount)
        {
            colorCount = g.VertexCount;
            coloring = new int[0];

            var isClique = g.EdgeCount == g.VertexCount * (g.VertexCount - 1) / 2;

            if (!isClique)
            {
                return false;
            }

            coloring = new int[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
            {
                coloring[i] = i + 1;
            }
            return true;
        }

        private class GraphColoringInfo
        {
            private readonly VertexInfo[] vertexInformation;

            public GraphColoringInfo(Graph g)
            {
                vertexInformation = new VertexInfo[g.VertexCount];
                VertexCount = vertexInformation.Length;
                ColorLimit = 0;

                for (var v = 0; v < g.VertexCount; v++)
                {
                    var vertexDegree = g.Degree(v);
                    MaxDegree = Math.Max(MaxDegree, vertexDegree);
                    MinDegree = Math.Min(MinDegree, vertexDegree);
                    var outNeighbours = g.OutNeighbors(v).ToArray();
                    vertexInformation[v].Reset(vertexDegree, outNeighbours);
                }
            }

            public int VertexCount { get; }
            public int MaxDegree { get; }
            public int MinDegree { get; }
            public int ColorLimit { get; private set; }

            public void Init(int colorLimit)
            {
                ColorLimit = colorLimit;
                for (var v = 0; v < vertexInformation.Length; v++)
                {
                    vertexInformation[v].Init(colorLimit);
                }
            }

            public void ChangeVertexColor(int vertex, int color)
            {
                if (vertexInformation[vertex].Color == color)
                {
                    return;
                }

                var wasColorSet = vertexInformation[vertex].Color != 0;

                for (var nIdx = 0; nIdx < vertexInformation[vertex].OutNeighbours.Length; nIdx++)
                {
                    var neighbour = vertexInformation[vertex].OutNeighbours[nIdx];

                    if (!wasColorSet)
                    {
                        vertexInformation[neighbour].UncoloredNeighboursCount--;
                    }
                    else
                    {
                        vertexInformation[neighbour].NeighboursColorsInfo[vertexInformation[vertex].Color - 1]--;

                        if (vertexInformation[neighbour].NeighboursColorsInfo[vertexInformation[vertex].Color - 1] == 0)
                        {
                            vertexInformation[neighbour].NeighboursColorsCount--;
                        }
                    }
                    if (color != 0)
                    {
                        if (vertexInformation[neighbour].NeighboursColorsInfo[color - 1] == 0)
                        {
                            vertexInformation[neighbour].NeighboursColorsCount++;
                        }

                        vertexInformation[neighbour].NeighboursColorsInfo[color - 1]++;
                    }
                }

                vertexInformation[vertex].Color = color;
            }

            public bool CanUseColor(int vertex, int color)
            {
                return vertexInformation[vertex].NeighboursColorsInfo[color - 1] == 0;
            }

            public int GetNextVertexForColoring()
            {
                var nextVertex = -1;
                var currentMinAvailableColors = int.MaxValue;
                var currentMaxDegree = 0;
                var currentMaxUncolorodNeighbours = 0;
                var currentMaxColorodNeighbours = 0;

                for (var v = 0; v < vertexInformation.Length; v++)
                {
                    if (vertexInformation[v].Color == 0)
                    {
                        var availableColors = ColorLimit - vertexInformation[v].NeighboursColorsCount;
                        var vertexDegree = vertexInformation[v].Degree;
                        var uncoloredNeighboursCount = vertexInformation[v].UncoloredNeighboursCount;
                        var coloredNeighboursCount = vertexDegree - uncoloredNeighboursCount;

                        void SetVals()
                        {
                            nextVertex = v;
                            currentMinAvailableColors = availableColors;
                            currentMaxDegree = vertexDegree;
                            currentMaxUncolorodNeighbours = uncoloredNeighboursCount;
                            currentMaxColorodNeighbours = coloredNeighboursCount;
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
                                if (coloredNeighboursCount > currentMaxColorodNeighbours)
                                {
                                    SetVals();
                                }
                                else if (coloredNeighboursCount == currentMaxColorodNeighbours)
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

            public int[] GetColoringCopy(out int usedColorsCount)
            {
                var coloringCopy = new int[vertexInformation.Length];
                var usedColors = new HashSet<int>();
                for (int i = 0; i < vertexInformation.Length; i++)
                {
                    var vertexInfo = vertexInformation[i];
                    usedColors.Add(vertexInfo.Color);
                    coloringCopy[i] = vertexInfo.Color;
                }
                usedColorsCount = usedColors.Count;
                return coloringCopy;
            }

            private struct VertexInfo
            {
                public void Reset(int degree, int[] outNeighbours)
                {
                    Degree = degree;
                    OutNeighbours = outNeighbours;
                    UncoloredNeighboursCount = OutNeighbours.Length;
                }

                public void Init(int colorCount)
                {
                    Color = 0;
                    UncoloredNeighboursCount = OutNeighbours.Length;
                    NeighboursColorsInfo = new int[colorCount];
                    NeighboursColorsCount = 0;
                }

                public int Degree;
                public int Color;
                public int UncoloredNeighboursCount;
                public int[] OutNeighbours;
                public int[] NeighboursColorsInfo;
                public int NeighboursColorsCount;
            }
        }
    }
}
