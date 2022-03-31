using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDR_GraphMix
{
    public class Dsatur
    {
        private readonly List<int> DSAT; // Degrees of saturation
        private readonly List<int> color1; // Vertex colours for the exact agorithm
        private readonly List<int> color2; // Colours for Dsatur
        private readonly List<int> Degree; // Degrees of the vertices
        private readonly int n; // Number of vertices
        private readonly int[][] adj; //[n][n];  // Graph adjacency matrix
        private bool finded = false; // Allows to stop the exact algorithm when a colouring has been found
        private int k; // k-degeneration

        private int CalculateDsatur()
        {
            int nb = 0, c, x, cmax = 0;
            for (int i = 0; i < n; i++)
            {
                color2.Add(0);
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
            }

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
                color2[x] = c;
                if (cmax < c)
                {
                    cmax = c;
                }
                nb++;
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
                if (color2[i] == 0 && (DSAT[i] > maxDSAT || (DSAT[i] == maxDSAT && Degree[i] > maxDeg)))
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
                if (adj[x][i] == 1 && (color2[i] == c))
                {
                    return false;
                }
            }
            return true;
        }

        int ChromaticNumber(int d) // Calculates the chromatic number by testing from d colours and decreasing k as far as possible
        {
            int k = d + 1;
            do
            {
                k--;
                finded = false;
                Colorexact(k);
            }
            while (finded);
            return k + 1;
        }

        void Colorexact(int k) // Test if the graph has a k-colouring by trying all combinations
        {
            for (int i = 0; i < n; i++)
            {
                color1.Add(0);
            }
            ColorRR(0, k);
        }

        void ColorRR(int x, int k) // Recursive function to test all possible colours for vertex x
        {
            if (x == n)
            {
                Console.WriteLine("\nColorExact Algorithm : Colouring in " + k + " colours finded");
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine("Colours of " + i + " : " + color1[i]);
                }
                finded = true;
            }
            else
                for (int c = 1; c <= k; c++)
                {
                    if (Suitable(x, c))
                    {
                        color1[x] = c;
                        ColorRR(x + 1, k);
                        if (finded)
                        {
                            return;
                        }
                        
                    }
                }   
        }

        bool Suitable(int x, int c) // Test if the colour c can be given to vertex x (it is not used by one of its neighbours)
        {
            for (int i = 0; i < x; i++) 
            {
                if (adj[x][i] == 1 && (color1[i] == c))
                {
                    return false;
                }
            } 
            return true;
        }


        public Dsatur(Dictionary<int, List<int>> graph)
        {
            int nbc;

            // Filling of adj

            List<int> allValues = new List<int>();

            foreach (List<int> l in graph.Values)
            {
                foreach (int v in l)
                {
                    if (!allValues.Contains(v))
                    {
                        allValues.Add(v);
                    }
                }
            }
            
            n = allValues.Count;
            adj = new int[n][];

            for (int i = 0; i < allValues.Count; i++)
            {
                for (int j = 0; j < allValues.Count; j++)
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
            }

            color1 = new List<int>(); 
            color2 = new List<int>();
            DSAT = new List<int>();
            Degree = new List<int>();

            // DSATUR Calculation
            k = CalculateDsatur();
            for (int i = 0; i < n; i++)
            {
                //Console.WriteLine("colours of " + i + " : " + color2[i]);
            }
            Console.WriteLine("DSAT Algorithm : Colouring in " + k + " colours.");


            // ColorExact calculation
            //nbc = ChromaticNumber(k);
            //Console.WriteLine("Chromatic number : " + nbc);
        }
        public int getK()
        {
            return this.k;
        }
        
    }
}
