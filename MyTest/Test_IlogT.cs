using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFCK.ThicknessMeter.MyTest
{
    internal class Test_Ilog<T> : ILogger<T>
    {
        public bool IsTraceEnabled => true;

        public bool IsDebugEnabled => true;
        
        public Test_Ilog() 
        { 
            
        }
        public void Debug(string message)
        {
            
        }

        public void Debug(Exception exception, string message)
        {
            throw new NotImplementedException();
        }

        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception exception, string message)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string message)
        {
            throw new NotImplementedException();
        }

        public void Fatal(Exception exception, string message)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public void Info(Exception exception, string message)
        {
            throw new NotImplementedException();
        }

        public void Trace(string message)
        {
            throw new NotImplementedException();
        }

        public void Trace(Exception exception, string message)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message)
        {
            throw new NotImplementedException();
        }

        public void Warn(Exception exception, string message)
        {
            throw new NotImplementedException();
        }
    }
}
