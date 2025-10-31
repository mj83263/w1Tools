using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class DGroupsInfoList : ObservableCollection<string>
    {
        
        public DGroupsInfoList() { this.Add("连接成功");this.Add("SystemLog");this.Add("Resource"); }
    }

    public class DGroupsInfo:INotifyPropertyChanged
    {
        public string Desc { set; get; }
        bool _IsSave = false;
        public bool IsSave { set=>Change(ref _IsSave,value); get=>_IsSave; }
        public DGroupsInfo(string desc,bool isSave) { this.Desc = desc; IsSave = isSave; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Change<T>(ref T d,T value, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if(d.Equals(value)) return;
            d = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
    public class DGroupsInfoList2 : ObservableCollection<DGroupsInfo>
    {

        public DGroupsInfoList2() { this.Add(new DGroupsInfo("连接成功",false)); this.Add(new DGroupsInfo( "SystemLog",false));this.Add(new DGroupsInfo("Resource", false)); }
    }

    public class Filter_NotDisplay : ObservableCollection<string>
    {
        public Filter_NotDisplay() 
        { 
            this.Add("包函该列表中的字串不显示");
            this.Add("标准片数据已更新");
        }
    }

}
