using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DDR_GraphMix
{
    class Program
    {
        static Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();

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

                        insertion(curNode, nextNode); // insert ints into 
                    }  
                }
            }

            displayGraph();
        }

        static void insertion(int curNode, int nextNode)
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

        static void displayGraph()
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
    }
}
