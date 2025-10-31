// See https://aka.ms/new-console-template for more information
var fileCheck=new FileSystemWatcher();
fileCheck.Path = "D:\\stakingVVnext\\KFCK.ThicknessMeter.App\\KFCK.ThicknessMeter\\bin\\Dev\\verifications";
fileCheck.Filters.Add("*.csv");
fileCheck.Filters.Add("*.json");
fileCheck.NotifyFilter= NotifyFilters.LastWrite;
fileCheck.EnableRaisingEvents = true;
string sharePath = @"\\192.168.1.151\s";
var myshare = Directory.CreateDirectory(Path.Combine(sharePath, "Mytest3"));
//fileCheck.Created += FileCheck_Created;



//void FileCheck_Created(object sender, FileSystemEventArgs e)
//{
//    Console. WriteLine(e.FullPath+" "+e.ChangeType);
//    File.Copy(e.FullPath, Path.Combine(myshare.FullName, e.Name));
//}

fileCheck.Changed += FileCheck_Changed;

void FileCheck_Changed(object sender, FileSystemEventArgs e)
{
    if(e.ChangeType== WatcherChangeTypes.Changed)
    {
        Console.WriteLine(e.FullPath + " " + e.ChangeType);
        //File.Copy(e.FullPath, Path.Combine(myshare.FullName, e.Name),true);
    }
}

Console.ReadKey();
//fileCheck.NotifyFilter = NotifyFilters.LastWrite;

