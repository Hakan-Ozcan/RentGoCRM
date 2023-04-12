namespace zRntCar.TestProject.adapter
{
    public class CarDecoratorBase : ICarDecarotor
    {
        internal ICarDecarotor Car;
        public CarDecoratorBase(ICarDecarotor car)
        {
            Car = car;
        }
        public virtual void PrintDetail()
        {
            Car.PrintDetail();
        }

        public virtual void AddPrice(decimal addedPrice)
        {
            Car.AddPrice(addedPrice);
        }

        public virtual void AddDescription(string addedDesc)
        {
            Car.AddDescription(addedDesc);
        }
    }
}
