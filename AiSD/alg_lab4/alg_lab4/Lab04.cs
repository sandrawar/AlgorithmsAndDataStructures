using System;
using ASD.Graphs;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections;
using System.Security.Cryptography;
using System.Linq;

namespace ASD
{
    public class Lab04 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego
        /// przy zalozeniu, ze pociagi odjezdzaja co godzine.
        /// </summary>
        /// <param name="graph">Graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>

        public int[] Lab04Stage1(DiGraph graph, int miastoStartowe, int K)
        {
            // TODO
            List<int> mozliweDoOdwiedzone = new List<int>();
            bool[] odwiedzone = new bool[graph.VertexCount];
            Queue<int> vertexes = new Queue<int>();
            int currVertex;
            
            int[] time = new int[graph.VertexCount];
            for(int i = 0; i < graph.VertexCount; i++)
            {
                time[i] = int.MaxValue;
            }


            time[miastoStartowe] = 8;
            odwiedzone[miastoStartowe] = true;
            vertexes.Enqueue(miastoStartowe);
            
            while (vertexes.Count > 0)
            {
                currVertex = vertexes.Dequeue();
                if (time[currVertex] >= K)
                {
                    continue;
                }

                foreach (int n in graph.OutNeighbors(currVertex))
                {

                    if (time[currVertex] + 1 <= K)
                    {
                        if (!odwiedzone[n])
                        {
                            time[n] = time[currVertex] + 1;
                            vertexes.Enqueue(n);
                            odwiedzone[n] = true;
                            mozliweDoOdwiedzone.Add(n);
                        }
                    }
                }

            }

            mozliweDoOdwiedzone.Add(miastoStartowe);
            mozliweDoOdwiedzone.Sort();

            return mozliweDoOdwiedzone.ToArray();
        }

        /// <summary>
        /// Etap 2 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego.
        /// Waga krawedzi oznacza, ze pociag rusza o tej godzinie
        /// </summary>
        /// <param name="graph">Wazony graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
        public int[] Lab04Stage2(DiGraph<int> graph, int miastoStartowe, int K)
        {
            // TODO
            List<int> mozliweDoOdwiedzenia = new List<int>();
            bool[] odwiedzone = new bool[graph.VertexCount];
            PriorityQueue<int, (int, int)> vertexes = new PriorityQueue<int, (int, int)>();
            int currVertex;
            int currTime;

            int[] time = new int[graph.VertexCount];
            
            for (int i = 0; i < graph.VertexCount; i++)
            {
                time[i] = int.MaxValue;
            }
            time[miastoStartowe] = 8;


            vertexes.Insert((miastoStartowe, 8), 8);
            odwiedzone[miastoStartowe] = true;

            while (vertexes.Count > 0)
            {
                (currVertex, currTime) = vertexes.Extract(); 
                if(currTime >= K || time[currVertex] < currTime)
                {
                    continue;
                }

                foreach (var e in graph.OutEdges(currVertex))
                {
                    if (graph.GetEdgeWeight(e.From, e.To) + 1 <= K && currTime <= graph.GetEdgeWeight(e.From, e.To))
                    {
                        if (time[e.To] > graph.GetEdgeWeight(e.From, e.To) + 1)
                        {
                            time[e.To] = graph.GetEdgeWeight(e.From, e.To) + 1;
                            vertexes.Insert((e.To, graph.GetEdgeWeight(e.From, e.To) + 1), graph.GetEdgeWeight(e.From, e.To) + 1);
                        }
                        if (!odwiedzone[e.To])
                        {
                            odwiedzone[e.To] = true;
                            mozliweDoOdwiedzenia.Add(e.To);
                        }
                    }
                }

            }

            mozliweDoOdwiedzenia.Add(miastoStartowe);
            mozliweDoOdwiedzenia.Sort();

            return mozliweDoOdwiedzenia.ToArray();
        }
    }
}


