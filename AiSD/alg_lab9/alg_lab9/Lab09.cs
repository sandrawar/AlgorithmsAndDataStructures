
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;

/// <summary>
/// Klasa rozszerzająca klasę Graph o rozwiązania problemów największej kliki i izomorfizmu grafów metodą pełnego przeglądu (backtracking)
/// </summary>
public static class Lab10GraphExtender
{
    /// <summary>
    /// Wyznacza największą klikę w grafie i jej rozmiar metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Badany graf</param>
    /// <param name="clique">Wierzchołki znalezionej największej kliki - parametr wyjściowy</param>
    /// <returns>Rozmiar największej kliki</returns>
    /// <remarks>
    /// Nie wolno modyfikować badanego grafu.
    /// </remarks>
    public static int MaxClique(this Graph g, out int[] clique)
    {
        List<int> S = new();
        List<int> bestS = new();
        MaxCliqueRec(0);

        void MaxCliqueRec(int k)
        {
            List<int> C = new();

            for(int i = k; i < g.VertexCount; i++)
            {
                bool isGood = true;
                foreach(var s in S)
                {
                    if (!g.HasEdge(s, i))
                    {
                        isGood = false;
                        break;
                    }
                }
                if(isGood)
                {
                    C.Add(i);
                }
            }

            if(S.Count + C.Count <= bestS.Count)
            {
                return;
            }

            else if(S.Count > bestS.Count)
            {
                bestS = S.GetRange(0, S.Count);
            }

            foreach(var c in C)
            {
                S.Add(c);
                MaxCliqueRec(c + 1);
                S.Remove(c);
            }
        }


        clique = bestS.ToArray();
        return clique.Length;
    }

    /// <summary>
    /// Bada izomorfizm grafów metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Pierwszy badany graf</param>
    /// <param name="h">Drugi badany graf</param>
    /// <param name="map">Mapowanie wierzchołków grafu h na wierzchołki grafu g (jeśli grafy nie są izomorficzne to null) - parametr wyjściowy</param>
    /// <returns>Informacja, czy grafy g i h są izomorficzne</returns>
    /// <remarks>
    /// 1) Uwzględniamy wagi krawędzi
    /// 3) Nie wolno modyfikować badanych grafów.
    /// </remarks>
    public static bool IsomorphismTest(this Graph<int> g, Graph<int> h, out int[] map)
    {
        if (g.VertexCount != h.VertexCount)
        {
            map = null;
            return false;
        }

        bool[] used = new bool[g.VertexCount];
        for(int i = 0; i < g.VertexCount; i++)
        {
            used[i] = false;
        }

        int[] permutation = new int[g.VertexCount];
        List<int[]> permutations = new();

        GeneratePermutations(0);

        foreach(var p in permutations)
        {
            if(isIsomorphism(p))
            {
                map = p;
                return true;
            }
        }
        map = null;
        return false;

        void GeneratePermutations(int k)
        {
            if(k == g.VertexCount)
            {
                permutations.Add(permutation.ToArray());
                return;
            }

            for(int i = 0; i < g.VertexCount; i++)
            {
                if (!used[i])
                {
                    used[i] = true;
                    permutation[k] = i;
                    GeneratePermutations(k + 1);
                    used[i] = false;
                }
            }
        }

        bool isIsomorphism(int[] p)
        {
            for(int i = 0; i < g.VertexCount; i++)
            {
                foreach(var n in g.OutNeighbors(i))
                {
                    if (!h.HasEdge(p[i], p[n]) || h.GetEdgeWeight(p[i], p[n]) != g.GetEdgeWeight(i, n))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

}

