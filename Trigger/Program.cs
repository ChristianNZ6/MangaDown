using System;
using System.IO;
using WebScrap_Union;

namespace Trigger
{
    class Program
    {
        static void Main(string[] args)
        {

			var mansgas = File.ReadAllLines(@"C:\Users\chris\Desktop\mangasReader.txt");


			foreach (var manga in mansgas)
			{
				try
				{
					WebScrap_Union.Program.DownManga(manga);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					throw;
				}
			}

			
        }
    }
}
