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
        public static Dictionary<string, long> DirSize(DirectoryInfo d)
        {
            var filesSize = new Dictionary<string, long>(); 
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            { 
                filesSize.Add(fi.Name, fi.Length); 
            } 

            return filesSize;
        }
        private static string FormatBytes(long bytes)
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
