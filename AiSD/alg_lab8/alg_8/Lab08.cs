using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ASD.Graphs;

namespace ASD
{
    public class Lab08 : MarshalByRefObject
    {
        /// <summary>Etap I: prace przedprojektowe</summary>
        /// <param name="l">Długość działki, którą dysponuje Kameleon Kazik.</param>
        /// <param name="h">Maksymalna wysokość budowli.</param>
        /// <param name="pleasure">Tablica rozmiaru [l,h] zawierająca wartości zadowolenia p(x,y) dla każdych x i y.</param>
        /// <returns>Odpowiedź na pytanie, czy istnieje budowla zadowalająca Kazika.</returns>
        public bool Stage1ExistsBuilding(int l, int h, int[,] pleasure)
        {
            int prog1 = Math.Min(l, h);
            int n = l * prog1 + 2;
            DiGraph<int> sito = new DiGraph<int>(n);
            int s = 0;
            int t = n - 1;
            int mozliwe = 0;


            int prog = l;
            for (int ih = 0; ih < prog1; ih++)
            {
                for (int il = 0; il < prog; il++)
                {
                    int obecny = ih * l + il + 1; 
                    if (pleasure[il, ih] >= ih)
                    {
                        sito.AddEdge(s, obecny, pleasure[il, ih]);
                        mozliwe += pleasure[il, ih];
                    }

                    if (ih >= 1 && il < l)
                    {
                        sito.AddEdge(obecny, obecny - l, int.MaxValue);
                        sito.AddEdge(obecny, obecny - l + 1, int.MaxValue);
                    }

                    sito.AddEdge(obecny, t, 1);
                }
                prog--;
            }

            var (flowValue, f) = Flows.FordFulkerson(sito, s, t);
            return mozliwe - flowValue > 0;
        }

        /// <summary>Etap II: kompletny projekt</summary>
        /// <param name="l">Długość działki, którą dysponuje Kameleon Kazik.</param>
        /// <param name="h">Maksymalna wysokość budowli.</param>
        /// <param name="pleasure">Tablica rozmiaru [l,h] zawierająca wartości zadowolenia p(x,y) dla każdych x i y.</param>
        /// <param name="blockOrder">Argument wyjściowy, w którym należy zwrócić poprawną kolejność ustawienia bloków w znalezionym rozwiązaniu;
        ///     kolejność jest poprawna, gdy przed blokiem (x,y) w tablicy znajdują się bloki (x,y-1) i (x+1,y-1) lub gdy y=0. 
        ///     Ustawiane bloki powinny mieć współrzędne niewychodzące poza granice obszaru budowy (0<=x<l, 0<=y<h).
        ///     W przypadku braku rozwiązania należy zwrócić null.</param>
        /// <returns>Maksymalna wartość zadowolenia z budowli; jeśli nie istnieje budowla zadowalająca Kazika, zależy zwrócić null.</returns>
        public int? Stage2GetOptimalBuilding(int l, int h, int[,] pleasure, out (int x, int y)[] blockOrder)
        {
            int prog1 = Math.Min(l, h);
            int n = l * prog1 + 2;
            DiGraph<int> sito = new DiGraph<int>(n);
            int s = 0;
            int t = n - 1;
            int mozliwe = 0;


            int prog = l;
            for (int ih = 0; ih < prog1; ih++)
            {
                for (int il = 0; il < prog; il++)
                {
                    int obecny = ih * l + il + 1;
                    if (pleasure[il, ih] > 1)
                    {
                        sito.AddEdge(s, obecny, pleasure[il, ih]);
                        mozliwe += pleasure[il, ih];
                    }
                    else if (pleasure[il, ih] == 1 && ih == 0)
                    {
                        sito.AddEdge(s, obecny, pleasure[il, ih]);
                        mozliwe += pleasure[il, ih];

                    }

                    if (ih >= 1 && il < l)
                    {
                        sito.AddEdge(obecny, obecny - l, int.MaxValue);
                        sito.AddEdge(obecny, obecny - l + 1, int.MaxValue);
                    }

                    sito.AddEdge(obecny, t, 1);
                }
                prog--;
            }

            var (flowValue, f) = Flows.FordFulkerson(sito, s, t);
            if (mozliwe <= flowValue)
            {
                blockOrder = null;
                return null;
            }

            List<(int, int)> postawioneBloki = new List<(int, int)>();
            DiGraph<int> siecResydualna = new DiGraph<int>(n);

            foreach (var e in sito.BFS().SearchAll())
            {
                int eW = e.Weight;
                if (f.HasEdge(e.From, e.To))
                {
                    int feW = f.GetEdgeWeight(e.From, e.To);
                    if (feW < eW)
                    {
                        siecResydualna.AddEdge(e.From, e.To, eW - feW);
                    }
                    siecResydualna.AddEdge(e.To, e.From, feW);
                }
                else if (eW > 0)
                {
                    siecResydualna.AddEdge(e.From, e.To, eW);
                }

            }

            bool[] bloki = new bool[n];

            Stack<int> S = new Stack<int>();
            S.Push(s);

            while (S.Count > 0)
            {
                int blok = S.Pop();
                foreach (var next in siecResydualna.OutNeighbors(blok))
                {
                    if (!bloki[next])
                    {
                        S.Push(next);
                        bloki[next] = true;
                    }
                }
            }

            for (int i = 1; i < n; i++)
            {
                if (bloki[i])
                {
                    int ih = (i - 1) / l;
                    int il = i - ih * l - 1;
                    postawioneBloki.Add((il, ih));
                }
            }
            blockOrder = postawioneBloki.ToArray();

            return mozliwe - flowValue;
        }        
    }
}

