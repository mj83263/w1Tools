namespace TestFilesCmp
{
    [TestClass]
    public sealed class Test1
    {
        Glog.Glog Log;
        [TestInitialize]
        public void Init()
        {
            Log = new Glog.Glog("./Log");
        }
        [DataRow(
            "C:\\Users\\mj83\\Desktop\\启动异常\\一架备份1108添加附加程序集后\\一架备份",
            "C:\\Users\\mj83\\Desktop\\启动异常\\一架备份1108添加附加程序集后 - 副本\\一架备份")
            ]
        [TestMethod]
        public void Cmpdirectory(string path1, string path2)
        {
            var files1 = Directory.GetFiles(path1).Select(a => new FileInfo(a)).ToArray();
            var files2 = Directory.GetFiles(path2).Select(a => new FileInfo(a)).ToArray();
            //共有的文件
            var comFiles = files1.Where(a => files2.Any(b => a.Name == b.Name)).ToArray();

            var onlyFiles1 = files1.Where(a => !files2.Any(b => a.Name == b.Name)).ToArray();
            var onlyFiles2 = files2.Where(a => !files1.Any(b => a.Name == b.Name)).ToArray();

            Log.Info($"仅在{path1}中存在的文件{onlyFiles1.Length}个");
            foreach (var f in onlyFiles1)
                Log.Info(f.Name);
            Log.Info($"仅在{path2}中存在的文件{onlyFiles2.Length}个");
            foreach (var f in onlyFiles2)
                Log.Info(f.Name);
            Log.Info($"公有的文件");
            foreach (var item in comFiles)
            {
                var f1 = files1.FirstOrDefault(a => a.Name == item.Name);
                var f2 = files2.FirstOrDefault(a => a.Name == item.Name);
                var bytes1 = File.ReadAllBytes(f1.FullName);
                var bytes2 = File.ReadAllBytes(f2.FullName);
                var isSame = bytes1.Zip(bytes2).All(a => a.First == a.Second);
                Log.Info($"{item.Name.PadRight(100)},是否相同:{isSame}\t\t");
            }

        }
    }
}
