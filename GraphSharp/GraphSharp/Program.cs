using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Document = iTextSharp.text.Document;

namespace GraphSharp
{
    class Program
    {
        static Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
        static Dictionary<int, int> vertexDegenerationTable;
        static Dictionary<int, List<int>> vertexDegenerationTableMatulaBeck;
        static readonly ReadOnlyCollection<string> dataFiles = new ReadOnlyCollection<string>(new string[] {
            "Crime",
            "ERoadNetworks",
            "Exemple",
            "Facebook",
            "Flickr",
            "JazzMusicians",
            "PoliticalBooks",
            "SisterCities",
            "Youtube"
        });
        static int DegenerationNumber = 0;
        static sbyte lastPercentage = -1;

        static void Main()
        {
            SelectMenu();
        }

        /// <summary>
        /// This function is used to insert values into the value dictionary 
        /// </summary>
        /// <param name="curNode">The node</param>
        /// <param name="nextNode">The node's neighbour</param>
        static void Insert(int curNode, int nextNode)
        {
            if (!graph.ContainsKey(curNode))
            { // if the current node is not in the keys
                List<int> list = new List<int>
                {
                    nextNode
                };
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
            Stopwatch watch = new Stopwatch();

            watch.Start();

            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("| Calculating degeneration number and filling the table |");
            Console.WriteLine("---------------------------------------------------------");

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

            Console.WriteLine("\nThe degeneration number is : " + k);
            DegenerationNumber = k;

            watch.Stop();

            ShowExecutionTime(watch.ElapsedMilliseconds);
            Console.WriteLine();
        }

        /// <summary>
        /// This function is used to create a graph with circles for k-degenerations numbers for the current graph
        /// </summary>
        static void CreatePDF(string filename)
        {
            string pdfFile = @"Resources\" + filename + ".pdf";

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

            int maxCircleSize = (int)(document.GetRight(-10) / 2) * 80 / 100;
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
                foreach (int v in vertexDegenerationTable.Keys)
                {
                    if (vertexDegenerationTable[v] == key)
                    {
                        // calculate the position of the little circles with initial position and angle
                        double posX = (double)(document.GetRight(-10) / 2 + circleSize * Math.Cos(angle * i));
                        double posY = (double)(document.GetTop(-10) / 2 + circleSize * Math.Sin(angle * i));

                        double elemtSize = 75 / DegenerationNumber;
                        if (elemtSize < 5)
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
        /// The function is used to calculate and display the degeneration number of the current graph with the Matula & Beck algorithm
        /// </summary>
        static void VertexDegenerationFillingMatulaBeck()
        {
            Stopwatch watch = new Stopwatch();

            watch.Start();

            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("| Calculating degeneration number and filling the table |");
            Console.WriteLine("---------------------------------------------------------");

            //Initialize an output list L
            vertexDegenerationTableMatulaBeck = new Dictionary<int, List<int>>();

            //Compute a number dv for each vertex v in G, the number of neighbors of v that are not already in L. Initially, these numbers are just the degrees of the vertices
            int[] d = new int[graph.Keys.Max() + 1];
            for (int j = 0; j < d.Length; j++)
            {
                d[j] = -1;
                if (graph.ContainsKey(j))
                {
                    d[j] = graph[j].Count;
                }
            }

            int BiggestI = d.Max();
            int DInitialNumberOfElements = 0;

            Console.WriteLine("1/2");
            //Initialize an array D such that D[i] contains a list of the vertices v that are not already in L for which dv = i
            List<int>[] D = new List<int>[BiggestI + 1];
            for (int j = 0; j <= BiggestI; j++)
            {
                ShowProgression(j, BiggestI);

                List<int> DjList = new List<int>();
                for (int key = 0; key < d.Length; key++)
                {
                    if (d[key] == j)
                    {
                        DjList.Add(key);
                        DInitialNumberOfElements++;
                    }
                }

                D[j] = DjList;
            }

            //Initialize k to 0
            int k = 0;

            int i = 0;
            bool DContainsValues = true;
            int DNumberOfRemovedElements = 0;

            ClearConsoleLines(2);
            Console.WriteLine("2/2");
            //Repeat n times
            while (DContainsValues)
            {
                ShowProgression(DNumberOfRemovedElements, DInitialNumberOfElements);

                DContainsValues = false;
                //Search for the lowest non-empty bucket
                if (D[i].Count == 0)
                {
                    for (int j = i; j < D.Length; j++)
                    {
                        if (D[j].Count > 0)
                        {
                            i = j;
                            DContainsValues = true;
                            break;
                        }
                    }
                }
                else
                {
                    DContainsValues = true;
                }

                k = Math.Max(k, i);

                if (DContainsValues)
                {
                    //Remove the first vertex in the lowest non-empty bucket
                    int v = D[i][0];
                    D[i].Remove(v);
                    DNumberOfRemovedElements++;
                    if (vertexDegenerationTableMatulaBeck.ContainsKey(k))
                    {
                        vertexDegenerationTableMatulaBeck[k].Add(v);

                    }
                    else
                    {
                        vertexDegenerationTableMatulaBeck.Add(k, new List<int> { v });
                    }

                    d[v] = -1;

                    //Update the neighbors of the removed vertex, after checking if they were not removed
                    foreach (int u in graph[v])
                    {
                        if (d[u] != -1)
                        {
                            if (i == d[u])
                            {
                                i--;
                            }
                            D[d[u]].Remove(u);
                            d[u] -= 1;
                            D[d[u]].Add(u);
                        }
                    }
                }
            }

            Console.WriteLine("\nThe degeneration number is : " + k);

            watch.Stop();

            ShowExecutionTime(watch.ElapsedMilliseconds);
            Console.WriteLine();
        }

        /// <summary>
        /// Calculate degeneration and chromatic numbers of all the files (except Flickr) and compare them at the end
        /// </summary>
        static void CompareDegenerationAndChromaticNumber()
        {
            List<List<string>> comparaison = new List<List<string>>();
            foreach (string file in dataFiles)
            {
                if (file != "Flickr")
                {
                    // reset
                    graph = new Dictionary<int, List<int>>();
                    DegenerationNumber = 0;

                    ReadFile(file);

                    VertexDegenerationFilling();
                    Dsatur d = new Dsatur(graph);
                    int degenerationNumber = DegenerationNumber;
                    int chromaticNumber = d.K;
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
            foreach (List<string> line in comparaison)
            {
                Console.WriteLine("\nfile : " + line[0] + "\ndegenerationNumber = " + line[1] + "\nchromaticNumber = " + line[2]);
            }
        }

        /// <summary>
        /// The function is to ask the user what file he want to use
        /// </summary>
        /// <returns>The name of the selected file</returns>
        static string SelectFile()
        {
            int choice;
            bool returnValue;
            int size = dataFiles.Count;

            //Order files by size
            List<(string fileName, long fileSize)> orderedFiles = new List<(string fileName, long fileSize)>();
            for (int i = 0; i < size; i++)
            {
                string fileName = @"Resources\" + dataFiles[i];
                FileInfo fi = new FileInfo(fileName);
                long fileSize = fi.Length;

                int index = orderedFiles.Count;
                for (int j = 0; j < orderedFiles.Count; j++)
                {
                    if (fileSize < orderedFiles[j].fileSize)
                    {
                        index = j;
                        break;
                    }
                }

                orderedFiles.Insert(index, (dataFiles[i], fileSize));
            }

            do
            {
                Console.WriteLine("CHOOSE A FILE :\n");

                for (int i = 0; i < size; i++)
                {
                    (string fileName, long fileSize) file = orderedFiles[i];
                    string fileName = @"Resources\" + file.fileName;

                    string tabulation = "\t";
                    if (fileName.Length < 11 + 7) // 11 is for the Resources\\
                    {
                        tabulation = "\t\t";
                    }

                    if (file.fileSize < 1024)
                    {
                        Console.WriteLine((i + 1) + ". \t" + file.fileName + tabulation + file.fileSize + "o");
                    }
                    else if (file.fileSize < 1048576)
                    {
                        Console.WriteLine((i + 1) + ". \t" + file.fileName + tabulation + Math.Round((double)file.fileSize / 1024, 1) + "Ko");
                    }
                    else
                    {
                        Console.WriteLine((i + 1) + ". \t" + file.fileName + tabulation + Math.Round((double)file.fileSize / 1048576, 1) + "Mo");
                    }
                }
                Console.WriteLine();
                int.TryParse(Console.ReadLine(), out choice);
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

            return orderedFiles[choice - 1].fileName;
        }

        /// <summary>
        /// Read a file to fills the variable graph
        /// </summary>
        /// <param name="file">Name of the file used</param>
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
                        string[] splitedLine = readLine.Split('\t'); // get the two ints of the line

                        int curNode = int.Parse(splitedLine[0]);
                        int nextNode = int.Parse(splitedLine[1]);

                        Insert(curNode, nextNode); // insert ints into 
                        Insert(nextNode, curNode); // insert ints into 
                    }
                }
            }
            Console.WriteLine();  //Line break
            Console.WriteLine();  //Line break
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
                Console.WriteLine("6. Calculate degeneration number with the Matula & Beck algorithm\n");
                int.TryParse(Console.ReadLine(), out int choice);

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

        /// <summary>
        /// Show the progression using a bar progressively filled with asterisks
        /// </summary>
        /// <param name="value">Value to calculate a percentage of progression</param>
        /// <param name="max">Maximum value of value to calculate a percentage of progression</param>
        public static void ShowProgression(int value, int max)
        {
            int pourcentage = value * 100 / max;
            if (pourcentage < lastPercentage)  // A new progression is happening
            {
                lastPercentage = -1;
            }
            if (pourcentage > lastPercentage)
            {
                lastPercentage = (sbyte)pourcentage;
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
                Console.Write("\r[" + barre + "] " + pourcentage + '%');
            }
        }

        /// <summary>
        /// Show the progression using a bar progressively filled with asterisks
        /// </summary>
        /// <param name="value">Value to calculate a percentage of progression</param>
        /// <param name="max">Maximum value of value to calculate a percentage of progression</param>
        public static void ShowProgression(long value, long max)
        {
            long pourcentage = value * 100 / max;
            if (pourcentage < lastPercentage)  // A new progression is happening
            {
                lastPercentage = -1;
            }
            if (pourcentage > lastPercentage)
            {
                lastPercentage = (sbyte)pourcentage;
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
                Console.Write("\r[" + barre + "] " + pourcentage + '%');
            }
        }

        /// <summary>
        /// Show the execution time using milliseconds passed in parameter
        /// </summary>
        /// <param name="ms">milliseconds of running time</param>
        public static void ShowExecutionTime(long ms)
        {
            long seconds = ms / 1000;
            long minutes = seconds / 60;
            long hours = minutes / 60;

            Console.Write("Execution Time: ");
            if (hours > 0)
            {
                Console.Write(hours + "h " + minutes % 60 + "min " + seconds % 60 + "s " + ms % 1000 + "ms\n");
            }
            else if (minutes > 0)
            {
                Console.Write(minutes + "min " + seconds % 60 + "s " + ms % 1000 + "ms\n");
            }
            else if (seconds > 0)
            {
                Console.Write(seconds + "s " + ms % 1000 + "ms\n");
            }
            else
            {
                Console.Write(ms + "ms\n");
            }
        }

        /// <summary>
        /// Clear a specified number of lines in the console
        /// </summary>
        /// <param name="lines">Number of lines to clear</param>
        public static void ClearConsoleLines(int lines)
        {
            for (int i = 0; i < lines; i++)
            {
                Console.CursorLeft = 0;
                Console.Write("\u001b[K");  //Clear the right of the line
                if (i + 1 != lines)
                {
                    Console.CursorTop -= 1;
                }
            }
        }
    }
}
