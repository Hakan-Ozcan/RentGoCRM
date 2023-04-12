using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iyzipay.Model.V2.Transaction;
using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.Logger;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.iyzico.Model;
using RntCar.SDK.Common;

namespace RntCar.IyzicoIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            LoggerHelper loggerHelper = new LoggerHelper();

            ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
            var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);

            IyzicoHelper iyzicoHelper = new IyzicoHelper(configs);
            var processStartDate = Convert.ToString(StaticHelper.GetConfiguration("processStartDate"));
            var processEndDate = Convert.ToString(StaticHelper.GetConfiguration("processEndDate"));
            var threadCountString = Convert.ToString(StaticHelper.GetConfiguration("threadCount"));
            if (string.IsNullOrWhiteSpace(processStartDate))
            {
                processStartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                processEndDate = processStartDate;
            }

            DateTime processDate = Convert.ToDateTime(processStartDate);
            while (processDate <= Convert.ToDateTime(processEndDate))
            {
                //Sunucu ile Türkiye arasýnda saat farký (+3) mevcuttur.
                //Saat farkýndan dolayý 22:00'da çalýþtý
                List<TransactionReportItem> transactionList = iyzicoHelper.GetTransactionListByDate(processDate);

                PaymentBL paymentBL = new PaymentBL();
                EntityCollection iyzilinkList = paymentBL.getIyzilinkTransaction();
                foreach (var transaction in transactionList)
                {
                    IyzicoTransactionObject iyzicoTransactionObject = new IyzicoTransactionObject();
                    iyzicoTransactionObject.Map(transaction);
                    paymentBL.checkIyzilinkPayment(iyzilinkList, iyzicoTransactionObject);
                    loggerHelper.traceInfo($"transaction conversationId {transaction.ConversationId}");
                    Console.WriteLine($"index {transactionList.IndexOf(transaction)}");
                    Console.WriteLine($"transaction conversationId {transaction.ConversationId}");
                }
                processDate = processDate.AddDays(1);
            }
        }
    }
}
