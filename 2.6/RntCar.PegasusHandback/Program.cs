using Microsoft.Xrm.Sdk;
using Renci.SshNet;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.PegasusHandback
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            const string host = "pgsftp.flypgs.com";
            const string username = "rentgop";
            const string password = "ZEWbm632";
            const string workingdirectory = @"..\ACCRUAL_HANDBACK";

            using (var client = new SftpClient(host, 22, username, password))
            {
                client.Connect();
                client.ChangeDirectory(workingdirectory);

                var directory = new DirectoryInfo(@"C:\BatchLogs\Pegasus");
                var myFile = (from f in directory.GetFiles()
                              orderby f.LastWriteTime descending
                              select f).First();

                //var myFile = (from f in directory.GetFiles()
                //              where f.Name == "ACCRUAL.RENTGO.20210705.txt"
                //              orderby f.LastWriteTime descending
                //              select f).First();

                var listDirectory = client.ListDirectory(workingdirectory);
                foreach (var file in listDirectory)
                {
                    string remoteFileName = file.Name;
                    var f = myFile.Name.Split('.')[2];
                    if (!file.Name.StartsWith(".") && file.Name.IndexOf(f) > -1)
                    {
                        using (Stream file1 = File.OpenWrite(@"C:\BatchLogs\Pegasus\Handback\" + remoteFileName))
                        {
                            client.DownloadFile(workingdirectory + @"\" + remoteFileName, file1);                           
                        }

                        string[] content = File.ReadAllLines(@"C:\BatchLogs\Pegasus\Handback\" + remoteFileName, System.Text.Encoding.GetEncoding("iso-8859-1"));
                        var lls = new List<List<string>>();
                        for (int i = 0; i < content.Length; i++)
                        {
                            try
                            {
                                if (i != 0)
                                {
                                    var splitted = content[i].Split(' ').Where(p => !string.IsNullOrEmpty(p)).ToList();
                                    if (splitted.Count >= 6)
                                    {
                                        BusinessLibrary.Repository.ContractRepository contractRepository = new BusinessLibrary.Repository.ContractRepository(crmServiceHelper.IOrganizationService);
                                        var c = contractRepository.getContractByPnrNumber(splitted[6].ToString(), new string[] { });

                                        if (splitted[8].Contains("000Success"))
                                        {
                                            Entity e = new Entity("rnt_contract");
                                            e.Id = c.Id;
                                            e["rnt_pegasusresponse"] = true;
                                            crmServiceHelper.IOrganizationService.Update(e);
                                        }
                                        else
                                        {
                                            Entity e = new Entity("rnt_contract");
                                            e.Id = c.Id;
                                            e["rnt_pegasusresponse"] = true;
                                            var str = "";
                                            for (int j = 8; j < 18; j++)
                                            {
                                               if(splitted.ElementAtOrDefault(j) == null)
                                                {
                                                    break;
                                                }
                                                str += splitted[j];
                                            }
                                            e["rnt_pegasusexceptiondetail"] = str;
                                            crmServiceHelper.IOrganizationService.Update(e);

                                        }

                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }                           

                        }
                    }

                }


            }
        }
    }
}
