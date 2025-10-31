using System.Reactive.Linq;
using System.Reactive.Disposables;
using Newtonsoft.Json;
namespace myBaseProider
{
    [TestClass]
    public sealed class Test1
    {
        class Info
        {
            public string Name { set; get; }
            public Int32 Age { set; get; }
            public object Flg { set; get; } = "";
            public override string ToString()
            {
                return string.Join(",", Name, Age.ToString(), Flg.ToString());
            }
        }

        class Tcc : MyBaseProvider<Info>
        {
            Glog.Glog Logger = new Glog.Glog("./Logs");
            IObservable<Info> source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Select(a=>new Info() { Name=a.ToString(),Age=(Int32)a});
            public override void Start()
            {
                _disposables = new CompositeDisposable();
                Logger.Info($"数据源启动了..");
                source
                    .Subscribe(a => OnPush(CreateSender(this, this.GetType(), "OutPut"), a)).DisposeWith(_disposables);
            }
            public override void Stop()
            {
                base.Stop();
                Logger.Info($"数据源关闭了..");
            }
        }
        Glog.Glog Logger;
        [TestInitialize]
        public void Init()
        {
            Logger = new Glog.Glog("./Logs");
        }
        [TestMethod]
        public void TestMethod1()
        {
            var tccPushSource = new Tcc();
            Func<Int32, Int32> GetNo = (seed) => (Int32)(new Random(seed).NextDouble() * 10);

            List<IDisposable> disposables = new List<IDisposable>();
            object _lockThis=new object();
            Action Create = () =>
            {
                Int32 cout = 0;
                var ticks = DateTime.Now.Ticks;
                var no = GetNo((Int32)ticks);
                var disposable=new System.Reactive.Disposables.CompositeDisposable();
                var kk = tccPushSource.PushStream
                .Take(no)
                .Do(a=>a.EventArgs.Flg=cout++)
                .Subscribe(
                onNext:  a => Logger.Info($"{ticks}|{no}:->{a.EventArgs.ToString()}"),
                onError: ex => 
                {
                    Logger.Error($"{ticks}|{no}:->流错误 {ex.Message}|{ex.StackTrace}");
                    disposable.Dispose();
                },
                onCompleted: () => 
                {
                    Logger.Info($"{ticks}|{no}:->完成，并释放");
                    disposable.Dispose();
                }
                ).DisposeWith(disposable);
                lock (_lockThis) 
                {
                    disposables.Add(kk);
                }
                
                System.Reactive.Disposables.Disposable.Create(()=>
                {
                    lock(_lockThis)
                        disposables.Remove(kk);
                }).DisposeWith(disposable);
            };
            Observable.Range(1, 50).Subscribe(a =>
            {
                Task.Run(() =>
                {
                    Create();
                });
            });
            System.Threading.Thread.Sleep(1000);
            Observable.Range(1, 50).Subscribe(a =>
            {
                Task.Run(() =>
                {
                    Create();
                });
            });
            while (!disposables.Any()) ;
            while (disposables.Any())
                System.Threading.Thread.Sleep(1000);
                

        }
    }
}
