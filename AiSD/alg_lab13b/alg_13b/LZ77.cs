using System;
using System.Collections.Generic;

namespace ASD
{
    public class LZ77 : MarshalByRefObject
    {
        /// <summary>
        /// Odkodowywanie napisu zakodowanego algorytmem LZ77. Dane kodowanie jest poprawne (nie trzeba tego sprawdzać).
        /// </summary>
        public string Decode(List<EncodingTriple> encoding)
        {
            string text = "";
            bool isFirst = true;
            foreach (var encodingTriple in encoding)
            {
                if(isFirst)
                {
                    text += encodingTriple.s;
                    int ile = encodingTriple.c;
                    while(ile > 0)
                    {
                        text += encodingTriple.s;
                        ile--;
                    }
                    isFirst = false;
                }
                else
                {
                    int ile = encodingTriple.c;
                    int skad = encodingTriple.p;
                    if (ile < skad + 1)
                    {
                        text += text.Substring(text.Length - skad - 1, ile);
                    }
                    else
                    {
                        int start = text.Length - skad - 1;
                        while (ile > 0)
                        {
                            text += text.Substring(start, Math.Min(ile, text.Length - start));
                            ile -= Math.Min(ile, text.Length - start);
                            start += Math.Min(ile, text.Length - start);
                        }
                    }

                    text += encodingTriple.s;

                }
            }
            return text;
        }

        /// <summary>
        /// Kodowanie napisu s algorytmem LZ77
        /// </summary>
        /// <returns></returns>
        public List<EncodingTriple> Encode(string s, int maxP)
        {
            return null;
        }
    }

    [Serializable]
    public struct EncodingTriple
    {
        public int p, c;
        public char s;

        public EncodingTriple(int p, int c, char s)
        {
            this.p = p;
            this.c = c;
            this.s = s;
        }
    }
}
