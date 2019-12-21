using System;
using WebScrap_Union;

namespace Trigger
{
    class Program
    {
        static void Main(string[] args)
        {
			try
			{
				WebScrap_Union.Program.DownManga("Kimetsu no yaiba");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
        }
    }
}
