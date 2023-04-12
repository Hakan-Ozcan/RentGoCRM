using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class PositionalBonusCalculationResult
    {
        public List<PositionalBonusCalculationData> Result { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
    }
}
