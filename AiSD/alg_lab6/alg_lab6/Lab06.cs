using ASD.Graphs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ASD
{
	public class Lab06 : MarshalByRefObject
	{
		/// <summary>Etap 1</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="start">Wierzchołek startowy (wejście z lasu).</param>
		/// <returns>Pierwszy element pary to informacja, czy rozwiązanie istnieje. Drugi element pary, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (bool possible, int[] path) Stage1(
			int n, DiGraph<int> c, Graph<int> g, int target, int start
		)
		{
			Queue<(int, int)> queue = new Queue<(int, int)>();
			List<int> path = new List<int>();
            (int, int)[,] lastOnPath = new (int, int)[g.VertexCount, n];
            bool[,] visited = new bool[g.VertexCount, n];

            for (int i = 0; i < n; i++)
			{
				queue.Enqueue((start, i));
                visited[start, i] = true;
            }

			while (queue.Count > 0)
			{
				(int currVertex, int currColour) = queue.Dequeue();

				if (currVertex == target)
				{
					while (currVertex != start)
					{
                        path.Add(currVertex);
                        (currVertex, currColour) = lastOnPath[currVertex, currColour];
						
					}
					path.Add(start);
					path.Reverse();
					return (true, path.ToArray());
				}

				foreach (var e in g.OutEdges(currVertex))
				{
					if ((currColour == e.Weight || c.HasEdge(currColour, e.Weight)) && !visited[e.To, e.Weight])
					{
						queue.Enqueue((e.To, e.Weight));
                        visited[e.To, e.Weight] = true;
                        lastOnPath[e.To, e.Weight] = (currVertex, currColour);
					}
				}
			}

			return (false, new int[0]);
		}

		/// <summary>Drugi etap</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="starts">Wierzchołki startowe (wejścia z lasu).</param>
		/// <returns>Pierwszy element pary to koszt najlepszego rozwiązania lub null, gdy rozwiązanie nie istnieje. Drugi element pary, tak jak w etapie 1, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (int? cost, int[] path) Stage2(
			int n, DiGraph<int> c, Graph<int> g, int target, int[] starts
		)
        {
            Queue<(int, int)> queue = new Queue<(int, int)>();
            List<int> path = new List<int>();
            (int, int)[,] lastOnPath = new (int, int)[g.VertexCount, n];
            int[,] cost = new int[g.VertexCount, n];

            for (int i = 0; i < g.VertexCount; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    cost[i, j] = int.MaxValue;
                }
            }

            foreach (int start in starts)
            {
                for (int i = 0; i < n; i++)
                {
                    cost[start, i] = 0;
                    queue.Enqueue((start, i));
                }
            }

            while (queue.Count > 0)
            {
                (int currVertex, int currColour) = queue.Dequeue();

                foreach (var e in g.OutEdges(currVertex))
                {
                    if (c.HasEdge(currColour, e.Weight))
                    {
                        if (cost[e.To, e.Weight] > cost[currVertex, currColour] + c.GetEdgeWeight(currColour, e.Weight) + 1)
                        {
                            cost[e.To, e.Weight] = cost[currVertex, currColour] + c.GetEdgeWeight(currColour, e.Weight) + 1;
                            lastOnPath[e.To, e.Weight] = (currVertex, currColour);
                            queue.Enqueue((e.To, e.Weight));
                        }
                    }

                    if (e.Weight == currColour)
                    {
                        if (cost[e.To, e.Weight] > cost[currVertex, currColour] + 1)
                        {
                            cost[e.To, e.Weight] = cost[currVertex, currColour] + 1;
                            lastOnPath[e.To, e.Weight] = (currVertex, currColour);
                            queue.Enqueue((e.To, e.Weight));
                        }
                    }

                }
            }

            int minCost = int.MaxValue;
            int currColourOnPath = 0;

            for (int i = 0; i < n; i++)
            {
                if (cost[target, i] < minCost)
                {
                    minCost = cost[target, i];
                    currColourOnPath = i;
                }
            }

            if (minCost == int.MaxValue)
                return (null, new int[0]);

            int currVertexOnPath = target;
            while (!starts.Contains(currVertexOnPath))
            {
                path.Add(currVertexOnPath);
                (currVertexOnPath, currColourOnPath) = lastOnPath[currVertexOnPath, currColourOnPath];
            }

            path.Add(currVertexOnPath);
            path.Reverse();

            return (minCost, path.ToArray());
        }
	}

}
