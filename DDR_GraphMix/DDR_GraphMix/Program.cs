using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Document = iTextSharp.text.Document;

namespace DDR_GraphMix
{
    class Program
    {
        static Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
        static Dictionary<int, int> vertexDegenerationTable;
        static int DegenerationNumber = 0;

        static void Main()
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("| Filling in the table from the file |");
            Console.WriteLine("--------------------------------------");
            graph = new Dictionary<int, List<int>>();

            //using (StreamReader streamReader = new StreamReader(@"Resources\exemple"))
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
            CreatePDF();
        }

        /// <summary>
        /// This function is used to insert values into the value dictionary 
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="nextNode"></param>
        static void Insert(int curNode, int nextNode)
        {
            if (!graph.ContainsKey(curNode))
            { // if the current node is not in the keys
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
            Console.WriteLine("-------------------------------");
            Console.WriteLine("| Displaying the loaded graph |");
            Console.WriteLine("-------------------------------");

            foreach (int key in graph.Keys) // for each keys
            {
                List<int> list = graph[key]; // get all nextNode of the key
                string line = "Node : " + key + " --> \n";
                foreach (int nextNode in list)
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
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("| Calculating degeneration number and filling the table |");
            Console.WriteLine("---------------------------------------------------------");

            Console.WriteLine("\n /!\\ Beware that this operation can take a long time /!\\");

            vertexDegenerationTable = new Dictionary<int, int>();
            Dictionary<int, List<int>> localGraph = graph.ToDictionary(entry => entry.Key, entry => entry.Value);
            int k = 0;

            while (localGraph.Count != 0)
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
                    foreach (int row in localGraph.Keys)
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

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("| Displaying degeneration for each vertex |");
            Console.WriteLine("-------------------------------------------");

            foreach (int key in vertexDegenerationTable.Keys.OrderBy(key => key))
            {
                Console.WriteLine(key + "\t" + vertexDegenerationTable[key]);
            }

            Console.WriteLine("\nThe degeneration number is  : " + k + "\n");
            DegenerationNumber = k;
        }

        static void CreatePDF()
        {
            string pdfFile = @"Resources\export.pdf";

            Document document = new Document(PageSize.A0, 10, 10, 10, 10);
            FileStream fs = new FileStream(pdfFile, FileMode.Create, FileAccess.Write);
            PdfWriter writer = PdfWriter.GetInstance(document, fs);
            document.Open();

            // the pdf content
            PdfContentByte cb = writer.DirectContent;

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("| Create table k --> number of k-degeneration |");
            Console.WriteLine("-----------------------------------------------");

            // create table k --> number of k-degeneration
            Dictionary<int, int> kNumbers = new Dictionary<int, int>();
            foreach (int key in vertexDegenerationTable.Keys.OrderBy(key => key))
            {
                if (!kNumbers.ContainsKey(vertexDegenerationTable[key]))
                {
                    kNumbers.Add(vertexDegenerationTable[key], 1);
                }
                else
                {
                    kNumbers[vertexDegenerationTable[key]]++;
                }
            }

            Console.WriteLine("---------------------");
            Console.WriteLine("| PDF file creation |");
            Console.WriteLine("---------------------");

            int maxCircleSize = (int)(document.GetRight(-10) / 2) * 80 /100;
            int minCircleSize = 150;
            foreach (int key in kNumbers.Keys.OrderBy(key => key)) // circles for loop
            {
                // circles creation
                int circleSize = maxCircleSize / DegenerationNumber * (DegenerationNumber - key) + minCircleSize;
                cb.SetColorStroke(new BaseColor(183, 3, 223));
                
                cb.Circle(document.GetRight(-10) / 2, document.GetTop(-10) / 2, circleSize);
                cb.Stroke();

                double angle = Math.PI * (360.0 / kNumbers[key]) / 180.0; // angle in rad

                int i = 0;
                foreach(int v in vertexDegenerationTable.Keys)
                {
                    if(vertexDegenerationTable[v] == key)
                    {
                        // calculate the position of the little circles with initial position and angle
                        double posX = (double)(document.GetRight(-10) / 2 + circleSize * Math.Cos(angle * i));
                        double posY = (double)(document.GetTop(-10) / 2 + circleSize * Math.Sin(angle * i));

                        double elemtSize = 75 / DegenerationNumber;
                        if(elemtSize < 5)
                        {
                            elemtSize = 5;
                        }
                        cb.Circle(posX, posY, elemtSize);

                        cb.SetColorFill(new BaseColor(0, 0, 0));
                        cb.Fill();

                        cb.SetColorStroke(new BaseColor(0, 0, 0));
                        cb.Stroke();

                        cb.BeginText();
                        cb.SetColorFill(new BaseColor(255, 255, 255));
                        cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), (float)elemtSize - 2);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, v.ToString(), (float)posX, (float)(posY - elemtSize / 5), 0);
                        cb.EndText();
                        i++;
                    }
                }
            }
            document.Close();
            fs.Close();
            writer.Close();
        }
    }
}
