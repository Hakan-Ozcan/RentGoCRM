namespace zRntCar.TestProject.adapter
{
    public class ABSDecorator : CarDecoratorBase
    {
        public ABSDecorator(ICarDecarotor car)
            : base(car)
        {
        }

        public override void PrintDetail()
        {
            base.Car.AddPrice(6.1m);
            base.Car.AddDescription("ABS added to current car.");
            base.Car.PrintDetail();
        }
    }
}
