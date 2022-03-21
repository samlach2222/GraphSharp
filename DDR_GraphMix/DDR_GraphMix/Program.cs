using System;
using System.IO;

namespace DDR_GraphMix
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader streamReader = new StreamReader(@"Resources\out.ego-gplus"))
            {
                while (!streamReader.EndOfStream)
                {
                    string ligne = streamReader.ReadLine();

                    Console.WriteLine(ligne);
                }
            }
        }
    }
}
