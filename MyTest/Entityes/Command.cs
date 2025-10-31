using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace KFCK.ThicknessMeter.MyTest
{
  
    public class Command
    {
        public class Args
        {

        }
        public class Names
        {
            public const string Clr = nameof(Clr);
            public const string Get = nameof(Get);
        }
        public class ClrArgs : Args
        {

        }
        public class GetArgs : Args
        {
            public string Path { set; get; }
            public string Name { set; get; }
            public string ResourcePath { set; get; }
        }
    }
    public class Command<T>:Command
        where T:Command.Args
    {
        
       
        public Command(string name,T value) 
        { 
           Name = name;
           Value = value;
        }
        public string Name { get; set; }
        public T Value { get; set; }
    }
}
