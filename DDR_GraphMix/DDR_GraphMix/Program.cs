using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DDR_GraphMix
{
    class Program
    {
        static Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
        static Dictionary<int, int> vertexDegenerationTable;

        static void Main(string[] args)
        {
            graph = new Dictionary<int, List<int>>();

            using (StreamReader streamReader = new StreamReader(@"Resources\out.ego-gplus"))
            {
                while (!streamReader.EndOfStream)
                {
                    string readLine = streamReader.ReadLine();
                    if (!readLine.StartsWith("%") && readLine != "") // read lines except lines who begin with "%"
                    {
                        string[] splitedLine = readLine.Split("\t"); // get the two ints of the line

                        int curNode = Int32.Parse(splitedLine[0]);
                        int nextNode = Int32.Parse(splitedLine[1]);

                        Insert(curNode, nextNode); // insert ints into 
                        Insert(nextNode, curNode); // insert ints into 
                    }  
                }
            }
            //DisplayGraph();
            VertexDegenerationFilling();

        }

        /// <summary>
        /// This function is used to insert values into the value dictionary 
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="nextNode"></param>
        static void Insert(int curNode, int nextNode)
        {
            if (!graph.ContainsKey(curNode)){ // if the current node is not in the keys
                List<int> list = new List<int>();
                list.Add(nextNode);
                graph.Add(curNode, list);
            }
            else // if the current node is in the keys
            {
                graph[curNode].Add(nextNode);
            }
        }

        /// <summary>
        /// This function is used to display in the console the nodes connected to each point 
        /// </summary>
        static void DisplayGraph()
        {
            foreach(int key in graph.Keys) // for each keys
            {
                List<int> list = graph[key]; // get all nextNode of the key
                string line = "Node : " + key + " --> \n";
                foreach(int nextNode in list)
                {
                        line += "\t" + nextNode + "\n";
                }
                Console.WriteLine(line);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void VertexDegenerationFilling()
        {
            vertexDegenerationTable = new Dictionary<int, int>();
            Dictionary<int, List<int>> localGraph = graph.ToDictionary(entry => entry.Key, entry => entry.Value);
            int k = 1;

            while(localGraph.Count != 0)
            {
                // filling a list with all key we have to delete
                List<int> removeKeys = new List<int>();
                foreach (int key in localGraph.Keys) // for each keys
                {
                    List<int> list = localGraph[key]; // get all nextNode of the key
                    if (list.Count <= k)
                    {
                        removeKeys.Add(key);
                    }
                }

                // Delete keys and delete values in nextnodes values
                foreach (int key in removeKeys) // for each keys
                {
                    localGraph.Remove(key);
                    foreach(int row in localGraph.Keys)
                    {
                        List<int> list = localGraph[row]; // get all nextNode of the key
                        list.Remove(key);
                    }
                    vertexDegenerationTable.Add(key, k);
                }

                // 
                if (removeKeys.Count == 0)
                {
                    k++;
                }
            }
            
            foreach(int key in vertexDegenerationTable.Keys.OrderBy(key => key))
            {
                Console.WriteLine(key + "\t" + vertexDegenerationTable[key]);
            }
            Console.WriteLine("Degeneration : " + k);
        }
    }
}
