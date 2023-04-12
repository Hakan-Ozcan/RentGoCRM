using System;

namespace RntCar.ClassLibrary
{
    public class CMSAdditionalProductData
    {
        public Guid additionalProductId { get; set; }

        public string detailContent { get; set; }
        public string detailImageURL { get; set; }
        public string previewText { get; set; }
        public string previewImageURL { get; set; }
        public string seoDescription { get; set; }
        public string seoTitle { get; set; }
        public string seoKeyword { get; set; }
    }
}
