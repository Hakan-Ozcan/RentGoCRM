using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class CalculatedBonusTransaction : CodeActivity
    {
        [Input("Contract Reference")]
        [ReferenceTarget("rnt_contract")]
        public InArgument<EntityReference> _contract { get; set; }

        [Input("Type")]
        [AttributeTarget("rnt_bonustransaction", "rnt_type")]
        public InArgument<OptionSetValue> _type { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var contractRef = _contract.Get<EntityReference>(context);
            var type = _type.Get<OptionSetValue>(context);
            Guid bonusTransactionId = initializer.PrimaryId;
            try
            {
                initializer.TracingService.Trace("type: " + type.Value);
                initializer.TracingService.Trace("contractId: " + contractRef.Id);
                Entity bonusTransaction = new Entity("rnt_bonustransaction", bonusTransactionId);

                ActionHelper actionHelper = new ActionHelper(initializer.Service);
                ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                CampaignRepository campaignRepository = new CampaignRepository(initializer.Service);
                ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);

                if (contractRef != null && contractRef.Id != Guid.Empty)
                {
                    var contractEntity = contractRepository.getContractById(contractRef.Id);
                    EntityReference campaignRef = contractEntity.GetAttributeValue<EntityReference>("rnt_campaignid");
                    string campaignCode = string.Empty;
                    decimal campaignRate = decimal.Zero;

                    if (campaignRef != null && campaignRef.Id != Guid.Empty)
                    {
                        Entity campaignEntity = campaignRepository.retrieveById("rnt_campaign", campaignRef.Id);
                        campaignCode = campaignEntity.GetAttributeValue<string>("rnt_campaigncode");
                        campaignRate = campaignEntity.GetAttributeValue<decimal>("rnt_campaignrate");
                    }

                    var birdId = contractEntity.GetAttributeValue<string>("rnt_birdid");

                    if (!string.IsNullOrWhiteSpace(birdId))
                    {
                        string[] parameters = configurationRepository.GetConfigurationByKey("hopiService.Parameters").Split(';');

                        initializer.TracingService.Trace("birdId: " + birdId);

                        var contractItems = contractItemRepository.getCompletedContractEquipmentByContractId(Convert.ToString(contractRef.Id), new string[] { "rnt_totalamount", "rnt_itemtypecode", "rnt_bonustransactionid" });

                        decimal billAmount = decimal.Zero;
                        decimal coin = decimal.Zero;

                        initializer.TracingService.Trace("contractItems.Count: " + contractItems.Count);
                        if (contractItems.Count > 0)
                        {
                            foreach (var item in contractItems)
                            {
                                var itemType = item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
                                EntityReference bonusTransactionRef = item.GetAttributeValue<EntityReference>("rnt_bonustransactionid");

                                initializer.TracingService.Trace("itemType " + itemType);
                                if (itemType == 1 || itemType == 5)//equipment or price difference
                                {
                                    if (bonusTransactionRef == null || bonusTransactionRef.Id == Guid.Empty)
                                    {
                                        billAmount += item.GetAttributeValue<Money>("rnt_totalamount").Value;

                                        initializer.TracingService.Trace("billAmount: " + billAmount);

                                        Entity updateContractItem = new Entity(item.LogicalName, item.Id);
                                        updateContractItem.Attributes["rnt_bonustransactionid"] = new EntityReference(bonusTransaction.LogicalName, bonusTransaction.Id);

                                        initializer.Service.Update(updateContractItem);

                                    }
                                }
                            }

                            coin = Math.Round((billAmount * campaignRate) / 100, 2);

                        }

                        initializer.TracingService.Trace("billAmount: " + billAmount);

                        ResponseResult actionResponse = null;

                        if (type.Value == 100000000 && billAmount > decimal.Zero) //earning
                        {
                            initializer.TracingService.Trace("earning");

                            NotifyCheckoutRequest notifyCheckoutParameters = new NotifyCheckoutRequest();

                            notifyCheckoutParameters.birdId = long.Parse(birdId);
                            notifyCheckoutParameters.merchantCode = parameters[1];
                            notifyCheckoutParameters.storeCode = parameters[0];
                            notifyCheckoutParameters.transactionId = Convert.ToString(bonusTransaction.Id);
                            notifyCheckoutParameters.dateTime = DateTime.UtcNow.AddHours(3);


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
                            campaignPayment.amount = billAmount;
                            campaignPayment.percent = 18;

                            campaign.amountDetails = new AmountDetails[]
                            {
                                campaignPayment
                            };
                            campaign.campaignCode = campaignCode;

                            ////Paracık kazanma durumu
                            campaign.benefit = new BenefitDetails();
                            campaign.benefit.coins = coin;

                            //% İndirim kazanma durumu
                            //AmountDetails discount = new AmountDetails();
                            //discount.amount = decimal.Zero;
                            //discount.percent = decimal.Zero;


                            notifyCheckoutParameters.usedCampaignDetails = new UsedCampaignDetails[]
                            {
                                campaign
                            };

                            var notifyCheckoutResponse = actionHelper.HopiSales(notifyCheckoutParameters);

                            initializer.TracingService.Trace("request: " + JsonConvert.SerializeObject(notifyCheckoutParameters));
                            initializer.TracingService.Trace("response: " + JsonConvert.SerializeObject(notifyCheckoutResponse));

                            actionResponse = notifyCheckoutResponse.ResponseResult;
                        }
                        else if (type.Value == 100000001) // return
                        {
                            initializer.TracingService.Trace("return");

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

                            initializer.TracingService.Trace("request: " + JsonConvert.SerializeObject(startReturnTransactionParameters));
                            initializer.TracingService.Trace("response: " + JsonConvert.SerializeObject(returnTransactionResponse));

                            actionResponse = returnTransactionResponse.ResponseResult;
                        }


                        bonusTransaction["rnt_moneyvalue"] = new Money(billAmount);
                        bonusTransaction["rnt_value"] = coin;
                        bonusTransaction["rnt_name"] = Convert.ToString(bonusTransaction.Id);

                        if (actionResponse != null && actionResponse.Result == true)
                        {
                            bonusTransaction["statuscode"] = new OptionSetValue(100000002);
                        }
                        else if (actionResponse != null && actionResponse.Result == false)
                        {
                            string[] errorDetail = (actionResponse.ExceptionDetail).Split('-');
                            bonusTransaction["rnt_errorcode"] = errorDetail[0];
                            bonusTransaction["rnt_errordescription"] = errorDetail[1];
                            bonusTransaction["statuscode"] = new OptionSetValue(100000004);
                        }
                        else
                        {
                            bonusTransaction["rnt_errorcode"] = "";
                            bonusTransaction["rnt_errordescription"] = "actionResponse is null";
                            bonusTransaction["statuscode"] = new OptionSetValue(100000004);
                        }

                        initializer.Service.Update(bonusTransaction);
                    }
                    else
                    {
                        throw new Exception("BirdId is null for the related transaction!");
                    }

                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);

            }
        }
    }
}
