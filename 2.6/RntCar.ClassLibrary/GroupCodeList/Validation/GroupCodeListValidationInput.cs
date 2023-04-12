using System;

namespace RntCar.ClassLibrary.GroupCodeList.Validation
{
    // Tolga AYKURT - 11.03.2019
    public class GroupCodeListValidationInput
    {
        // Tolga AYKURT - 11.03.2019
        public Guid PriceListId { get; set; }

        // Tolga AYKURT - 11.03.2019
        public Guid GroupCodeId { get; set; }

        // Tolga AYKURT - 11.03.2019
        public int MinDay { get; set; }

        // Tolga AYKURT - 11.03.2019
        public int MaxDay { get; set; }

        // Tolga AYKURT - 11.03.2019
        /// <summary>
        /// Verilecek olan validayon messajında kullanılmak için isteniyor. Validasyona herhangi bir etkisi yok.
        /// </summary>
        public Guid InitiatingUserId { get; set; }
    }
}
