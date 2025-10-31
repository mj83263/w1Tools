using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KFCK.ThicknessMeter.Services;
using Newtonsoft.Json;

using ReactiveUI;
namespace KFCK.ThicknessMeter.MyTest
{
    public partial class EnvironmentVariablesUserControl1 : UserControl
    {
        public class Dinfo : ReactiveUI.ReactiveObject
        {
            string _name=string.Empty;
            public string Name { set=>this.RaiseAndSetIfChanged(ref _name,value); get=>_name; }
            string _value=string.Empty;
            public string Value { set => this.RaiseAndSetIfChanged(ref _value, value); get => _value; }
        }
        System.Collections.Concurrent.ConcurrentDictionary<string, Dinfo> Vdata = new System.Collections.Concurrent.ConcurrentDictionary<string, Dinfo>();
        static Dictionary<string, System.Reflection.PropertyInfo> envProsDic { set; get; } = null;
        EnvironmentVariables Source;
        public EnvironmentVariablesUserControl1(EnvironmentVariables source)
        {

            envProsDic = envProsDic ?? typeof(EnvironmentVariables).GetProperties().ToDictionary(a => a.Name, b => b);
            foreach (var prop in envProsDic)
                Vdata.AddOrUpdate(prop.Key, new Dinfo() { Name = prop.Key }, (k, v) =>v);
            InitializeComponent();
            source.PropertyChanged += source_PropertyChanged;
            this.Disposed += (obj, e) => source.PropertyChanged -= source_PropertyChanged;
            Source = source;
            dgv1.Rows.Clear();
            dgv1.Columns.Clear();
            dgv1.AutoGenerateColumns = false;
            dgv1.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "名称", Name = "name", DataPropertyName = "Name", AutoSizeMode=   DataGridViewAutoSizeColumnMode.AllCells,ReadOnly=true});
            dgv1.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "值", Name = "Value", DataPropertyName = "Value", AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill, ReadOnly=false });
            dgv1.DataSource = Vdata.Values;
            dgv1.CellValueChanged += dgv1_CellValueChanged;
        }

        private void dgv1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0&&e.RowIndex>=0)
            {
                var dgv = sender as DataGridView;
                var cell = dgv.Rows[e.RowIndex].Cells[e.RowIndex];
                var pro = cell.OwningRow.Cells["Name"].Value.ToString();
                var value=cell.OwningRow.Cells["Value"].ToString();
                if( envProsDic.TryGetValue(pro,out var propertyInfo)&&propertyInfo.PropertyType.IsValueType)
                {
                    var realevalue=Convert.ChangeType(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(Source, realevalue);
                }
            }
        }

        static System.Reflection.PropertyInfo[] envPros = null;
        private void source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
            var env = sender as EnvironmentVariables;
           
            if(envProsDic.TryGetValue(e.PropertyName,out var pro)&&pro is not null)
            {
                var v = pro.GetValue(env);
                if (pro.PropertyType.IsValueType)
                    Vdata.AddOrUpdate(e.PropertyName, new Dinfo() { Name=e.PropertyName,Value=v?.ToString()}, (k, j) =>
                    {
                        j.Value= v?.ToString();
                        return j;
                    });
                else
                    Vdata.AddOrUpdate(e.PropertyName, new Dinfo() { Name = e.PropertyName, Value =  JsonConvert.SerializeObject(v)}, (k, j) =>
                    {
                        j.Value = JsonConvert.SerializeObject(v);
                        return j;
                    });
            }
            
        }
        
    }
}
