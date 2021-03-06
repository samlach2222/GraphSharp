using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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
            "Exemple",
            "ExempleLooping",
            "Facebook",
            "Flickr",
            "JazzMusicians",
            "PoliticalBooks",
            "SisterCities",
            "Youtube",
            "UK-Domains",
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
            int numberRemovedKeys = 0;

            // copy graph
            //Dictionary<int, List<int>> localGraph = graph.ToDictionary(entry => entry.Key, entry => new List<int>(entry.Value));
            int n = graph.Keys.Max() + 1;
            List<int>[] localGraph = new List<int>[n];
            for (int i = 0; i < n; i++)
            {
                if (graph.Keys.Contains(i))
                {
                    localGraph[i] = new List<int>(graph[i]);
                }
                else
                {
                    localGraph[i] = null;
                    numberRemovedKeys++;
                }
            }
            int k = 0;

            while (numberRemovedKeys < n)
            {
                ShowProgression(numberRemovedKeys, n);

                // filling a list with all key we have to delete
                List<int> removeKeys = new List<int>();
                for (int key = 0; key < n; key++) // for each keys
                {
                    if (localGraph[key]?.Count <= k) // get all nextNode of the key
                    {
                        removeKeys.Add(key);
                    }
                }

                // Delete keys and delete values in nextnodes values
                foreach (int key in removeKeys) // For each keys to remove
                {
                    foreach (int neighborOfKey in localGraph[key]) // For each neighbors of the keys to remove
                    {
                        if (neighborOfKey != key) // If the node is self-linking it will be removed when setting its neighbors to null
                        {
                            localGraph[neighborOfKey].Remove(key);
                        }
                    }
                    localGraph[key] = null;
                    vertexDegenerationTable.Add(key, k);

                    numberRemovedKeys++;
                    ShowProgression(numberRemovedKeys, n);
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
            int vertexDegenerationTableKeysMax = vertexDegenerationTable.Keys.Max();
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
                ShowProgression(key, vertexDegenerationTableKeysMax);
            }

            Console.WriteLine("\n---------------------");
            Console.WriteLine("| PDF file creation |");
            Console.WriteLine("---------------------");

            int maxCircleSize = (int)(document.GetRight(-10) / 2) * 80 / 100;
            int minCircleSize = 150;
            int kNumbersKeysMax = kNumbers.Keys.Max();
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
                ShowProgression(key, kNumbersKeysMax);
            }
            Console.WriteLine("\n---------------------------------");
            Console.WriteLine("| Exporting and opening the PDF |");
            Console.WriteLine("---------------------------------");
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

            Console.WriteLine();
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

            Console.WriteLine("1/2");
            //Compute a number dv for each vertex v in G, the number of neighbors of v that are not already in L. Initially, these numbers are just the degrees of the vertices
            //Initialize an array D such that D[i] contains a list of the vertices v that are not already in L for which dv = i
            int[] d = new int[graph.Keys.Max() + 1];
            int BiggestI = graph.Max(x => x.Value.Count);
            int DInitialNumberOfElements = 0;
            int dLength = d.Length;
            List<int>[] D = new List<int>[BiggestI + 1];
            for (int j = 0; j < BiggestI + 1; j++)  // Initialise D
            {
                D[j] = new List<int>();
            }
            for (int j = 0; j < dLength; j++)
            {
                d[j] = -1;
                if (graph.ContainsKey(j))
                {
                    d[j] = graph[j].Count;
                    DInitialNumberOfElements++;
                    D[d[j]].Add(j);
                }

                ShowProgression(j + 1, dLength);
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
        /// Calculate degeneration and chromatic numbers of all the files (except UK-Domains) and compare them at the end
        /// </summary>
        static void CompareDegenerationAndChromaticNumber()
        {
            List<List<string>> comparaison = new List<List<string>>();
            foreach (string file in dataFiles)
            {
                if (file != "UK-Domains" && file != "Flickr")
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
            Console.WriteLine();
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
                long fileSize;
                if (dataFiles[i] == "UK-Domains")
                {
                    fileSize = long.MaxValue; // very big number to be the last
                }
                else
                {
                    FileInfo fi = new FileInfo(fileName);
                    fileSize = fi.Length;
                }

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
                Console.WriteLine("CHOOSE A FILE OR 0 TO GO BACK :\n");

                for (int i = 0; i < size; i++)
                {
                    (string fileName, long fileSize) file = orderedFiles[i];
                    string fileName = @"Resources\" + file.fileName;

                    if (file.fileName == "UK-Domains") // last element
                    {
                        string tabulation = "\t";
                        if (fileName.Length < 11 + 7) // 11 is for the Resources\\
                        {
                            tabulation = "\t\t";
                        }
                        if (!File.Exists("Resources/UK-Domains")) // if the file dont exist
                        {
                            Console.WriteLine((i + 1) + ".\t" + file.fileName + tabulation + "4Go\t[6 Go RAM needed + 504Mo Download]");
                        }
                        else
                        {
                            Console.WriteLine((i + 1) + ".\t" + file.fileName + tabulation + "4Go\t[6 Go RAM needed]");
                        }
                    }
                    else
                    {
                        string tabulation = "\t";
                        if (fileName.Length < 11 + 7) // 11 is for the Resources\\
                        {
                            tabulation = "\t\t";
                        }

                        if (file.fileSize < 1024)
                        {
                            Console.WriteLine((i + 1) + ".\t" + file.fileName + tabulation + file.fileSize + "o");
                        }
                        else if (file.fileSize < 1048576)
                        {
                            Console.WriteLine((i + 1) + ".\t" + file.fileName + tabulation + Math.Round((double)file.fileSize / 1024, 1) + "Ko");
                        }
                        else
                        {
                            Console.WriteLine((i + 1) + ".\t" + file.fileName + tabulation + Math.Round((double)file.fileSize / 1048576, 1) + "Mo");
                        }
                    }
                }
                Console.WriteLine();
                choice = int.TryParse(Console.ReadLine(), out choice) ? choice : -1;
                if (choice < 0 || choice > size)
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

            if (choice == 0)
            {
                Console.Clear();
                return null;
            }
            else if (choice == size) // last file, to download
            {
                if (!File.Exists("Resources/UK-Domains")) // if the file dont exist
                {
                    DownloadAndTreatVeryBigFile();
                }
                return orderedFiles[choice - 1].fileName;
            }
            else
            {
                return orderedFiles[choice - 1].fileName;
            }
        }

        static void DownloadAndTreatVeryBigFile()
        {
            if (!File.Exists("Resources/download.tsv.dimacs10-uk-2002.tar.bz2") || new FileInfo("Resources/download.tsv.dimacs10-uk-2002.tar.bz2").Length < 528152876)  //Download only if the file isn't already downloaded
            {
                Console.WriteLine("Downloading...");
                using (var client = new WebClient())
                {
                    using (var completedSignal = new AutoResetEvent(false))
                    {
                        client.DownloadFileCompleted += (s, e) =>
                        {
                            completedSignal.Set();
                        };

                        client.DownloadProgressChanged += (s, e) => ShowProgression(e.ProgressPercentage, 100);
                        client.DownloadFileAsync(new Uri("http://konect.cc/files/download.tsv.dimacs10-uk-2002.tar.bz2"), "Resources/download.tsv.dimacs10-uk-2002.tar.bz2");

                        completedSignal.WaitOne();
                    }
                }
                Console.WriteLine("\n");
            }

            Console.WriteLine("Extracting...");
            BZip2InputStream bz2is = new BZip2InputStream(File.OpenRead("Resources/download.tsv.dimacs10-uk-2002.tar.bz2"));
            TarArchive tarArchive = TarArchive.CreateInputTarArchive(bz2is, null);
            tarArchive.ExtractContents("Resources/");
            tarArchive.Close();
            File.Move("Resources/dimacs10-uk-2002/out.dimacs10-uk-2002", "Resources/UK-Domains");
            Directory.Delete("Resources/dimacs10-uk-2002", true);
            bz2is.Close();
            File.Delete("Resources/download.tsv.dimacs10-uk-2002.tar.bz2");
            Console.WriteLine("Done !\n");
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

            lastPercentage = -1; // Some files are immediately 100% loaded
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
                Console.WriteLine("0. Quit\n");
                Console.WriteLine("1. Display the graph\n");
                Console.WriteLine("2. Calculate degeneration number\n");
                Console.WriteLine("3. Calculate chromatic number\n");
                Console.WriteLine("4. Generate a PDF with vertex degeneration\n");
                Console.WriteLine("5. Compare degeneration and chromatic numbers for a lots of graphs\n");
                Console.WriteLine("6. Calculate degeneration number with the Matula & Beck algorithm\n");
                int choice = int.TryParse(Console.ReadLine(), out choice) ? choice : -1;

                switch (choice)
                {
                    case 0:
                        Console.WriteLine();
                        returnValue = false;
                        break;
                    case 1:
                        Console.WriteLine();
                        string file1 = SelectFile();
                        if (file1 != null)
                        {
                            ReadFile(file1);
                            DisplayGraph();
                            PauseAndClearScreen();
                        }
                        returnValue = true;
                        break;
                    case 2:
                        Console.WriteLine();
                        string file2 = SelectFile();
                        if (file2 != null)
                        {
                            ReadFile(file2);
                            VertexDegenerationFilling();
                            PauseAndClearScreen();
                        }
                        returnValue = true;
                        break;
                    case 3:
                        Console.WriteLine();
                        string file3 = SelectFile();
                        if (file3 != null)
                        {
                            ReadFile(file3);
                            new Dsatur(graph);
                            PauseAndClearScreen();
                        }
                        returnValue = true;
                        break;
                    case 4:
                        Console.WriteLine();
                        string file4 = SelectFile();
                        if (file4 != null)
                        {
                            ReadFile(file4);
                            VertexDegenerationFilling();
                            CreatePDF(file4);
                            PauseAndClearScreen();
                        }
                        returnValue = true;
                        break;
                    case 5:
                        Console.WriteLine();
                        CompareDegenerationAndChromaticNumber();
                        PauseAndClearScreen();
                        returnValue = true;
                        break;
                    case 6:
                        Console.WriteLine();
                        string file5 = SelectFile();
                        if (file5 != null)
                        {
                            ReadFile(file5);
                            VertexDegenerationFillingMatulaBeck();
                            PauseAndClearScreen();
                        }
                        returnValue = true;
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

        /// <summary>
        /// Pause the console and clear it after a key is pressed
        /// </summary>
        public static void PauseAndClearScreen()
        {
            Console.Write("\nPress any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
        }
    }
}
