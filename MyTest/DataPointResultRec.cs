using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac.Features.Indexed;

using CsvHelper;
using CsvHelper.Configuration;

using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;

namespace KFCK.ThicknessMeter.MyTest
{
    public class DataPointResultRec /*: ICSVFactory*/
    {
        string bPath = "./";
        readonly IUtility Utility;
        readonly ICsvUtility CsvUtility;
        public DataPointResultRec(IUtility utility, ICsvUtility csvUtility, IEnvironmentVariables environmentVariables,
            IIndex<DataPointTypes, Func<DataFileTripClassMapContext, ClassMap<DataFileTrip>>> fileTripClassMaps,
            ICustomFileReportOutput customFileReportOutput = null)
        {
            Utility = utility;
            bPath = utility.GetAppFilePath(utility.GetAppFilePath("MyTestData"));
            utility.EnsureDirExist(bPath);
        }

        string GetFileName(string name)
        {
            return name+"_" + DateTime.Now.ToString("yyMMddHHmmss")+".csv";
        }
        CsvHelper.IWriter writer;
        public void Start(string name)
        {
            writer = new CsvHelper.CsvWriter(new StreamWriter( Path.Combine(bPath, GetFileName(name))),culture: System.Globalization.CultureInfo.CurrentUICulture);
        }
        public void Write(string name, DataPointResult data)
        {
            writer.WriteField(name);
            writer.WriteField(data.MinX);
            foreach (var x in data.DataPoints)
            {
                writer.WriteField(x.ToString());
            }
            writer.Flush();
            writer.NextRecord();
        }
        public void End() 
        {
            writer.Flush();
            writer.Dispose();
        }
 
    }
}
