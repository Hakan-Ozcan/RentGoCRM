using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using OfficeOpenXml;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RntCar.ProcessHGSDifference
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var basePath = "C:\\Creatif\\BatchApplications\\RntCar.ProcessHGSDifference\\files";

            HGSDifferenceRepository hGSDifferenceRepository = new HGSDifferenceRepository(crmServiceHelper.IOrganizationService);
            AnnotationRepository annotationRepository = new AnnotationRepository(crmServiceHelper.IOrganizationService);
            ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
            EquipmentRepository equipmentRepository = new EquipmentRepository(crmServiceHelper.IOrganizationService);
            AnnotationBL annotationBL = new AnnotationBL(crmServiceHelper.IOrganizationService);
            LoggerHelper helper = new LoggerHelper();
            try
            {
                var list = hGSDifferenceRepository.getDraftHGSDifference();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var equipments = equipmentRepository.getAllEquipments();
                helper.traceInfo("list count " + list.Count);
                foreach (var item in list)
                {
                    helper.traceInfo("item id " + item.Id);
                    Guid fileGuid = Guid.NewGuid();
                    var relatedMonth = item.GetAttributeValue<OptionSetValue>("rnt_month").Value;
                    var relatedYear = item.GetAttributeValue<OptionSetValue>("rnt_year").Value;

                    string fullMonthName = new DateTime(relatedYear, relatedMonth, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("tr"));
                    helper.traceInfo("fullMonthName " + fullMonthName);
                    var startDate = new DateTime(relatedYear, relatedMonth, 1);
                    var lastDayOfMonth = startDate.AddMonths(1).AddDays(-1);
                    var endDate = new DateTime(relatedYear, relatedMonth, lastDayOfMonth.Day);



                    List<HGSBankList> hgsBankList = new List<HGSBankList>();
                    var path = string.Format(@"{1}\{0}.xlsx", fileGuid, basePath);
                    helper.traceInfo("path " + path);
                    var file = annotationRepository.getLatestAnnotationByObjectIdByFileName(item.Id, "banka.xlsx");
                    File.WriteAllBytes(path, Convert.FromBase64String(file.GetAttributeValue<string>("documentbody")));

                    FileInfo existingFile = new FileInfo(path);
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        //get the first worksheet in the workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int colCount = worksheet.Dimension.End.Column;  //get Column Count
                        int rowCount = worksheet.Dimension.End.Row;     //get row count
                        for (int row = 2; row <= rowCount; row++)
                        {
                            HGSBankList hgs = new HGSBankList
                            {
                                hgsNumber = Convert.ToString(worksheet.Cells[row, 1].Text),
                                plateNumber = Convert.ToString(worksheet.Cells[row, 2].Text),
                                entryLocation = Convert.ToString(worksheet.Cells[row, 4].Text),
                                exitLocation = Convert.ToString(worksheet.Cells[row, 6].Text),
                                entryDate = string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? null : (DateTime?)Convert.ToDateTime(worksheet.Cells[row, 5].Text),
                                exitDate = string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? null : (DateTime?)Convert.ToDateTime(worksheet.Cells[row, 7].Text),
                                amount = Convert.ToDecimal(worksheet.Cells[row, 8].Text)
                            };
                            hgsBankList.Add(hgs);
                        }
                    }

                    var newList = hgsBankList.Where(p => (p.entryDate == null || p.entryDate.Value.Month == relatedMonth) || (p.exitDate == null || p.exitDate.Value.Month == relatedMonth)).ToList();
                    var maxDate = newList.Max(p => p.exitDate);
                    var minDate = newList.Min(p => p.exitDate);

                    helper.traceInfo("minDate " + minDate);
                    helper.traceInfo("maxDate " + maxDate);

                    var contractItems = contractItemRepository.getCompletedContractItemsByGivenDays(minDate.Value.AddMonths(-6).ToString("yyyy-MM-dd"),
                                                                                                    maxDate.Value.AddMonths(6).ToString("yyyy-MM-dd"));

                    helper.traceInfo("contractItems count " + contractItems.Count);
                    Parallel.ForEach(newList, hgs =>
                    {
                        var equipment = equipments.ToList().Where(p => p.GetAttributeValue<string>("rnt_platenumber") == hgs.plateNumber).FirstOrDefault();

                        if (equipment == null)
                        {
                            hgs.contractNumber = "Plaka CRM'de bulunamadı";
                            return;
                        }
                        var items = contractItems.Where(p => p.Contains("rnt_equipment") && p.GetAttributeValue<EntityReference>("rnt_equipment").Id == equipment.Id).ToList();

                        var res = items.Where(p => hgs.exitDate.IsBetween(p.GetAttributeValue<DateTime>("rnt_pickupdatetime"), p.GetAttributeValue<DateTime>("rnt_dropoffdatetime"))).FirstOrDefault();

                        if (res != null)
                        {
                            hgs.contractNumber = Convert.ToString(res.GetAttributeValue<AliasedValue>("contract.rnt_pnrnumber").Value);
                        }
                        else
                        {
                            hgs.contractNumber = "Sözleşme CRM'de bulunamadı";
                        }
                    });
                    helper.traceInfo("GenerateExcel start" + newList.Count);

                    ExportToExcel(newList, basePath + "\\output\\" + fileGuid.ToString() + ".xlsx");
                    //GenerateExcel(ConvertToDataTable(newList), basePath + "\\output\\" + fileGuid.ToString() + ".xlsx");
                    helper.traceInfo("GenerateExcel end");
                    annotationBL.createNewAnnotation(new AnnotationData
                    {
                        DocumentBody = Convert.ToBase64String(File.ReadAllBytes(basePath + "\\output\\" + fileGuid.ToString() + ".xlsx")),
                        NoteText = "HGS karşılaştırma",
                        ObjectId = item.Id,
                        Subject = "HGS karşılaştırma",
                        ObjectName = "rnt_hgsdifference"

                    });
                    helper.traceInfo("email start");
                    ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                    Entity fromActivityParty = new Entity("activityparty");
                    fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));

                    var recipients = new EntityCollection();
                    string recipientsAddress = StaticHelper.GetConfiguration("recipientList");
                    var addressList = recipientsAddress.Split(';');
                    foreach (var address in addressList)
                    {
                        recipients.Entities.Add(new Entity("activityparty")
                        {
                            ["addressused"] = address
                        });
                    }

                    Entity createemail = new Entity("email");
                    createemail["from"] = new Entity[] { fromActivityParty };
                    createemail["to"] = recipients;


                    createemail["subject"] = string.Format("HGS Karşılaştırma - {0} - {1}", fullMonthName, relatedYear);
                    //createemail["description"] = message;
                    createemail["directioncode"] = true;
                    Guid emailId = crmServiceHelper.IOrganizationService.Create(createemail);

                    Entity attachment = new Entity("activitymimeattachment");
                    attachment["subject"] = "HGS Karşılaştırma";
                    attachment["filename"] = "Hgskarsilastırma.xlsx";
                    attachment["body"] = Convert.ToBase64String(File.ReadAllBytes(basePath + "\\output\\" + fileGuid.ToString() + ".xlsx"));
                    attachment["mimetype"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    attachment["attachmentnumber"] = 1;
                    attachment["objectid"] = new EntityReference("email", emailId);
                    attachment["objecttypecode"] = "email";
                    crmServiceHelper.IOrganizationService.Create(attachment);

                    SendEmailRequest sendEmailRequest = new SendEmailRequest
                    {
                        EmailId = emailId,
                        TrackingToken = "",
                        IssueSend = true
                    };

                    SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
                    helper.traceInfo("email end");
                    new XrmHelper(crmServiceHelper.IOrganizationService).setState("rnt_hgsdifference", item.Id, 0, 100000000);
                    helper.traceInfo("all operations end");
                }
            }
            catch (Exception ex)
            {
                helper.traceInfo("exception : " + ex.Message);
            }

        }

        public static void ExportToExcel(IEnumerable<HGSBankList> bankList, string path)
        {
            FileInfo f = new FileInfo(path);
            using (var excelFile = new ExcelPackage(new FileInfo(path)))
            {
                var worksheet = excelFile.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].LoadFromCollection(Collection: bankList, PrintHeaders: true);
                excelFile.Save();
            }
        }
        static DataTable ConvertToDataTable<T>(List<T> models)
        {
            // creating a data table instance and typed it as our incoming model   
            // as I make it generic, if you want, you can make it the model typed you want.  
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties of that model  
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Loop through all the properties              
            // Adding Column name to our datatable  
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names    
                dataTable.Columns.Add(prop.Name);
            }
            // Adding Row and its value to our dataTable  
            foreach (T item in models)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows    
                    values[i] = Props[i].GetValue(item, null);
                }
                // Finally add value to datatable    
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static void GenerateExcel(DataTable dataTable, string path)
        {

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            // create a excel app along side with workbook and worksheet and give a name to it  
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook excelWorkBook = excelApp.Workbooks.Add();
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheet = excelWorkBook.Sheets[1];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;
            foreach (DataTable table in dataSet.Tables)
            {
                //Add a new worksheet to workbook with the Datatable name  
                Microsoft.Office.Interop.Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                excelWorkSheet.Name = table.TableName;

                // add all the columns  
                for (int i = 1; i < table.Columns.Count + 1; i++)
                {
                    excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
                }

                // add all the rows  
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    for (int k = 0; k < table.Columns.Count; k++)
                    {
                        excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                    }
                }
            }
            excelWorkBook.SaveAs(path); // -> this will do the custom  
            excelWorkBook.Close();
            excelApp.Quit();
        }
    }
    public class HGSBankList
    {
        public string hgsNumber { get; set; }
        public string plateNumber { get; set; }
        public DateTime? entryDate { get; set; }
        public DateTime? exitDate { get; set; }
        public string entryLocation { get; set; }
        public string exitLocation { get; set; }
        public decimal amount { get; set; }
        public string contractNumber { get; set; }
    }
}
