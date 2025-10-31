
using AutoMapper;
using Grpc.Core;
using KFCK.Entities;
using KFCK.ThicknessMeter.Communication;
using KFCK.ThicknessMeter.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMFormulasChange
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var automapCfg = new MapperConfiguration(cfg => cfg.AddProfile<KFCK.ThicknessMeter.CommunicationProfile>());
            var Mapper = automapCfg.CreateMapper();

            var channel = new Channel("127.0.0.1:8970", ChannelCredentials.Insecure);
            var client = new client(channel);
            var formula2 = new KFCK.ThicknessMeter.Configuration.Formula() 
            {
                ProductType = "TestFormula"
            };
            formatFormula(formula2);
            var identityContext = KFCK.Entities.IdentityContext.SystemIdentity;
            Int32 count = 0;
            while (true) 
            {
                Console.WriteLine("任意键设置修改配置方测试 三次");
                Console.ReadKey();
                formula2.ShiftFactor += 0.1;
                formula2.ProductType = $"TestFormula{count++}";
                var req = new SetFormulaRequest()
                {
                    Formula = Mapper.Map<KFCK.ThicknessMeter.Communication.Formula>(formula2),
                    IdentityContext = Mapper.Map<KFCK.ThicknessMeter.Communication.IdentityContext>(identityContext)
                };
                var k = client.SetFormulaAsync(req, default).GetAwaiter().GetResult();
                //k = client.SetFormulaAsync(req, default).GetAwaiter().GetResult();
                //k = client.SetFormulaAsync(req, default).GetAwaiter().GetResult();
                Enumerable.Range(0, 1).AsParallel().ForAll(async a =>
                {
                    await client.SetFormulaAsync(req, default);
                    await client.SetFormulaAsync(req, default);
                    await client.SetFormulaAsync(req, default);
                });
                //Enumerable.Range(0, 1).AsParallel().ForAll(async a =>
                //{
                //    await client.SetFormulaAsync(req, default);
                //});
            }

            Console.WriteLine("");
        }
        static void formatFormula(KFCK.ThicknessMeter.Configuration.Formula formula)
        {
            const int digits = 2;
            formula.SubstrateWeight = Math.Round(formula.SubstrateWeight, digits);
            formula.ASurfaceWeight = Math.Round(formula.ASurfaceWeight, digits);
            formula.BSurfaceWeight = Math.Round(formula.BSurfaceWeight, digits);
            formula.ToleranceUpper = Math.Round(formula.ToleranceUpper, digits);
            formula.WarningToleranceUpper = Math.Round(formula.WarningToleranceUpper, digits);
        }
    }
}
