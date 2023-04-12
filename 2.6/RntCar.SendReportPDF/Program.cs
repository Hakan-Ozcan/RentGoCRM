using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace RntCar.SendReportPDF
{
    class Program
    {
        static Document document = new Document();

        static DateTime DateTimeYesterday = DateTime.Now.AddDays(-1);
        public static string DocumentPageSavePath;

        public static int DocumentPageFormat;
        public static int DocumentPageOrientation;
        public static double DocumentPageTopMargin;
        public static double DocumentPageBottomMargin;
        public static double DocumentPageRightMargin;
        public static double DocumentPageLeftMargin;
        public static string DocumentPageHeaderDistance;

        public static string DocumentPageHeader;
        public static string DocumentPageHeaderFontName;
        public static string DocumentPageHeaderFontSize;

        public static double TableRowHeight;
        public static double TableRowTopPadding;

        public static double TableBordersBottomWidth;
        public static int TableBordersLeftIndent;

        public static double TableHeaderFontSize;
        public static string TableHeaderFontName;

        public static double TableRowFontSize;
        public static string TableRowFontName;

        public static double TableRowFontBoldSize;
        public static string TableRowFontBoldName;

        public static void SetParametersFromConfig()
        {
            DocumentPageSavePath = ConfigurationManager.AppSettings.Get("DocumentPageSavePath");

            DocumentPageFormat = int.Parse(ConfigurationManager.AppSettings.Get("DocumentPageFormat"));
            DocumentPageOrientation = int.Parse(ConfigurationManager.AppSettings.Get("DocumentPageOrientation"));
            DocumentPageTopMargin = Convert.ToDouble(ConfigurationManager.AppSettings.Get("DocumentPageTopMargin"));
            DocumentPageBottomMargin = Convert.ToDouble(ConfigurationManager.AppSettings.Get("DocumentPageBottomMargin"));
            DocumentPageRightMargin = Convert.ToDouble(ConfigurationManager.AppSettings.Get("DocumentPageRightMargin"));
            DocumentPageLeftMargin = Convert.ToDouble(ConfigurationManager.AppSettings.Get("DocumentPageLeftMargin"));
            DocumentPageHeaderDistance = ConfigurationManager.AppSettings.Get("DocumentPageHeaderDistance");

            DocumentPageHeader = ConfigurationManager.AppSettings.Get("DocumentPageHeader");
            DocumentPageHeaderFontName = ConfigurationManager.AppSettings.Get("DocumentPageHeaderFontName");
            DocumentPageHeaderFontSize = ConfigurationManager.AppSettings.Get("DocumentPageHeaderFontSize");

            TableRowHeight = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TableRowHeight"));
            TableRowTopPadding = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TableRowTopPadding"));

            TableBordersBottomWidth = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TableBordersBottomWidth"));
            TableBordersLeftIndent = int.Parse(ConfigurationManager.AppSettings.Get("TableBordersLeftIndent"));

            TableHeaderFontSize = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TableHeaderFontSize"));
            TableHeaderFontName = ConfigurationManager.AppSettings.Get("TableHeaderFontName");

            TableRowFontSize = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TableRowFontSize"));
            TableRowFontName = ConfigurationManager.AppSettings.Get("TableRowFontName");

            TableRowFontBoldSize = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TableRowFontBoldSize"));
            TableRowFontBoldName = ConfigurationManager.AppSettings.Get("TableRowFontBoldName");



            document.DefaultPageSetup.PageFormat = (PageFormat)DocumentPageFormat;
            document.DefaultPageSetup.Orientation = (MigraDoc.DocumentObjectModel.Orientation)DocumentPageOrientation;
            document.DefaultPageSetup.HeaderDistance = DocumentPageHeaderDistance;
            document.DefaultPageSetup.RightMargin = DocumentPageRightMargin;
            document.DefaultPageSetup.LeftMargin = DocumentPageLeftMargin;
            document.DefaultPageSetup.TopMargin = DocumentPageTopMargin;
            document.DefaultPageSetup.BottomMargin = DocumentPageBottomMargin;



        }
        public static Table createTable(EquipmentAvailabilityResponse listOfValues)
        {

            Table table = new Table();
            table.Style = "Table";
            table.Borders.Bottom.Width = TableBordersBottomWidth;
            table.Borders.Bottom.Color = Color.FromRgb(211, 211, 211);
            table.Rows.LeftIndent = TableBordersLeftIndent;

            Column tableColumn;
            int TotalWidth = (int)document.DefaultPageSetup.PageHeight - (int)document.DefaultPageSetup.LeftMargin - (int)document.DefaultPageSetup.RightMargin;
            int sectionWidth = (TotalWidth) / 21;


            var test = listOfValues.EquipmentAvailabilityDatas.Where(x => x.CurrentBranchId == Guid.Empty).ToList();

            for (int i = 0; i < 3; i++)
            {
                EquipmentAvailabilityData TotalRow = new EquipmentAvailabilityData()
                {
                    CurrentBranch = i == 0 ? "KURUMSAL TOPLAM" : (i == 1 ? "BAYİ TOPLAM" : "GENELTOPLAM"),
                    Total = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && !(x.CurrentBranchId != Guid.Empty))).Sum(x => x.Total),
                    RentalCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && !(x.CurrentBranchId != Guid.Empty))).Sum(x => x.RentalCount),
                    AvailableCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.AvailableCount),
                    InServiceCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.InServiceCount),
                    InTransferCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.InTransferCount),
                    LostStolenCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.LostStolenCount),
                    FirstTransferCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.FirstTransferCount),
                    DailyReservationCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.DailyReservationCount),
                    RemainingReservationCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.RemainingReservationCount),
                    OutgoingContractCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.OutgoingContractCount),
                    RentalContractCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.RentalContractCount),
                    FurtherReservationCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.FurtherReservationCount),
                    ReachedRevenue = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.ReachedRevenue),
                    DailyRevenue = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.DailyRevenue),
                    MissingInventoriesCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.MissingInventoriesCount),
                    SecondHandTransferWaitingConfirmationCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.SecondHandTransferWaitingConfirmationCount),
                    WaitingForMaintenanceCount = listOfValues.EquipmentAvailabilityDatas.Where(x => (i == 0 && !x.IsFranchise) || (i == 1 && x.IsFranchise) || (i == 2 && x.CurrentBranchId != Guid.Empty)).Sum(x => x.WaitingForMaintenanceCount)
                };
                var ListIndex = 0;
                if (i == 0) // KURUMSAL TOPLAM 
                    ListIndex = listOfValues.EquipmentAvailabilityDatas.FindIndex(x => x.IsFranchise);
                else
                    ListIndex = listOfValues.EquipmentAvailabilityDatas.Count();

                listOfValues.EquipmentAvailabilityDatas.Insert(ListIndex, TotalRow);
            }

            for (int i = 0; i < 18; i++)
            {
                tableColumn = new Column();
                if (i == 0)
                {
                    tableColumn = table.AddColumn(sectionWidth * 4);
                    tableColumn.Format.Alignment = ParagraphAlignment.Left;
                }
                else
                {
                    tableColumn = table.AddColumn(sectionWidth);
                    tableColumn.Format.Alignment = ParagraphAlignment.Center;
                }
            }

            Row row = table.AddRow();
            row.Format.Font.Size = 6;

            var headerCell = new Cell();
            row.HeadingFormat = true;
            headerCell = row.Cells[0];
            headerCell.AddParagraph().AddFormattedText("Şube", TextFormat.Bold);
            headerCell = row.Cells[1];
            headerCell.AddParagraph().AddFormattedText("Aktif Filo", TextFormat.Bold);
            headerCell = row.Cells[2];
            headerCell.AddParagraph().AddFormattedText("Kirada", TextFormat.Bold);
            headerCell = row.Cells[3];
            headerCell.AddParagraph().AddFormattedText("Uygun", TextFormat.Bold);
            headerCell = row.Cells[4];
            headerCell.AddParagraph().AddFormattedText("Servisteki Araçlar", TextFormat.Bold);
            headerCell = row.Cells[5];
            headerCell.AddParagraph().AddFormattedText("Transfer", TextFormat.Bold);
            headerCell = row.Cells[6];
            headerCell.AddParagraph().AddFormattedText("Çalıntı", TextFormat.Bold);
            headerCell = row.Cells[7];
            headerCell.AddParagraph().AddFormattedText("İlk Transfer", TextFormat.Bold);
            headerCell = row.Cells[8];
            headerCell.AddParagraph().AddFormattedText("Toplam Filo", TextFormat.Bold);
            headerCell = row.Cells[9];
            headerCell.AddParagraph().AddFormattedText("Doluluk", TextFormat.Bold);
            headerCell = row.Cells[10];
            headerCell.AddParagraph().AddFormattedText("Gün. Rez.", TextFormat.Bold);
            headerCell = row.Cells[11];
            headerCell.AddParagraph().AddFormattedText("Kalan Rez", TextFormat.Bold);
            headerCell = row.Cells[12];
            headerCell.AddParagraph().AddFormattedText("Gün. Çıkış", TextFormat.Bold);
            headerCell = row.Cells[13];
            headerCell.AddParagraph().AddFormattedText("Gün. Dönüş", TextFormat.Bold);
            headerCell = row.Cells[14];
            headerCell.AddParagraph().AddFormattedText("İleri Rez.", TextFormat.Bold);
            headerCell = row.Cells[15];
            headerCell.AddParagraph().AddFormattedText("Ciro", TextFormat.Bold);
            headerCell = row.Cells[16];
            headerCell.AddParagraph().AddFormattedText("Günlük Ciro", TextFormat.Bold);
            headerCell = row.Cells[17];
            headerCell.AddParagraph().AddFormattedText("Araç Başı Gelir", TextFormat.Bold);

            Font font = new Font();
            font.Bold = false;
            font.Name = TableRowFontName;
            font.Size = TableRowFontSize;

            Font fontBold = new Font();
            fontBold.Bold = true;
            fontBold.Name = TableRowFontBoldName;
            fontBold.Size = TableRowFontBoldSize;

            Font tempFont = new Font();
            //create rows from list of values
            foreach (var value in listOfValues.EquipmentAvailabilityDatas)
            {
                Row tableRow = table.AddRow();
                tableRow.Height = TableRowHeight;
                tableRow.TopPadding = TableRowTopPadding;
                tableRow.Cells[0].AddParagraph().AddFormattedText(value.CurrentBranch, fontBold);

                tempFont = value.CurrentBranchId == Guid.Empty ? fontBold : font;
                tableRow.Cells[1].AddParagraph().AddFormattedText((value.RentalCount + value.AvailableCount + value.InServiceCount + value.WaitingForMaintenanceCount + value.InTransferCount + value.MissingInventoriesCount + value.SecondHandTransferWaitingConfirmationCount).ToString(), tempFont);
                tableRow.Cells[2].AddParagraph().AddFormattedText(value.RentalCount.ToString(), tempFont);
                tableRow.Cells[3].AddParagraph().AddFormattedText((value.AvailableCount + value.WaitingForMaintenanceCount).ToString(), tempFont);
                tableRow.Cells[4].AddParagraph().AddFormattedText(value.InServiceCount.ToString(), tempFont);
                tableRow.Cells[5].AddParagraph().AddFormattedText((value.InTransferCount + value.MissingInventoriesCount + value.SecondHandTransferWaitingConfirmationCount).ToString(), tempFont);
                tableRow.Cells[6].AddParagraph().AddFormattedText(value.LostStolenCount.ToString(), tempFont);
                tableRow.Cells[7].AddParagraph().AddFormattedText(value.FirstTransferCount.ToString(), tempFont);
                tableRow.Cells[8].AddParagraph().AddFormattedText(value.Total.ToString(), tempFont);
                tableRow.Cells[9].AddParagraph().AddFormattedText(value.Availability.ToString() + "%", tempFont);
                tableRow.Cells[10].AddParagraph().AddFormattedText(value.DailyReservationCount.ToString(), tempFont);
                tableRow.Cells[11].AddParagraph().AddFormattedText(value.RemainingReservationCount.ToString(), tempFont);
                tableRow.Cells[12].AddParagraph().AddFormattedText(value.OutgoingContractCount.ToString(), tempFont);
                tableRow.Cells[13].AddParagraph().AddFormattedText(value.RentalContractCount.ToString(), tempFont);
                tableRow.Cells[14].AddParagraph().AddFormattedText(value.FurtherReservationCount.ToString(), tempFont);
                tableRow.Cells[15].AddParagraph().AddFormattedText(Math.Round(value.ReachedRevenue.Value, 0).ToString("N0", CultureInfo.InvariantCulture), tempFont);
                tableRow.Cells[16].AddParagraph().AddFormattedText(Math.Round(value.DailyRevenue.Value, 0).ToString("N0", CultureInfo.InvariantCulture), tempFont);
                tableRow.Cells[17].AddParagraph().AddFormattedText(Math.Round((Convert.ToDouble(value.ReachedRevenue) / value.Total), 0).ToString("N0", CultureInfo.InvariantCulture), tempFont);

            }
            return table;
        }
        public static void generatePDF(List<Table> listOfTables)
        {
            string pageHeaderText = DocumentPageHeader;
            Style style = document.Styles["Normal"];
            style.Font.Name = DocumentPageHeaderFontName;
            style.Font.Size = DocumentPageHeaderFontSize;

            Section page = document.AddSection();
            //header
            Paragraph header = page.Headers.Primary.AddParagraph();
            header.AddText(pageHeaderText);
            header.Format.Alignment = ParagraphAlignment.Center;

            page.PageSetup.DifferentFirstPageHeaderFooter = false;
            //footer
            //Paragraph footer = page.Footers.Primary.AddParagraph();
            //footer.AddText(pageFooterText);
            //footer.Format.Alignment = ParagraphAlignment.Center;


            foreach (var table in listOfTables)
            {
                page.Add(table);
            }

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            DocumentPageSavePath = DocumentPageSavePath + @"\" + DateTimeYesterday.ToString("dd-MM-yyyy_") + DocumentPageHeader + ".pdf";
            pdfRenderer.PdfDocument.Save(@DocumentPageSavePath);
        }
        public static CrmServiceHelper crmServiceHelper;

        static void Main(string[] args)
        {
            SetParametersFromConfig();
            string pdfName = $"{DateTimeYesterday.ToString("dd-MM-yyyy")}_Rentuna Info Raporu.pdf";
            string pdfFilePath = DocumentPageSavePath + pdfName;

            crmServiceHelper = new CrmServiceHelper();

            OrganizationRequest request = new OrganizationRequest("rnt_Report_GetEquipmentAvailabilityReport");
            request["publishDate"] = DateTime.Now.ToString();
            request["startDate"] = DateTimeYesterday;
            request["endDate"] = DateTimeYesterday;
            
            var response = crmServiceHelper.IOrganizationService.Execute(request);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            var Items = jss.Deserialize<EquipmentAvailabilityResponse>(response.Results["serviceResponse"].ToString()); 

            var table = createTable(Items);
            var tables = new List<Table>();
            tables.Add(table);
            generatePDF(tables);


            Guid teamId = new Guid("1421F3C1-3C67-ED11-9561-6045BD9059D9");
            QueryExpression userQuery = new QueryExpression("systemuser");
            userQuery.ColumnSet = new ColumnSet(true);
            LinkEntity teamLink = new LinkEntity("systemuser", "teammembership", "systemuserid", "systemuserid", JoinOperator.Inner);
            ConditionExpression teamCondition = new ConditionExpression("teamid", ConditionOperator.Equal, teamId);
            teamLink.LinkCriteria.AddCondition(teamCondition);
            userQuery.LinkEntities.Add(teamLink);
            EntityCollection retrievedUsers = crmServiceHelper.IOrganizationService.RetrieveMultiple(userQuery);
            EntityCollection toActivityPartyList = new EntityCollection();
            foreach (Entity user in retrievedUsers.Entities)
            {
                Entity toActivityParty = new Entity("activityparty");
                toActivityParty["partyid"] = new EntityReference("systemuser", user.Id);
                toActivityPartyList.Entities.Add(toActivityParty);
            }
            var array = toActivityPartyList.Entities.ToArray();
             

            ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
            Entity fromActivityParty = new Entity("activityparty"); 

            fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));
 
            
            Entity email = new Entity("email");
            email["from"] = new Entity[] { fromActivityParty };
            email["to"] = array;
            //email["regardingobjectid"] = new EntityReference("rnt_contract", contract.Id);
            email["subject"] = $"Info Raporu({DateTimeYesterday.ToString("dd-MM-yyyy")})";
            email["description"] = "";//result;
            email["directioncode"] = true;
            Guid emailId = crmServiceHelper.IOrganizationService.Create(email);
            
            //e-mail attachment ekleme 
            byte[] bytes = File.ReadAllBytes(pdfFilePath); 
            File.WriteAllBytes(pdfFilePath, bytes);

            Entity linkedAttachment = new Entity("activitymimeattachment");
            linkedAttachment.Attributes["objectid"] = new EntityReference("email", emailId);
            linkedAttachment.Attributes["objecttypecode"] = "email";
            linkedAttachment.Attributes["filename"] = pdfName;
            linkedAttachment.Attributes["mimetype"] = "application/pdf";
            linkedAttachment.Attributes["body"] = Convert.ToBase64String(bytes);
            crmServiceHelper.IOrganizationService.Create(linkedAttachment);

            SendEmailRequest sendEmailRequest = new SendEmailRequest
            {
                EmailId = emailId,
                TrackingToken = "",
                IssueSend = true
            };

            SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
        }
    }
}
