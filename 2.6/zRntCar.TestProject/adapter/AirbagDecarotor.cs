namespace zRntCar.TestProject.adapter
{
    public class AirbagDecarotor : CarDecoratorBase
    {
        public AirbagDecarotor(ICarDecarotor car)
            : base(car)
        {
        }

        public override void PrintDetail()
        {
            base.Car.AddPrice(3.4m);
            base.Car.AddDescription("Airbag added to current car.");
            base.Car.PrintDetail();
        }
    }
}
