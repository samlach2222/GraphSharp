using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSharp
{
    public class Dsatur
    {
        private readonly int[] DSAT; // Degrees of saturation
        private readonly int[] color; // Colours for Dsatur
        private readonly int[] Degree; // Degrees of the vertices
        private readonly int n; // Number of vertices
        private readonly List<int>[] adj; //[n];  // Graph adjacency matrix
        private readonly int k; // k-degeneration

        public int K => k;

        private int CalculateDsatur()
        {
            int nb = 0, c, x, cmax = 0;

            Program.ClearConsoleLines(2);
            Console.WriteLine("2/3");
            for (int i = 0; i < n; i++)
            {
                Degree[i] = adj[i].Count;
                DSAT[i] = Degree[i];
                Program.ShowProgression(i + 1, n);
            }

            Program.ClearConsoleLines(2);
            Console.WriteLine("3/3");
            while (nb < n)  // As long as we have not coloured all the vertices
            {
                c = 1;
                x = DsatMax(); // We choose the vertex of DSAT max not yet coloured
                while (!SuitableDSAT(x, c)) // We look for the smallest available colour for this vertex
                {
                    c++;
                }
                foreach (int j in adj[x]) // Updating of DSATs
                {
                    if (SuitableDSAT(j, c)) // j had no neighbour coloured c, so we increment its DSAT
                    {
                        DSAT[j]++;
                    }
                }
                color[x] = c;
                if (cmax < c)
                {
                    cmax = c;
                }
                nb++;

                Program.ShowProgression(nb, n);
            }

            return cmax;
        }

        private int DsatMax()
        {
            int maxDeg = -1;
            int maxDSAT = -1;
            int smax = 0;
            for (int i = 0; i < n; i++)
            {
                if (color[i] == 0 && (DSAT[i] > maxDSAT || (DSAT[i] == maxDSAT && Degree[i] > maxDeg)))
                {
                    maxDSAT = DSAT[i];
                    maxDeg = Degree[i];
                    smax = i;
                }
            }
            return smax;
        }

        private bool SuitableDSAT(int x, int c) // Test if colour c can be given to vertex x - version for DSATUR
        {
            foreach (int i in adj[x])
            {
                if (color[i] == c)
                {
                    return false;
                }
            }
            return true;
        }

        public Dsatur(Dictionary<int, List<int>> graph)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("| Calculating chromatic number |");
            Console.WriteLine("--------------------------------");

            //int nbc;

            // Filling of adj
            Console.WriteLine("1/3");
            n = graph.Keys.Max() + 1;
            adj = new List<int>[n];
            for (int i = 0; i < n; i++)
            {
                if (graph.Keys.Contains(i))
                {
                    adj[i] = new List<int>(graph[i]);
                }
                else
                {
                    adj[i] = new List<int>();
                }
                Program.ShowProgression(i + 1, n);
            }

            color = new int[n];
            DSAT = new int[n];
            Degree = new int[n];

            // DSATUR Calculation
            k = CalculateDsatur();  // The steps 2 and 3 are in the method CalculateDsatur
            Console.WriteLine();
            Console.WriteLine("DSAT Algorithm : Colouring in " + k + " colours.\n");
        }

    }
}
