using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.IntegrationHelper;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.HGSFTPIntegration
{
    class Program
    {
        private static CrmServiceHelper crmServiceHelper;
        private static string configValue;

        private static ConfigurationBL configurationBL;
        private static HGSFTPBL hgsftpBL;
        private static HGSTransitListBL hgsTransitListBL;
        private static ContractItemBL contractBL;
        private static EquipmentBL equipmentBL;

        private static LoggerHelper loggerHelper;
        private static string processStartDate;
        private static string processEndDate;
        private static string processDate;
        private static string[] serverInfoList;
        private static string[] loginInfoList;
        private static string[] titleInfoList;
        private static string[] pathInfoList;
        private static string fileprefix;
        private static char delimeter;
        private static int recordCount;
        private static bool isFtp = true;

        private static List<FtpConsensusDetail> ftpConsensusDetailList;
        private static EntityCollection equipmentList;

        static void Main(string[] args)
        {
            configValue = StaticHelper.GetConfiguration("configValue");
            processStartDate = StaticHelper.GetConfiguration("processStartDate");
            processEndDate = StaticHelper.GetConfiguration("processEndDate");
            string isFtpString = StaticHelper.GetConfiguration("isFtp");
            string processModeString = StaticHelper.GetConfiguration("processMode");

            crmServiceHelper = new CrmServiceHelper();
            if (string.IsNullOrWhiteSpace(processModeString) || processModeString == "FTP")
            {
                if (!string.IsNullOrWhiteSpace(isFtpString) && !Convert.ToBoolean(isFtpString))
                {
                    isFtp = false;
                }
                if (string.IsNullOrWhiteSpace(processStartDate))
                {
                    processStartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    processEndDate = processStartDate;
                }
                configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                loggerHelper = new LoggerHelper();
                FillCRMConfig();
                hgsftpBL = new HGSFTPBL(crmServiceHelper.IOrganizationService);
                hgsTransitListBL = new HGSTransitListBL(crmServiceHelper.IOrganizationService);
                contractBL = new ContractItemBL(crmServiceHelper.IOrganizationService);
                equipmentBL = new EquipmentBL(crmServiceHelper.IOrganizationService);

                equipmentList = equipmentBL.getEquipmentWithHGSLabel();

                while (Convert.ToDateTime(processStartDate) <= Convert.ToDateTime(processEndDate))
                {
                    ftpConsensusDetailList = new List<FtpConsensusDetail>();
                    processDate = processStartDate.Replace("-", "");
                    DataTable dtCSV = new DataTable();
                    if (isFtp)
                    {
                        MoveFTPFileLocalPath();
                    }
                    dtCSV = ReadYKBFTPFileData();

                    HGSFtpProcess(dtCSV);
                }
            }

            if (string.IsNullOrWhiteSpace(processModeString) || processModeString == "PAYMENT")
            {
                HGSHelper hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService);
                hgsHelper.MakePaymentHGS();
            }
            if (string.IsNullOrWhiteSpace(processModeString) || processModeString == "INVOICE")
            {
                HGSHelper hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService);
                hgsHelper.CreateInvoiceForHGS();
            }
        }



        private static void FillCRMConfig()
        {
            string serverInfo = configurationBL.GetConfigurationByName(configValue + "sftpServer");
            serverInfoList = serverInfo.Split(';');
            string loginInfo = configurationBL.GetConfigurationByName(configValue + "sftpLogin");
            loginInfoList = loginInfo.Split(';');
            string pathInfo = configurationBL.GetConfigurationByName(configValue + "sftpPath");
            pathInfoList = pathInfo.Split(';');
            string titleInfo = configurationBL.GetConfigurationByName(configValue + "sftpFileTitle");
            titleInfoList = titleInfo.Split(';');
            fileprefix = configurationBL.GetConfigurationByName(configValue + "sftpFilePrefix");
            delimeter = Convert.ToChar(configurationBL.GetConfigurationByName(configValue + "sftpFileDelimeter"));

        }

        private static string DateTimeFormat(string date, string time)
        {
            date = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2);
            time = time.Substring(0, 2) + ":" + time.Substring(2, 2) + ":" + time.Substring(4, 2);
            return date + " " + time;
        }

        private static string SFTPFileMove(string fileName)
        {
            SFTPObject ftpObject = new SFTPObject();
            ftpObject.sftphostName = serverInfoList[0];
            ftpObject.sftpPort = Convert.ToInt16(serverInfoList[1]);
            ftpObject.userName = loginInfoList[0];
            ftpObject.password = loginInfoList[1];
            ftpObject.sourceFilePath = pathInfoList[0] + fileName;
            ftpObject.targetFilePath = pathInfoList[1] + fileName;

            SFTPHelper ftpHelper = new SFTPHelper();
            ftpHelper.GetFileInFTP(ftpObject);
            return ftpObject.targetFilePath;
        }

        private static DataTable ReadCSV(string filePath)
        {

            DataTable dtCSV = new DataTable();
            foreach (var item in titleInfoList)
            {
                dtCSV.Columns.Add(item);
            }

            string[] lines = System.IO.File.ReadAllLines(filePath, System.Text.UTF8Encoding.Default);

            int rows = lines.Count();
            recordCount = rows;
            int cols = titleInfoList.Count();
            for (int r = 0; r < rows; r++)
            {
                string line = lines[r];
                FtpConsensusDetail ftpConsensusDetail = new FtpConsensusDetail();
                ftpConsensusDetail.rowDetail = line;
                ftpConsensusDetail.fileRow = r;
                try
                {
                    string[] columns = line.Split(delimeter);
                    DataRow dr = dtCSV.NewRow();
                    for (int c = 0; c < cols; c++)
                    {
                        dr[c] = columns[c].Trim();

                    }
                    dtCSV.Rows.Add(dr);

                    ftpConsensusDetail.amount = Convert.ToDecimal(columns[8].Trim());
                    ftpConsensusDetail.transactionId = Convert.ToString(columns[9]);
                    ftpConsensusDetail.indexOf = r;
                    ftpConsensusDetail.isException = false;
                }
                catch (Exception ex)
                {
                    ftpConsensusDetail.exceptionDetail = ex.Message;
                    ftpConsensusDetail.isException = true;
                }
                ftpConsensusDetailList.Add(ftpConsensusDetail);

            }
            return dtCSV;
        }

        private static void MoveFTPFileLocalPath()
        {
            string fileName = fileprefix + processDate + ".CSV";

            string targetFileName = SFTPFileMove(fileName);
        }

        private static DataTable ReadYKBFTPFileData()
        {
            string fileName = fileprefix + processDate + ".CSV";

            string targetFileName = pathInfoList[1] + fileName;

            DataTable dtCSV = ReadCSV(targetFileName);
            return dtCSV;
        }

        private static void HGSFtpProcess(DataTable dtCSV)
        {

            Guid ftpConsensusId = hgsftpBL.CraeteOrUpdateFtpConsensus(recordCount, fileprefix + processDate);

            loggerHelper.traceInfo($"File {fileprefix + processDate}.CSV  Record Count:{dtCSV.Rows.Count}");
            Console.WriteLine($"File {fileprefix + processDate}.CSV  Record Count:{dtCSV.Rows.Count}");
            foreach (DataRow row in dtCSV.Rows)
            {
                int indexOf = dtCSV.Rows.IndexOf(row);
                try
                {
                    EntityReference contractRef = new EntityReference();
                    EntityReference hgsLabelRef = new EntityReference();
                    Guid equipmentId = Guid.Empty;

                    DateTime entryDate = DateTime.MinValue;
                    if (!string.IsNullOrWhiteSpace(Convert.ToString(row[2])))
                    {
                        entryDate = Convert.ToDateTime(DateTimeFormat(Convert.ToString(row[2]), Convert.ToString(row[3])));
                    }

                    DateTime exitDate = Convert.ToDateTime(DateTimeFormat(Convert.ToString(row[5]), Convert.ToString(row[6])));

                    HGSTransitData data = new HGSTransitData();
                    data.hgsNumber = Convert.ToString(row[0]);
                    data.plateNumber = Convert.ToString(row[1]);
                    data.entryLocation = Convert.ToString(row[4]);
                    data.exitLocation = Convert.ToString(row[7]);
                    if (entryDate != DateTime.MinValue)
                    {
                        data._entryDateTime = entryDate;
                    }
                    data._exitDateTime = exitDate;
                    data.amount = Convert.ToDecimal(Convert.ToString((row[8])));

                    var equipment = equipmentList.Entities.Where(x => x.GetAttributeValue<string>("rnt_platenumber") == data.plateNumber).FirstOrDefault();
                    if (equipment != null && equipment.Id != Guid.Empty)
                    {
                        equipmentId = equipment.Id;
                        hgsLabelRef = equipment.GetAttributeValue<EntityReference>("rnt_hgslabelid");
                        contractRef = hgsTransitListBL.GetContractForHGSDate(data._entryDateTime, data._exitDateTime, new EntityReference(equipment.LogicalName, equipment.Id));
                    }

                    EntityCollection hgsTransitList = hgsTransitListBL.getHGSTransitListByEquipmentId(data._exitDateTime, equipmentId, data.amount, Convert.ToString(ftpConsensusId));
                    if (hgsTransitList.Entities.Count == 0)
                    {
                        hgsTransitList = hgsTransitListBL.getHGSTransitListByPlateNumber(data._exitDateTime, data.plateNumber, data.amount, Convert.ToString(ftpConsensusId));
                    }

                    if (hgsTransitList.Entities.Count == 0)
                    {
                        loggerHelper.traceInfo($"Row Index:{indexOf} hgsTransitList.Entities==0");

                        Console.WriteLine($"Row Index:{indexOf} hgsTransitList.Entities==0");
                        hgsTransitListBL.createHGSList(data, ftpConsensusId, indexOf, contractRef.Id, equipmentId, hgsLabelRef.Id);
                    }
                    else if (hgsTransitList.Entities.Count == 1)
                    {
                        loggerHelper.traceInfo($"Row Index:{indexOf} hgsTransitList.Entities.Count==1");
                        Console.WriteLine($"Row Index:{indexOf} hgsTransitList.Entities.Count==1");
                        OptionSetValue statusCode = hgsTransitList.Entities[0].GetAttributeValue<OptionSetValue>("statuscode");
                        hgsTransitListBL.updateHGSListWithData(data, hgsTransitList.Entities[0].Id, ftpConsensusId, indexOf, contractRef.Id, equipmentId, hgsLabelRef.Id, statusCode.Value);
                    }
                    else
                    {
                        List<Entity> checkValueList = hgsTransitList.Entities.Where(x => x.GetAttributeValue<int>("rnt_ftprownumber") == indexOf).ToList();
                        if (checkValueList.Count == 0)
                        {
                            checkValueList = hgsTransitList.Entities.Where(x => x.GetAttributeValue<DateTime>("rnt_exitdatetime").AddSeconds(-1 * x.GetAttributeValue<DateTime>("rnt_exitdatetime").Second) == data._exitDateTime.AddSeconds(-1 * data._exitDateTime.Second)).ToList();
                        }

                        if (checkValueList.Count == 0)
                        {
                            loggerHelper.traceInfo($"Row Index:{indexOf} checkValueList.Count==0");
                            Console.WriteLine($"Row Index:{indexOf} checkValueList.Count==0");
                            hgsTransitListBL.createHGSList(data, ftpConsensusId, indexOf, contractRef.Id, equipmentId, hgsLabelRef.Id);
                        }
                        else if (checkValueList.Count == 1)
                        {
                            loggerHelper.traceInfo($"Row Index:{indexOf} checkValueList.Count==1");
                            Console.WriteLine($"Row Index:{indexOf} checkValueList.Count==1");
                            OptionSetValue statusCode = checkValueList[0].GetAttributeValue<OptionSetValue>("statuscode");
                            hgsTransitListBL.updateHGSListWithData(data, hgsTransitList.Entities[0].Id, ftpConsensusId, indexOf, contractRef.Id, equipmentId, hgsLabelRef.Id, statusCode.Value);
                        }
                        else
                        {
                            loggerHelper.traceInfo($"Row Index:{indexOf} Duplicate Record Process");
                            Console.WriteLine($"Row Index:{indexOf} Duplicate Record Process");
                            int index = ftpConsensusDetailList.FindIndex(x => x.indexOf == indexOf);
                            ftpConsensusDetailList[index].isException = true;
                            string idList = "Duplicate Id List: ";
                            foreach (var checkValue in checkValueList)
                            {
                                idList += Convert.ToString(checkValue.Id) + " : ";
                            }
                            ftpConsensusDetailList[index].exceptionDetail = idList;
                            loggerHelper.traceInfo($"Row Index:{index} Duplicate Record");
                        }
                    }
                    Console.WriteLine($"Row Index:{indexOf} Process");
                }
                catch (Exception ex)
                {
                    int index = ftpConsensusDetailList.FindIndex(x => x.indexOf == indexOf);
                    ftpConsensusDetailList[index].isException = true;
                    ftpConsensusDetailList[index].exceptionDetail = ex.Message;
                    loggerHelper.traceInfo($"Row Index:{index} Ex Message:{ex.Message}");
                }
            }
            List<FtpConsensusDetail> ftpConsensusExceptionDetailList = ftpConsensusDetailList.Where(x => x.isException).ToList();
            foreach (var ftpConsensusExceptionDetail in ftpConsensusExceptionDetailList)
            {
                hgsftpBL.CraeteFtpConsensusDetail(ftpConsensusExceptionDetail, ftpConsensusId);
                loggerHelper.traceInfo($"Row Index:{ftpConsensusExceptionDetail.rowDetail} Ex Detail:{ftpConsensusExceptionDetail.exceptionDetail}");
            }
            processStartDate = Convert.ToDateTime(processStartDate).AddDays(1).ToString("yyyy-MM-dd");
        }
    }
}

