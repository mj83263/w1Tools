using Autofac;

namespace TestProject4AutoFactor
{
    [TestClass]
    public sealed class Test1
    {
        interface Ia
        {
            void start();
            void stop();    
        }
        interface Ib : Ia
        {

        }
        class Ca : Ib
        {
            public void Foo()
            {
                throw new NotImplementedException();
            }
        }
        [TestMethod]
        public void TestMethod1()
        {
            var builer = new Autofac.ContainerBuilder();
            builer.RegisterType<Ca>().As<Ib>();
            builer.Register(contex =>
            {
                var m = contex.Resolve<Ib>();
                return m;
            })
                .OnActivated(ib=>ib.Instance.start())
                .OnRelease(ib=>ib.stop())
                .Keyed<Ib>("KK").SingleInstance();

        }
    }
}
