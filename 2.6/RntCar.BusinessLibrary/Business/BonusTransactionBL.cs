using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class BonusTransactionBL : BusinessHandler
    {
        public BonusTransactionBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public BonusTransactionBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public void hopiProcess(Entity bonusTransaction) 
        {
            int type = bonusTransaction.GetAttributeValue<OptionSetValue>("rnt_type").Value;
            EntityReference contractItemRef = bonusTransaction.GetAttributeValue<EntityReference>("rnt_contractitem");

            this.Trace("type: " + type);
            this.Trace("contractItemID: " + contractItemRef.Id);

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            CampaignRepository campaignRepository = new CampaignRepository(this.OrgService);
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            ActionHelper actionHelper = new ActionHelper(this.OrgService);

            if (contractItemRef != null && contractItemRef.Id != Guid.Empty)
            {
                var contractItemEntity = contractItemRepository.getContractItemId(contractItemRef.Id);
                EntityReference campaignRef = contractItemEntity.GetAttributeValue<EntityReference>("rnt_campaignid");
                string campaignCode = string.Empty;
                var campaignRate = decimal.Zero;

                if (campaignRef != null && campaignRef.Id != Guid.Empty)
                {
                    Entity campaignEntity = campaignRepository.retrieveById("rnt_campaign", campaignRef.Id);
                    campaignCode = campaignEntity.GetAttributeValue<string>("rnt_campaigncode");
                    campaignRate = campaignEntity.GetAttributeValue<decimal>("rnt_campaignrate");
                }

                EntityReference contractRef = contractItemEntity.GetAttributeValue<EntityReference>("rnt_contractid");

                if (contractRef != null && contractRef.Id != Guid.Empty)
                {
                    this.Trace("contractRef: " + contractRef.Id);
                    Entity contractEntity = contractRepository.getContractById(contractRef.Id, new string[] { "rnt_birdid", "rnt_itemtypecode" });
                    var birdId = contractEntity.GetAttributeValue<string>("rnt_birdid");
                    if (!string.IsNullOrWhiteSpace(birdId))
                    {
                        string[] parameters = configurationRepository.GetConfigurationByKey("hopiService.Parameters").Split(';');

                        this.Trace("birdId: " + birdId);


                        var contractItems = contractItemRepository.getContractItemsByContractId(contractRef.ToString(), new string[] { "rnt_totalamount", "rnt_itemtypecode" });

                        decimal billAmount = decimal.Zero;

                        if (contractItems.Count > 0)
                        {
                            foreach (var item in contractItems)
                            {
                                var itemType = item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;

                                if (itemType == 1 || itemType == 2)//equipment or additional products
                                {
                                    billAmount += item.GetAttributeValue<Money>("rnt_totalamount").Value;
                                    this.Trace("billAmount: " + billAmount);
                                }
                            }
                        }

                        this.Trace("billAmount: " + billAmount);

                        ResponseResult actionResponse = null;

                        if (type == 100000000 && billAmount > decimal.Zero) //earning
                        {
                            this.Trace("earning");

                            NotifyCheckoutRequest notifyCheckoutParameters = new NotifyCheckoutRequest();

                            notifyCheckoutParameters.birdId = long.Parse(birdId);
                            notifyCheckoutParameters.merchantCode = parameters[1];
                            notifyCheckoutParameters.storeCode = parameters[0];
                            notifyCheckoutParameters.transactionId = Convert.ToString(bonusTransaction.Id);
                            notifyCheckoutParameters.dateTime = DateTime.UtcNow.AddHours(3);

                            AmountDetails campaignFreePayment = new AmountDetails(); // Kampanya kapsamlarına dahil olmayan, vergi gruplarına göre ayrılmış alışveriş tutarları.
                            campaignFreePayment.amount = billAmount;
                            campaignFreePayment.percent = 18;

                            notifyCheckoutParameters.campaignFreePaymentDetails = new AmountDetails[]
                            {
                                 campaignFreePayment
                            };

                            AmountDetails subTotal = new AmountDetails();
                            subTotal.amount = billAmount;
                            subTotal.percent = 18;

                            notifyCheckoutParameters.subtotalDetails = new AmountDetails[] // İndirimden önceki ürün tutarı
                            {
                                subTotal
                            };

                            AmountDetails payment = new AmountDetails();
                            payment.amount = billAmount;
                            payment.percent = 18;

                            notifyCheckoutParameters.paymentDetails = new AmountDetails[]  //Vergi gruplarına göre ayrılmış alışveriş tutarı.
                            {
                                 payment
                            };

                            UsedCampaignDetails campaign = new UsedCampaignDetails(); //Alışveriş kapsamında üyenin faydalandığı kampanyalar.

                            AmountDetails campaignPayment = new AmountDetails();
                            campaignPayment.amount = decimal.Zero;
                            campaignPayment.percent = decimal.Zero;

                            campaign.amountDetails = new AmountDetails[]
                            {
                                campaignPayment
                            };
                            campaign.campaignCode = campaignCode;

                            ////Paracık kazanma durumu
                            campaign.benefit = new BenefitDetails();
                            campaign.benefit.coins = campaignRate;

                            //% İndirim kazanma durumu
                            AmountDetails discount = new AmountDetails();
                            discount.amount = decimal.Zero;
                            discount.percent = decimal.Zero;

                            notifyCheckoutParameters.usedCampaignDetails = new UsedCampaignDetails[]
                            {
                                campaign
                            };

                            //TransactionInfo transactionData = new TransactionInfo();
                            //transactionData.barcode = "XYZ";
                            //transactionData.amount = 90;
                            //transactionData.quantity = 1;
                            //transactionData.campaign = new[] { "" };

                            //notifyCheckoutParameters.transactionInfos = new TransactionInfo[] 
                            //{
                            //transactionData
                            //};

                            var notifyCheckoutResponse = actionHelper.HopiSales(notifyCheckoutParameters);

                            this.Trace("request: " + JsonConvert.SerializeObject(notifyCheckoutParameters));
                            this.Trace("response: " + JsonConvert.SerializeObject(notifyCheckoutResponse));

                            actionResponse = notifyCheckoutResponse.ResponseResult;
                        }
                        else if (type == 100000001) // return
                        {
                            this.Trace("return");

                            StartReturnTransactionRequest startReturnTransactionParameters = new StartReturnTransactionRequest();
                            startReturnTransactionParameters.merchantCode = parameters[1];
                            startReturnTransactionParameters.storeCode = parameters[0];
                            startReturnTransactionParameters.transactionId = "";
                            startReturnTransactionParameters.campaignFreeAmount = 0;
                            startReturnTransactionParameters.returnCampaignDetails.campaignCode = "";
                            startReturnTransactionParameters.returnCampaignDetails.returnPayment = decimal.Zero;
                            startReturnTransactionParameters.returnCampaignDetails.requestedCoinReturnAmount = decimal.Zero;
                            startReturnTransactionParameters.transactionInfos.amount = decimal.Zero;
                            startReturnTransactionParameters.transactionInfos.barcode = "";
                            startReturnTransactionParameters.transactionInfos.campaign = "";
                            startReturnTransactionParameters.transactionInfos.quantity = 0;

                            var returnTransactionResponse = actionHelper.HopiReturn(startReturnTransactionParameters);

                            this.Trace("request: " + JsonConvert.SerializeObject(startReturnTransactionParameters));
                            this.Trace("response: " + JsonConvert.SerializeObject(returnTransactionResponse));

                            actionResponse = returnTransactionResponse.ResponseResult;
                        }

                        if (actionResponse.Result == false)
                        {
                            string[] errorDetail = (actionResponse.ExceptionDetail).Split('-');
                            bonusTransaction["rnt_errorcode"] = errorDetail[0];
                            bonusTransaction["rnt_errordescription"] = errorDetail[1];
                            bonusTransaction["statuscode"] = new OptionSetValue(100000004);
                        }
                        else
                        {
                            bonusTransaction["statuscode"] = new OptionSetValue(100000002);
                        }
                    }
                    else
                    {
                        throw new Exception("");
                    }
                }
            }
        }
    }
}
