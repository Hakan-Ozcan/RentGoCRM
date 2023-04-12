using System;

namespace zRntCar.TestProject.adapter
{
    public class Car : ICarDecarotor
    {
        public string Model { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        public Car()
        {
            Price = 10.6m;
        }

        public void PrintDetail()
        {
            Console.WriteLine(Description);
        }

        public void AddPrice(decimal addedPrice)
        {
            Price += addedPrice;
        }

        public void AddDescription(string addedDesc)
        {
            Description = "Model: " + Model + " Brand: " + Brand + " Current Price: " + Price.ToString() + " " + addedDesc;
        }
    }
}
