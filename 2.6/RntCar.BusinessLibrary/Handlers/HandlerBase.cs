using System;

namespace RntCar.BusinessLibrary.Handlers
{
    public class HandlerBase
    {
        public DateTime DateNow { get; set; } = DateTime.Now;
        public readonly string paymentXmlPath = "rnt_/Data/Xml/PaymentErrorMessages.xml";
        public readonly string ErrorMessageXml = "rnt_/Data/Xml/ErrorMessages.xml";
        public readonly string availabilityXmlPath = "rnt_/Data/Xml/AvailabilityErrorMessages.xml";
        public readonly string tabletXmlPath = "rnt_/Data/Xml/TabletErrorMessages.xml";
        public readonly string reservationXmlPath = "rnt_/Data/Xml/ReservationMessages.xml";
        public readonly string contractXmlPath = "rnt_/Data/Xml/ContractErrorMessages.xml";
        public readonly string branchXmlPath = "rnt_/Data/Xml/BranchErrorMessages.xml";
        public readonly string additionalProductXmlPath = "rnt_/Data/Xml/AdditionalProductMessages.xml";
        public readonly string mongoDbXmlPath = "rnt_/Data/Xml/MongoDbErrorMessages.xml";
        public readonly string invoiceXmlPath = "rnt_/Data/Xml/InvoiceErrorMessage.xml";
        public readonly string webXmlPath = "rnt_/Data/Xml/WebErrorMessages.xml";
        public readonly string mobileXmlPath = "rnt_/Data/Xml/MobileErrorMessages.xml";
        public readonly string couponCodeXmlPath = "rnt_/Data/Xml/CouponCodeErrorMessages.xml";
    }
}
