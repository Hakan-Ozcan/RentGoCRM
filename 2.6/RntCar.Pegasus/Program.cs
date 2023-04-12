using Microsoft.Xrm.Sdk;
using Renci.SshNet;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.Pegasus
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);

            ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
            var pegasusID = configurationBL.GetConfigurationByName("PegasusCampaignId");

            var contracts = contractRepository.getCompletedPegasusContracts(pegasusID);
            StringBuilder sb = new StringBuilder("H");
            var year = DateTime.Now.Year.ToString();
            var month = DateTime.Now.Month.ToString().Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            var day = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            sb.AppendLine(year + month + day);

            var record_identifier = "D";
            var partnerX = "RENTGO";
            var activityClassification = "REGO";
            foreach (var item in contracts)
            {
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(item.GetAttributeValue<EntityReference>("rnt_customerid").Id, new string[] { "emailaddress1", "rnt_dialcode", "firstname", "lastname", "mobilephone" });
                var str = record_identifier;
                var memberNumber = customer.GetAttributeValue<string>("rnt_dialcode") + customer.GetAttributeValue<string>("mobilephone");
                memberNumber = StaticHelper.completeEmptyCharacterByGivenLength(memberNumber, 16);
                str += memberNumber;

                str += StaticHelper.completeEmptyCharacterByGivenLength(customer.GetAttributeValue<string>("firstname").Substring(0, 2), 30);
                str += StaticHelper.completeEmptyCharacterByGivenLength(customer.GetAttributeValue<string>("lastname").Substring(0, 2), 30);


                var dropoffDateTime = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                var d_year = dropoffDateTime.Year.ToString();
                var d_month = dropoffDateTime.Month.ToString().Length == 1 ? "0" + dropoffDateTime.Month.ToString() : dropoffDateTime.Month.ToString();
                var d_day = dropoffDateTime.Day.ToString().Length == 1 ? "0" + dropoffDateTime.Day.ToString() : dropoffDateTime.Day.ToString();
                str += d_year + d_month + d_day;

                str += StaticHelper.completeEmptyCharacterByGivenLength(partnerX, 10);
                str += StaticHelper.completeEmptyCharacterByGivenLength(activityClassification, 10);
                str += StaticHelper.completeEmptyCharacterByGivenLength("ISTANBUL", 10);
                str += StaticHelper.completeEmptyCharacterByGivenLength(item.GetAttributeValue<string>("rnt_pnrnumber"), 20);
                str += StaticHelper.completeZeroCharacterByGivenLength(item.GetAttributeValue<Money>("rnt_generaltotalamount").Value, 10);
                var totalDuration = item.GetAttributeValue<decimal>("rnt_totalduration");
                var point = decimal.Zero;
                if (totalDuration >= 3)
                {
                    point = 20000;
                }
                else if (totalDuration >= 2 && totalDuration < 3)
                {
                    point = 9000;
                }
                else
                {
                    point = 6000;
                }
                str += StaticHelper.completeZeroCharacterByGivenLength(point, 10);
                str += "0000000.00";
                //str += customer.GetAttributeValue<string>("emailaddress1");
                sb.AppendLine(str);
                Entity e = new Entity("rnt_contract");
                e.Id = item.Id;
                e["rnt_pegasuspoint"] = point;
                e["rnt_benefitfrompegasus"] = true;
                e["rnt_pegasussenddate"] = DateTime.UtcNow.AddHours(3);// Türkiye lokasyon saati
                e["rnt_pegasussendvalue"] = str;// Türkiye lokasyon saati
                crmServiceHelper.IOrganizationService.Update(e);
            }
            sb.AppendLine("F" + StaticHelper.completeZeroCharacterByGivenLength(contracts.Count, 8));

            const string host = "pgsftp.flypgs.com";
            const string username = "rentgop";
            const string password = "ZEWbm632";
            //const string workingdirectory = @"..\ACCRUAL_HANDBACK";
            const string workingdirectory = @"..\ACCRUAL";

            string uploadfile = @"C:\BatchLogs\Pegasus\" + string.Format("ACCRUAL.RENTGO.{0}.txt", DateTime.Now.ToString("yyyyMMdd"));

            File.WriteAllText(@"C:\BatchLogs\Pegasus\" + string.Format("ACCRUAL.RENTGO.{0}.txt", DateTime.Now.ToString("yyyyMMdd")), sb.ToString(), Encoding.Default);

            using (var client = new SftpClient(host, 22, username, password))
            {
                client.Connect();
                using (var fileStream = new FileStream(uploadfile, FileMode.Open))
                {

                    Console.WriteLine("Uploading {0} ({1:N0} bytes)", uploadfile, fileStream.Length);
                    client.ChangeDirectory(workingdirectory);
                    Console.WriteLine("Changed directory to {0}", workingdirectory);

                    var listDirectory = client.ListDirectory(workingdirectory);
                    //foreach (var file in listDirectory)
                    //{
                    //    string remoteFileName = file.Name;
                    //    if (!file.Name.StartsWith("."))
                    //    {
                    //        using (Stream file1 = File.OpenWrite(@"C:\BatchLogs\Pegasus\Handback\" + remoteFileName))
                    //        {
                    //            client.DownloadFile(workingdirectory + @"\" + remoteFileName, file1);
                    //        }
                    //    }

                    //}
                    client.BufferSize = 4 * 1024; // bypass Payload error large files
                    //client.DeleteFile(string.Format("ACCRUAL.RENTGO.{0}.txt", DateTime.Now.ToString("yyyyMMdd")));
                    client.UploadFile(fileStream, Path.GetFileName(uploadfile));
                }
            }
        }
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
    }
}
