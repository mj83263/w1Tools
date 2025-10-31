using Newtonsoft.Json;

namespace TestProject1
{

    public class InfoBase 
    {
        [JsonProperty(Order = -1)]
        public string Desc { get; set; }

        [JsonProperty(Order = -2)]
        public DateTime UTim { get; set; } = DateTime.Now;

    }

    public class Info : InfoBase
    {
        [JsonProperty(Order = 1)]
        public object Data { get; set; }

        public Info(object obj, string desc)
        {
            Data = obj;
            base.Desc = desc;
        }

        public Info()
        {
        }
    }
    public class Info<T> : InfoBase where T : class
    {
        [JsonProperty(Order = 1)]
        public T Data { get; set; }

        public Info(T obj, string desc)
        {
            Data = obj;
            base.Desc = desc;
        }

        public Info()
        {
        }
    }
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var jj = new Info<object>("1234", "testDesc");
            var jj2 = JsonConvert.SerializeObject(jj);
            Console.WriteLine(jj2);
        }
    }
}