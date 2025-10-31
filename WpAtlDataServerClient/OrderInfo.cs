using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpAtlDataServerClient
{
    public class OrderInfo
    {
        public string Name { set; get; }
        public string Desc { set; get; }
        public override string ToString()
        {
            return Name;
        }
    }

    public class DOrderInfoList : ObservableCollection<OrderInfo>
    {

        public DOrderInfoList() {}
    }

}
