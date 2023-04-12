namespace RntCar.DocumentStatusChanger
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Updates reservations and their related items states to NoShow Status or Canceled, according to DateTime parameter.
            DocumentStatusChangerHelper.make();
        }
    }
}
