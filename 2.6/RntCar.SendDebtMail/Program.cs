using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using OfficeOpenXml;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RntCar.SendDebtMail
{
    class Program
    {
        static void Main(string[] args)
        {
            // Microsoft.Office.Interop.Excel.Workbook WBook = null;
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                #region commented
                //var exportToExcelRequest = new OrganizationRequest("ExportToExcel");
                //exportToExcelRequest.Parameters = new ParameterCollection();

                //exportToExcelRequest.Parameters.Add(new KeyValuePair<string, object>("View",
                //   new EntityReference("userquery", new Guid("b89a44a5-b15c-ea11-a811-000d3a2c0643"))));
                //exportToExcelRequest.Parameters.Add(new KeyValuePair<string, object>("FetchXml", @"
                //                                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                //                                      <entity name='rnt_contract'>
                //                                         <attribute name='rnt_name' />
                //                                         <attribute name='rnt_pickupbranchid' />
                //                                         <attribute name='rnt_dropoffbranchid' />
                //                                         <attribute name='rnt_totalamount' />
                //                                         <attribute name='rnt_netpayment' />
                //                                        <filter type='and'>
                //                                          <condition attribute='statuscode' operator='eq' value='100000001' />
                //                                          <condition attribute='rnt_dropoffdatetime' operator='yesterday' />
                //                                        </filter>
                //                                      </entity>
                //                                    </fetch>"));
                //exportToExcelRequest.Parameters.Add(new KeyValuePair<string, object>("LayoutXml", @"
                //                                    <grid name='resultset' object='2' jump='rnt_name' select='1' icon='1' preview='1'>
                //                                        <row name='result' id='rnt_contractid'>
                //                                            <cell name='rnt_name' width='300' displayName='huhu' />
                //                                            <cell name='rnt_pickupbranchid' width='300' />
                //                                            <cell name='rnt_dropoffbranchid' width='300' />
                //                                            <cell name='rnt_totalamount' width='300' />
                //                                            <cell name='rnt_netpayment' width='300' />
                //                                        </row>
                //                                    </grid>"));

                //exportToExcelRequest.Parameters.Add(new KeyValuePair<string, object>("QueryApi", ""));
                //exportToExcelRequest.Parameters.Add(new KeyValuePair<string, object>("QueryParameters",
                //    new InputArgumentCollection()));
                //var exportToExcelResponse = crmServiceHelper.IOrganizationService.Execute(exportToExcelRequest);
                //var fileName = string.Format("Borçlu Sözleşmeler{0}.xlsx", DateTime.Today.AddDays(-1).ToString("ddMMyyyy"));
                //if (exportToExcelResponse.Results.Any())
                //{
                //    File.WriteAllBytes(fileName, exportToExcelResponse.Results["ExcelFile"] as byte[]);

                //}
                #region Excel Operations
                //try
                //{
                //    Application ExcelObj = new Application();

                //    var d = AppDomain.CurrentDomain.BaseDirectory;
                //    WBook = ExcelObj.Workbooks.Open(d + fileName);
                //    Worksheet wSheet = (Worksheet)WBook.Sheets[1];
                //    Range usedRange = wSheet.UsedRange;
                //    int counter = 0;
                //    foreach (Range row in usedRange.Rows)
                //    {
                //        #region  Column Turkish
                //        if (counter == 0)
                //        {
                //            string[] rowData = new String[row.Columns.Count];
                //            for (int i = 0; i < row.Columns.Count; i++)
                //            {
                //                var value = Convert.ToString(row.Cells[1, i + 1].Value2);
                //                if (value == "Name")
                //                {
                //                    wSheet.Cells[1, i + 1].Value2 = "Sözleşme Numarası";
                //                }
                //                else if (value == "Pickup Branch")
                //                {
                //                    wSheet.Cells[1, i + 1].Value2 = "Alış Ofisi";
                //                }
                //                else if (value == "Dropoff Branch")
                //                {
                //                    wSheet.Cells[1, i + 1].Value2 = "İade Ofisi";
                //                }
                //                else if (value == "Total Amount")
                //                {
                //                    wSheet.Cells[1, i + 1].Value2 = "Toplam Tutar";
                //                }
                //                else if (value == "Net Payment")
                //                {
                //                    wSheet.Cells[1, i + 1].Value2 = "Ödenen";
                //                    wSheet.Cells[1, i + 2].Value2 = "Borç Tutarı";
                //                }
                //            }

                //        }
                //        #endregion
                //        else if (counter != 0)
                //        {
                //            for (int i = 0; i < row.Columns.Count; i++)
                //            {
                //                //means last column
                //                if (i == 7)
                //                {
                //                    wSheet.Cells[counter + 1, 9].Formula = string.Format("=SUM(G{0},-H{0}", counter + 1);
                //                }
                //            }
                //        }
                //        counter++;
                //    }

                //    WBook.Save();
                //}
                //catch (Exception ex)
                //{

                //    throw;
                //}
                //finally
                //{
                //    WBook.Close();
                //}
                #endregion
                #endregion

                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contracts = contractRepository.getYesterdayCompletedContracts();
                string fileName = string.Format("\\dosya\\borc{0}.xlsx", DateTime.Now.AddDays(-1).ToString("ddMMyyyy"));
                var grouped = contracts.OrderBy(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name)
                                       .GroupBy(u => u.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id)
                                       .Select(grp => grp.ToList())
                                       .ToList();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("Borç Raporu");
                    var excelWorksheet = excel.Workbook.Worksheets["Borç Raporu"];
                    List<string[]> headerRow = new List<string[]>()
                    {
                          new string[] { "OFİS",
                                         "KAPANAN SÖZLEŞME SAYISI",
                                         "BORÇLU SÖZLEŞME SAYISI",
                                         "BORÇLU SÖZLEŞME ORANI",
                                         "TOPLAM SÖZLEŞME TUTARI",
                                         "TOPLAM TEMİNAT TUTARI",
                                         "TOPLAM KREDİ KARTLI TAHSİLAT TUTARI",
                                         "CARİ KURUMSAL SÖZLEŞME TUTARI",
                                         "CARİ ACENTA SÖZLEŞME TUTARI",
                                         "CARİ BROKER SÖZLEŞME TUTARI",
                                         "BORÇ TUTARI",
                                         "BORÇ TUTARI ORANI"}
                                        };
                    string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";
                    excelWorksheet.Cells[headerRange].LoadFromArrays(headerRow);
                    excelWorksheet.Cells[headerRange].Style.Font.Bold = true;
                    excelWorksheet.Cells[headerRange].Style.Font.Size = 11;
                    excelWorksheet.Cells[headerRange].AutoFitColumns();

                    var cellData = new List<object[]>();
                    foreach (var item in grouped)
                    {
                        var branch = item.FirstOrDefault().GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name;
                        var allClosedContracts = item.Count;
                        //rnt_totalamount
                        //rnt_netpayment
                        var debst = item.Where(p => p.GetAttributeValue<Money>("rnt_totalamount").Value > (p.GetAttributeValue<Money>("rnt_netpayment").Value)).ToList();
                        var ratio = decimal.Round(Convert.ToDecimal(debst.Count * 100) / Convert.ToDecimal(allClosedContracts), 2);
                        var generalTotalAmount = item.Sum(p => p.GetAttributeValue<Money>("rnt_generaltotalamount").Value);
                        var totalAmount = item.Sum(p => p.GetAttributeValue<Money>("rnt_totalamount").Value);
                        var creditCardAmount = item.Sum(p => p.GetAttributeValue<Money>("rnt_netpayment").Value);
                        var corporateTotalAmount = item.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Kurumsal)
                                                       .Sum(p => p.GetAttributeValue<Money>("rnt_corporatetotalamount").Value);
                        var brokerTotalAmount = item.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Broker)
                                                      .Sum(p => p.GetAttributeValue<Money>("rnt_corporatetotalamount").Value);
                        var agencyTotalAmount = item.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Acente)
                                                      .Sum(p => p.GetAttributeValue<Money>("rnt_corporatetotalamount").Value);

                        var totalDepositAmount = item.Where(p => p.GetAttributeValue<Money>("rnt_totalamount").Value <= p.GetAttributeValue<Money>("rnt_netpayment").Value)
                                                     .Sum(p => p.GetAttributeValue<Money>("rnt_netpayment").Value - p.GetAttributeValue<Money>("rnt_totalamount").Value);
                        //var debtAmount = generalTotalAmount - creditCardAmount - corporateTotalAmount - brokerTotalAmount - agencyTotalAmount - totalDepositAmount;
                        var debtAmount = creditCardAmount - totalDepositAmount - generalTotalAmount + corporateTotalAmount + brokerTotalAmount + agencyTotalAmount;
                        var debtRatio = decimal.Round(Convert.ToDecimal(debtAmount * 100) / Convert.ToDecimal(generalTotalAmount), 2);
                        cellData.Add(new object[] { branch,
                                                    allClosedContracts,
                                                    debst.Count,
                                                    "%" + ratio,
                                                    generalTotalAmount,
                                                    totalDepositAmount,//5
                                                    creditCardAmount,
                                                    corporateTotalAmount,
                                                    agencyTotalAmount,//8
                                                    brokerTotalAmount,//9
                                                    debtAmount,
                                                    "%" + debtRatio});
                    }
                    cellData.Add(new object[] { "TOPLAM",
                                                 cellData.Sum(p => Convert.ToDecimal(p[1])),
                                                 cellData.Sum(p => Convert.ToDecimal(p[2])),
                                                 "",
                                                 cellData.Sum(p => Convert.ToDecimal(p[4])),
                                                 cellData.Sum(p => Convert.ToDecimal(p[5])),
                                                 cellData.Sum(p => Convert.ToDecimal(p[6])),
                                                 cellData.Sum(p => Convert.ToDecimal(p[7])),
                                                 cellData.Sum(p => Convert.ToDecimal(p[8])),
                                                 cellData.Sum(p => Convert.ToDecimal(p[9])),
                                                 cellData.Sum(p => Convert.ToDecimal(p[10])),
                                                 "" });


                    excelWorksheet.Cells[2, 1].LoadFromArrays(cellData);

                    excel.Workbook.Worksheets.Add("Borç Raporu Detayı");
                    var excelWorksheetDetail = excel.Workbook.Worksheets["Borç Raporu Detayı"];
                    List<string[]> headerRowDetail = new List<string[]>()
                    {
                          new string[] { "SÖZLEŞME NO",
                                         "ALIŞ OFİSİ",
                                         "İADE OFİSİ",
                                         "ALIŞ TARİHİ",
                                         "İADE TARİHİ",
                                         "PLAKA",
                                         "GRUP KODU",
                                         "FİYATLANDIRMA GRUP KODU",
                                         "MÜŞTERİ",
                                         "KURUM",
                                         "BORÇ",
                                         "CRM LİNKİ"
                          }
                    };
                    string headerRangeDetail = "A1:" + Char.ConvertFromUtf32(headerRowDetail[0].Length + 64) + "1";
                    excelWorksheetDetail.Cells[headerRangeDetail].LoadFromArrays(headerRowDetail);
                    excelWorksheetDetail.Cells[headerRangeDetail].Style.Font.Bold = true;
                    excelWorksheetDetail.Cells[headerRangeDetail].Style.Font.Size = 11;
                    excelWorksheetDetail.Cells[headerRangeDetail].AutoFitColumns();
                    var cellDataDetail = new List<object[]>();
                    foreach (var item in contracts.OrderBy(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name))
                    {
                        cellDataDetail.Add(new object[] {
                                            item.GetAttributeValue<string>("rnt_name"),
                                            item.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name,
                                            item.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Name,
                                            item.GetAttributeValue<DateTime>("rnt_pickupdatetime").ToString("dd/MM/yyyy"),
                                            item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy"),
                                            item.GetAttributeValue<EntityReference>("rnt_equipmentid").Name,
                                            item.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name,
                                            item.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Name,
                                            item.Contains("rnt_customerid") ?  item.GetAttributeValue<EntityReference>("rnt_customerid").Name : string.Empty,
                                            item.Contains("rnt_corporateid") ?  item.GetAttributeValue<EntityReference>("rnt_corporateid").Name : string.Empty,
                                            item.GetAttributeValue<Money>("rnt_totalamount").Value < item.GetAttributeValue<Money>("rnt_netpayment").Value ?
                                            0  :
                                            item.GetAttributeValue<Money>("rnt_totalamount").Value - item.GetAttributeValue<Money>("rnt_netpayment").Value
                                        });
                    }
                    excelWorksheetDetail.Cells[2, 1].LoadFromArrays(cellDataDetail);
                    //ExcelWorksheet workSheet = excel.Workbook.Worksheets[1];

                    //var start = workSheet.Dimension.Start;
                    //var end = workSheet.Dimension.End;
                    //for (int row = start.Row + 1; row <= end.Row; row++)
                    //{
                    //    workSheet.Cells[row, end.Column].Hyperlink = new Uri(workSheet.Cells[row, end.Column].Text);
                    //    workSheet.Cells[row, end.Column].Value = "CRM Linki";
                    //}                 
                    FileInfo excelFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + fileName);
                    excel.SaveAs(excelFile);

                }

                #region Mail Operations
                //sending mail
                Entity fromActivityParty = new Entity("activityparty");
                fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(StaticHelper.GetConfiguration("adminId")));

                var toEmails = StaticHelper.GetConfiguration("to").Split(';');
                List<Entity> toActivityParty = new List<Entity>();
                foreach (var item in toEmails)
                {
                    Entity e = new Entity("activityparty");
                    e["partyid"] = new EntityReference("contact", new Guid(item));
                    toActivityParty.Add(e);
                }

                //Entity e1 = new Entity("activityparty");
                //e1["partyid"] = new EntityReference("account", new Guid("86d638ff-3113-4943-8f56-26f351adf44b"));                
                //toActivityParty.Add(e1);

                Entity e2 = new Entity("activityparty");
                e2["partyid"] = new EntityReference("account", new Guid("a067fca8-f361-ea11-a811-000d3a2d0476"));
                toActivityParty.Add(e2);

                var ccEmails = StaticHelper.GetConfiguration("cc").Split(';');
                List<Entity> ccActivityParty = new List<Entity>();
                foreach (var item in ccEmails)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        Entity e = new Entity("activityparty");
                        e["partyid"] = new EntityReference("contact", new Guid(item));
                        ccActivityParty.Add(e);
                    }

                }
                //create email

                Entity email = new Entity("email");
                email["from"] = new Entity[] { fromActivityParty };
                email["to"] = toActivityParty.ToArray();
                if (ccActivityParty.Count > 0)
                {
                    email["cc"] = ccActivityParty.ToArray();
                }
                email["subject"] = "Borçlu sözleşmeler - " + DateTime.Today.AddDays(-1).ToString("ddMMyyyy");
                email["description"] = "Borçlu Sözleşmeler ektedir.";
                email["directioncode"] = true;
                Guid emailId = crmServiceHelper.IOrganizationService.Create(email);

                //Create email attachment

                Entity _EmailAttachment = new Entity("activitymimeattachment");
                _EmailAttachment["subject"] = "Borçlu Sözleşmeler";
                _EmailAttachment["objectid"] = new EntityReference("email", emailId);
                _EmailAttachment["objecttypecode"] = "email";
                _EmailAttachment["filename"] = fileName;
                _EmailAttachment["body"] = Convert.ToBase64String(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + fileName));
                _EmailAttachment["mimetype"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                crmServiceHelper.IOrganizationService.Create(_EmailAttachment);

                SendEmailRequest SendEmail = new SendEmailRequest();
                SendEmail.EmailId = emailId;
                SendEmail.TrackingToken = "";
                SendEmail.IssueSend = true;
                SendEmailResponse res = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(SendEmail);
                #endregion
            }
            catch (Exception ex)
            {

            }

        }
    }
}
