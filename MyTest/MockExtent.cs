using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using DevExpress.Entity.Model;

using KFCK.ThicknessMeter.Services;
using KFCK.ThicknessMeter.Controls;
using DevExpress.Entity.ProjectModel;
using KFCK.ThicknessMeter.MyTest.Fiters;
using KFCK.ThicknessMeter.Filters;
namespace KFCK.ThicknessMeter.MyTest
{
    public static class MockExtent
    {
        public static void RegisterMock(this Autofac.ContainerBuilder container)
        {
            container.RegisterType<mockDevice>().AsSelf().SingleInstance();
            container.RegisterType<MyTestUserControl1>().AsSelf().SingleInstance();
            container.RegisterType<DeubegWind>().AsSelf().SingleInstance();
            container.RegisterOptionItemModule<MyTest2OptionUserControl1>(Metadata.MyTestOption);
            //远程数所通迅服务


            //测试分区过滤器
            //container.RegisterType<TripItFilter>().Keyed<IDataPointFilter>(WellKnownFilterType.TripIt).ExternallyOwned();

            //注册通用趟数据提供者
            container.RegisterType<TestTripOutputProvider>().As<ITripOutputProvider>();
            //注册射线通道专用过滤器配置（实体、单例）
            container.RegisterType<TestFilterConfigs>().SingleInstance();
            //测试DataServer的趟数据输出
            //container.RegisterAutoExecutor<DataServerTripOut0>();
            //container.RegisterAutoExecutor<DataServerTripOut1>();

            //container.RegisterType<DataPointResultRec>();
            container.RegisterType<RemoteDataPointResultRec>().AsSelf().As<IRemoteDataPointResultRec>();
            container.RegisterType<GlobalRemoteRec>().SingleInstance().As<IRemotedRec>();
            container.RegisterType<MyTest.Entrance>().SingleInstance().AsImplementedInterfaces();
            //container.RegisterType<TestCustomFilterConfigs>().SingleInstance();
            container.RegisterServiceModule<KFCK.ThicknessMeter.MyTest.Service.DataServer>(MWellKnownServiceKeys.DataServer0, Metadata.DataServer);
            container.RegisterType<CustomFilterConfigsBak>().SingleInstance();

            //container.RegisterType<KFCK.ThicknessMeter.MyTest.Fiters.TripPartitionItFilter>().Keyed<IDataPointFilter>(WellKnownFilterType.TripPartitionIt).ExternallyOwned();

            //访类型读取DataNotifier信息
            container.RegisterType<MyTest.Service.DataNotifierInfo>().AsSelf().SingleInstance();
        }
    }
}
