using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ASD
{
    public class WaterCalculator : MarshalByRefObject
    {

        /*
         * Metoda sprawdza, czy przechodząc p1->p2->p3 skręcamy w lewo 
         * (jeżeli idziemy prosto, zwracany jest fałsz).
         */
        private bool leftTurn(Point p1, Point p2, Point p3)
        {
            Point w1 = new Point(p2.x - p1.x, p2.y - p1.y);
            Point w2 = new Point(p3.x - p2.x, p3.y - p2.y);
            double vectProduct = w1.x * w2.y - w2.x * w1.y;
            return vectProduct > 0;
        }


        /*
         * Metoda wyznacza punkt na odcinku p1-p2 o zadanej współrzędnej y.
         * Jeżeli taki punkt nie istnieje (bo cały odcinek jest wyżej lub niżej), zgłaszany jest wyjątek ArgumentException.
         */
        private Point getPointAtY(Point p1, Point p2, double y)
        {
            if (p1.y != p2.y)
            {
                double newX = p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y);
                if ((newX - p1.x) * (newX - p2.x) > 0)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point(p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y), y);
            }
            else
            {
                if (p1.y != y)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point((p1.x + p2.x) / 2, y);
            }
        }


        public double[] PointDepths(Point[] points)
        {
            var depths = new double[points.Length];
            
            int i = 0;
            while (i < points.Length)
            {

                List<Point> currPart = new List<Point>();
                currPart.Add(points[i++]);

                while (i < points.Length)
                {
                    if (points[i - 1].x <= points[i].x)
                    {
                        currPart.Add(points[i++]);
                    }
                    else
                    {
                        break;
                    }

                }

                var currPartArray = currPart.ToArray();
                int currStart = 0, currEnd = currPart.Count - 1;

                double edgeY = Math.Min(currPartArray[currStart].y, currPartArray[currEnd].y);

                while (currStart != currEnd)
                {
                    edgeY =Math.Max( Math.Min(currPartArray[currStart].y, currPartArray[currEnd].y), edgeY);

                    if (currPartArray[currStart].y < currPartArray[currEnd].y)
                    {
                        depths[i - currPart.Count + currStart] = Math.Max(0, edgeY - currPartArray[currStart].y);
                        currStart++;
                    }
                    else
                    {
                        depths[i - currPart.Count + currEnd] = Math.Max(0, edgeY - currPartArray[currEnd].y);
                        currEnd--;
                    }
                }

                depths[i - currPart.Count + currStart] = 0;

                while (i < points.Length && points[i - 1].x > points[i].x)
                {
                    i++;
                }

                if (i == points.Length) break;
                i--;
            }

            return depths;
        }



        /// <summary>
        /// Funkcja zwraca objętość wody, jaka zatrzyma się w górach.
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double WaterVolume(Point[] points)
        {
            double[] depths = PointDepths(points);

            double ans = 0;
            for (int i = 1;  i < points.Length; i++)
            {
                
                if (depths[i] > 0)
                {
                    int paddleStart = i;

                    while (i + 1 < points.Length)
                    {
                        if (depths[i + 1] > 0)
                        {
                            ans += (depths[i] + depths[i + 1]) * (points[i + 1].x - points[i].x) / 2;
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    double firstTriangalBase = points[paddleStart].x - getPointAtY(points[paddleStart - 1], points[paddleStart], depths[paddleStart] + points[paddleStart].y).x;
                    double firstTriangalVolume = depths[paddleStart] * firstTriangalBase / 2;

                    double lastTriangalBase = getPointAtY(points[i], points[i+1], depths[i] + points[i].y).x - points[i].x;
                    double lastTriangalVolume = depths[i] * lastTriangalBase / 2;

                    ans += firstTriangalVolume;
                    ans += lastTriangalVolume;
                }
            }

            return ans;
        }
    }

    [Serializable]
    public struct Point
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
