namespace RntCar.ClassLibrary
{
    // Tolga AYKURT - 04.03.2019
    public class BlackListValidationResponse : ResponseBase
    {
        #region MEMBERS
        // Tolga AYKURT - 04.03.2019
        public BlackListData BlackList { get; set; }
        #endregion

        #region CONSTRUCTORS
        // Tolga AYKURT - 04.03.2019
        public BlackListValidationResponse()
        {
            this.ResponseResult = new ResponseResult();
        }
        #endregion
    }
}
