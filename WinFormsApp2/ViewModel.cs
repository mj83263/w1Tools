using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
namespace WinFormsApp2
{
    public class ViewModel:ReactiveUI.ReactiveObject
    {
        [DataMember,Reactive]
        public Int32 TestData { set; get; }
    }
}
