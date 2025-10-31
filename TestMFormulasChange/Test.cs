
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using KFCK.ThicknessMeter.Communication;
using KFCK.ThicknessMeter.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMFormulasChange
{
    class CommunicatorImplementation : Communicator.CommunicatorBase
    {

    }
    class client : Communicator.CommunicatorClient
    {
        public client(Channel channel) : base(channel) { }
        public override Empty SetFormula(SetFormulaRequest request, CallOptions options)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm.ss.fff")}SetFormula{request.Formula.ProductType}");
            return base.SetFormula(request, options);
        }
        public override AsyncUnaryCall<Empty> SetFormulaAsync(SetFormulaRequest request, CallOptions options)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm.ss.fff")}SetFormulaAsync{request.Formula.ProductType}");
            AsyncUnaryCall<Empty> tmp =default(AsyncUnaryCall<Empty>);
            try
            {
                tmp= base.SetFormulaAsync(request, options); 
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"{ex.Message} {ex.StackTrace}");
            }
            return tmp;
        }
    }
}
