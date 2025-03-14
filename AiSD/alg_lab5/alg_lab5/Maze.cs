using ASD.Graphs;
using System;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Maze : MarshalByRefObject
    {

        /// <summary>
        /// Wersje zadania I oraz II
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt bez dynamitów lub z dowolną ich liczbą
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="withDynamite">informacja, czy dostępne są dynamity 
        /// Wersja I zadania -> withDynamites = false, Wersja II zadania -> withDynamites = true</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany (dotyczy tylko wersji II)</param> 
        public int FindShortestPath(char[,] maze, bool withDynamite, out string path, int t = 0)
        {
            path = "";
            DiGraph<int> graph = new DiGraph<int>(maze.GetLength(0)*maze.GetLength(1));
            int s = 0;
            int e = 0;
            
            for(int i = 0; i < maze.GetLength(0); i++)
            {
                for(int j = 0; j < maze.GetLength(1); j++)
                {
                    if (maze[i, j] == 'O' || maze[i, j] == 'S' || maze[i, j] == 'E')
                    {
                        if (i > 0 && (maze[i - 1,j] == 'O' || maze[i - 1, j] == 'S' || maze[i - 1, j] == 'E'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i - 1) * maze.GetLength(1) + j, 1);
                        }
                        if(i > 0 && maze[i - 1, j] == 'X' && withDynamite == true)
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i - 1) * maze.GetLength(1) + j, t);

                        }

                        if (j > 0 && (maze[i, j-1] == 'O' || maze[i, j-1] == 'S' || maze[i, j-1] == 'E'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j - 1, 1);
                        }
                        if (j > 0 && maze[i, j-1] == 'X' && withDynamite == true)
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j - 1, t);

                        }

                        if (i < maze.GetLength(0) - 1 && (maze[i + 1, j] == 'O' || maze[i + 1, j] == 'S' || maze[i + 1, j] == 'E'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i + 1) * maze.GetLength(1) + j, 1);
                        }

                        if (i < maze.GetLength(0) - 1 && maze[i + 1, j] == 'X' && withDynamite == true)
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i + 1) * maze.GetLength(1) + j, t);

                        }

                        if (j < maze.GetLength(1) - 1 && (maze[i, j + 1] == 'O' || maze[i, j + 1] == 'S' || maze[i, j + 1] == 'E'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j + 1, 1);
                        }

                        if (j < maze.GetLength(1) - 1 && maze[i, j+1] == 'X' && withDynamite == true)
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j + 1, t);

                        }
                    }
                    if (maze[i,j] == 'S')
                    {
                        s = i * maze.GetLength(1) + j;
                    }
                    if (maze[i, j] == 'E')
                    {
                        e = i * maze.GetLength(1) + j;
                    }
                    if (maze[i,j] == 'X' && withDynamite == true)
                    {
                        if(i > 0 && maze[i - 1, j] == 'X')
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i - 1) * maze.GetLength(1) + j, t);
                        }
                        if (i > 0 && !(maze[i - 1, j] == 'X'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i - 1) * maze.GetLength(1) + j, 1);
                        }
                        
                        if (j > 0 && maze[i, j - 1] == 'X')
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j - 1, t);

                        }
                        if (j > 0 && !(maze[i, j - 1] == 'X'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j - 1, 1);

                        }
                        
                        if (i < maze.GetLength(0) - 1 && maze[i + 1, j] == 'X')
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i + 1) * maze.GetLength(1) + j, t);
                        }
                        if (i < maze.GetLength(0) - 1 && !(maze[i + 1, j] == 'X'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, (i + 1) * maze.GetLength(1) + j, 1);
                        }
                        if (j <  maze.GetLength(1) - 1 && maze[i, j + 1] == 'X')
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j + 1, t);

                        }
                        if (j < maze.GetLength(1) - 1 && !(maze[i, j + 1] == 'X'))
                        {
                            graph.AddEdge(i * maze.GetLength(1) + j, i * maze.GetLength(1) + j + 1, 1);

                        }

                    }
                }
            }

            PathsInfo<int> p = Paths.Dijkstra(graph, s);
            if(!p.Reachable(s,e))
            {
                return -1;
            }
            else
            {
                int j = maze.GetLength(1);
                int[] pathArr = p.GetPath(s, e);
                for (int i = 0; i < pathArr.Length - 1; i++)
                {
                    int diff = pathArr[i + 1] - pathArr[i];
                    if (diff == 1)
                    {
                        path += 'E';
                    }
                    else if (diff == -1)
                    {
                        path += 'W';
                    }
                    else if (diff == j)
                    {
                        path += 'S';
                    }
                    else if (diff == -j)
                    {
                        path += 'N';
                    }
                }
                return p.GetDistance(s, e);
            }
        }

        /// <summary>
        /// Wersja III i IV zadania
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt z użyciem co najwyżej k lasek dynamitu
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="k">liczba dostępnych lasek dynamitu, dla wersji III k=1</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany</param>
        public int FindShortestPathWithKDynamites(char[,] maze, int k, out string path, int t)
        {
            path = "";
            return -1;
        }
    }
}