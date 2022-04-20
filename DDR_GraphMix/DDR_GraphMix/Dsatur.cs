using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DDR_GraphMix
{
    public class Dsatur
    {
        private readonly List<int> DSAT; // Degrees of saturation
        private readonly List<int> color; // Colours for Dsatur
        private readonly List<int> Degree; // Degrees of the vertices
        private readonly int n; // Number of vertices
        private readonly int[][] adj; //[n][n];  // Graph adjacency matrix
        private readonly int k; // k-degeneration

        public int K => k;

        private int CalculateDsatur()
        {
            int nb = 0, c, x, cmax = 0;

            Program.ClearConsoleLines(2);
            Console.WriteLine("3/4");
            for (int i = 0; i < n; i++)
            {
                color.Add(0);
                DSAT.Add(0); 
                Degree.Add(0);
                for (int j = 0; j < n; j++)
                {
                    if (adj[i][j] == 1)
                    {
                        Degree[i]++;
                    }
                    
                } 
                DSAT[i] = Degree[i];
                Program.ShowProgression(i + 1, n);
            }

            Program.ClearConsoleLines(2);
            Console.WriteLine("4/4");
            while (nb < n)  // As long as we have not coloured all the vertices
            {
                c = 1;
                x = DsatMax(); // We choose the vertex of DSAT max not yet coloured
                while (!SuitableDSAT(x, c)) // We look for the smallest available colour for this vertex
                { 
                    c++;
                }
                for (int j = 0; j < n; j++) // Updating of DSATs
                {
                    if (adj[x][j] == 1 && SuitableDSAT(j, c)) // j had no neighbour coloured c, so we increment its DSAT
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
                    maxDeg = Degree[i]; smax = i;
                }
            }
            return smax;
        }

        private bool SuitableDSAT(int x, int c) // Test if colour c can be given to vertex x - version for DSATUR
        {
            for (int i = 0; i < n; i++) 
            {
                if (adj[x][i] == 1 && (color[i] == c))
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

            List<int> allValues = new List<int>();

            int graphCount = graph.Count;
            Console.WriteLine("1/4");
            foreach (List<int> l in graph.Values)
            {
                foreach (int v in l)
                {
                    if (!allValues.Contains(v))
                    {
                        allValues.Add(v);
                    }
                }
                Program.ShowProgression(allValues.Count, graphCount);
            }
            
            n = allValues.Max() + 1;
            adj = new int[n][];

            Program.ClearConsoleLines(2);
            Console.WriteLine("2/4");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (adj[i] == null)
                    {
                        adj[i] = new int[n];
                    }
                    if (graph.Keys.Contains(i))
                    {
                        if (graph[i].Contains(j))
                        {
                            adj[i][j] = 1;
                        }
                        else
                        {
                            adj[i][j] = 0;
                        }
                    }
                    else {
                        adj[i][j] = 0;
                    }
                }
                Program.ShowProgression(i+1, n);
            }

            color = new List<int>();
            DSAT = new List<int>();
            Degree = new List<int>();

            // DSATUR Calculation
            k = CalculateDsatur();  // The steps 3 and 4 are in the method CalculateDsatur
            Program.ConsoleWriter.Flush();
            Console.WriteLine();
            Console.WriteLine("DSAT Algorithm : Colouring in " + k + " colours.\n");
        }
        
    }
}
