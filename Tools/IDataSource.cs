using IPlatform.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public interface IDataSource<T>:IPull<T>,IPush<T>
    {
        string Name { set; get; }
        T Next();
        T Previous();
    }
}
