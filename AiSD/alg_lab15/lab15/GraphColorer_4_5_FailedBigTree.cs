using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using ASD.Graphs;

namespace ASD2
{
    public class GraphColorer_4_5_FailedBigTree : MarshalByRefObject
    {
        /// <summary>
        /// Metoda znajduje kolorowanie zadanego grafu g używające najmniejsze możliwej liczby kolorów.
        /// </summary>
        /// <param name="g">Graf (nieskierowany)</param>
        /// <returns>Liczba użytych kolorów i kolorowanie (coloring[i] to kolor wierzchołka i). Kolory mogą być dowolnymi liczbami całkowitymi.</returns>
        public (int numberOfColors, int[] coloring) FindBestColoring(Graph g)
        {
            //GraphDebugHelper.StartSearchProcess(g);

            var bestColoring = new int[0];

            var colorCount = 0;
            if (ColoringIfEmpty(g, out bestColoring, out colorCount)
                || ColoringIfNoEdges(g, out bestColoring, out colorCount)
                || ColoringIfClique(g, out bestColoring, out colorCount))
            {
                //GraphDebugHelper.GraphColoringResult(colorCount, bestColoring);
                return (colorCount, bestColoring);
            }

            var gci = new GraphColoringInfo(g);

            var notEnoughColorCount = 1;
            var enoughColorCount = gci.MaxDegree + 1;

            //GraphDebugHelper.GraphChromaticNumberSearchStart(gci.MaxDegree, gci.MinDegree);

            var initCheck = true;

            while (notEnoughColorCount < enoughColorCount - 1)
            {
                //GraphDebugHelper.StartSearchIteration(initCheck, notEnoughColorCount, enoughColorCount);

                colorCount = initCheck
                    ? enoughColorCount
                    : enoughColorCount - 1;
                //: Math.Min((notEnoughColorCount + enoughColorCount) / 2, enoughColorCount - 1);

                initCheck = false;

                gci.Init(colorCount);

                //GraphDebugHelper.StartChromaticCheckFor(colorCount);
                if (ColorGraphRecursive(gci, 0))
                {
                    bestColoring = gci.GetColoringCopy(out var usedColorsCount);
                    enoughColorCount = usedColorsCount;
                    //GraphDebugHelper.ChromaticCheckSuccess(colorCount, enoughColorCount, bestColoring);
                }
                else
                {
                    notEnoughColorCount = colorCount;
                    //GraphDebugHelper.ChromaticCheckFail(colorCount);
                }
            }

            //GraphDebugHelper.GraphColoringResult(enoughColorCount, bestColoring);
            return (enoughColorCount, bestColoring);
        }

        private static bool ColorGraphRecursive(GraphColoringInfo gci, int usedColorsCount)
        {
            var nextVertex = gci.GetNextVertex();
            if (nextVertex == -1)
            {
                return true;
            }

            var colorCount = gci.ColorCount;

            //GraphDebugHelper.StartVertexColoring(nextVertex);
            var colorLimit = Math.Min(colorCount, usedColorsCount + 1);
            for (int color = 1; color <= colorLimit; color++)
            {
                var canUseColor = gci.CanUseColor(nextVertex, color);
                if (canUseColor)
                {
                    //GraphDebugHelper.TryVertexColor(nextVertex, color);

                    gci.ChangeVertexColor(nextVertex, color);

                    var nextUsedColorsCount = color > usedColorsCount ? usedColorsCount + 1 : usedColorsCount;
                    if (ColorGraphRecursive(gci, nextUsedColorsCount))
                    {
                        //GraphDebugHelper.VertexColoringCompleted(true, nextVertex, gci.GetColoringCopy(out var tmp1));
                        return true;
                    }

                    gci.ChangeVertexColor(nextVertex, 0);
                }
            }

            //GraphDebugHelper.VertexColoringCompleted(false, nextVertex, gci.GetColoringCopy(out var tmp2));
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
            private readonly VertexInfo[] Vertexes;

            public GraphColoringInfo(Graph g)
            {
                Vertexes = new VertexInfo[g.VertexCount];
                VertexCount = Vertexes.Length;
                ColorCount = 0;

                for (var v = 0; v < g.VertexCount; v++)
                {
                    var degree = g.Degree(v);
                    MaxDegree = Math.Max(MaxDegree, degree);
                    MinDegree = Math.Min(MinDegree, degree);
                    var outNeighbours = g.OutNeighbors(v).ToArray();
                    Vertexes[v].Reset(v, degree, outNeighbours);
                }
            }

            public int VertexCount { get; }
            public int MaxDegree { get; }
            public int MinDegree { get; }
            public int ColorCount { get; private set; }

            public void Init(int colorCount)
            {
                ColorCount = colorCount;
                for (var v = 0; v < Vertexes.Length; v++)
                {
                    Vertexes[v].Init(colorCount);
                }
            }

            public void ChangeVertexColor(int vertex, int color)
            {
                if (Vertexes[vertex].Color == color)
                {
                    return;
                }

                var wasColorSet = Vertexes[vertex].Color != 0;

                for (var nIdx = 0; nIdx < Vertexes[vertex].OutNeighbours.Length; nIdx++)
                {
                    var neighbour = Vertexes[vertex].OutNeighbours[nIdx];

                    if (!wasColorSet)
                    {
                        Vertexes[neighbour].UncoloredNeighbours--;
                    }
                    else
                    {
                        Vertexes[neighbour].NeighboursColorsInfo[Vertexes[vertex].Color - 1]--;
                    }
                    if (color != 0)
                    {
                        Vertexes[neighbour].NeighboursColorsInfo[color - 1]++;
                    }
                }

                Vertexes[vertex].Color = color;
            }

            public bool CanUseColor(int vertex, int color)
            {
                /*foreach (var neighbor in Vertexes[vertex].OutNeighbours)
                {
                    if (Vertexes[neighbor].Color == color)
                    {
                        return false;
                    }
                }*/
                return Vertexes[vertex].NeighboursColorsInfo[color - 1] == 0;
            }

            public int GetNextVertex()
            {
                int NextVertex = -1;
                int CurrentMinAvailableColors = int.MaxValue;
                int CurrentMaxDegree = 0;
                int CurrentMaxUncolorodNeighbours = 0;
                int CurrentMaxColorodNeighbours = 0;

                for (var v = 0; v < Vertexes.Length; v++)
                {
                    if (Vertexes[v].Color == 0)
                    {
                        var neighbourColorsCount = 0;
                        for (int i = 0; i < Vertexes[v].NeighboursColorsInfo.Length; i++)
                        {
                            if (Vertexes[v].NeighboursColorsInfo[i] != 0)
                            {
                                neighbourColorsCount++;
                            }
                        }

                        var availableColors = ColorCount - neighbourColorsCount;
                        var vertexDegree = Vertexes[v].Degree;
                        var uncoloredNeighboursCount = Vertexes[v].UncoloredNeighbours;
                        var coloredNeighbours = vertexDegree - uncoloredNeighboursCount;

                        void SetVals()
                        {
                            NextVertex = v;
                            CurrentMinAvailableColors = availableColors;
                            CurrentMaxDegree = vertexDegree;
                            CurrentMaxUncolorodNeighbours = uncoloredNeighboursCount;
                            CurrentMaxColorodNeighbours = coloredNeighbours;
                        }

                        if (availableColors < CurrentMinAvailableColors)
                        {
                            SetVals();
                        }
                        else if (availableColors == CurrentMinAvailableColors)
                        {
                            if (vertexDegree > CurrentMaxDegree)
                            {
                                SetVals();
                            }
                            else if (vertexDegree == CurrentMaxDegree)
                            {
                                if (coloredNeighbours > CurrentMaxColorodNeighbours)
                                {
                                    SetVals();
                                }
                                else if (coloredNeighbours == CurrentMaxColorodNeighbours)
                                {
                                    if (uncoloredNeighboursCount > CurrentMaxUncolorodNeighbours)
                                    {
                                        SetVals();
                                    }
                                }
                            }
                        }
                    }
                }

                return NextVertex;
            }

            public int[] GetColoringCopy(out int usedColorsCount)
            {
                var coloringCopy = new int[Vertexes.Length];
                var usedColors = new HashSet<int>();
                for (int i = 0; i < Vertexes.Length; i++)
                {
                    var vertexInfo = Vertexes[i];
                    usedColors.Add(vertexInfo.Color);
                    coloringCopy[i] = vertexInfo.Color;
                }
                usedColorsCount = usedColors.Count;
                return coloringCopy;
            }

            private struct VertexInfo
            {
                public void Reset(int vertex, int degree, int[] outNeighbours)
                {
                    Vertex = vertex;
                    Degree = degree;
                    OutNeighbours = outNeighbours;
                    UncoloredNeighbours = OutNeighbours.Length;
                }

                public void Init(int colorCount)
                {
                    Color = 0;
                    UncoloredNeighbours = 0;
                    UncoloredNeighbours = OutNeighbours.Length;
                    NeighboursColorsInfo = new int[colorCount];
                }

                public int Vertex;
                public int Degree;
                public int Color;
                public int UncoloredNeighbours;
                public int[] OutNeighbours;
                public int[] NeighboursColorsInfo;
            }

            private class NextVertexInfo
            {
                public int NextVertex = -1;
                public int CurrentMinAvailableColors = int.MaxValue;
                public int CurrentMaxAvailableColors = 0;
                public int CurrentMinDegree = int.MaxValue;
                public int CurrentMaxDegree = 0;
                public int CurrentMinUncolorodNeighbours = int.MaxValue;
                public int CurrentMaxUncolorodNeighbours = 0;
                public int CurrentMinColorodNeighbours = int.MaxValue;
                public int CurrentMaxColorodNeighbours = 0;
            }
        }
    }
}
