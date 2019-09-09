using Download_mangas;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MangasScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            MyMangasEntities mangasEntities = new MyMangasEntities();


            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;
            var mangapages = 1;

            //try
            //{
            do
            {
                WebPage results;

                try
                {
                    results = browser.NavigateToPage(new Uri("https://unionmangas.top/lista-mangas/a-z/" + mangapages + "/*"), HttpVerb.Get);
                }
                catch (Exception)
                {

                    break;
                }




                HtmlDocument hdoc = new HtmlDocument();
                hdoc.LoadHtml(results.Html.InnerHtml);


                //Get value with given xpath
                string mangaXpath = "//div[@class ='col-md-3 col-xs-6 text-center bloco-manga']/a[2]";
                var mangas = hdoc.DocumentNode.SelectNodes(mangaXpath).Select(node => node.GetAttributeValue("href")).ToArray();

                foreach (var manga in mangas)
                {
                    var href = manga;

                    var capsPage = browser.NavigateToPage(new Uri(href), HttpVerb.Get);

                    hdoc.LoadHtml(capsPage.Html.InnerHtml);

                    //string mangaNameXpath = "/html/body/div[1]/div[1]/div[1]/div[@class='row']//div[@class='col-md-12']/h2";
                    //var mangaName = hdoc.DocumentNode.SelectNodes(mangaNameXpath);

                    string GenXpath = "/html/body/div[1]/div[1]/div[1]/div[3]/div[4]/h4/a";

                    var gen = hdoc.DocumentNode.SelectNodes(GenXpath).ToArray().OrderBy(x => x.InnerText).ToList();

                    var pass = false;

                    foreach (var g in gen)
                    {
                        if (g.InnerText.Contains("Yaoi") | /*g.Text.Contains("Shoujo Ai")|*/ g.InnerText.Contains("Shounen Ai"))
                        {
                            Console.WriteLine("Manga pulado");

                            pass = true;

                        }

                        //mangasEntities = new MyMangasEntities();
                        var notExistInBank = mangasEntities.Generos.Where(x => x.Genres == g.InnerText).Count() == 0;

                        if (notExistInBank)
                        {

                            mangasEntities.Generos.Add(new Generos
                            {
                                Genres = g.InnerText
                            });

                            mangasEntities.SaveChanges();

                        }

                    }


                    string nameXpath = "/html/body/div[1]/div[1]/div[1]/div[@class='row']//div[@class='col-md-12']/h2";

                    var nameManga = hdoc.DocumentNode.SelectSingleNode(nameXpath).InnerText.ToString();


                    string capXpath = "//div[@class ='col-xs-6 col-md-6']//a";

                    List<string> nameCaps;

                    try
                    {
                        nameCaps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().OrderBy(x => x.InnerText).Select(x => x.InnerText).ToList();
                    }
                    catch (Exception)
                    {

                        continue;
                    }

                    var caps = hdoc.DocumentNode.SelectNodes(capXpath).Select(node => node.GetAttributeValue("href")).ToArray();

                    Array.Sort(caps);


                    //var exist = mangasEntities.Mangas.Where(x => x.NameManga == nameManga && x.Capitulos.Count == caps.Count()).Count() > 0;
                    var mangaNExist = mangasEntities.Mangas.Where(x => x.NameManga == nameManga).Count() == 0;

                    if (/*exist ||*/ pass)
                    {
                        //Console.WriteLine("Ja existe no banco: " + nameManga);

                        continue;
                    }
                    if (mangaNExist)
                    {
                        Mangas Mangas = new Mangas();

                        Mangas.NameManga = nameManga;
                        Mangas.NumberOfCaps = caps.Count();

                        mangasEntities.Mangas.Add(Mangas);

                        mangasEntities.SaveChanges();
                    }


                    Console.WriteLine("Acessando: " + nameManga);

                    mangasEntities = new MyMangasEntities();

                    var relationexist = mangasEntities.RelationMangaGenero.Where(x => x.Mangas.NameManga == nameManga).Select(x => x.Generos.Genres).Count() != gen.Count;


                    if (relationexist)
                    {
                        foreach (var g in gen)
                        {
                            mangasEntities.RelationMangaGenero.Add(new RelationMangaGenero
                            {
                                ID_Genero = mangasEntities.Generos.Where(x => x.Genres == g.InnerText).Select(x => x.ID).First(),
                                ID_Manga = mangasEntities.Mangas.Where(x => x.NameManga == nameManga).Select(x => x.ID).First()

                            });
                        }
                    }




                    mangasEntities.SaveChanges();
                    var count = 0;
                    foreach (var cap in caps)
                    {
                        string[] pages = { };
                        var CAPNAME = nameCaps;
                        var atualCap = CAPNAME[count];
                        var capexist = mangasEntities.Capitulos.Where(x => x.Mangas.NameManga == nameManga && x.NameCap == atualCap).Count() == 0;

                        if (capexist)
                        {
                            //count++;
                            //continue;

                            Capitulos capitulos = new Capitulos();

                            capitulos.NameCap = CAPNAME[count];
                            capitulos.ID_Manga = mangasEntities.Mangas.Where(x => x.NameManga == nameManga).Select(x => x.ID).First();


                            var pagesPage = browser.NavigateToPage(new Uri(cap), HttpVerb.Get);
                            hdoc.LoadHtml(pagesPage.Html.InnerHtml);
                            string pagesXpath = "//*[@id='leitor']/div[4]/div[4]/img";

                            try
                            {
                                pages = hdoc.DocumentNode.SelectNodes(pagesXpath).Select(node => node.GetAttributeValue("src")).ToArray();
                            }
                            catch (Exception)
                            {
                                continue;
                                
                            }

                            

                            capitulos.NumberOfPages = pages.Count();
                            mangasEntities.Capitulos.Add(capitulos);

                            mangasEntities.SaveChanges();




                        }
                        else
                        {
                            WebPage pagesPage;
                            bool contue = true;
                            do
                            {
                                try
                                {
                                    pagesPage = browser.NavigateToPage(new Uri(cap), HttpVerb.Get);
                                    hdoc.LoadHtml(pagesPage.Html.InnerHtml);

                                    contue = false;
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(5000);
                                    contue = true;

                                }
                            } while (contue);

                            

                            
                            
                            string pagesXpath = "//*[@id='leitor']/div[4]/div[4]/img";
                            pages = hdoc.DocumentNode.SelectNodes(pagesXpath).Select(node => node.GetAttributeValue("src")).ToArray();
                        }
                        var i = 1;
                        foreach (var page in pages)
                        {


                            

                            if (page.Contains(".png"))
                            {
                                mangasEntities = new MyMangasEntities();

                                var pageExist = mangasEntities.Paginas.Where(x => x.Capitulos.Mangas.NameManga == nameManga && x.Capitulos.NameCap == atualCap && x.NumberPage == "Pagina " + i).Count() > 0;

                                if (pageExist)
                                {
                                    Console.WriteLine(DateTime.Now+" Pagina " + i + " do " + atualCap + " de " + nameManga + " ja existe no banco, pulando");

                                    i++;
                                    continue;
                                }

                                Console.WriteLine(DateTime.Now + "  Baixando: " + nameManga + "\\" + CAPNAME[count] + "\\Pagina " + i);

                                mangasEntities = new MyMangasEntities();
                                SaveImageBank(mangasEntities, nameManga, "Pagina " + i, CAPNAME[count], ImageFormat.Png, page);
                                //SaveImage(folder + namebook + "\\" + nameCap + "\\Pagina " + i, ImageFormat.Png, todow);
                                i++;
                            }
                            else
                            {
                                var pageExist = mangasEntities.Paginas.Where(x => x.Capitulos.Mangas.NameManga == nameManga && x.Capitulos.NameCap == atualCap && x.NumberPage == "Pagina " + i).Count() > 0;

                                if (pageExist)
                                {
                                    Console.WriteLine(DateTime.Now+" Pagina " + i + " do " + atualCap +" de "+ nameManga + " ja existe no banco, pulando");
                                    i++;
                                    continue;
                                }

                                Console.WriteLine(DateTime.Now + "  Baixando: " + nameManga + "\\" + CAPNAME[count] + "\\Pagina " + i);

                                mangasEntities = new MyMangasEntities();
                                SaveImageBank(mangasEntities, nameManga, "Pagina " + i, CAPNAME[count], ImageFormat.Jpeg, page);
                                //SaveImage(folder + namebook + "\\" + nameCap + "\\Pagina " + i, ImageFormat.Jpeg, todow);
                                i++;
                            }
                        }

                        count++;
                    }

                }

                mangapages++;

            } while (true);

            Console.WriteLine("Finalizado: " + DateTime.Now);
            //}
            //catch (Exception e )
            //{
            //    Console.WriteLine(e);

            //    Console.WriteLine(DateTime.Now + " FINALIZADO.");
            //    Console.ReadKey();
            //}



            //var doc = new HtmlAgilityPack.HtmlDocument();
            //doc.LoadHtml("https://unionmangas.top/lista-mangas/a-z");
            //var res = doc.DocumentNode.SelectNodes("//div[@class ='col-md-3 col-xs-6 text-center bloco-manga']");
            //var res = results.("col-md-3 col-xs-6 text-center bloco-manga");
        }
        public static bool SaveImageBank(MyMangasEntities mangasEntities, string book, string filename, string nameCap, ImageFormat format, string imageUrl)
        {
            Paginas paginas = new Paginas();
            try
            {





                //DirectoryInfo director = new DirectoryInfo(filename);

                //var dir = director.Parent.FullName;

                //if (!Directory.Exists(dir))
                //{
                //    Directory.CreateDirectory(dir);
                //}

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(imageUrl);

                // THIS LINE IS THE IMPORTANT ONE
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
                                "Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
                                ".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
                                "InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                Stream stream = response.GetResponseStream();
                //Image img = Image.FromStream(stream);
                //stream.Close();


                //WebClient client = new WebClient();



                //Stream stream = client.OpenRead(imageUrl);
                Bitmap bitmap;
                bitmap = new Bitmap(stream);
                var array = BitmapToByteArray(bitmap);
                paginas.NumberPage = filename;
                paginas.ID_Cap = mangasEntities.Capitulos.Where(x => x.NameCap == nameCap && x.Mangas.NameManga == book).Select(x => x.ID).First();
                paginas.BinaryImage = array;
                //mangasEntities.Paginas.
                //if (bitmap != null)
                //{
                //    bitmap.Save(filename + "." + format.ToString(), format);
                //}
                mangasEntities.Paginas.Add(paginas);

                mangasEntities.SaveChanges();
                stream.Flush();
                stream.Close();
                //client.Dispose();
                stream.Close();



                return true;
            }
            catch (Exception e)
            {

                return false;
            }


        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {

            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }
    }
}
