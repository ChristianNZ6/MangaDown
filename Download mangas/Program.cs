using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Download_mangas
{
    public static class Program
    {
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            ChromeDriver driver = new ChromeDriver(options);
            driver.Url = "https://unionmangas.top/lista-mangas/a-z";
            
            bool next = false;
            string folder = @"D:\Manga\";
            //IReadOnlyList<IWebElement> gen = new ChromeDriver().FindElements(By.XPath(""));

            MyMangasEntities mangasEntities = new MyMangasEntities();


            //mangasEntities.


            bool newgen = false;

            do
            {
                Thread.Sleep(5000);
                var bks = driver.FindElementsByXPath("/html/body/div[1]/div[1]/div[1]/div[10]/div/a[1]");
                if (bks.Count==0)
                {
                    Screenshot(driver, @"C:\1\p.png");
                }
                foreach (var bk in bks)
                {
                    //driver.FindElementByPartialLinkText(bk.GetAttribute("HREF").ToString()).SendKeys(Keys.Control + Keys.Return);

                    //bk.SendKeys(Keys.Control + Keys.Return);

                    TryGetfirst(driver, bk);

                    //driver.SwitchTo().Window(driver.WindowHandles[1]);
                   


                    var gen = driver.FindElementsByXPath("/html/body/div[1]/div[1]/div[1]/div[3]/div[4]/h4/a");
                    
                    var pass = false;
                    newgen = false;
                    foreach (var g in gen)
                    {
                        if (g.Text.Contains("Yaoi") | /*g.Text.Contains("Shoujo Ai")|*/ g.Text.Contains("Shounen Ai"))
                        {

                            pass = true;

                        }

                        //mangasEntities = new MyMangasEntities();
                        var notExistInBank = mangasEntities.Generos.Where(x => x.Genres == g.Text).Count() == 0;

                        if (notExistInBank)
                        {
                            
                            mangasEntities.Generos.Add(new Generos
                            {
                                Genres = g.Text
                            });

                            mangasEntities.SaveChanges();

                        }

                    }

                    Thread.Sleep(5000);
                    var namebook = driver.FindElementByXPath("/html/body/div[1]/div[1]/div[1]/div[1]/div/h2").Text;

                    var caps = driver.FindElementsByXPath("//div[@class='row lancamento-linha']");

                    mangasEntities = new MyMangasEntities();

                    var exist = mangasEntities.Mangas.Where(x => x.NameManga.Contains(namebook)).Count() != 0 ;
                    //string[] directorys = Directory.GetDirectories(folder);

                    if (exist)
                    {

                        pass = true;
                    }

                    if (pass)
                    {

                        driver.SwitchTo().Window(driver.WindowHandles[1]).Close();
                        driver.SwitchTo().Window(driver.WindowHandles[0]);
                        Console.WriteLine(DateTime.Now + " PULANDO");
                        continue;
                    }

                    Mangas mangas = new Mangas();

                    mangas.NameManga = namebook;
                    mangas.NumberOfCaps = caps.Count;
                    
                    mangasEntities.Mangas.Add(mangas);
                    
                    mangasEntities.SaveChanges();

                    mangasEntities = new MyMangasEntities();

                    foreach (var g in gen)
                    {
                        mangasEntities.RelationMangaGenero.Add(new RelationMangaGenero
                        {
                            ID_Genero = mangasEntities.Generos.Where(x => x.Genres == g.Text).Select(x => x.ID).First(),
                            ID_Manga = mangasEntities.Mangas.Where(x => x.NameManga == namebook).Select(x=>x.ID).First()

                        });
                    }



                    mangasEntities.SaveChanges();

                    mangasEntities = new MyMangasEntities();

                    foreach (var cap in caps.ToArray().OrderBy(x => x.Text))
                    {
                        Regex regex = new Regex(@"Cap\.\s+(\d+(\.\d+)?)");



                        var nameCap = regex.Match(cap.Text).Groups[0].ToString();

                        Capitulos capitulos = new Capitulos();

                        capitulos.NameCap = nameCap;
                        capitulos.ID_Manga = mangasEntities.Mangas.Where(x => x.NameManga == namebook).Select(x => x.ID).First();


                        var links = TryGet(driver, nameCap);


                        
                        capitulos.NumberOfPages = links.Count;
                        mangasEntities.Capitulos.Add(capitulos);

                        mangasEntities.SaveChanges();
                        //driver.SwitchTo().Window(driver.WindowHandles[2]);

                        //var links = driver.FindElementsByXPath("//*[@id='leitor']/div[4]/div[4]/img");
                        //var k = links[0].GetAttribute("SRC").ToString();
                        
                        var i = 1;
                        foreach (var link in links)
                        {
                            var todow = link.GetAttribute("SRC").ToString();

                            Console.WriteLine(DateTime.Now + "  Baixando:" + namebook + "\\" + nameCap + "\\Pagina " + i);

                            if (todow.Contains(".png"))
                            {
                                mangasEntities = new MyMangasEntities();

                                SaveImageBank(mangasEntities, namebook, "Pagina " + i, nameCap, ImageFormat.Png, todow);
                                SaveImage(folder + namebook + "\\" + nameCap + "\\Pagina " + i, ImageFormat.Png, todow);
                                i++;
                            }
                            else
                            {
                                mangasEntities = new MyMangasEntities();
                                SaveImageBank(mangasEntities, namebook, "Pagina " + i, nameCap, ImageFormat.Jpeg, todow);
                                SaveImage(folder + namebook + "\\" + nameCap + "\\Pagina " + i, ImageFormat.Jpeg, todow);
                                i++;
                            }
                        }

                        driver.SwitchTo().Window(driver.WindowHandles[2]).Close();
                        driver.SwitchTo().Window(driver.WindowHandles[1]);

                    }

                    driver.SwitchTo().Window(driver.WindowHandles[1]).Close();
                    driver.SwitchTo().Window(driver.WindowHandles[0]);


                    //var selectElement = new SelectElement(dropdown1);



                }

                //var element = FindElementIfExists(driver, By.TagName(">"));

                next = false;
                if (driver.FindElementsByXPath("/html/body/div[1]/div[1]/div[1]/div[12]/nav/ul/li[6]/a/span[1]").Count > 0)
                {
                    driver.FindElementByXPath("/html/body/div[1]/div[1]/div[1]/div[12]/nav/ul/li[6]/a").Click();

                    next = true;

                }
                if (driver.FindElementsByXPath("/html/body/div[1]/div[1]/div[1]/div[12]/nav/ul/li[8]/a/span[1]").Count > 0)
                {
                    driver.FindElementByXPath("/html/body/div[1]/div[1]/div[1]/div[12]/nav/ul/li[8]/a").Click();
                    next = true;
                }

            } while (next);

            driver.Close();

            Console.WriteLine("FINALIZADO:" + DateTime.Now);
            Console.ReadKey();
        }

        public static bool SaveImage(string filename, ImageFormat format, string imageUrl)
        {
            try
            {
                DirectoryInfo director = new DirectoryInfo(filename);

                var dir = director.Parent.FullName;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(imageUrl);
                Bitmap bitmap;
                bitmap = new Bitmap(stream);

                if (bitmap != null)
                {
                    bitmap.Save(filename + "." + format.ToString(), format);
                }

                stream.Flush();
                stream.Close();
                client.Dispose();

                return true;
            }
            catch (Exception e)
            {

                return false;
            }


        }

        public static bool SaveImageBank(MyMangasEntities mangasEntities,string book ,string filename,string nameCap, ImageFormat format, string imageUrl)
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
        public static IReadOnlyCollection<IWebElement> TryGet(ChromeDriver driver, string nomeCap)
        {
            //IReadOnlyList<IWebElement> gen;
            IReadOnlyCollection<IWebElement> links = Array.Empty<IWebElement>();
            bool retry = false;

            

            do
            {

                

                try
                {
                    driver.FindElementByPartialLinkText(nomeCap).SendKeys(Keys.Control + Keys.Return);

                    driver.SwitchTo().Window(driver.WindowHandles[2]);

                    links = driver.FindElementsByXPath("//*[@id='leitor']/div[4]/div[4]/img");

                    retry = false;

                }
                catch
                {
                    driver.SwitchTo().Window(driver.WindowHandles[2]).Close();
                    driver.SwitchTo().Window(driver.WindowHandles[1]);

                    retry = true;

                    
                }

            } while (retry);

            

            return links;





        }
        public static void Screenshot(IWebDriver driver, string screenshotsPasta)
        {
            ITakesScreenshot camera = driver as ITakesScreenshot;
            Screenshot foto = camera.GetScreenshot();
            foto.SaveAsFile(screenshotsPasta, ScreenshotImageFormat.Png);
        }
        public static IReadOnlyCollection<IWebElement> TryGetfirst(ChromeDriver driver, IWebElement bk)
        {
            //IReadOnlyList<IWebElement> gen;
            IReadOnlyCollection<IWebElement> links = Array.Empty<IWebElement>();
            bool retry = false;



            do
            {



                try
                {

                    Thread.Sleep(5000);
                    bk.SendKeys(Keys.Control + Keys.Return);

                    driver.SwitchTo().Window(driver.WindowHandles[1]);

                    retry = false;

                }
                catch
                {
                    driver.SwitchTo().Window(driver.WindowHandles[1]).Close();
                    driver.SwitchTo().Window(driver.WindowHandles[0]);

                    retry = true;

                }

            } while (retry);



            return links;





        }
        public static IWebElement FindElementIfExists(this IWebDriver driver, By by)
        {
            var elements = driver.FindElements(by);
            return (elements.Count >= 1) ? elements.First() : null;
        }
        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
