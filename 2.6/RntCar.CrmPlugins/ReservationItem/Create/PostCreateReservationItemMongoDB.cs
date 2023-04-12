using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.ReservationItem.Create
{
    public class PostCreateReservationItemMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            var itemTypeCode = postImage.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
            if (itemTypeCode == (int)ClassLibrary._Enums_1033.rnt_reservationitem_rnt_itemtypecode.Equipment)
            {

                ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service, initializer.TracingService);
                var res = reservationItemBL.CreateReservationItemInMongoDB(postImage);
                if (!res.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                }
                reservationItemBL.updateMongoDBCreateRelatedFields(postImage, res.Id);

                if (!string.IsNullOrEmpty(res.campaignId))
                {

                    Entity e = new Entity("rnt_reservation");
                    e.Id = postImage.GetAttributeValue<EntityReference>("rnt_reservationid").Id;
                    e["rnt_campaignid"] = new EntityReference("rnt_campaign", new Guid(res.campaignId));
                    initializer.Service.Update(e);

                    Entity e1 = new Entity("rnt_reservationitem");
                    e1.Id = postImage.Id;
                    e1["rnt_campaignid"] = new EntityReference("rnt_campaign", new Guid(res.campaignId));
                    initializer.Service.Update(e1);

                    ContractRepository contractRepository = new ContractRepository(initializer.Service);
                    var c = contractRepository.getContractByReservationId(postImage.GetAttributeValue<EntityReference>("rnt_reservationid").Id.ToString());

                    if (c != null)
                    {
                        initializer.TraceMe("sözleşme güncelleniyor ");
                        Entity contract = new Entity("rnt_contract");
                        contract.Id = c.Id;
                        contract["rnt_campaignid"] = new EntityReference("rnt_campaign", new Guid(res.campaignId));
                        initializer.Service.Update(contract);
                    }

                    ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                    var item = contractItemRepository.getContractItemByReservationItemId(postImage.Id);
                    if(item != null)
                    {
                        Entity contractItem = new Entity("rnt_contractitem");
                        contractItem.Id = item.Id;
                        contractItem["rnt_campaignid"] = new EntityReference("rnt_campaign", new Guid(res.campaignId));
                        initializer.Service.Update(contractItem);
                    }
                }

            }
        }
    }
}
