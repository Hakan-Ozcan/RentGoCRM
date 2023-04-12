namespace RntCar.ClassLibrary
{
    public class DummyContactData
    {
        /// <summary>
        /// Müşteri adı
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Müşteri soyadı
        /// </summary>
        public string surname { get; set; }
        public string fullName { get; set; }
        /// <summary>
        /// Müşteri e-posta adresi
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Müşteri telefon numarası
        /// </summary>
        public string phoneNumber { get; set; }
        /// <summary>
        /// Müşteri TC kimlik numarası veya pasaport numarası
        /// </summary>
        public string governmentId { get; set; }
        /// <summary>
        /// Entegratör firmaya ait rezervasyon numarası
        /// </summary>
        public string referenceNumber { get; set; }
    }
}
