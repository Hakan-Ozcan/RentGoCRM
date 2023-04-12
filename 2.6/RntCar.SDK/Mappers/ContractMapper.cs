namespace RntCar.SDK.Mappers
{
    public class ContractMapper
    {
        public static int getContractItemStatusCodeByContractStatusCode(int contractStatusCode)
        {
            var contractItemStatusCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.WaitingForDelivery;
            switch (contractStatusCode)
            {
                case (int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.WaitingForDelivery:
                    contractItemStatusCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.WaitingForDelivery;
                    break;
                case (int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.Rental:
                    contractItemStatusCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Rental;
                    break;
                case (int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.Completed:
                    contractItemStatusCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed;
                    break;
                default:
                    break;
            }
            return contractItemStatusCode;
        }    
    }
}
