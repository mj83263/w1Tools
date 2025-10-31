using atlTcpPackage;

using Newtonsoft.Json;
using ZDevTools.InteropServices;

namespace TestAtlTcpPackage
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var jsonStr = "{\"ResponseType\":0,\"DataAction\":0,\"NeedResponse\":false,\"Header\":\"OK\",\"Data\":\"urEwRQAAgD9vEgM7s9suRQAAgD9vEgM7\"}";
            var resource = JsonConvert.DeserializeObject<atlTcpPackage.PackInfo.ResponsePackage>(jsonStr);
            var result = (float[])new atlTcpPackage.AtlFormatterConverter().Convert(resource.Data, typeof(float[]));
            Console.WriteLine(jsonStr);
            Console.WriteLine($"Result:{string.Join(",", result.Select(a=>a.ToString()))}");
            var kk = Convert.FromBase64String("urEwRQAAgD9vEgM7s9suRQAAgD9vEgM7");
            var result2= (float[])new atlTcpPackage.AtlFormatterConverter().Convert(kk, typeof(float[]));
            Console.WriteLine($"Result:{string.Join(",", result2.Select(a => a.ToString()))}");
        }
        [TestMethod]
        public void TestMethod2() 
        {
            var jsonStr = "{\"ResponseType\":2,\"DataAction\":0,\"NeedResponse\":true,\"Header\":\"SPECIAL_BI\",\"Data\":\"\"}";
            var req = JsonConvert.DeserializeObject<atlTcpPackage.PackInfo.RequestPackage>(jsonStr);
            Console.WriteLine("");
        }
        [TestMethod]
        public void TestMethod3()
        {
            var str = "W3sic3RyTW9KdWFuSGFvIjoiTjIxMTI1MTAzMDQ3MDI4UzAiLCJzdHJKb2JObyI6Ij8/Iiwic3RyU2NhblRpbWUiOiIyMDI1LTEwLTMwIDA5OjM2OjAwIiwic3RyTWVhc3VyZVBvc2l0aW9uIjoiTTQiLCJzdHJNYWNoaW5lTm8iOiJDQTcwNiIsInN0ckZpbG1UeXBlIjoiPz8/PyIsInN0clBsYXRGb3JtSUQiOiJNNlMiLCJzdHJNSSI6IkJPUC1HVEItNTQ2NzkxIiwic3RyTW9kZWwiOiI1NDY3OTEtMTAwTCIsInN0ckRvdWJsZU9yU2luZ2xlIjoiPz8iLCJzdHJNZWFzdXJlVHlwZSI6Ij8iLCJzdHJTaW1wbGluZ1Bvc2l0aW9uIjoiTTQxIiwic3RySnVkZ2VSZXN1bHQiOiJORyIsInN0clZhbHVlMSI6Ijk5LjU2JSIsInN0clZhbHVlMiI6IjEwMC4wMSUiLCJzdHJWYWx1ZTMiOiIxMDAuNTMlIiwic3RyVmFsdWU0IjoiMTAwLjk4JSIsInN0clZhbHVlNSI6IjEwMS4zNiUiLCJzdHJWYWx1ZTYiOiIxMDEuNjUlIiwic3RyVmFsdWU3IjoiMTAxLjg0JSIsInN0clZhbHVlOCI6IjEwMS45MyUiLCJzdHJWYWx1ZTkiOiIxMDEuODYlIiwic3RyVmFsdWUxMCI6IjEwMS40MiUiLCJzdHJWYWx1ZTExIjoiMTAwLjkxJSIsInN0clZhbHVlMTIiOiIxMDEuMTIlIiwic3RyVmFsdWUxMyI6IjEwMS4zMiUiLCJzdHJWYWx1ZTE0IjoiMTAwLjg5JSIsInN0clZhbHVlMTUiOiIxMDAuMTIlIiwic3RyVmFsdWUxNiI6Ijk5LjUyJSIsInN0clZhbHVlMTciOiI5OS43MSUiLCJzdHJWYWx1ZTE4IjoiMTAwLjA4JSIsInN0clZhbHVlMTkiOiI5OS4zOSUiLCJzdHJNaWRWYWx1ZSI6IjEzMC4xOCIsInN0ck1pZFN0YXJ0IjoiODcyODM1My4xMiIsInN0ck1pZEVuZCI6Ijg3MjkwNjMuNzgiLCJzdHJUcmFuc2l0aW9uUG9pbnQiOiI/Pz8/Pz8/Pz8/Iiwic3RySXNVcGxvYWRTdWNjIjoiIiwic3RyTmVnYXRpdmVWYWx1ZSI6IjcuMjkiLCJzdHJQb3NpdGl2ZVZhbHVlIjoiNy4yOSIsInN0cksiOiIwLjk1NzgiLCJzdHJCIjoiMjAuMzAxNyJ9LHsic3RyTW9KdWFuSGFvIjoiTjIxMTI1MTAzMDQ3MDI4UzAiLCJzdHJKb2JObyI6Ij8/Iiwic3RyU2NhblRpbWUiOiIyMDI1LTEwLTMwIDA5OjM2OjAwIiwic3RyTWVhc3VyZVBvc2l0aW9uIjoiTTQiLCJzdHJNYWNoaW5lTm8iOiJDQTcwNiIsInN0ckZpbG1UeXBlIjoiPz8/PyIsInN0clBsYXRGb3JtSUQiOiJNNlMiLCJzdHJNSSI6IkJPUC1HVEItNTQ2NzkxIiwic3RyTW9kZWwiOiI1NDY3OTEtMTAwTCIsInN0ckRvdWJsZU9yU2luZ2xlIjoiPz8iLCJzdHJNZWFzdXJlVHlwZSI6Ij8iLCJzdHJTaW1wbGluZ1Bvc2l0aW9uIjoiTTQxIiwic3RySnVkZ2VSZXN1bHQiOiJPSyIsInN0clZhbHVlMSI6IjYxLjA2JSIsInN0clZhbHVlMiI6IjY0LjY5JSIsInN0clZhbHVlMyI6IjY5LjA2JSIsInN0clZhbHVlNCI6IjczLjk2JSIsInN0clZhbHVlNSI6Ijc4Ljc2JSIsInN0clZhbHVlNiI6IjgzLjA2JSIsInN0clZhbHVlNyI6Ijg2Ljg2JSIsInN0clZhbHVlOCI6IjkwLjI1JSIsInN0clZhbHVlOSI6Ijk0LjA4JSIsInN0clZhbHVlMTAiOiI5Ni41NCUiLCJzdHJWYWx1ZTExIjoiOTcuODclIiwic3RyVmFsdWUxMiI6Ijk5LjI5JSIsInN0clZhbHVlMTMiOiI5OS45NCUiLCJzdHJWYWx1ZTE0IjoiMTAwLjQlIiwic3RyVmFsdWUxNSI6Ijk5LjgxJSIsInN0clZhbHVlMTYiOiIxMDAuMTclIiwic3RyVmFsdWUxNyI6IjEwMC4yNyUiLCJzdHJWYWx1ZTE4IjoiOTkuOTUlIiwic3RyVmFsdWUxOSI6IjEwMC4xMyUiLCJzdHJNaWRWYWx1ZSI6IjEzMC4xOCIsInN0ck1pZFN0YXJ0IjoiODcyODM1My4xMiIsInN0ck1pZEVuZCI6Ijg3MjkwNjMuNzgiLCJzdHJUcmFuc2l0aW9uUG9pbnQiOiI/Pz8/Pz8/Pz8/Iiwic3RySXNVcGxvYWRTdWNjIjoiIiwic3RyTmVnYXRpdmVWYWx1ZSI6IjcuMjkiLCJzdHJQb3NpdGl2ZVZhbHVlIjoiNy4yOSIsInN0cksiOiIwLjk1NzgiLCJzdHJCIjoiMjAuMzAxNyJ9LHsic3RyTW9KdWFuSGFvIjoiTjIxMTI1MTAzMDQ3MDI4UzAiLCJzdHJKb2JObyI6Ij8/Iiwic3RyU2NhblRpbWUiOiIyMDI1LTEwLTMwIDA5OjM2OjE5Iiwic3RyTWVhc3VyZVBvc2l0aW9uIjoiTTQiLCJzdHJNYWNoaW5lTm8iOiJDQTcwNiIsInN0ckZpbG1UeXBlIjoiPz8/PyIsInN0clBsYXRGb3JtSUQiOiJNNlMiLCJzdHJNSSI6IkJPUC1HVEItNTQ2NzkxIiwic3RyTW9kZWwiOiI1NDY3OTEtMTAwTCIsInN0ckRvdWJsZU9yU2luZ2xlIjoiPz8iLCJzdHJNZWFzdXJlVHlwZSI6Ij8iLCJzdHJTaW1wbGluZ1Bvc2l0aW9uIjoiTTQyIiwic3RySnVkZ2VSZXN1bHQiOiJPSyIsInN0clZhbHVlMSI6Ijk5LjQ4JSIsInN0clZhbHVlMiI6Ijk5LjY0JSIsInN0clZhbHVlMyI6Ijk5LjglIiwic3RyVmFsdWU0IjoiOTkuOSUiLCJzdHJWYWx1ZTUiOiI5OS45OCUiLCJzdHJWYWx1ZTYiOiIxMDAuMTklIiwic3RyVmFsdWU3IjoiMTAwLjYlIiwic3RyVmFsdWU4IjoiMTAxLjA5JSIsInN0clZhbHVlOSI6IjEwMS40NyUiLCJzdHJWYWx1ZTEwIjoiMTAxLjQxJSIsInN0clZhbHVlMTEiOiIxMDEuMyUiLCJzdHJWYWx1ZTEyIjoiMTAxLjI2JSIsInN0clZhbHVlMTMiOiIxMDEuMDclIiwic3RyVmFsdWUxNCI6IjEwMC42OSUiLCJzdHJWYWx1ZTE1IjoiMTAwLjI0JSIsInN0clZhbHVlMTYiOiI5OS4wMyUiLCJzdHJWYWx1ZTE3IjoiOTkuMzglIiwic3RyVmFsdWUxOCI6Ijk5LjM3JSIsInN0clZhbHVlMTkiOiI5OS41MiUiLCJzdHJNaWRWYWx1ZSI6IjEzMC4wOSIsInN0ck1pZFN0YXJ0IjoiODcyOTkwNy4yNSIsInN0ck1pZEVuZCI6Ijg3MzA2MTYuMzUiLCJzdHJUcmFuc2l0aW9uUG9pbnQiOiI/Pz8/Pz8/Pz8/Iiwic3RySXNVcGxvYWRTdWNjIjoiIiwic3RyTmVnYXRpdmVWYWx1ZSI6IjcuMjkiLCJzdHJQb3NpdGl2ZVZhbHVlIjoiNy4yOSIsInN0cksiOiIwLjk1NzgiLCJzdHJCIjoiMjAuMzAxNyJ9LHsic3RyTW9KdWFuSGFvIjoiTjIxMTI1MTAzMDQ3MDI4UzAiLCJzdHJKb2JObyI6Ij8/Iiwic3RyU2NhblRpbWUiOiIyMDI1LTEwLTMwIDA5OjM2OjE5Iiwic3RyTWVhc3VyZVBvc2l0aW9uIjoiTTQiLCJzdHJNYWNoaW5lTm8iOiJDQTcwNiIsInN0ckZpbG1UeXBlIjoiPz8/PyIsInN0clBsYXRGb3JtSUQiOiJNNlMiLCJzdHJNSSI6IkJPUC1HVEItNTQ2NzkxIiwic3RyTW9kZWwiOiI1NDY3OTEtMTAwTCIsInN0ckRvdWJsZU9yU2luZ2xlIjoiPz8iLCJzdHJNZWFzdXJlVHlwZSI6Ij8iLCJzdHJTaW1wbGluZ1Bvc2l0aW9uIjoiTTQyIiwic3RySnVkZ2VSZXN1bHQiOiJPSyIsInN0clZhbHVlMSI6IjU4LjclIiwic3RyVmFsdWUyIjoiNjAuNDclIiwic3RyVmFsdWUzIjoiNjIuMTIlIiwic3RyVmFsdWU0IjoiNjQuNjYlIiwic3RyVmFsdWU1IjoiNjguNzglIiwic3RyVmFsdWU2IjoiNzQuODElIiwic3RyVmFsdWU3IjoiODEuNzQlIiwic3RyVmFsdWU4IjoiODguMSUiLCJzdHJWYWx1ZTkiOiI5My44OSUiLCJzdHJWYWx1ZTEwIjoiOTYuMjElIiwic3RyVmFsdWUxMSI6Ijk2Ljg2JSIsInN0clZhbHVlMTIiOiI5OC4xNyUiLCJzdHJWYWx1ZTEzIjoiOTkuNzElIiwic3RyVmFsdWUxNCI6IjEwMC44NCUiLCJzdHJWYWx1ZTE1IjoiOTkuOTQlIiwic3RyVmFsdWUxNiI6IjEwMC4yMSUiLCJzdHJWYWx1ZTE3IjoiMTAwLjA3JSIsInN0clZhbHVlMTgiOiIxMDAuMiUiLCJzdHJWYWx1ZTE5IjoiMTAwLjAzJSIsInN0ck1pZFZhbHVlIjoiMTMwLjA5Iiwic3RyTWlkU3RhcnQiOiI4NzI5OTA3LjI1Iiwic3RyTWlkRW5kIjoiODczMDYxNi4zNSIsInN0clRyYW5zaXRpb25Qb2ludCI6Ij8/Pz8/Pz8/Pz8iLCJzdHJJc1VwbG9hZFN1Y2MiOiIiLCJzdHJOZWdhdGl2ZVZhbHVlIjoiNy4yOSIsInN0clBvc2l0aXZlVmFsdWUiOiI3LjI5Iiwic3RySyI6IjAuOTU3OCIsInN0ckIiOiIyMC4zMDE3In0seyJzdHJNb0p1YW5IYW8iOiJOMjExMjUxMDMwNDcwMjhTMCIsInN0ckpvYk5vIjoiPz8iLCJzdHJTY2FuVGltZSI6IjIwMjUtMTAtMzAgMDk6MzY6MzciLCJzdHJNZWFzdXJlUG9zaXRpb24iOiJNNCIsInN0ck1hY2hpbmVObyI6IkNBNzA2Iiwic3RyRmlsbVR5cGUiOiI/Pz8/Iiwic3RyUGxhdEZvcm1JRCI6Ik02UyIsInN0ck1JIjoiQk9QLUdUQi01NDY3OTEiLCJzdHJNb2RlbCI6IjU0Njc5MS0xMDBMIiwic3RyRG91YmxlT3JTaW5nbGUiOiI/PyIsInN0ck1lYXN1cmVUeXBlIjoiPyIsInN0clNpbXBsaW5nUG9zaXRpb24iOiJNNDMiLCJzdHJKdWRnZVJlc3VsdCI6Ik5HIiwic3RyVmFsdWUxIjoiMTAwLjA2JSIsInN0clZhbHVlMiI6IjEwMC4xNyUiLCJzdHJWYWx1ZTMiOiIxMDAuMzMlIiwic3RyVmFsdWU0IjoiMTAwLjUxJSIsInN0clZhbHVlNSI6IjEwMC42OCUiLCJzdHJWYWx1ZTYiOiIxMDAuODUlIiwic3RyVmFsdWU3IjoiMTAxJSIsInN0clZhbHVlOCI6IjEwMS4xMiUiLCJzdHJWYWx1ZTkiOiIxMDEuMjElIiwic3RyVmFsdWUxMCI6IjEwMS40JSIsInN0clZhbHVlMTEiOiIxMDEuNSUiLCJzdHJWYWx1ZTEyIjoiMTAxLjQ1JSIsInN0clZhbHVlMTMiOiIxMDEuMjglIiwic3RyVmFsdWUxNCI6IjEwMC4zNSUiLCJzdHJWYWx1ZTE1IjoiOTkuOTQlIiwic3RyVmFsdWUxNiI6IjEwMC4wMSUiLCJzdHJWYWx1ZTE3IjoiOTkuODMlIiwic3RyVmFsdWUxOCI6Ijk5LjUxJSIsInN0clZhbHVlMTkiOiI5OS43MyUiLCJzdHJNaWRWYWx1ZSI6IjEzMC4xMCIsInN0ck1pZFN0YXJ0IjoiODczMTQ1OS4zNSIsInN0ck1pZEVuZCI6Ijg3MzIxNjcuNjUiLCJzdHJUcmFuc2l0aW9uUG9pbnQiOiI/Pz8/Pz8/Pz8/Iiwic3RySXNVcGxvYWRTdWNjIjoiIiwic3RyTmVnYXRpdmVWYWx1ZSI6IjcuMjkiLCJzdHJQb3NpdGl2ZVZhbHVlIjoiNy4yOSIsInN0cksiOiIwLjk1NzgiLCJzdHJCIjoiMjAuMzAxNyJ9LHsic3RyTW9KdWFuSGFvIjoiTjIxMTI1MTAzMDQ3MDI4UzAiLCJzdHJKb2JObyI6Ij8/Iiwic3RyU2NhblRpbWUiOiIyMDI1LTEwLTMwIDA5OjM2OjM3Iiwic3RyTWVhc3VyZVBvc2l0aW9uIjoiTTQiLCJzdHJNYWNoaW5lTm8iOiJDQTcwNiIsInN0ckZpbG1UeXBlIjoiPz8/PyIsInN0clBsYXRGb3JtSUQiOiJNNlMiLCJzdHJNSSI6IkJPUC1HVEItNTQ2NzkxIiwic3RyTW9kZWwiOiI1NDY3OTEtMTAwTCIsInN0ckRvdWJsZU9yU2luZ2xlIjoiPz8iLCJzdHJNZWFzdXJlVHlwZSI6Ij8iLCJzdHJTaW1wbGluZ1Bvc2l0aW9uIjoiTTQzIiwic3RySnVkZ2VSZXN1bHQiOiJPSyIsInN0clZhbHVlMSI6IjY0Ljc5JSIsInN0clZhbHVlMiI6IjcwLjYlIiwic3RyVmFsdWUzIjoiNzYuNzQlIiwic3RyVmFsdWU0IjoiODIuODUlIiwic3RyVmFsdWU1IjoiODguNDElIiwic3RyVmFsdWU2IjoiOTIuNjMlIiwic3RyVmFsdWU3IjoiOTUuMjklIiwic3RyVmFsdWU4IjoiOTYuNTQlIiwic3RyVmFsdWU5IjoiOTcuMyUiLCJzdHJWYWx1ZTEwIjoiOTguMDElIiwic3RyVmFsdWUxMSI6Ijk4LjQ5JSIsInN0clZhbHVlMTIiOiI5OC45MiUiLCJzdHJWYWx1ZTEzIjoiOTkuNTIlIiwic3RyVmFsdWUxNCI6IjEwMC4yNCUiLCJzdHJWYWx1ZTE1IjoiOTkuNTYlIiwic3RyVmFsdWUxNiI6Ijk5Ljk5JSIsInN0clZhbHVlMTciOiI5OS42OSUiLCJzdHJWYWx1ZTE4IjoiOTkuODIlIiwic3RyVmFsdWUxOSI6IjEwMC4wNyUiLCJzdHJNaWRWYWx1ZSI6IjEzMC4xMCIsInN0ck1pZFN0YXJ0IjoiODczMTQ1OS4zNSIsInN0ck1pZEVuZCI6Ijg3MzIxNjcuNjUiLCJzdHJUcmFuc2l0aW9uUG9pbnQiOiI/Pz8/Pz8/Pz8/Iiwic3RySXNVcGxvYWRTdWNjIjoiIiwic3RyTmVnYXRpdmVWYWx1ZSI6IjcuMjkiLCJzdHJQb3NpdGl2ZVZhbHVlIjoiNy4yOSIsInN0cksiOiIwLjk1NzgiLCJzdHJCIjoiMjAuMzAxNyJ9XQ==";
            var kk=Convert.FromBase64String(str);
            var utf8Str=System.Text.Encoding.Unicode.GetString(kk);
            //var atlPackage = new atlTcpPackage.AtlFormatterConverter();
            //var result =(string) atlPackage.Convert(kk, typeof(string));
            //var tim = atlPackage.dateTimeFromBytes(result.calibTime);
            //Console.WriteLine(JsonConvert.SerializeObject(result));
            Console.WriteLine("");
        }

        [TestMethod]
        public void TestMarshArr()
        {
            float[] temp = new float[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            List<byte> bytes1=new List<byte>();
            foreach (var item in temp)
                bytes1.AddRange(BitConverter.GetBytes(item));
            var bytes2=MarshalHelper.ArrayToBytes<float>(temp).ToList();
            foreach(var item in bytes1.Zip(bytes2))
                Assert.IsTrue(item.First==item.Second);
            Console.WriteLine("");
        }

        [TestMethod]
        public void Testddd()
        {
            atlTcpPackage.PackInfo.ResponseType m = (atlTcpPackage.PackInfo.ResponseType)2;
            Console.WriteLine("");
        }
    }
}
