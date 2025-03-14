using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labratoria_ASD2_2024
{
    public class Lab14 : MarshalByRefObject
    {
        /// <summary>
        /// Znajduje wszystkie maksymalne palindromy długości przynajmniej 2 w zadanym słowie. Wykorzystuje Algorytm Manachera.
        /// 
        /// Palindromy powinny być zwracane jako lista par (indeks pierwszego znaku, długość palindromu), 
        /// tzn. para (i, d) oznacza, że pod indeksem i znajduje się pierwszy znak d-znakowego palindromu.
        /// 
        /// Kolejność wyników nie ma znaczenia.
        /// 
        /// Można założyć, że w tekście wejściowym nie występują znaki '#' i '$' - można je wykorzystać w roli wartowników
        /// </summary>
        /// <param name="text">Tekst wejściowy</param>
        /// <returns>Tablica znalezionych palindromów</returns>
        /// 
   
        public (int startIndex, int length)[] FindPalindromes(string text)
        {
            List<(int, int)> odp = new List<(int, int)>();

            odp.AddRange(FindOddLengthPalindromes(text));
            odp.AddRange(FindEvenLengthPalindromes(text));

            return odp.ToArray();

            List<(int, int)> FindEvenLengthPalindromes(string text1)
            {
                int[] R = new int[text1.Length];
                int c = 0;
                int r = 0;

                for (int i = 0; i < text1.Length; i++)
                {
                    int mirr = 2 * c - i; 

                    if (i < r)
                    {
                        if (R[mirr] == 0)
                        {
                            R[i] = 0;
                        }
                        else if (R[mirr] < r - i + 1)
                        {
                            R[i] = R[mirr];
                        }
                        else if (R[mirr] > r - i + 1)
                        {
                            R[i] = r - i + 1;
                        }
                        else
                        {
                            R[i] = r - i + 1;
                        }
                    }

                    while (i + R[i] < text1.Length && i - R[i] - 1 >= 0 && text1[i + R[i]] == text1[i - R[i] - 1])
                    {
                        R[i]++;
                    }

                    if (i + R[i] - 1 > r)
                    {
                        c = i;
                        r = i + R[i] - 1;
                    }
                }
                List<(int, int)> result = new List<(int, int)>();
                for (int i = 0; i < text1.Length; i++)
                {
                    if (R[i] > 0)
                    {
                        result.Add((i - R[i], 2 * R[i]));
                    }
                }

                return result;
            }


            List<(int, int)> FindOddLengthPalindromes(string text2)
            {
                int[] R = new int[text2.Length];
                int c = 0, r = 0; 

                for (int i = 0; i < text2.Length; i++)
                {
                    int mirr = 2 * c - i; 

                    if (i < r)
                    {
                        if (R[mirr] == 0)
                        {
                            R[i] = 0;
                        }
                        else if (R[mirr] < r - i)
                        {
                            R[i] = R[mirr];
                        }
                        else if (R[mirr] > r - i)
                        {
                            R[i] = r - i;
                        }
                        else
                        {
                            R[i] = r - i;
                        }
                    }

                    while (i + R[i] + 1 < text2.Length && i - R[i] - 1 >= 0 && text2[i + R[i] + 1] == text2[i - R[i] - 1])
                    {
                        R[i]++;
                    }

                    if (i + R[i] > r)
                    {
                        c = i;
                        r = i + R[i];
                    }
                }
                List<(int, int)> result = new List<(int, int)>();
                for (int i = 0; i < text2.Length; i++)
                {
                    if (R[i] > 0)
                    {
                        result.Add((i - R[i], 2 * R[i] + 1));
                    }
                }

                return result;
            }

        }
    }

}
