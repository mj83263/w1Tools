using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace myBaseProider
{
   
    public abstract class MyBaseProvider<T>
    {
        
        /// <summary>
        /// 可以做为事件Sender传递出去
        /// </summary>
        public class EventSender
        {
            public object Sender { set; get; }
            public Type Type { set; get; }
            public string Method { set; get; }
            public EventSender(object sender,Type type,string method) 
            {
                Sender = sender;Type = type;Method = method;
            }
        }
        public EventSender CreateSender(object sender,Type type,string mehtod)=>new EventSender(sender,type,mehtod);

        protected System.Reactive.Disposables.CompositeDisposable _disposables;

        /// <summary>
        /// 事件绑定
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="add"></param>
        /// <param name="remove"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable EventBind<TArg>(
            Action<TArg> handler,
            Action<Action<TArg>> add,
            Action<Action<TArg>> remove
            )
        {
            add(handler);
            return Disposable.Create(() => remove(handler));
        }



        public string Name { set; get; }
        public MyBaseProvider()
        {
            Name = this.GetType().Name;
        }
        public abstract void Start();
        public virtual void Stop() { _disposables?.Dispose(); }
        //不想用结构体的副本
        public delegate void EventHandlerReadOnly<TEventArgs>(EventSender sender, in TEventArgs e);
        /// <summary>
        /// 事件输出
        /// </summary>
        public EventHandlerReadOnly<T> OnPush;
        IObservable<EventPattern<EventSender, T>> _PushStream = null;
        object _lock = new object();
        
        /// <summary>
        /// 流式输出，有接入时自动开启数据，所有接入都断开时，关闭数据源
        /// </summary>
        public IObservable<EventPattern<EventSender, T>> PushStream 
        {
            get
            {
                lock (_lock)
                {
                    if (_PushStream == null)
                    {
                        _PushStream = Observable.Create<EventPattern<EventSender, T>>(obs =>
                        {
                            lock (_lock)
                            {
                                EventHandlerReadOnly<T> handler = (EventSender s, in T e) => obs.OnNext(new EventPattern<EventSender, T>(s, e));
                                OnPush += handler;
                                this.Start();
                                return Disposable.Create(() =>
                                {
                                    lock (_lock) 
                                    {
                                        OnPush -= handler;
                                        this.Stop();
                                    }
                                });
                            }
                        })
                        .Publish()
                        .RefCount();
                    }
                    return _PushStream;
                }
            }
        }

    }
}
