using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SendMongoDBFileSizeMail
{
    internal class Program
    {
        public static Dictionary<string, long> DirSize(DirectoryInfo d)//Bu metot, bir dizindeki dosyaların boyutlarını hesaplamak için kullanılabilir. Özellikle, bir uygulamanın kullanıcı tarafından seçilen bir dizindeki dosyaların boyutlarını toplamak veya bir raporda kullanmak gibi senaryolarda kullanılabilir.
        {
            var filesSize = new Dictionary<string, long>(); //"filesSize" adlı boş bir sözlük (dictionary) oluşturuluyor. Bu sözlük, dosya adlarını anahtar (key) olarak ve dosya boyutlarını değer (value) olarak tutacak.
            FileInfo[] fis = d.GetFiles();//"d" adlı bir klasördeki tüm dosyaların bilgileri "fis" adlı bir diziye (array) alınıyor. Bu işlem, System.IO namespace'inde yer alan FileInfo sınıfıyla yapılıyor.
            foreach (FileInfo fi in fis)
            { 
                filesSize.Add(fi.Name, fi.Length); //"filesSize" sözlüğüne, dosya adı ("fi.Name") anahtar olarak ve dosya boyutu ("fi.Length") değer olarak ekleniyor.
            } 

            return filesSize;//: Son olarak, "filesSize" sözlüğü geri döndürülüyor. Bu sayede, bu fonksiyonu çağıran diğer kodlar bu sözlük içindeki dosya adlarına ve boyutlarına erişebilirler.
        }
        private static string FormatBytes(long bytes)//Bu kod verilen bir sayısal değeri (byte cinsinden) daha okunaklı hale getirmek için kullanılıyor. Bu amaçla, verilen sayıyı uygun bir birimde (byte, kilobyte, megabyte, gigabyte veya terabyte) formatlayarak, daha anlaşılır bir biçimde gösteriliyor.
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        static void Main(string[] args)
        {
            var path= ConfigurationManager.AppSettings.Get("MongoDBDataFolder");

            var smtpSenderMail = ConfigurationManager.AppSettings.Get("SmtpSenderMail");
            var smtpSenderPassword = ConfigurationManager.AppSettings.Get("SmtpSenderPassword");
            var smtpRecipientMails = ConfigurationManager.AppSettings.Get("SmtpRecipientMails");
            var host = "smtp.office365.com";
            var port = 587;

            var subject = "MongoDB Dosya Boyutları";
            var body = @"<table><tr><th></th><th>Dosya Adı</th><th>Boyut</th></tr>";

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var dirSize = DirSize(directoryInfo).OrderByDescending(x=>x.Value);
            foreach (var file in dirSize)
            {
                body += $"<tr><td>• &#9;</td><td>{file.Key}</td><td>{FormatBytes(file.Value)}</td></tr>"; 
            } 
            body += "</table>";
             
            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(smtpSenderMail, smtpSenderPassword),
                EnableSsl = true
            };
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(smtpSenderMail);
            mailMessage.To.Add(smtpRecipientMails);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            client.Send(mailMessage);

            Console.WriteLine("Email sent");
             
        }
    }
}
