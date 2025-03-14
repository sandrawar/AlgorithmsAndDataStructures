using System;
using System.Linq;
using System.Collections.Generic;
using ASD.Graphs;
using System.ComponentModel;
using System.IO;

namespace ASD
{
    public class Lab10 : MarshalByRefObject
    {

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt>">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <returns>Informację czy istnieje droga przez labirytn oraz tablicę reprezentującą kolejne wierzchołki na drodze. W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.</returns>
        public (bool routeExists, int[] route) FindEscape(Graph labyrinth, int startingTorches, int[] roomTorches, int debt, int[] roomGold)
        {
            int N = labyrinth.VertexCount;
            var visited = new bool[N];
            List<int> path = new List<int>();

            if (FindEscapeRecursive(labyrinth, startingTorches, 0, roomTorches, debt, roomGold, visited, 0, path))
            {
                return (true, path.ToArray());
            }

            return (false, null);


            bool FindEscapeRecursive(Graph labirynth, int currTorches, int currGold, int[] roomTorches, int debt, int[] roomGold, bool[] visited, int vertex, List<int> path)
            {
                visited[vertex] = true;
                path.Add(vertex);
                currGold += roomGold[vertex];
                currTorches += roomTorches[vertex];

                if (vertex == N - 1 && currGold >= debt) { return true; }

                if (currTorches > 0)
                {
                    foreach (int nvertex in labirynth.OutNeighbors(vertex))
                    {
                        if (!visited[nvertex])
                        {
                            currTorches -= 1;
                            if (FindEscapeRecursive(labirynth, currTorches, currGold, roomTorches, debt, roomGold, visited, nvertex, path))
                            {
                                return true;
                            }
                            currTorches += 1;
                        }
                    }
                }
                visited[vertex] = false;
                path.RemoveAt(path.Count - 1);
                currGold -= roomGold[vertex];
                currTorches -= roomTorches[vertex];
                return false;
            }
        }

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <param name="dragonDelay">Opóźnienie z jakim wystartuje smok</param>
        /// <returns>Informację czy istnieje droga przez labirynt oraz tablicę reprezentującą kolejne wierzchołki na drodze. W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.</returns>

        public (bool routeExists, int[] route) FindEscapeWithHeadstart(Graph labyrinth, int startingTorches, int[] roomTorches, int debt, int[] roomGold, int dragonDelay)
        {
            int N = labyrinth.VertexCount;
            var visited = new bool[N];

            int[] nextVisited = new int[labyrinth.VertexCount];
            List<int> path = new List<int>();

            if (FindEscapeRecursive(labyrinth, startingTorches, 0, roomTorches, debt, roomGold, visited, 0, path, dragonDelay, nextVisited, 0, 0))
            {
                return (true, path.ToArray());
            }

            return (false, null);


            bool FindEscapeRecursive(Graph labyrinth, int currTorches, int currGold, int[] roomTorches, int debt, int[] roomGold, bool[] visited, int vertex, List<int> path, int dragonDelay, int[] nextVisited, int turn, int dragonPosition)
            {
                visited[vertex] = true;
                path.Add(vertex);
                currGold += roomGold[vertex];
                currTorches += roomTorches[vertex];
                int prevGold = roomGold[vertex];
                int prevTorches = roomTorches[vertex];
                roomGold[vertex] = 0;
                roomTorches[vertex] = 0;

                if (vertex == N - 1 && currGold >= debt) { return true; }

                int prevDragonPos = -1;

                if (dragonDelay == turn)
                {
                    dragonPosition = 0;
                    visited[dragonPosition] = true;
                }
                else if (dragonDelay < turn)
                {
                    prevDragonPos = dragonPosition;
                    dragonPosition = nextVisited[dragonPosition];
                    visited[dragonPosition] = true;
                }

                if (currTorches > 0)
                {
                    foreach (int nvertex in labyrinth.OutNeighbors(vertex))
                    {
                        if (!visited[nvertex])
                        {
                            currTorches -= 1;
                            turn += 1;
                            nextVisited[vertex] = nvertex;
                            if (FindEscapeRecursive(labyrinth, currTorches, currGold, roomTorches, debt, roomGold, visited, nvertex, path, dragonDelay, nextVisited, turn, dragonPosition))
                            {
                                return true;
                            }
                            turn -= 1;
                            currTorches += 1;
                        }
                    }
                }

                path.RemoveAt(path.Count - 1);
                roomGold[vertex] = prevGold;
                roomTorches[vertex] = prevTorches;
                currGold -= roomGold[vertex];
                currTorches -= roomTorches[vertex];

                if (dragonDelay == turn)
                {
                    visited[0] = false;
                }
                else if (dragonDelay < turn)
                {
                    visited[dragonPosition] = false;
                    dragonPosition = prevDragonPos;
                }

                return false;
            }
        }


    }
}
