using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text;
using System.Linq;

namespace RntCar.WriteServiceLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            createLog(@"C:\\webservicelogs", "WebServiceLogs");
            createLog(@"C:\\weblogs", "WebLogs");
            createLog(@"C:\\brokerservicelogs", "BrokerLogs");


        }

        private static void createLog(string folderPath,string logCollection)
        {
            var client = new MongoClient(StaticHelper.GetConfiguration("MongoDBHostName"));
            var database = client.GetDatabase(StaticHelper.GetConfiguration("MongoDBDatabaseName"));
           //StaticHelper.GetConfiguration("FolderPath");

            var collection = database.GetCollection<LogData>(logCollection);
            int j = 1;
            foreach (string file in Directory.GetFiles(folderPath))
            {
                try
                {
                    var content = File.ReadLines(file, Encoding.Default);

                    string[] meta = new string[] { };
                    string[] header = new string[] { };
                    List<string> messages = new List<string>();
                    var l = content.ToList();
                    if(l.Count == 0)
                    {
                        try
                        {
                            File.Delete(file);
                            continue;
                        }
                        catch(Exception ex)
                        {
                            continue;
                        }
                    }
                    foreach (var item in content)
                    {
                        meta = item.Split('-');
                        header = meta[0].Split(' ');
                        var parts = item.Split('-');
                        string message = "";
                        for (int i = 2; i < parts.Length; i++)
                        {
                            message += parts[i].Trim();
                        }

                        messages.Add(message);
                    }
                    var m = meta.Length > 1 ? meta[1].Trim() : meta[0].Trim();
                    LogData log = new LogData
                    {
                        type = header[0].Trim(),
                        controller = header[2].Trim(),
                        date = m,
                        message = messages,
                        method = file.Split('\\')[3].Split('.')[0].Trim()
                    };

                    collection.InsertOne(log);

                    File.Delete(file);
                    Console.WriteLine(j);
                    j++;
                }
                catch (Exception ex)
                {
                    j++;
                }
            }
        }

        public class LogData
        {
            public ObjectId _id { get; set; }
            public string date { get; set; }
            public string type { get; set; }
            public string controller { get; set; }
            public string method { get; set; }
            public List<string> message { get; set; }
        }
    }
}
