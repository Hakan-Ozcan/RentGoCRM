using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Campaign.Pegasus;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Reservation
{
    //trigger on Reservation / Contract Create
    public class PostCreatePegasusCampaign : IPlugin
    {
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity entity;
                initializer.PluginContext.GetContextPostImages<Entity>(initializer.PostImgKey, out entity);

                if (entity.Contains("rnt_campaignid"))
                {
                    initializer.TraceMe("rnt_campaignid : " + entity.GetAttributeValue<EntityReference>("rnt_campaignid").Id.ToString());

                    SystemParameterBL systemParameterBL = new SystemParameterBL(initializer.Service);
                    var pegasusEnabled = systemParameterBL.getPegasusCampaignEnabled();

                    initializer.TraceMe("pegasusEnabled : " + pegasusEnabled);

                    if (pegasusEnabled)
                    {
                        ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                        var pegasusID = configurationBL.GetConfigurationByName("PegasusCampaignId");

                        var externalURL = configurationBL.GetConfigurationByName("ExternalCrmWebApiUrl");
                        if (new Guid(pegasusID) == entity.GetAttributeValue<EntityReference>("rnt_campaignid").Id)
                        {
                            if (entity.LogicalName == "rnt_reservation")
                            {
                                var resType = entity.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value;
                                //bireysel ve kurumsallarda!
                                if (resType == (int)rnt_ReservationTypeCode.Bireysel ||
                                   resType == (int)rnt_ReservationTypeCode.Kurumsal)
                                {

                                    RestSharpHelper helper = new RestSharpHelper(externalURL, "checkpegasusmembership", Method.POST);
                                    helper.AddJsonParameter<CheckPegasusMembershipRequest>(new CheckPegasusMembershipRequest
                                    {
                                        authBaseUrl = configurationBL.GetConfigurationByName("PegasusAuthUrl"),
                                        bolbolurl = configurationBL.GetConfigurationByName("PegasusbolbolURL"),
                                        PegasusAuthValues = configurationBL.GetConfigurationByName("PegasusAuthValues"),
                                        customerId = entity.GetAttributeValue<EntityReference>("rnt_customerid").Id
                                    });
                                    var r = helper.Execute<PegasusMemberResponse>();
                                    initializer.TraceMe(JsonConvert.SerializeObject(r));
                                    if (r.code != "00000")
                                    {
                                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(r.description);
                                    }
                                }
                            }
                            else
                            {
                                var conractType = entity.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value;
                                //bireysel ve kurumsallarda!
                                if (conractType == (int)rnt_ReservationTypeCode.Acente ||
                                    conractType == (int)rnt_ReservationTypeCode.Broker)
                                {
                                    RestSharpHelper helper = new RestSharpHelper(externalURL, "checkpegasusmembership", Method.POST);
                                    helper.AddJsonParameter<CheckPegasusMembershipRequest>(new CheckPegasusMembershipRequest
                                    {
                                        authBaseUrl = configurationBL.GetConfigurationByName("PegasusAuthUrl"),
                                        bolbolurl = configurationBL.GetConfigurationByName("PegasusbolbolURL"),
                                        PegasusAuthValues = configurationBL.GetConfigurationByName("PegasusAuthValues"),
                                        customerId = entity.GetAttributeValue<EntityReference>("rnt_customerid").Id
                                    });
                                    var r = helper.Execute<PegasusMemberResponse>();
                                    if (r.code != "00000")
                                    {
                                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(r.description);
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                initializer.TraceMe("PostCreatePegasusCampaign exception : " + ex.Message);
                string data = "System.Exception:";

                var splitted = ex.Message.Split(new string[] { data }, StringSplitOptions.None);
                throw new Exception(splitted.Length >  1 ? splitted[1] : ex.Message);

            }
        }
    }
}
