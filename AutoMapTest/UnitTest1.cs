using AutoMapper;
using static AutoMapTest.miscellaneous;
using autoMapExLib;
using Newtonsoft.Json;
namespace AutoMapTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Foo, FooDto>());

            var mapper = config.CreateMapper();

            Foo foo = new Foo { ID = 1, Name = "Tom" };

            FooDto dto = mapper.Map<FooDto>(foo);
            Assert.IsTrue(foo.ID==dto.ID&&foo.Name==dto.Name);
        }

        [TestMethod]
        public void TestMethod2() 
        {

            var config = new MapperConfiguration(cfg => 
            {
                cfg.AddMaps("autoMapExLib");
            });
            config.CompileMappings();//编译映射
            var mapper = config.CreateMapper();
            var tc1 = new Tc1() { Id = 1, Name = "n1" };
            var tc2=mapper.Map<Tc2>(tc1);
            var tc3 = mapper.Map<Tc2>(tc2);
            Assert.IsTrue(tc1.Id==tc2.Id&&tc1.Name==tc2.Name);
        }
        //集合类型的映射
        [TestMethod]
        public void TestMethod3()
        {
            var confg = new MapperConfiguration(cfg => cfg.AddProfile<autoMapExLib.MyProfile>());
            var mapper = confg.CreateMapper();
            List<Tc1> tc1s = Enumerable.Range(0, 10).Select(a => new Tc1() { Id = a, Name = $"N{a}" }).ToList();
            var tc2 = mapper.Map<List<Tc2>>(tc1s);
            Assert.IsTrue(tc1s.Zip(tc2).All(a => a.First.Id == a.Second.Id && a.First.Name == a.Second.Name));
        }

        public class Employee
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        public class Employee2 : Employee
        {
            public string DeptName { get; set; }
        }

        public class EmployeeDto
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        public class EmployeeDto2 : EmployeeDto
        {
            public string DeptName { get; set; }
        }

        //多态
        [TestMethod]
        public void TestMethod4()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Employee, EmployeeDto>().Include<Employee2,EmployeeDto2>();
                cfg.CreateMap<Employee2, EmployeeDto2>();
            });
            var mapper = config.CreateMapper();
            var employees = new[]
            {
                new Employee { ID = 1, Name = "Tom" },
                new Employee2 { ID = 2, Name = "Jerry", DeptName = "R & D" }
            };

            var dto = mapper.Map<Employee[], EmployeeDto[]>(employees);
            Console.WriteLine("");
        }

        
        class Ttc1 : autoMapExLib.Tc1
        {
            public string GetDesc()
            {
                return $"{this.Id}_{this.Name}";
            }
        }
        public class Ttc2 : autoMapExLib.Tc2
        {
            //这里set属性是必须的
            public string Desc { get; set; }
        }

        //方法到属性的映射
        [TestMethod]
        public void TestMethod5() 
        { 
           var config=new MapperConfiguration(cfg=>cfg.CreateMap<Ttc1, Ttc2>());
            var mapper = config.CreateMapper();
            var ttc1 = new Ttc1() { Id = 1 ,Name="N1"};
            var ttc2 = mapper.Map<Ttc2>(ttc1);
            Console.WriteLine("");
        }

        //自定义映射
        public class Employee3
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public DateTime JoinTime { get; set; }
        }

        public class EmployeeDto3
        {
            public int EmployeeID { get; set; }

            public string EmployeeName { get; set; }

            public int JoinYear { get; set; }
        }

        [TestMethod]
        public void TestMethod6()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Employee3, EmployeeDto3>()
                .ForMember(dest=>dest.EmployeeID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Name));
            });
            var mapper = config.CreateMapper();

            var e3=new Employee3() { ID = 1, Name = "N1",JoinTime=DateTime.Now };
            var dto3=mapper.Map<EmployeeDto3>(e3);
            Console.WriteLine("");
        }
        [TestMethod]
        public void TestMethod7() 
        {
            string asciistr =  "AFcBAAAAAFRJTUUAAAAAAAAAAAAAAAAAAAAAAAAAAAcAAAAH6QUHDhMA";
            
            Console.WriteLine("");
        }
    }
}