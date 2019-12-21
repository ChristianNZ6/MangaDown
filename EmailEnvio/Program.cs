using System;
using System.IO;
using System.Net.Mail;
using System.Threading;

namespace EmailEnvio
{
   public class Program
    {
        static void Main(string[] args)
        {
            
        }

        public static void SendEmaail(string filepath,string fileName)
        {
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.Timeout = 360000;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("christianjorge2009@gmail.com", "Tiocris1");
            MailMessage mail = new MailMessage();
            mail.Sender = new System.Net.Mail.MailAddress("christianjorge2009+EnvioManga@gmail.com", "ENVIADOR");
            mail.From = new MailAddress("christianjorge2009+EnvioManga@gmail.com", "Manga Adicionado");
            mail.To.Add(new MailAddress("CHRISTIANJORGE2009_6ULABI@kindle.com", "RECEBEDOR"));
            mail.Subject = "";
            //mail.Body = "teste";
            mail.Attachments.Add(new Attachment(filepath));
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;
            

            var contue = true;
            while (contue)
            {
                try
                {
                    client.Send(mail);
                    Console.WriteLine(fileName + " enviado");
                    var a = 1;

                    contue = false;
                }
                catch (System.Exception erro)
                {

                    Console.WriteLine(fileName + " não enviado, tentando novamente");

                    Thread.Sleep(5000);



                    //trata erro
                }
            }
           
            //finally
            //{
            //    mail = null;
            //}
        }
    }
}
