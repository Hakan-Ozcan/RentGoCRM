using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Report;
using RntCar.SDK.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Report.Actions
{
    public class ExecuteGetadditionalProductsforBranch_Main : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);
            try
            {
                string _GetadditionalProductsforBranchMainRequest;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("GetadditionalProductsforBranchMainRequest", out _GetadditionalProductsforBranchMainRequest);
                pluginInitializer.TraceMe("GetadditionalProductsforBranchMainRequest: " + _GetadditionalProductsforBranchMainRequest);
                var serialized = JsonConvert.DeserializeObject<GetadditionalProductsforBranchMainRequest>(_GetadditionalProductsforBranchMainRequest);

                var lastDay = int.MaxValue;
                if (serialized.month == DateTime.Now.Month)
                {
                    lastDay = DateTime.Now.Date.Day;
                }
                else
                {
                    lastDay = DateTime.DaysInMonth(DateTime.Now.Year, serialized.month);
                }
                pluginInitializer.TraceMe("lastDay : " + lastDay);
                ConfigurationRepository configurationRepository = new ConfigurationRepository(pluginInitializer.Service);
                var adminId = new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid"));
                var webAdminId = new Guid(configurationRepository.GetConfigurationByKey("WebUserId"));
                var additionalProductYear = configurationRepository.GetConfigurationByKey("AdditionalProductYear");

                var processYear = DateTime.Now.Year;
                if (!string.IsNullOrWhiteSpace(additionalProductYear))
                {
                    processYear = Convert.ToInt32(additionalProductYear);
                }
                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(pluginInitializer.Service);
                var additionalProducts = additionalProductRepository.getAdditionalProducts();

                InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(pluginInitializer.Service);
                //List<Entity> invoiceItems = new List<Entity>();
                pluginInitializer.TraceMe("huhu");
                pluginInitializer.TraceMe("serialized.pickupbranchId : " + JsonConvert.SerializeObject(serialized.pickupbranchId.Select(p => p.value).ToList()));
                var _invoiceItems = invoiceItemRepository.getInvoiceItemsByGivenDatesByPickupBranch(Convert.ToDateTime(string.Format("{1}-{0}-01", serialized.month, processYear)),
                                                                                               Convert.ToDateTime(string.Format("{2}-{1}-{0}", lastDay, serialized.month, processYear)),
                                                                                                serialized.pickupbranchId.Select(p => (object)p.value).ToList());

                pluginInitializer.TraceMe("invoiceItems : " + _invoiceItems.Count);
                List<Entity> _reservationItems = new List<Entity>();
                ReservationItemRepository reservationItemRepository = new ReservationItemRepository(pluginInitializer.Service);

                _reservationItems = reservationItemRepository.getCompletedReservationItemsByBranchId(serialized.pickupbranchId.Select(p => (object)p.value).ToList(), new string[] { "createdby", "rnt_pickupbranchid" }, Convert.ToDateTime(string.Format("{1}-{0}-01", serialized.month, processYear)),
                                                                                           Convert.ToDateTime(string.Format("{2}-{1}-{0}", lastDay, serialized.month, processYear)));


                var reservationItems = _reservationItems.Where(p => _invoiceItems.Any(l => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == ((EntityReference)(l.GetAttributeValue<AliasedValue>("contract.rnt_pickupbranchid").Value)).Id)).ToList();
                ConcurrentBag<UserBasedBonusCalculationAdditionalProductData> userBasedBonusCalculationAdditionalProductDatas = new ConcurrentBag<UserBasedBonusCalculationAdditionalProductData>();
                pluginInitializer.TraceMe("reservationItems : " + reservationItems.Count);
                Parallel.ForEach(_invoiceItems, invoiceItem =>
                 {
                     UserBasedBonusCalculationAdditionalProductData d = new UserBasedBonusCalculationAdditionalProductData
                     {
                         Amount = invoiceItem.GetAttributeValue<decimal>("rnt_netamount"),
                         QueryDate = DateTime.Now.Date,
                         InvoiceDate = invoiceItem.GetAttributeValue<DateTime>("rnt_invoicedate").ToString("dd/MM/yyyy"),
                         Branch = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contract.rnt_pickupbranchid").Value)).Name
                     };
                     int itemTypeCode = ((OptionSetValue)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_itemtypecode").Value)).Value;

                     // Don't calculate if item type is equipment
                     if (itemTypeCode != (int)rnt_contractitem_rnt_itemtypecode.Equipment)
                     {
                         // Take tablet user
                         if (invoiceItem.Attributes.Contains("contractitem.rnt_externalusercreatedbyid"))
                         {
                             d.UserName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_externalusercreatedbyid").Value)).Name;
                             d.UserId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_externalusercreatedbyid").Value)).Id;

                             d.TabletUserId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_externalusercreatedbyid").Value)).Id;
                             d.TabletUserName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_externalusercreatedbyid").Value)).Name;
                         }
                         else if (invoiceItem.Attributes.Contains("contractitem.rnt_reservationitemid"))
                         {
                             var resItemId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_reservationitemid").Value)).Id;
                             var reservationItem = reservationItems.Where(p => p.Id == resItemId).FirstOrDefault();

                             if (reservationItem != null)
                             {
                                 var userId = reservationItem.GetAttributeValue<EntityReference>("createdby").Id;

                                 // Take contract user
                                 if (userId == adminId || userId == webAdminId)
                                 {
                                     d.UserName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Name;
                                     d.UserId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Id;

                                     d.ContractUserId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Id;
                                     d.ContractUserName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Name;

                                     d.ResUserName = reservationItem.GetAttributeValue<EntityReference>("createdby").Name;
                                     d.ResUserId = reservationItem.GetAttributeValue<EntityReference>("createdby").Id;
                                 }
                                 // Take reservation user
                                 else
                                 {
                                     d.UserName = reservationItem.GetAttributeValue<EntityReference>("createdby").Name;
                                     d.UserId = reservationItem.GetAttributeValue<EntityReference>("createdby").Id;

                                     d.ContractUserId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Id;
                                     d.ContractUserName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Name;

                                     d.ResUserName = reservationItem.GetAttributeValue<EntityReference>("createdby").Name;
                                     d.ResUserId = reservationItem.GetAttributeValue<EntityReference>("createdby").Id;
                                 }
                             }
                         }
                         else
                         {
                             d.UserName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Name;
                             d.UserId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Id;

                             d.ContractUserId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Id;
                             d.ContractUserName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.createdby").Value)).Name;
                         }
                          ;
                         d.AdditionalProductName = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_additionalproductid").Value)).Name;
                         d.AdditionalProductId = ((EntityReference)(invoiceItem.GetAttributeValue<AliasedValue>("contractitem.rnt_additionalproductid").Value)).Id;
                         d.ContractId = ((Guid)(invoiceItem.GetAttributeValue<AliasedValue>("contract.rnt_contractid").Value));
                         d.ContractNumber = ((string)(invoiceItem.GetAttributeValue<AliasedValue>("contract.rnt_contractnumber").Value));

                         //pluginInitializer.TraceMe(".ContractNumber : " + d.ContractNumber);
                         userBasedBonusCalculationAdditionalProductDatas.Add(d);
                     }

                 });

                pluginInitializer.TraceMe("userBasedBonusCalculationAdditionalProductDatas : " + userBasedBonusCalculationAdditionalProductDatas.Count);
                pluginInitializer.PluginContext.OutputParameters["GetadditionalProductsforBranchMainResponse"] = JsonConvert.SerializeObject(userBasedBonusCalculationAdditionalProductDatas);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
