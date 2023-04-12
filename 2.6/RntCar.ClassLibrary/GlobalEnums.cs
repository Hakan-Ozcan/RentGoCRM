namespace RntCar.ClassLibrary
{
    public class GlobalEnums
    {
        public enum SmsContentCode
        {
            //smsContentCodes:
            //1. rez create success - 100
            ReservationCreateSuccessSms = 100,
            //2. rez create eksik - 200
            ReservationCreateMissingKnowledgeSms = 200,
            //3. rez update - 300
            ReservationUpdateSms = 300,
            //4. arac teslim - 400
            CarDeliverySms = 400,
            //5. sözleşme close - 500
            ContractCloseSms = 500,
            //6. müşteri create-update - 600 
            ContactCreateUpdateSms = 600,
            //7. müşteri create - 700 
            ContactCreateSms = 700,
            //8. müşteri update - 800
            ContactUpdateSms = 800,
            //deposit refund
            DepositRefund = 900,

            ManuelPaymentRefund = 2000,

            ManuelPaymentPayment = 2100,

            ManuelPaymentAdditionalProduct = 2200

        }
        public enum OperationType
        {
            Create = 1,
            Update = 2
        }
        public enum PaymentProvider
        {
            iyzico = 1,
            nettahsilat = 2
        }
        public enum StateCode
        {
            Active = 0,
            Passive = 1
        };
        public enum DocumentType
        {
            reservation = 1,
            contract = 2
        };

        public enum Channel
        {
            Web = 10,
            Branch = 20,
            CallCenter = 30,
            Mobile = 40
        };
        public enum AvailabilityFactorType
        {
            MinimumReservationDay = 1,
            AvailabilityClosure = 2,
            IncreaseCapacity = 3
        };
        public enum ConnectionType
        {
            default_Service = 1,
            WebAPI = 2,
            Both = 3,
            Empty = 4
        };
        public enum MessageNames
        {
            Create = 1,
            Update = 2
        };
        public enum RoleType
        {
            C = 1,
            I = 2
        };
        public enum ItemTypeCode
        {
            Equipment = 1,
            AdditionalProduct = 2,
            Fine = 3
        };
        public enum ProcessType
        {
            Creation = 1,
            Extending = 2,
            ReservationCreation = 3,
            Delivery = 4,
            ReservationReduce = 5,
            Refund = 6
        };

        public enum DocumentUpdateType
        {
            notChanged = 1,
            shiftedWithAllowedDuration = 2,
            shiftedWithNotAllowedDuration = 3,
            extended = 4,
            shorten = 5,
            monthly = 6
        }
        public enum PricingType
        {
            individual = 1,
            corporate = 2,
            broker = 3,
            agency = 4
        }
        public enum DurationCode
        {
            Daily = 1,
            Monthly = 2
        }
        public enum CustomerType
        {
            Individual = 1,
            Corporate = 2,
            Broker = 3,
            Agency = 4
        }
        public enum LangId
        {
            English = 1033,
            Turkish = 1055
        }

        public enum GroupCodeChangeType
        {
            NotChanged = 10,
            Upgrade = 20,
            Downgrade = 30,
            Upsell = 40,
            Downsell = 50
        }

        public enum BonusType
        {
            AdditionalProduct = 10,
            Sales = 20
        }

        public enum CreditCardHidden
        {
            Yes = 10,
            No = 20
        }

        public enum CouponCodeStatusCode
        {
            Generated = 1,
            Burned = 2,
            Used = 3
        }

        public enum CreditCardStatusCode
        {
            Yes = 10,
            No = 20
        }

        public enum KABISStatus
        {
            Waiting = 1,
            Rental = 2,
            Complated = 3
        }

        public enum HGSTransitListStatusCode
        {
            Invoiced = 1,
            NotInvoiced = 100000000,
            ContractNotFound = 100000001,
            PlateNotFound = 100000002,
            HGSNotFound = 100000003,
        }

        public enum IntegrationStatusCode
        {
            Stabil = 1,
            Testing = 100000000,
            Success = 100000001,
            Failed = 100000002,
            Retry = 100000003,
        }
    }
}
