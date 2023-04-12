using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class UserBasedBonusCalculationResult
    {
        public List<UserBasedBonusCalculationData> Result { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
    }
}
