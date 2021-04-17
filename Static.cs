using System;
using System.Collections.Generic;
using System.Text;

namespace Tree
{
	class Static
	{
        static Random _rnd = new Random();

        static private void Swap(ref int n1, ref int n2)
        {
            int v1 = n1;
            n1 = n2;
            n2 = v1;
        }

        public static int[] GetOrdered(int size)
        {
            int[] position = new int[size];
            for (int i = 0; i < size; ++i)
            {
                position[i] = i;
            }
            return position;
        }

        public static int[] Shuffle(int size)
        {
            int[] position = new int[size];
            for (int i = 0; i < size; ++i)
            {
                position[i] = i;
            }
            for (int i = 0; i < position.Length; ++i)
            {
                int plusPos = _rnd.Next() % position.Length;
                int next = i + plusPos;
                if (next > position.Length - 1) next -= (position.Length - 1);

                int el1 = position[i];
                int el2 = position[next];
                Swap(ref el1, ref el2);
                position[i] = el1;
                position[next] = el2;
            }
            return position;
        }

        static public double PearsonCorrelation(double[] x, double[] y)
        {
            int length = x.Length;
            if (length > y.Length)
            {
                length = y.Length;
            }

            double xy = 0.0;
            double x2 = 0.0;
            double y2 = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xy += x[i] * y[i];
                x2 += x[i] * x[i];
                y2 += y[i] * y[i];
            }
            xy /= (double)(length);
            x2 /= (double)(length);
            y2 /= (double)(length);
            double xav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xav += x[i];
            }
            xav /= length;
            double yav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                yav += y[i];
            }
            yav /= length;
            double ro = xy - xav * yav;
            ro /= Math.Sqrt(x2 - xav * xav);
            ro /= Math.Sqrt(y2 - yav * yav);
            return ro;
        }

        static List<int> GetOrderedArray(int n)
        {
            List<int> array = new List<int>();
            for (int i = 0; i < n; ++i)
            {
                array.Add(i);
            }
            return array;
        }

        static public void ShowGroups(List<int[]> groups)
        {
            Console.WriteLine("The groups of input parameters for layer 0:");
            foreach (int[] data in groups)
            {
                foreach (int d in data)
                {
                    Console.Write(" {0}", d);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static private bool IsValid(int[] group)
        {
            int max = group[0];
            for (int i = 1; i < group.Length; ++i)
            {
                if (max < group[i]) max = group[i];
            }
            int[] stat = new int[max + 1];
            for (int i = 0; i < stat.Length; ++i)
            {
                stat[i] = 0;
            }
            for (int i = 0; i < group.Length; ++i)
            {
                ++stat[group[i]];
                if (stat[group[i]] > 1) return false;
            }
            return true;
        }

        static public List<int[]> GetRandomGroups(int N, int size, int layers)
        {
            int nGroups = (int)Math.Pow(2.0, layers - 1.0);

            //. get all combinations
            List<int[]> combinations = new List<int[]>();
            int ii = 0;
            byte[] state = new byte[size];
            for (int jj = 0; jj < size; ++jj) state[jj] = 0;
            while (true)
            {
                int[] row = new int[size];
                if (0 == ii) row[0] = 0;
                for (int jj = 0; jj < size; ++jj)
                {
                    if (0 != ii || 0 != jj)
                    {
                        if (0 == state[jj]) row[jj] = row[jj - 1] + 1;
                        if (1 == state[jj]) row[jj] = combinations[ii - 1][jj] + 1;
                        if (2 == state[jj]) row[jj] = combinations[ii - 1][jj];
                        if ((N - size + jj) == row[jj])
                        {
                            if (jj > 0) if (2 == state[jj - 1]) state[jj - 1] = 1;
                            state[jj] = 0;
                        }
                    }
                }
                combinations.Add(row);
                if (row[size - 1] < N - 1)
                {
                    for (int jj = 0; jj < size - 1; ++jj) state[jj] = 2;
                    state[size - 1] = 1;
                }
                byte maxState = 0;
                for (int jj = 0; jj < size; ++jj)
                {
                    if (state[jj] > maxState) maxState = state[jj];
                }
                ++ii;
                if (0 == maxState) break;
            }
            int nCombinations = ii;

            //. get random indices, without repetition within a block of size N
            Random rnd = new Random();
            List<int> positions = GetOrderedArray(nCombinations);
            int[] groupInds = new int[nGroups];
            for (int i = 0; i < nGroups; ++i)
            {
                int random = rnd.Next() % positions.Count;
                groupInds[i] = positions[random];
                positions.RemoveAt(random);
                if (0 == positions.Count)
                {
                    positions = GetOrderedArray(nCombinations);
                }
            }

            //. fill groups out of combinations
            List<int[]> groups = new List<int[]>();
            for (int i = 0; i < nGroups; ++i)
            {
                groups.Add(combinations[groupInds[i]]);
            }

            return groups;
        }

        public static U InitializeU(int nt, int[] nx, double[] xmin, double[] xmax, double ymin, double ymax, int[] group)
        {
            U u = new U(group);
            ymin /= (double)(nt);
            ymax /= (double)(nt);
            u.Initialize(nt, nx, xmin, xmax);
            u.SetRandom(ymin, ymax);
            return u;
        }

        public static U InitializeU(int nt, int nx, double xmin, double xmax, double ymin, double ymax, int[] group)
        {
            U u = new U(group);
            double[] mins = new double[nt];
            double[] maxs = new double[nt];
            for (int i = 0; i < nt; ++i)
            {
                mins[i] = xmin;
                maxs[i] = xmax;
            }

            ymin /= (double)(nt);
            ymax /= (double)(nt);
            u.Initialize(nt, nx, mins, maxs);
            u.SetLinear(ymin, ymax);
            return u;
        }
    }
}

