﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Document = iTextSharp.text.Document;
using static DDR_GraphMix.Dsatur;
using System.Text;
using System.Diagnostics;

namespace DDR_GraphMix
{
    class Program
    {
        static Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
        static Dictionary<int, int> vertexDegenerationTable;
        static List<int> vertexDegenerationTableMatulaBeck;
        static readonly List<string> dataFiles = new List<string>();
        static int DegenerationNumber = 0;
        static StreamWriter consoleWriter;

        public static StreamWriter ConsoleWriter { get => consoleWriter; }

        static void Main()
        {
            consoleWriter = new StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8, 10240);
            Console.CancelKeyPress += delegate {
                // Ensures consoleWriter is always closed when closing the program with Ctrl+C or Ctrl+Break
                consoleWriter.Dispose();
            };

            try
            {
                FillingDataSets();
                SelectMenu();
            }
            finally
            {
                // Ensures consoleWriter is always closed whether an exception occurs or not
                consoleWriter.Dispose();
            }
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
        /// The function is used to calculate and display the degeneration number of the current graph
        /// </summary>
        static void VertexDegenerationFilling()
        {
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("| Calculating degeneration number and filling the table |");
            Console.WriteLine("---------------------------------------------------------");

            Console.WriteLine(" /!\\ Beware that this operation can take a long time /!\\");

            vertexDegenerationTable = new Dictionary<int, int>();

            // copy graph
            Dictionary<int, List<int>> localGraph = graph.ToDictionary(entry => entry.Key, entry => new List<int>(entry.Value));
            int k = 0;

            int localGraphInitialSize = localGraph.Count;

            while (localGraph.Count != 0)
            {
                ShowProgression(localGraphInitialSize - localGraph.Count, localGraphInitialSize);

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

                int removeKeysCount = removeKeys.Count;

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

                    ShowProgression(localGraphInitialSize - localGraph.Count, localGraphInitialSize);
                }

                if (removeKeys.Count == 0)
                {
                    k++;
                }
            }
            consoleWriter.Flush();

            Console.WriteLine("\nThe degeneration number is : " + k + "\n");
            DegenerationNumber = k;
        }

        /// <summary>
        /// This function is used to create a graph named "export.pdf" with circles for k-degenerations numbers for the current graph
        /// </summary>
        static void CreatePDF(string filename)
        {
            string pdfFile = @"Resources\"+filename+".pdf";

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
            Console.WriteLine("---------------------\n");

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

            //Open the generated pdf
            new Process
            {
                StartInfo = new ProcessStartInfo(pdfFile)
                {
                    UseShellExecute = true
                }
            }.Start();
        }

        /// <summary>
        /// The function is used to calculate and display the degeneration number of the current graph with Matula & Beck algorithm
        /// </summary>
        static void VertexDegenerationFillingMatulaBeck()
        {
            //Initialize an output list L
            vertexDegenerationTableMatulaBeck = new List<int>();

            //Compute a number dv for each vertex v in G, the number of neighbors of v that are not already in L. Initially, these numbers are just the degrees of the vertices
            Dictionary<int, int> d = graph.ToDictionary(entry => entry.Key, entry => entry.Value.Count);

            int BiggestI = d.Values.Max();

            //Initialize an array D such that D[i] contains a list of the vertices v that are not already in L for which dv = i
            List<int>[] D = new List<int>[BiggestI + 1];
            for (int i = 0; i <= BiggestI; i++)
            {
                List<int> DiList = new List<int>();
                foreach (int key in d.Keys)
                {
                    if (d[key] == i)
                    {
                        DiList.Add(key);
                    }
                }

                D[i] = DiList;
            }

            //Initialize k to 0
            int k = 0;

            bool DContainsValues = true;

            //Repeat n times
            while (DContainsValues)
            {
                //Scan the array cells D[0], D[1],... until finding an i for which D[i] is nonempty
                DContainsValues = false;
                for (int i = 0; i < BiggestI; i++)
                {
                    if (D[i].Any())  //If the list contains elements
                    {
                        DContainsValues = true;

                        //Set k to max(k,i)
                        k = Math.Max(k, i);

                        //Select a vertex v from D[i]. Add v to the beginning of L and remove it from D[i].
                        int v = D[i][0];
                        vertexDegenerationTableMatulaBeck.Insert(0, v);
                        D[i].Remove(v);

                        //For each neighbor w of v not already in L, subtract one from dw and move w to the cell of D corresponding to the new value of dw
                        List<int> movedNeighbors = new List<int>();
                        foreach (int w in graph[v])
                        {
                            if (!vertexDegenerationTableMatulaBeck.Contains(w))
                            {
                                d[w] -= 1;
                                D[d[w]].Add(w);
                                movedNeighbors.Add(w);
                            }
                        }
                        foreach (int w in movedNeighbors)
                        {
                            D[i].Remove(w);
                        }
                    }
                }
            }

            Console.WriteLine("L :");
            foreach (int vertex in vertexDegenerationTableMatulaBeck)
            {
                Console.WriteLine("    "+vertex);
            }
        }

        /// <summary>
        /// This function is a list with all files we want to use
        /// </summary>
        static void FillingDataSets()
        {
            dataFiles.Add("Crime");
            dataFiles.Add("ERoadNetworks");
            dataFiles.Add("Exemple");
            dataFiles.Add("Facebook");
            dataFiles.Add("Flickr");
            dataFiles.Add("GooglePlus");
            dataFiles.Add("JazzMusicians");
            dataFiles.Add("Physicians");
            dataFiles.Add("PoliticalBooks");
            dataFiles.Add("SisterCities");
            dataFiles.Add("Twitter");
            dataFiles.Add("Youtube");
        }

        static void CompareDegenerationAndChromaticNumber()
        {
            List<List<string>> comparaison = new List<List<string>>();
            foreach(string file in dataFiles)
            {
                if(file != "Flickr")
                {
                    // reset
                    graph = new Dictionary<int, List<int>>();
                    DegenerationNumber = 0;

                    ReadFile(file);

                    VertexDegenerationFilling();
                    Dsatur d = new Dsatur(graph);
                    int degenerationNumber = DegenerationNumber;
                    int chromaticNumber = d.getK();
                    List<string> list = new List<string>
                {
                    file,
                    degenerationNumber.ToString(),
                    chromaticNumber.ToString()
                };
                    comparaison.Add(list);
                }
                
            }

            // display values
            foreach(List<string> line in comparaison)
            {
                Console.WriteLine("\nfile : " + line[0] + "\ndegenerationNumber = " + line[1] + "\nchromaticNumber = " + line[2]);
            }
        }

        /// <summary>
        /// The function is to ask the user what file he want to use
        /// </summary>
        /// <returns>string filename</returns>
        static string SelectFile()
        {
            int choice;
            bool returnValue;
            do
            {
                Console.WriteLine("CHOOSE A FILE :\n");
                int size = dataFiles.Count;
                for (int i = 1; i <= size; i++)
                {
                    string fileName = @"Resources\" + dataFiles[i - 1];
                    FileInfo fi = new FileInfo(fileName);
                    double fileSize = fi.Length;
                    string tabulation = "\t";
                    if(fileName.Length < 11 + 7) // 11 is for the Resources\\
                    {
                        tabulation = "\t\t";
                    }
                    if(fileSize < 1024)
                    {
                        Console.WriteLine(i + ". " + "\t" + dataFiles[i - 1] + tabulation + Math.Round((double)fi.Length, 1) + " o");
                    }
                    else if (fileSize < 1048576)
                    {
                        Console.WriteLine(i + ". " + "\t" + dataFiles[i - 1] + tabulation + Math.Round((double)fi.Length / 1024, 1) + " Ko");
                    }
                    else
                    {
                        Console.WriteLine(i + ". " + "\t" + dataFiles[i - 1] + tabulation + Math.Round((double)fi.Length / 1048576, 1) + " Mo");
                    }
                }
                Console.WriteLine();
                choice = Int32.Parse(Console.ReadLine());
                if (choice <= 0 || choice > size)
                {
                    Console.WriteLine("/!\\ BAD VALUE ! /!\\\n");
                    returnValue = true;
                }
                else
                {
                    Console.WriteLine();
                    returnValue = false;
                }
            }
            while (returnValue);

            return dataFiles[choice - 1];
        }

        static void ReadFile(string file)
        {
            graph = new Dictionary<int, List<int>>();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("| Filling in the table from the file |");
            Console.WriteLine("--------------------------------------");

            using (StreamReader streamReader = new StreamReader(@"Resources\" + file))
            {
                long fileLength = streamReader.BaseStream.Length;
                while (!streamReader.EndOfStream)
                {
                    ShowProgression(streamReader.BaseStream.Position, fileLength);
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
                consoleWriter.Flush();
                Console.WriteLine();  //Line break
                Console.WriteLine();  //Line break
            }

        }

        /// <summary>
        /// The function is the display select menu for the user to help him to choose options
        /// </summary>
        static void SelectMenu()
        {
            bool returnValue;
            do
            {
                Console.WriteLine("WELCOME TO OUR GRAPH PROJECT, CHOOSE AN OPTION IN THE LIST :\n");
                Console.WriteLine("1. Display the graph\n");
                Console.WriteLine("2. Calculate degeneration number\n");
                Console.WriteLine("3. Calculate chromatic number\n");
                Console.WriteLine("4. Generate a PDF with vertex degeneration\n");
                Console.WriteLine("5. Compare degeneration and chromatic numbers for a lots of graphs\n");
                Console.WriteLine("6. Calculate degeneration number with Matula Beck Algorithm\n");
                int choice = Int32.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        Console.WriteLine();
                        string file1 = SelectFile();
                        ReadFile(file1);
                        DisplayGraph();
                        returnValue = false;
                        break;
                    case 2:
                        Console.WriteLine();
                        string file2 = SelectFile();
                        ReadFile(file2);
                        VertexDegenerationFilling();
                        returnValue = false;
                        break;
                    case 3:
                        Console.WriteLine();
                        string file3 = SelectFile();
                        ReadFile(file3);
                        new Dsatur(graph);
                        returnValue = false;
                        break;
                    case 4:
                        Console.WriteLine();
                        string file4 = SelectFile();
                        ReadFile(file4);
                        VertexDegenerationFilling();
                        CreatePDF(file4);
                        returnValue = false;
                        break;
                    case 5:
                        Console.WriteLine();
                        CompareDegenerationAndChromaticNumber();
                        returnValue = false;
                        break;
                    case 6:
                        Console.WriteLine();
                        string file5 = SelectFile();
                        ReadFile(file5);
                        VertexDegenerationFillingMatulaBeck();
                        returnValue = false;
                        break;
                    default:
                        Console.WriteLine("/!\\ BAD VALUE ! /!\\\n");
                        returnValue = true;
                        break;
                }
            }
            while (returnValue);
        }

        public static void ShowProgression(int value, int max)
        {
            int pourcentage = value * 100 / max;
            string barre = "";
            for (int i = 0; i < 10; i++)
            {
                if (pourcentage / 10 > i)
                {
                    barre += '*';
                }
                else
                {
                    barre += ' ';
                }
            }
            consoleWriter.Write("\r[" + barre + "] " + pourcentage + '%');
        }

        public static void ShowProgression(long value, long max)
        {
            long pourcentage = value * 100 / max;
            string barre = "";
            for (int i = 0; i < 10; i++)
            {
                if (pourcentage / 10 > i)
                {
                    barre += '*';
                }
                else
                {
                    barre += ' ';
                }
            }
            consoleWriter.Write("\r[" + barre + "] " + pourcentage + '%');
        }

        public static void ClearConsoleLines(int lines)
        {
            consoleWriter.Flush();
            for (int i = 0; i < lines; i++)
            {
                Console.CursorLeft = 0;
                Console.Write("\u001b[K");  //Clear the right of the line)
                if (i + 1 != lines)
                {
                    Console.CursorTop -= 1;
                }
            }
        }
    }
}
