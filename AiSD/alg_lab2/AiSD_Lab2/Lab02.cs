using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ASD
{
    public class Lab02 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - wyznaczenie najtańszej trasy, zgodnie z którą pionek przemieści się z pozycji poczatkowej (0,0) na pozycję docelową
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="moves">tablica z dostępnymi ruchami i ich kosztami (di - o ile zwiększamy numer wiersza, dj - o ile zwiększamy numer kolumnj, cost - koszt ruchu)</param>
        /// <returns>(bool result, int cost, (int, int)[] path) - result ma wartość true jeżeli trasa istnieje, false wpp., cost to minimalny koszt, path to wynikowa trasa</returns>
        public (bool result, int cost, (int i, int j)[] path) Lab02Stage1(int n, int m, ((int di, int dj) step, int cost)[] moves)
        {
            bool possible = false;
            int[,] Tab = new int[n, m];
            List<(int,int)> Best_moves = new List<(int,int)> ();
            Tab[0,0] = 0;
            for(int i = 0; i < n; i++) 
            {
                for(int j = 0; j < m; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Tab[i, j] = 1000000;
                    }
                    foreach (var move in moves)
                    {
                        int old_i = i - move.step.di;
                        int old_j = j - move.step.dj;
                        if ( old_i >= 0 && old_j >= 0)
                        {
                            int c = Tab[old_i, old_j] + move.cost;
                            if (c <= Tab[i,j])
                            {
                                Tab[i, j] = c;
                            }
                        }
                    }

                }
            }

            int val = Tab[n - 1, 0];
            int last_j = 0;
            for(int j = 0; j < m; j++)
            {
                if ((Tab[n - 1, j]) < val)
                {
                    val = Tab[n - 1, j];
                    last_j = j;
                }
            }
            if (val < 1000000)
            {
                possible = true;
                Best_moves.Add((n - 1, last_j));
            }
            
            int path_i = n - 1;
            int path_j = last_j;
            bool change = true;
            while((path_i > 0 || path_j > 0) && change == true)
            {
                change = false;
                int i_change = 0;
                int j_change = 0;
                (int, int) step = (0, 0);
                int C = 10000000;
                foreach(var move in moves)
                {
                    int old_path_i = path_i - move.step.di;
                    int old_path_j = path_j - move.step.dj;
                    if(old_path_i >= 0 && old_path_j >= 0 && Tab[old_path_i, old_path_j] < 10000000)
                    {
                        int c = Tab[old_path_i, old_path_j];
                        if(c + move.cost <= C)
                        {
                            C = c + move.cost;
                            step = (old_path_i, old_path_j);
                            i_change = move.step.di;
                            j_change = move.step.dj;
                            change = true;
                        }
                    }
                }
                path_i -= i_change;
                path_j -= j_change;
                Best_moves.Add(step);
            }

            Best_moves.Reverse();
            (int, int)[] Best_moves_a = Best_moves.ToArray();

            if (possible == false)
            {
                Best_moves_a = null;
            } 
            return (possible, val, Best_moves_a);
        }


        /// <summary>
        /// Etap 2 - wyznaczenie najtańszej trasy, zgodnie z którą pionek przemieści się z pozycji poczatkowej (0,0) na pozycję docelową - dodatkowe założenie, każdy ruch może być wykonany co najwyżej raz
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="moves">tablica z dostępnymi ruchami i ich kosztami (di - o ile zwiększamy numer wiersza, dj - o ile zwiększamy numer kolumnj, cost - koszt ruchu)</param>
        /// <returns>(bool result, int cost, (int, int)[] path) - result ma wartość true jeżeli trasa istnieje, false wpp., cost to minimalny koszt, path to wynikowa trasa</returns>
        public (bool result, int cost, (int i, int j)[] pat) Lab02Stage2(int n, int m, ((int di, int dj) step, int cost)[] moves)
        {
            bool possible = false;
            int[,] Tab = new int[n, m];
            List<(int di, int dj)>[,] UsedMoves = new List<(int di, int dj)>[n, m];
            for (int i = 0; i < n; i++)
            {
                for(int j = 0; j < m; j++)
                {
                    UsedMoves[i,j] = new List<(int di, int dj)>();
                }

            }
            List<(int, int)> Best_moves = new List<(int, int)>();
            Tab[0, 0] = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Tab[i, j] = 1000000;
                    }
                    foreach (var move in moves)
                    {
                        int old_i = i - move.step.di;
                        int old_j = j - move.step.dj;
                        if (old_i >= 0 && old_j >= 0)
                        {
                            int c = Tab[old_i, old_j] + move.cost;
                            if (c <= Tab[i, j] && UsedMoves[old_i,old_j].Contains(move.step) == false)
                            {
                                Tab[i, j] = c;
                                UsedMoves[i,j] = UsedMoves[old_i,old_j].GetRange(0, UsedMoves[old_i,old_j].Count);
                                UsedMoves[i,j].Add(move.step);
                            }
                        }
                    }

                }
            }

            int val = Tab[n - 1, 0];
            int last_j = 0;
            for (int j = 0; j < m; j++)
            {
                if ((Tab[n - 1, j]) < val)
                {
                    val = Tab[n - 1, j];
                    last_j = j;
                }
            }
            if (val < 1000000)
            {
                possible = true;
                Best_moves.Add((n - 1, last_j));
            }

            int path_i = n - 1;
            int path_j = last_j;
            bool change = true;
            while ((path_i > 0 || path_j > 0) && change == true)
            {
                change = false;
                int i_change = 0;
                int j_change = 0;
                (int, int) step = (0, 0);
                int C = 1000000;
                foreach (var move in moves)
                {
                    int old_path_i = path_i - move.step.di;
                    int old_path_j = path_j - move.step.dj;
                    if (old_path_i >= 0 && old_path_j >= 0 && Tab[old_path_i, old_path_j] < 10000000)
                    {
                        int c = Tab[old_path_i, old_path_j];
                        if (c + move.cost <= C && UsedMoves[old_path_i, old_path_j].Contains(move.step) == false)
                        {
                            C = c + move.cost;
                            step = (old_path_i, old_path_j);
                            i_change = move.step.di;
                            j_change = move.step.dj;
                            change = true;
                        }
                    }
                }
                path_i -= i_change;
                path_j -= j_change;
                Best_moves.Add(step);
            }

            Best_moves.Reverse();
            (int, int)[] Best_moves_a = Best_moves.ToArray();

            if (possible == false)
            {
                Best_moves_a = null;
            }

            return (possible, val, Best_moves_a);

        }
    }
}