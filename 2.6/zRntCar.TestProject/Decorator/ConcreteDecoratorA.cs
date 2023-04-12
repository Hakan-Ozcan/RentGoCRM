using System;

namespace zRntCar.TestProject.Decorator
{
    class ConcreteDecoratorA : Decorator

    {
        public override void Operation()
        {
            base.Operation();
            Console.WriteLine("ConcreteDecoratorA.Operation()");
        }
    }
}
