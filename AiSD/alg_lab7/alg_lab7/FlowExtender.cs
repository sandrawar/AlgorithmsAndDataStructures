using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public static class FlowExtender
    {

        /// <summary>
        /// Metod wylicza minimalny s-t-przekrój.
        /// </summary>
        /// <param name="undirectedGraph">Nieskierowany graf</param>
        /// <param name="s">wierzchołek źródłowy</param>
        /// <param name="t">wierzchołek docelowy</param>
        /// <param name="minCut">minimalny przekrój</param>
        /// <returns>wartość przekroju</returns>
        public static double MinCut(this Graph<double> undirectedGraph, int s, int t, out Edge<double>[] minCut)
        {
            DiGraph<double> graph = new DiGraph<double>(undirectedGraph.VertexCount);


            var (flowValue, f) = Flows.FordFulkerson(undirectedGraph, s, t);

            foreach (Edge<double> e in undirectedGraph.DFS().SearchAll())
            {
                if (f.HasEdge(e.From, e.To))
                {
                    if (undirectedGraph.GetEdgeWeight(e.From, e.To) - f.GetEdgeWeight(e.From, e.To) != 0)
                    {
                        graph.AddEdge(e.From, e.To, undirectedGraph.GetEdgeWeight(e.From, e.To) - f.GetEdgeWeight(e.From, e.To));
                    } 
                } 
                else
                {
                    if (undirectedGraph.GetEdgeWeight(e.From, e.To) != 0)
                    {
                        graph.AddEdge(e.From, e.To, undirectedGraph.GetEdgeWeight(e.From, e.To));
                    }
                }
            }

            List<int> S = new();
            List<int> T = new(); 
            PathsInfo<double> p = Paths.Dijkstra(graph, s);

            for(int i = 0; i < undirectedGraph.VertexCount; i++)
            {
                if(p.Reachable(s, i))
                {
                    S.Add(i);
                }
                else { T.Add(i); }
            }

            List<Edge<double>> cutEdges = new();
            foreach(Edge<double> e in graph.DFS().SearchAll())
            {
                if(S.Contains(e.From) && T.Contains(e.To))
                {
                    cutEdges.Add(e);
                }
            }

            minCut = cutEdges.ToArray();
            return (int)cutEdges.Sum(edge => edge.Weight);
        }

        /// <summary>
        /// Metada liczy spójność krawędziową grafu oraz minimalny zbiór rozcinający.
        /// </summary>
        /// <param name="undirectedGraph">nieskierowany graf</param>
        /// <param name="cutingSet">zbiór krawędzi rozcinających</param>
        /// <returns>spójność krawędziowa</returns>
        public static int EdgeConnectivity(this Graph<double> undirectedGraph, out Edge<double>[] cutingSet)
        {
            cutingSet = null;
            return 0;
        }
        
    }
}
