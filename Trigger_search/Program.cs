using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper_Union;
using WebScrapper_MangaHost;
namespace Trigger_search
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
					var mangas = manga.Split(';');

					//int index = int.Parse(mangas[1]);

					WebScrapper_MangaLivre.Program.DownManga(mangas[0]);
					//WebScraper_Union.Program.DownManga(mangas[0]);
					//WebScrapper_MangaHost.Program.DownManga(mangas[0]);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);

					Console.ReadKey();
				}
			}
		}
    }
}
